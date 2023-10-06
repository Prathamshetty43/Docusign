using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using jwt.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace jwt.Services
{
    public class DocuSignService
    {
        private readonly string _basePath = "https://demo.docusign.net/restapi";
        private readonly string _accountId = AccessToken.AccountId;
        private readonly HttpClient _httpClient;

        public DocuSignService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SendDocumentForSigning(DocusignForm docusignForm, string accessToken)
        {
            try
            {
                // Define the envelope with the document and recipient.
                EnvelopeDefinition envelopeDefinition = MakeEnvelope(docusignForm);

                // Initialize the DocuSign client.
                var docuSignClient = new DocuSignClient(_basePath);
                docuSignClient.Configuration.DefaultHeader.Add("Authorization", "Bearer " + accessToken);

                // Create an instance of the EnvelopesApi.
                EnvelopesApi envelopesApi = new EnvelopesApi(docuSignClient);

                // Create the envelope in DocuSign.
                var envelopeSummary = await envelopesApi.CreateEnvelopeAsync(_accountId, envelopeDefinition);

                return envelopeSummary.EnvelopeId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send the document for signing: {ex.Message}");
            }
        }

        public EnvelopeDefinition MakeEnvelope(DocusignForm docusignForm)
        {
            byte[] documentBytes = File.ReadAllBytes("thatch-sample.pdf");

            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = "Please sign this document",
                Documents = new List<Document>
                {
                    new Document
                    {
                        DocumentBase64 = Convert.ToBase64String(documentBytes),
                        Name = "thatch-sample",
                        FileExtension = "pdf",
                        DocumentId = "1" // Unique identifier for the document

                    }
                },
                Recipients = new Recipients
                {
                    Signers = new List<Signer>
                    {
                        new Signer
                        {
                            Email = docusignForm.TenantEmail,
                            Name = docusignForm.TenantName,
                            RecipientId = "1",
                            Tabs = new Tabs
                            {
                                FullNameTabs = new List<FullName>
                                {
                                    new FullName
                                    {
                                        AnchorString = "/name/",
                                        AnchorUnits = "pixels",
                                    }
                                },
                                EmailTabs = new List<Email>
                                {
                                    new Email
                                    {
                                        AnchorString = "/address/",
                                        AnchorUnits = "pixels",
                                        Value = docusignForm.TenantEmail,
                                    }
                                },
                                SignHereTabs = new List<SignHere>
                                {
                                    new SignHere
                                    {
                                        AnchorString = "/sign/",
                                        AnchorUnits = "pixels",
                                    }
                                },
                                DateTabs = new List<Date>
                                {
                                    new Date
                                    {
                                        AnchorString = "/start/",
                                        AnchorUnits = "pixels",
                                        Value = docusignForm.StartDate,
                                    },
                                    new Date
                                    {
                                            AnchorString = "/end/",
                                            AnchorUnits = "pixels",
                                            Value = docusignForm.EndDate,
                                    }

                                },
                                TextTabs = new List<Text>
                                {
                                    new Text
                                    {
                                        Value = docusignForm.mobile,
                                        AnchorString = "/mobile/",
                                        AnchorUnits = "pixels",
                                    },
                                    new Text
                                    {
                                        Value = docusignForm.Rent.ToString(),
                                        AnchorString = "/rent/",
                                        AnchorUnits = "pixels",
                                    },
                                }
                            }
                        }
                    }
                },
                Status = "sent"
            };

            return envelopeDefinition;
        }

        public async Task<Envelope> GetEnvelopeStatus(string accessToken, string accountId, string envelopeId)
        {
            try
            {
                // Authenticate with DocuSign using the access token.
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                // Specify the endpoint to get envelope status.
                string statusUrl = $"{_basePath}/v2/accounts/{accountId}/envelopes/{envelopeId}";

                // Send a GET request to retrieve envelope status.
                HttpResponseMessage response = await _httpClient.GetAsync(statusUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Parse the response to get envelope status.
                    var json = await response.Content.ReadAsStringAsync();
                    var envelope = JsonConvert.DeserializeObject<Envelope>(json);
                    return envelope;
                }
                else
                {
                    throw new Exception($"Failed to retrieve envelope status. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving envelope status: {ex.Message}");
            }
        }

        public async Task<byte[]> DownloadSignedDocument(string accessToken, string accountId, string envelopeId, string documentId)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                string downloadUrl = $"{_basePath}/v2/accounts/{accountId}/envelopes/{envelopeId}/documents/{documentId}";

                HttpResponseMessage response = await _httpClient.GetAsync(downloadUrl);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    throw new Exception($"Failed to download document. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while downloading the document: {ex.Message}");
            }
        }
    }
}
