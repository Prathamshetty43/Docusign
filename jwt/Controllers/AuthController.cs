using jwt.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using jwt.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using DocuSign.eSign.Api;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace jwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DocuSignAuthenticationService _authService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DocuSignService _docuSignService;

        public AuthController(DocuSignAuthenticationService authService, IHttpClientFactory httpClientFactory, DocuSignService docuSignService)
        {
            _authService = authService;
            _httpClientFactory = httpClientFactory;
            _docuSignService = docuSignService;
        }
        [EnableCors("AllowDocuSign")]
        [HttpGet()]
        public ActionResult Authenticate()
        {
            var authUrl = _authService.GetAuthorizationUrl();
            return Redirect(authUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(jwtToken))
            {
                var claims = GetClaimsFromJwtToken(jwtToken);
                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                return Ok($"Authenticated via JWT for user {userId}");
            }
            else if (!string.IsNullOrEmpty(code))
            {
                var accessToken = await _authService.ExchangeCodeForAccessTokenAsync(code);
                AccessToken.token = accessToken;
                var userInfo = await GetUserInfoFromDocuSign(accessToken);
                return Ok(new { UserInfo = userInfo, mode = "cors" });
            }
            else
            {
                return BadRequest("User did not grant consent or provide a JWT token.");
            }
        }

        [HttpPost("sendDocument")]
        public async Task<IActionResult> SendDocument([FromBody] DocusignForm docusignForm)
        {
            try
            {
                var envelopeId = await _docuSignService.SendDocumentForSigning(docusignForm, AccessToken.token);
                AccessToken.EnvelopId = envelopeId;
                return Ok($"Document sent for signing. Envelope ID: {envelopeId}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("getEnvelopeStatus")]
        public async Task<IActionResult> GetEnvelopeStatus(string EnvelopId)
        {
            try
            {
                var envelope = await _docuSignService.GetEnvelopeStatus(AccessToken.token, AccessToken.AccountId, EnvelopId);
                return Ok(envelope);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("downloadDocument")]
        public async Task<IActionResult> DownloadDocument(string EnvelopId)
        {
            try
            {
                byte[] documentBytes = await _docuSignService.DownloadSignedDocument(AccessToken.token, AccessToken.AccountId, EnvelopId, AccessToken.DocumentId);
                return File(documentBytes, "application/pdf", "SignedDocument.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        private IEnumerable<Claim> GetClaimsFromJwtToken(string jwtToken)
        {
            return new List<Claim>();
        }

        private async Task<DocuSignUserInfo> GetUserInfoFromDocuSign(string accessToken)
        {
            var userInfoEndpoint = "https://account-d.docusign.com/oauth/userinfo";

            using (var client = _httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await client.GetAsync(userInfoEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<DocuSignUserInfo>(json);
                    return userInfo;
                }
                else
                {
                    throw new Exception("Failed to fetch user info from DocuSign.");
                }
            }
        }
    }
}
