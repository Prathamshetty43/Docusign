using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace jwt.Services
{
    public class DocuSignAuthenticationService
    {
        private readonly IOptions<DocuSignConfig> _docuSignConfig;
        private readonly IHttpClientFactory _httpClientFactory;

        public DocuSignAuthenticationService(IOptions<DocuSignConfig> docuSignConfig, IHttpClientFactory httpClientFactory)
        {
            _docuSignConfig = docuSignConfig;
            _httpClientFactory = httpClientFactory;
        }

        public string GetAuthorizationUrl()
        {
            var config = _docuSignConfig.Value;
            var authorizationEndpoint = "https://account-d.docusign.com/oauth/auth";
            var responseType = "code";
            var scope = "signature";
            var oauthUrl = $"{authorizationEndpoint}?" +
                $"response_type={responseType}&" +
                $"scope={scope}&" +
                $"client_id={config.ClientId}&" +
                $"redirect_uri={config.RedirectUri}";

            return oauthUrl;
        }

        public async Task<string> ExchangeCodeForAccessTokenAsync(string code)
        {
            var config = _docuSignConfig.Value;
            var tokenEndpoint = "https://account-d.docusign.com/oauth/token";

            using (var client = _httpClientFactory.CreateClient())
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", config.ClientId),
                    new KeyValuePair<string, string>("client_secret", config.ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", config.RedirectUri)
                });

                var response = await client.PostAsync(tokenEndpoint, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<DocuSignTokenResponse>(json);
                    return tokenResponse.AccessToken;
                }
                else
                {
                    throw new Exception("Failed to exchange code for access token with DocuSign.");
                }
            }
        }

    }

    public class DocuSignTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
