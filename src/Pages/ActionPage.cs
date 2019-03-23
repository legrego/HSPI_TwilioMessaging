using HomeSeerAPI;
using NullGuard;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using Hspi.Utils;

namespace Hspi.Pages
{
    internal class ActionPage : PageHelper, IEventPage
    {
        public ActionPage(IHSApplication HS, PluginConfig pluginConfig) : base(HS, pluginConfig, "Events")
        {
        }

        public string Name()
        {
            return "Twilio: send a message";
        }

        public string BuildEditUI([AllowNull] string uniqueControlId,
                                   IPlugInAPI.strTrigActInfo actionInfo)
        {
            SendMessageActionConfig config = SendMessageActionConfig.DeserializeActionConfig(actionInfo.DataIn);

            string toNumber = config.ToNumber;
            if (toNumber == null)
            {
                toNumber = "";
            }

            string message = config.Message;
            if (message == null)
            {
                message = "";
            }

            var toField = FormTextBox("ToNumber" + uniqueControlId, "To", toNumber);

            var messageField = FormTextBox("Message" + uniqueControlId, "Message", message);

            return toField + "<br>" + messageField;
        }

        public string BuildViewUI(IPlugInAPI.strTrigActInfo actionInfo)
        {
            var config = SendMessageActionConfig.DeserializeActionConfig(actionInfo.DataIn);
            return $"Twilio sends a message to {config.ToNumber}";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IPlugInAPI.strMultiReturn ProcessPostUI([AllowNull] NameValueCollection postData,
                                                                        IPlugInAPI.strTrigActInfo actionInfo)
        {
            var value = new IPlugInAPI.strMultiReturn
            {
                TrigActInfo = actionInfo
            };

            var config = new SendMessageActionConfig();
            if (postData != null && postData.HasKeys())
            {
                config.ToNumber = postData[0];
                config.Message = postData[1];
                value.DataOut = SendMessageActionConfig.SerializeActionConfig(config);
            }
            return value;
        }

        public bool IsConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            SendMessageActionConfig config = SendMessageActionConfig.DeserializeActionConfig(actionInfo.DataIn);
            return config.IsValid();
        }

        public bool HandleEvent(IPlugInAPI.strTrigActInfo actionInfo, TwilioServiceFacade twilioService)
        {
            SendMessageActionConfig config = SendMessageActionConfig.DeserializeActionConfig(actionInfo.DataIn);
            if (config.IsValid())
            {
                twilioService.SendMessageToTwilio(pluginConfig, config);
                return true;
            }
            return false;
        }
    }
}