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

                        SendMessageActionConfig config = new SendMessageActionConfig()
                        {
                            ToNumber = toNumber,
                            Message = message,
                        };

                        SendMessageToTwilio(config);
                        return null;
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

        #region "Trigger Override"
        public override bool HasTriggers => true;
        public override int TriggerCount => 1;
        protected override int GetTriggerCount()
        {
            return 1;
        }

        public override bool get_HasConditions(int triggerNumber) => false;

        public override string get_TriggerName(int triggerNumber)
        {
            switch (triggerNumber)
            {
                case TriggerReceiveMessageTANumber:
                    return $"Twilio: A text message is received from or containing...";

                default:
                    return base.get_TriggerName(triggerNumber);
            }
        }

        public override string TriggerBuildUI([AllowNull]string uniqueControlId, IPlugInAPI.strTrigActInfo triggerInfo)
        {
            switch (triggerInfo.TANumber)
            {
                case TriggerReceiveMessageTANumber:
                    System.Text.StringBuilder stb = new System.Text.StringBuilder();
                    var page = new TriggerPage(HS, this.pluginConfig);
                    return page.ReceiveMessageTriggerBuildUI(uniqueControlId, ReceiveMessageTriggerConfig.DeserializeTriggerConfig(triggerInfo.DataIn));

                default:
                    return base.ActionBuildUI(uniqueControlId, triggerInfo);
            }
        }

        public override IPlugInAPI.strMultiReturn TriggerProcessPostUI([AllowNull] NameValueCollection postData, IPlugInAPI.strTrigActInfo actionInfo)
        {
            var value = new IPlugInAPI.strMultiReturn
            {
                TrigActInfo = actionInfo
            };

            if (postData != null && postData.HasKeys())
            {
                ReceiveMessageTriggerConfig config = new ReceiveMessageTriggerConfig(postData);
                value.DataOut = ReceiveMessageTriggerConfig.SerializeTriggerConfig(config);
            }
            return value;
        }

        public override bool get_TriggerConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            ReceiveMessageTriggerConfig config = ReceiveMessageTriggerConfig.DeserializeTriggerConfig(actionInfo.DataIn);
            return config.IsValid();
        }

        public override string TriggerFormatUI(IPlugInAPI.strTrigActInfo actionInfo)
        {
            ReceiveMessageTriggerConfig config = ReceiveMessageTriggerConfig.DeserializeTriggerConfig(actionInfo.DataIn);
            return string.Format("Twilio: when an SMS message containing '{0}' from {1} is received", config.Message, config.FromDisplay);
        }
        #endregion

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

        private string BuildTriggerBody(string idSuffix, ReceiveMessageTriggerConfig config)
        {
            string fromNumber = config.FromNumber;
            if (fromNumber == null)
            {
                fromNumber = "";
            }

            string message = config.Message;
            if (message == null)
            {
                message = "";
            }

            var messageField = new Scheduler.clsJQuery.jqTextBox("Message" + idSuffix, "text", message, "Events", 100, true)
            {
                id = NameToIdWithPrefix("Message" + idSuffix)
            };

            return messageField.Build();
        }

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
        protected static string NameToIdWithPrefix(string name)
        {
            return $"{ IdPrefix}{NameToId(name)}";
        }

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

        private void ScheduleRefreshTrigger(int dueTime = triggerRefreshFrequencyMillis)
        {
            intervalRefreshTimer?.Dispose();
            intervalRefreshTimer = new Timer((x) => RefreshTriggers(), null,
                                             dueTime,
                                             Timeout.Infinite);
        }

        private void RefreshTriggers()
        {
            LogDebug("Refreshing Triggers");

            var triggers = Callback.TriggerMatches(Name, TriggerReceiveMessageTANumber, -1);

            if (triggers == null || triggers.Length == 0)
            {
                LogDebug("No triggers exist; aborting refresh");
                return;
            }

            var messages = twilioService.GetMessagesFromTwilio(pluginConfig, triggerRefreshFrequencyMillis / 1000);

            LogDebug(string.Format("Checking triggers against {0} messages", messages.Count));

            foreach (var strTrigActInfo in triggers)
            {
                if (ShutdownCancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var config = ReceiveMessageTriggerConfig.DeserializeTriggerConfig(strTrigActInfo.DataIn);
                if (config.IsValid())
                {
                    string messageToLower = config.Message.ToLower();
                    bool shouldFire = messages.Exists((MessageResource obj) =>
                    {
                        bool bodyMatches = obj.Body.ToLower().Contains(messageToLower);
                        bool fromMatches = config.FromNumber.IsNullOrWhiteSpace() || config.FromNumber == obj.From.ToString();
                        return fromMatches && bodyMatches;
                    });

                    if (shouldFire)
                    {
                        LogDebug("Firing trigger");
                        Callback.TriggerFire(Name, strTrigActInfo);
                    }
                }
                else
                {
                    LogDebug("Skipping trigger with invalid config");
                }
            }

            ScheduleRefreshTrigger();
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
        private Timer intervalRefreshTimer;
        private const int ActionSendMessageTANumber = 1;
        private const int TriggerReceiveMessageTANumber = 1;
        private bool disposedValue = false;
        private const string IdPrefix = "id_";
        private const int triggerRefreshFrequencyMillis = 15000;
    }
}