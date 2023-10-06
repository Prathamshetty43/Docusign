using System.Net.Http.Headers;
namespace jwt.Services
{
    public class OAuthUserInfoService
    {
        private readonly HttpClient _httpClient;

        public OAuthUserInfoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetUserInfoAsync(string userinfoEndpoint)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(userinfoEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }
    }
}
