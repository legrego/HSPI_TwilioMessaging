using HomeSeerAPI;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hspi
{
	class TwilioServiceFacade
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

			if(message == null || message.Length == 0)
			{
				this.Log.LogWarning("No message configured! Message won't send");
				return;
			}
			if(messageConfig.ToNumber == null || messageConfig.ToNumber.Length == 0)
			{
				this.Log.LogWarning("No 'To' number configured! Message won't send");
				return;
			}
			else
			{
				to = new PhoneNumber(HS.ReplaceVariables(messageConfig.ToNumber));
			}

			message = HS.ReplaceVariables(message);


			TwilioClient.Init(pluginConfig.AccountSID, pluginConfig.AuthToken);

			var publishedMessage = MessageResource.Create(
				to: to,
				from: from,
				body: message
			);

			this.Log.LogInfo("Published message with Sid: " + publishedMessage.Sid);
		}
	}
}
