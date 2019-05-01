using System;
using Hspi.Utils;

namespace Hspi
{
    using Hspi.Exceptions;
    using System.Collections.Specialized;
    using System.Text;

    /// <summary>
    /// Class to store trigger configuration
    /// </summary>

    public class ReceiveMessageTriggerConfig
    {
        public bool FromAnyNumber { get; set; }

        public string FromNumber { get; set; }

        public string FromDisplay { 
            get { 
                if (FromNumber.IsNullOrWhiteSpace())
                {
                    return "any number";
                }
                return FromNumber;
            }
        }

        public string Message { get; set; }

        public bool IsValid()
        {
            return !Message.IsNullOrWhiteSpace();
        }

        public ReceiveMessageTriggerConfig() { }

        public ReceiveMessageTriggerConfig(NameValueCollection postData)
        {
            if (postData != null && postData.HasKeys())
            {
                foreach(string key in postData.Keys)
                {
                    if (key == null) continue;
                    if (key.StartsWith("From_", StringComparison.InvariantCulture))
                    {
                        FromNumber = postData[key];
                    }
                    if (key.StartsWith("Message_", StringComparison.InvariantCulture))
                    {
                        Message = postData[key];
                    }
                }
            }
        }

        public static byte[] SerializeTriggerConfig(ReceiveMessageTriggerConfig cfg)
        {
			if(cfg == null)
			{
				throw new HspiException("configuration parameter is required");
			}

            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(cfg);

            return Encoding.Unicode.GetBytes(jsonString);
        }

        public static ReceiveMessageTriggerConfig DeserializeTriggerConfig(byte[] configuration)
        {
            var configInstance = new ReceiveMessageTriggerConfig();
            if (configuration == null || configuration.Length == 0)
            {
                configInstance.FromNumber = "";
                configInstance.Message = "";
            } else
            {
                string jsonString = Encoding.Unicode.GetString(configuration);
                configInstance = Newtonsoft.Json.JsonConvert.DeserializeObject<ReceiveMessageTriggerConfig>(jsonString);
            }

            return configInstance;
        }
    };
}