using System;
using System.Collections.Generic;
using System.Text;

namespace FMDC.Utility.PubSubUtility.PubSubEventTypes
{
	public enum LogLevel
	{
		DEBUG,
		VERBOSE,
		INFO,
		WARN,
		ERROR
	}


	public class LogMessageEvent : PubSubEventBase
	{
		public string Message { get; set; }
		public LogLevel Level { get; set; }
	}
}
