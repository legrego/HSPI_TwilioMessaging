using System.IO;

namespace Hspi
{
    using Hspi.Utils;
    using Hspi.Exceptions;
    using System.Text;

    /// <summary>
    /// Class to store action configuration
    /// </summary>
   
    public class SendMessageActionConfig
    {
        public string ToNumber { get; set; }

        public string Message { get; set; }

        public bool IsValid()
        {
            bool toValid = !this.ToNumber.IsNullOrWhiteSpace();
            bool messageValid = !this.Message.IsNullOrWhiteSpace();
            return toValid && messageValid;
        }

        public static byte[] SerializeActionConfig(SendMessageActionConfig cfg)
        {
			if(cfg == null)
			{
				throw new HspiException("configuration parameter is required");
			}

            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(cfg);

            return Encoding.Unicode.GetBytes(jsonString);
        }

        public static SendMessageActionConfig DeserializeActionConfig(byte[] configuration)
        {
            var configInstance = new SendMessageActionConfig();
            if (configuration == null || configuration.Length == 0)
            {
                configInstance.ToNumber = "";
                configInstance.Message = "";
            } else
            {
                try
                {
                    string jsonString = Encoding.Unicode.GetString(configuration);
                    configInstance = Newtonsoft.Json.JsonConvert.DeserializeObject<SendMessageActionConfig>(jsonString);
                }
                catch
                {
                    configInstance = DeserializeLegacyActionConfig(configuration);
                }

            }

            return configInstance;
        }

        private static SendMessageActionConfig DeserializeLegacyActionConfig(byte[] configuration)
        {
            var configInstance = new SendMessageActionConfig();

            try
            {
                using (var ms = new MemoryStream(configuration))
                {
                    using (var br = new BinaryReader(ms, Encoding.UTF8))
                    {
                        configInstance.ToNumber = br.ReadString();
                        configInstance.Message = br.ReadString();
                    }
                }
            }
            catch
            {
                configInstance.ToNumber = "";
                configInstance.Message = "Error reading config";
            }

            return configInstance;
        }

    };
}