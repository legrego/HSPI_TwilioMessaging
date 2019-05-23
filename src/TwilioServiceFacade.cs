using HomeSeerAPI;
using Twilio;
using Twilio.Base;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Hspi
{
    internal class TwilioServiceFacade
    {
        private Logger Log { get; set; }
        private IHSApplication HS { get; set; }

        public TwilioServiceFacade(IHSApplication HS, bool enableDebug = true)
        {
            this.HS = HS;
            this.Log = new Logger(TwilioMessagingData.PlugInName, HS, enableDebug);
        }

        public void SendMessageToTwilio(PluginConfig pluginConfig, SendMessageActionConfig messageConfig)
        {
            this.Log.LogDebug("Starting SendMessageToTwilio");

            PhoneNumber to;
            PhoneNumber from = new PhoneNumber(pluginConfig.FromNumber);

            string message = messageConfig.Message;

            if (string.IsNullOrEmpty(message))
            {
                this.Log.LogWarning("No message configured! Message won't send");
                return;
            }
            if (string.IsNullOrEmpty(messageConfig.ToNumber))
            {
                this.Log.LogWarning("No 'To' number configured! Message won't send");
                return;
            }

            to = new PhoneNumber(HS.ReplaceVariables(messageConfig.ToNumber));

            message = HS.ReplaceVariables(message);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            TwilioClient.Init(pluginConfig.AccountSID, pluginConfig.AuthToken);

            var publishedMessage = MessageResource.Create(
                to,
                from: from,
                body: message
            );

            this.Log.LogInfo("Published message with Sid: " + publishedMessage.Sid);
        }

        public List<MessageResource> GetMessagesFromTwilio(PluginConfig pluginConfig, int secondsAgo)
        {
            this.Log.LogDebug("Starting GetMessagesFromTwilio");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            TwilioClient.Init(pluginConfig.AccountSID, pluginConfig.AuthToken);

            ReadMessageOptions options = new ReadMessageOptions
            {
                To = new PhoneNumber(pluginConfig.FromNumber),
                DateSentAfter = DateTime.Now.AddSeconds(secondsAgo * -1)
            };

            ResourceSet<MessageResource> messages = MessageResource.Read(options);
            foreach (MessageResource message in messages)
            {
                string from = message.From.ToString();
                string body = message.Body;
                string created = message.DateCreated.GetValueOrDefault(DateTime.MinValue).ToLongDateString();
                this.Log.LogDebug(string.Format("From: {0}, created: {1}, body: {2}", from, created, body));
            }

            return messages.Where((message) => message.DateCreated.GetValueOrDefault(DateTime.MinValue).CompareTo(options.DateSentAfter) >= 0).ToList();
        }
    }
}
