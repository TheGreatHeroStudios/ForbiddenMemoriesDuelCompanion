using FMDC.Utility.PubSubUtility;
using FMDC.Utility.PubSubUtility.PubSubEventTypes;

namespace FMDC.Utility
{
	public static class LoggingUtility
	{
		public static void LogDebug(string message)
		{
			PublishLogMessage(LogLevel.DEBUG, message);
		}

		public static void LogVerbose(string message)
		{
			PublishLogMessage(LogLevel.VERBOSE, message);
		}

		public static void LogInfo(string message)
		{
			PublishLogMessage(LogLevel.INFO, message);
		}

		public static void LogWarning(string message)
		{
			PublishLogMessage(LogLevel.WARN, message);
		}

		public static void LogError(string message)
		{
			PublishLogMessage(LogLevel.ERROR, message);
		}

		public static void PublishLogMessage(LogLevel level, string message)
		{
			LogMessageEvent messageEvent = new LogMessageEvent
			{
				Level = level,
				Message = message
			};

			PubSubManager.Publish(messageEvent);
		}
	}
}
