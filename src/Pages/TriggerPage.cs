using HomeSeerAPI;
using NullGuard;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using Hspi.Utils;

namespace Hspi.Pages
{
    internal class TriggerPage : PageHelper, IEventPage
    {
        public TriggerPage(IHSApplication HS, PluginConfig pluginConfig, Logger logger) : base(HS, pluginConfig, "Events", logger)
        {
        }

        public string Name()
        {
            return "Twilio: when a message is received from or containing...";
        }

        public bool IsConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            var config = ReceiveMessageTriggerConfig.DeserializeTriggerConfig(actionInfo.DataIn);
            return config.IsValid();
        }

        public string BuildEditUI([AllowNull] string uniqueControlId,
                                               IPlugInAPI.strTrigActInfo actInfo)
        {
            var config = ReceiveMessageTriggerConfig.DeserializeTriggerConfig(actInfo.DataIn);
            StringBuilder stb = new StringBuilder();

            IncludeResourceScript(stb, "ReceiveMessageTriggerScript", uniqueControlId);

            string anyNumberCheckbox = FormCheckBox("AnyNumber" + uniqueControlId, "From any number", config.FromAnyNumber, true);

            string numberInput = FormTextBox("FromNumber" + uniqueControlId, "From number", config.FromNumber);
            string numberLine = string.Format("<div id=\"FromNumberWrap{0}\">{1}</div>", uniqueControlId, numberInput);

            stb.Append(anyNumberCheckbox).Append("<br />");

            stb.Append(numberLine).Append("<br />");

            stb.Append(FormTextBox("Message" + uniqueControlId, "Message contains", config.Message));

            return stb.ToString();
        }

        public string BuildViewUI(IPlugInAPI.strTrigActInfo actInfo)
        {
            var config = ReceiveMessageTriggerConfig.DeserializeTriggerConfig(actInfo.DataIn);
            return string.Format("A message from {0} containing the text '{1}' is received", config.FromDisplay, config.Message);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IPlugInAPI.strMultiReturn ProcessPostUI([AllowNull] NameValueCollection postData,
                                                                        IPlugInAPI.strTrigActInfo actionInfo)
        {
            IPlugInAPI.strMultiReturn result = new IPlugInAPI.strMultiReturn();
            result.DataOut = actionInfo.DataIn;
            result.TrigActInfo = actionInfo;
            result.sResult = string.Empty;

            this.logger.LogDebug(string.Join("; ", postData.AllKeys));
            ReceiveMessageTriggerConfig config = new ReceiveMessageTriggerConfig();
            foreach (string key in postData.AllKeys)
            {
                if (key.StartsWith("AnyNumber", StringComparison.InvariantCulture))
                {
                    config.FromAnyNumber = postData[key] == "checked";
                }
                if (key.StartsWith("FromNumber", StringComparison.InvariantCulture))
                {
                    config.FromNumber = postData[key];
                }
                if (key.StartsWith("Message", StringComparison.InvariantCulture))
                {
                    config.Message = postData[key];
                }
            }

            if (config.FromAnyNumber)
            {
                config.FromNumber = string.Empty;
            }

            result.DataOut = ReceiveMessageTriggerConfig.SerializeTriggerConfig(config);

            return result;
        }

        public bool HandleEvent(IPlugInAPI.strTrigActInfo actionInfo, TwilioServiceFacade twilioService)
        {
            return false;
        }
    }
}