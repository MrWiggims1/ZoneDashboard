using System.Net.Http.Headers;
using ZoneProductionDashBoard;

namespace ZoneProductionLibrary.ProductionServices.Main;

public partial class ProductionService
{
    public async Task DownloadTrelloFileAsync(string url, string path)
    {
        if (File.Exists(IProductionService.FileBasePath + path))
            return;

        using (HttpClient client = new HttpClient()) 
        {
            using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("OAuth", "oauth_consumer_key=\"" + DashboardConfig.TrelloApiKey + "\", oauth_token=\"" + DashboardConfig.TrelloUserToken + "\"");

                HttpResponseMessage response = await client.SendAsync(req);

                response.EnsureSuccessStatusCode();

                Stream content = await response.Content.ReadAsStreamAsync();

                await using (FileStream fs =
                             new FileStream(IProductionService.FileBasePath + path, FileMode.CreateNew))
                {
                    await content.CopyToAsync(fs);
                }
            }
        }

        Log.Logger.Debug("New trello file:{url} downloaded to {path}", url, IProductionService.FileBasePath + path);
    }
}