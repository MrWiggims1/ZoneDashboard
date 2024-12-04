using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace ZoneProductionDashBoard
{
    public static class DashboardConfig
    {
        public static IConfiguration? Configuration;
        
        public static bool AllowOverviewPage
            => Debugger.IsAttached ? true : bool.TryParse(Configuration?["AllowOverviewPage"], out bool res) ? res : false;

        public static string TrelloApiKey 
            => Configuration?.GetValue<string>("TrelloApiKey")?? throw new ArgumentNullException("TrelloApiKey", "TrelloApiKey must be added to config file.");
        
        public static string TrelloUserToken 
            => Configuration?["TrelloUserToken"]?? throw new ArgumentNullException("TrelloUserToken", "TrelloUserToken must be added to config file.");
        
        public static string WebhookCallbackUrl
            => Configuration?["WebhookCallbackURL"]?? throw new ArgumentNullException("WebhookCallbackURL", "WebhookCallbackURL must be added to config file.");
        
        public static bool EnableWebhooks
            => bool.TryParse(Configuration?["enableWebhooks"], out bool res) ? res : false;

        public static bool UseDevProductionService
            => bool.TryParse(Configuration?["UseDevProductionService"], out bool res) ? res : false;
         
        
        public static void AddConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}