using HomeSeerAPI;
using NullGuard;
using Scheduler;
using System;
using System.Collections.Specialized;
using System.Threading;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Hspi
{
    /// <summary>
    /// Plugin class for Weather Underground
    /// </summary>
    /// <seealso cref="Hspi.HspiBase" />
    [NullGuard(ValidationFlags.Arguments | ValidationFlags.NonPublic)]
    internal class TwilioMessagingPlugin : HspiBase
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
                        LogWarning("SendMessage not yet implemented");
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to execute function with {ex.GetFullMessage()}");
                return null;
            }
        }

        #endregion "Script Override"

        #region "Action Override"

        public override int ActionCount()
        {
            return 1;
        }

        public override string get_ActionName(int actionNumber)
        {
            switch (actionNumber)
            {
                case ActionSendMessageTANumber:
                    return $"{Name}: Send a Message";

                default:
                    return base.get_ActionName(actionNumber);
            }
        }

        public override string ActionBuildUI([AllowNull]string uniqueControlId, IPlugInAPI.strTrigActInfo actionInfo)
        {
            switch (actionInfo.TANumber)
            {
                case ActionSendMessageTANumber:
                    System.Text.StringBuilder stb = new System.Text.StringBuilder();
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart(uniqueControlId + "div", ""));
                    stb.Append(BuildActionBody(
                        uniqueControlId, 
                        SendMessageActionConfig.DeserializeActionConfig(actionInfo.DataIn)
                        ));
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());
                    return stb.ToString();

                default:
                    return base.ActionBuildUI(uniqueControlId, actionInfo);
            }
        }

        public override bool ActionConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            LogDebug("Checking if action is configured...");
            var config = SendMessageActionConfig.DeserializeActionConfig(actionInfo.DataIn);
            return config.ToNumber != null
                && config.ToNumber.Length > 0
                && config.Message != null
                && config.Message.Length > 0;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public override IPlugInAPI.strMultiReturn ActionProcessPostUI([AllowNull] NameValueCollection postData, IPlugInAPI.strTrigActInfo actionInfo)
        {
            LogDebug("Handling ActionProcessPostUI");
            var value = new IPlugInAPI.strMultiReturn();
            value.TrigActInfo = actionInfo;

            var config = new SendMessageActionConfig();
            if (postData != null && postData.HasKeys())
            {
                foreach(var key in postData.Keys)
                {
                    LogDebug(key + " has a value of " + postData[key.ToString()]);
                }

                LogDebug("Setting number to " + postData[0]);
                LogDebug("Setting message to " + postData[1]);
                config.ToNumber = postData[0];
                config.Message = postData[1];
                value.DataOut = SendMessageActionConfig.SerializeActionConfig(config);
            }
            return value;
        }

        public override string ActionFormatUI(IPlugInAPI.strTrigActInfo actionInfo)
        {
            LogDebug("Getting formatted action");
            switch (actionInfo.TANumber)
            {
                case ActionSendMessageTANumber:
                    var config = SendMessageActionConfig.DeserializeActionConfig(actionInfo.DataIn);
                    return $"Twilio sends a message to {config.ToNumber}";

                default:
                    return base.ActionFormatUI(actionInfo);
            }
        }

        public override bool HandleAction(IPlugInAPI.strTrigActInfo actionInfo)
        {
            try
            {
                switch (actionInfo.TANumber)
                {
                    case ActionSendMessageTANumber:
                        var config = SendMessageActionConfig.DeserializeActionConfig(actionInfo.DataIn);
                        SendMessageToTwilio(config);
                        return true;

                    default:
                        return base.HandleAction(actionInfo);
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to execute action with {ex.GetFullMessage()}");
                return false;
            }
        }

        #endregion "Action Override"

        private string BuildActionBody(string idSuffix, SendMessageActionConfig config)
        {
            LogDebug("Building Action Body");
            string toNumber = config.ToNumber;
            if (toNumber == null)
            {
                toNumber = "";
            }

            string message = config.Message;
            if (message == null )
            {
                message = "";
            }

            var toField = new Scheduler.clsJQuery.jqTextBox("ToNumber" + idSuffix, "", toNumber, "Events", 20, false);
            toField.label = "<strong>To</strong>";

            var messageField = new Scheduler.clsJQuery.jqTextBox("Message" + idSuffix, "", message, "Events", 100, false);
            messageField.label = "<strong>Message</strong>";

			var saveBtn = new Scheduler.clsJQuery.jqButton("submit" + idSuffix, "Save", "Events", true);

			return toField.Build() + "<br>" + messageField.Build() + "<br>" + saveBtn.Build();
        }

        public void SendMessageToTwilio(SendMessageActionConfig config)
        {
			this.twilioService.SendMessageToTwilio(this.pluginConfig, config);
        }

        private void RegisterConfigPage()
        {
            string link = ConfigPage.Name;
            HS.RegisterPage(link, Name, string.Empty);

            HomeSeerAPI.WebPageDesc wpd = new HomeSeerAPI.WebPageDesc()
            {
                plugInName = Name,
                link = link,
                linktext = "Configuration",
                page_title = $"{Name} Configuration",
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
                cancellationTokenSourceForUpdateDevice.Dispose();
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

        private CancellationTokenSource cancellationTokenSourceForUpdateDevice = new CancellationTokenSource();
        private ConfigPage configPage;
        private PluginConfig pluginConfig;
		private TwilioServiceFacade twilioService;
        private const int ActionSendMessageTANumber = 1;
        private bool disposedValue = false;
    }
}