using System.IO;

namespace Hspi
{
    using Hspi.Exceptions;
    using System.Text;

    /// <summary>
    /// Class to store action configuration
    /// </summary>
   
    public class SendMessageActionConfig
    {
        public string ToNumber { get; set; }

        public string Message { get; set; }

		public static byte[] SerializeActionConfig(SendMessageActionConfig cfg)
        {
			if(cfg == null)
			{
				throw new HspiException("configuration parameter is required");
			}

            string toNumber = cfg.ToNumber;
            string message = cfg.Message;

            byte[] buffer;

            MemoryStream ms = null;
            BinaryWriter bw = null;
            try
            {
                ms = new MemoryStream();
                bw = new BinaryWriter(ms, Encoding.UTF8);
                bw.Write(toNumber);
                bw.Write(message);

                buffer = ms.ToArray();
            }
            finally
            {
				if(bw != null)
				{
					bw.Dispose();
				}
				if (ms != null)
                {
                    ms.Dispose();
                }
            }
            
            return buffer;
        }

        public static SendMessageActionConfig DeserializeActionConfig(byte[] configuration)
        {
            var srx2 = new SendMessageActionConfig();
            if (configuration == null || configuration.Length == 0)
            {
                srx2.ToNumber = "";
                srx2.Message = "";
            } else
            {
                try
                {
                    using (var ms = new MemoryStream(configuration))
                    {
                        using (var br = new BinaryReader(ms, Encoding.UTF8))
                        {
                            srx2.ToNumber = br.ReadString();
                            srx2.Message = br.ReadString();
                        }
                    }
                }
                catch
                {
                    srx2.ToNumber = "";
                    srx2.Message = "Error reading config";
                }
            }

            return srx2;
        }
    };
}