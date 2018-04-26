using HomeSeerAPI;
using System;
using System.Globalization;

namespace Hspi
{
	class Logger
	{
		private string Name { get; set; }
		public IHSApplication HS { get; set; }
		public bool EnableDebug { get; set; }

		public Logger(string name, IHSApplication HS = null, bool enableDebug = true)
		{
			this.Name = name;
			this.HS = HS;
			this.EnableDebug = enableDebug;
		}

		public void LogDebug(string message)
		{
			if(this.EnableDebug)
			{
				HS.WriteLog(this.Name, String.Format(CultureInfo.InvariantCulture, "Debug:{0}", message));
			}
		}

		public void LogError(string message)
		{
			HS.WriteLogEx(this.Name, String.Format(CultureInfo.InvariantCulture, "Error:{0}", message), "#FF0000");
		}

		public void LogInfo(string message)
		{
			HS.WriteLog(this.Name, message);
		}

		public void LogWarning(string message)
		{
			HS.WriteLogEx(this.Name, String.Format(CultureInfo.InvariantCulture, "Warning:{0}", message), "#D58000");
		}
	}
}
