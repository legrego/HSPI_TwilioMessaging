using HomeSeerAPI;
using NullGuard;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using Hspi.Utils;
using System.Web;

namespace Hspi.Pages
{
    internal class TwilioWebhookPage : PageHelper
    {
        public TwilioWebhookPage(IHSApplication HS, PluginConfig pluginConfig) : base(HS, pluginConfig, Name)
        {
        }

        /// <summary>
        /// Gets the name of the web page.
        /// </summary>
        public static string Name => pageName;

        public string GetWebPage()
        {
            AddBody("Hello world");
            return BuildPage();
        }

        public string PostBackProc(string data, [AllowNull]string user, int userRights)
        {
            NameValueCollection parts = HttpUtility.ParseQueryString(data);
            foreach (var part in parts)
            {
                Console.WriteLine(part);
            }
            return "";
        }

        private static readonly string pageName = $"{TwilioMessagingData.PlugInName} Webhook".Replace(' ', '_');
    }
}