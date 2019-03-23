using HomeSeerAPI;
using NullGuard;
using Scheduler;
using System;
using System.Collections.Specialized;
using System.Threading;
using Hspi.Pages;
using Hspi.Utils;
using Hspi.Exceptions;
using Twilio.Rest.Api.V2010.Account;

namespace Hspi
{
    /// <summary>
    /// Plugin class for Twilio Messaging
    /// </summary>
    /// <seealso cref="Hspi.HspiBase" />
    [NullGuard(ValidationFlags.Arguments | ValidationFlags.NonPublic)]
    internal partial class TwilioMessagingPlugin : HspiBase
    {
        public TwilioMessagingPlugin()
            : base(TwilioMessagingData.PlugInName)
        {

        }

        public override string InitIO(string port)
        {
            string result = string.Empty;
            try
            {
                pluginConfig = new PluginConfig(HS);
                configPage = new ConfigPage(HS, pluginConfig);
                LogInfo("Starting Plugin");
#if DEBUG
                pluginConfig.DebugLogging = true;
#endif
                LogConfiguration();

                pluginConfig.ConfigChanged += PluginConfig_ConfigChanged;

                RegisterConfigPage();

                twilioService = new TwilioServiceFacade(HS, pluginConfig.DebugLogging);

                ScheduleRefreshTrigger();

                LogDebug("Plugin Started");
            }
            catch (Exception ex)
            {
                result = $"Failed to initialize PlugIn With {ex.GetFullMessage()}";
                LogError(result);
            }

            return result;
        }

        private void LogConfiguration()
        {
            LogDebug($"AccountSID:{pluginConfig.AccountSID}");
        }

        private void PluginConfig_ConfigChanged(object sender, EventArgs e)
        {

        }

        public override void LogDebug(string message)
        {
            if (pluginConfig.DebugLogging)
            {
                base.LogDebug(message);
            }
        }

        public override string GetPagePlugin(string page, [AllowNull]string user, int userRights, [AllowNull]string queryString)
        {

            if (page == ConfigPage.Name)
            {
                return configPage.GetWebPage();
            }

            return string.Empty;
        }

        public override string PostBackProc(string page, string data, [AllowNull]string user, int userRights)
        {
            if (page == ConfigPage.Name)
            {
                return configPage.PostBackProc(data, user, userRights);
            }

            return string.Empty;
        }

        #region "Script Override"

        public override object PluginFunction([AllowNull]string functionName, [AllowNull] object[] parameters)
        {
            try
            {
                switch (functionName)
                {
                    case null:
                        return null;

                    case "SendMessage":
                        LogInfo("Sending message via 'SendMessage' plugin function");

                        if (parameters == null || parameters.Length != 2)
                        {
                            string count = parameters == null ? "none" : parameters.Length.ToString();
                            LogError("SendMessage: expected 2 parameters, but found " + count);
                            return null;
                        }

                        string toNumber = parameters[0] as string;
                        string message = parameters[1] as string;

                        SendMessageActionConfig config = new SendMessageActionConfig
                        {
                            ToNumber = toNumber,
                            Message = message
                        };

                        SendMessageToTwilio(config);
                        return null;

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to execute function with {ex.GetFullMessage()}");
                return null;
            }
        }

        #endregion "Script Override"

        private static string NameToId(string name)
        {
            return name.Replace(' ', '_');
        }

        public void SendMessageToTwilio(SendMessageActionConfig config)
        {
            this.twilioService.SendMessageToTwilio(this.pluginConfig, config);
        }

        private void RegisterConfigPage()
        {
            string link = ConfigPage.Name;
            HS.RegisterPage(link, Name, string.Empty);

            HomeSeerAPI.WebPageDesc wpd = new HomeSeerAPI.WebPageDesc
            {
                plugInName = Name,
                link = link,
                linktext = "Configuration",
                page_title = $"{Name} Configuration"
            };
            Callback.RegisterConfigLink(wpd);
            Callback.RegisterLink(wpd);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (pluginConfig != null)
                {
                    pluginConfig.ConfigChanged -= PluginConfig_ConfigChanged;
                }
                if (configPage != null)
                {
                    configPage.Dispose();
                }

                if (pluginConfig != null)
                {
                    pluginConfig.Dispose();
                }

                disposedValue = true;
            }

            base.Dispose(disposing);
        }
        private ConfigPage configPage;

        private PluginConfig pluginConfig;
        private TwilioServiceFacade twilioService;
        private Timer intervalRefreshTimer;
        private const int ActionSendMessageTANumber = 1;
        private const int TriggerReceiveMessageTANumber = 1;
        private bool disposedValue;
        private const string IdPrefix = "id_";
    }
}