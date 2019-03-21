using HomeSeerAPI;
using NullGuard;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using Hspi.Utils;

namespace Hspi.Pages
{
    internal class TriggerPage : PageHelper
    {
        public TriggerPage(IHSApplication HS, PluginConfig pluginConfig) : base(HS, pluginConfig, "Events")
        {
        }

        public string ReceiveMessageTriggerBuildUI([AllowNull] string uniqueControlId,
                                               ReceiveMessageTriggerConfig config)
        {
            StringBuilder stb = new StringBuilder();

            IncludeResourceScript(stb, "ReceiveMessageTriggerScript", uniqueControlId);

            string anyNumberCheckbox = FormCheckBox("AnyNumber" + uniqueControlId, "From any number", config.FromNumber.IsNullOrWhiteSpace(), true);

            string numberInput = FormTextBox("FromNumber" + uniqueControlId, "From number", config.FromNumber);
            string numberLine = string.Format("<div id=\"FromNumberWrap{0}\">{1}</div>", uniqueControlId, numberInput);

            stb.Append(anyNumberCheckbox).Append("<br />");

            stb.Append(numberLine).Append("<br />");

            stb.Append(FormTextBox("Message" + uniqueControlId, "Message contains", config.Message));

            return stb.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IPlugInAPI.strMultiReturn ReceiveMessageTriggerProcessPostUI([AllowNull] NameValueCollection postData,
                                                                        IPlugInAPI.strTrigActInfo actionInfo)
        {
            IPlugInAPI.strMultiReturn result = new IPlugInAPI.strMultiReturn();
            result.DataOut = actionInfo.DataIn;
            result.TrigActInfo = actionInfo;
            result.sResult = string.Empty;
            if (postData != null && postData.Count > 0)
            { 

            ReceiveMessageTriggerConfig config = new ReceiveMessageTriggerConfig()
                {
                    Message = postData[0]
                };

                result.DataOut = ReceiveMessageTriggerConfig.SerializeTriggerConfig(config);
            }

            return result;
        }
    }
}