
namespace Hspi
{
    /// <summary>
    /// Class for the main program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The homeseer server address.  Defaults to the local computer but can be changed through the command line argument, server=address.
        /// </summary>
        private static string serverAddress = "192.168.10.20";

        private const int serverPort = 10400;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private static void Main(string[] args)
        {
            // parse command line arguments
            foreach (string sCmd in args)
            {
                string[] parts = sCmd.Split('=');
                switch (parts[0].ToUpperInvariant())
                {
                    case "SERVER":
                        serverAddress = parts[1];
                        break;
                }
            }

            using (var plugin = new HSPI_TwilioMessaging.HSPI())
            {
                plugin.Connect(serverAddress, serverPort);
                plugin.WaitforShutDownOrDisconnect();
            }
        }
    }
}