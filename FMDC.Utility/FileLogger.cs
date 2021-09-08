using FMDC.Model;
using FMDC.Utility.PubSubUtility;
using FMDC.Utility.PubSubUtility.PubSubEventTypes;
using System;
using System.IO;

namespace FMDC.Utility
{
	public class FileLogger : IDisposable
	{
		#region Fields
		private string _filePath = FileConstants.DEFAULT_LOG_FILEPATH;
		private StreamWriter _fileStream = null;
		private LogLevel _minimumLogLevel = LogLevel.INFO;
		#endregion



		#region Interface Implementation
		public void Dispose()
		{
			//Write one blank line to delimit the text, 
			//then close the file stream and unsubscribe from LogMessageEvent
			_fileStream.WriteLine("");
			_fileStream.Dispose();
			PubSubManager.Unsubscribe<LogMessageEvent>();
		}
		#endregion



		#region Constructor
		public FileLogger(string logFileName, string logFilePath = null, LogLevel minimumLogLevel = LogLevel.INFO)
		{
			_minimumLogLevel = minimumLogLevel;

			//If the provided path is valid, set it as the location to write log data.
			//Otherwise, write the log file data to the current executable's directory
			if (logFilePath != null && Directory.Exists(logFilePath))
			{
				_filePath = logFilePath;
			}

			//Open the file stream for writing
			_fileStream = File.AppendText($"{_filePath}\\{logFileName}");

			//Write a few lines of audit data to the log file
			_fileStream.WriteLine(FileConstants.LOG_FILE_HEADER_DIVIDER);
			_fileStream.WriteLine
			(
				string.Format
				(
					FileConstants.LOG_FILE_SESSION_STAMP_TEMPLATE,
					DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
				)
			);
			_fileStream.WriteLine(FileConstants.LOG_FILE_HEADER_DIVIDER);

			//Subscribe to LogMessageEvents and specify the method used to handle logged messages
			PubSubManager.Subscribe<LogMessageEvent>(HandleLogMessageEvent);
		}
		#endregion



		#region Public Methods
		public void WriteLine(string text)
		{
			_fileStream.WriteLine(text);
		}
		#endregion



		#region PubSub Event Handlers
		private void HandleLogMessageEvent(LogMessageEvent e)
		{
			if (e.Level >= _minimumLogLevel)
			{
				_fileStream.WriteLine($"[{e.Level.ToString()}] {e.Message}");
			}
		}
		#endregion
	}
}
