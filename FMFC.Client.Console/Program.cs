using FMDC.Data.DataLoader;
using FMDC.Data.DataLoader.Implementations;
using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Utility;
using FMDC.Utility.PubSubUtility;
using FMDC.Utility.PubSubUtility.PubSubEventTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMDC.Client.DataLoader
{
    class Program
    {
		#region Fields
		private static bool _useVerboseOutput = false;
		private static bool _waitOnExit = false;
		private static int _exitCode = 0;
		private static FileLogger _fileLogger;
		#endregion



		#region Entry Point
		static void Main(string[] args)
        {
			try
			{
				PubSubManager.Subscribe<LogMessageEvent>(HandleLogMessageEvent);
				_fileLogger = 
					new FileLogger
					(
						FileConstants.DATA_LOADER_LOG_FILE_NAME,
						Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
					);

				PrepareConsole(args);

				//TODO: Build factory that returns collection of data loaders needed to load database
				#region Card Loader
				CardDataLoader cardLoader = new CardDataLoader();
				IEnumerable<Card> cardList = cardLoader.LoadDataIntoMemory();
				#endregion

				#region Secondary Type Loader
				SecondaryTypeDataLoader secondaryTypeLoader = new SecondaryTypeDataLoader(cardList);
				IEnumerable<SecondaryType> secondaryTypes = secondaryTypeLoader.LoadDataIntoMemory();
				#endregion

				#region Card Image Loader
				CardImageDataLoader cardImageLoader = new CardImageDataLoader();
				IEnumerable<GameImage> cardImages = cardImageLoader.LoadDataIntoMemory();
				#endregion

				#region Fusion Loader
				FusionDataLoader fusionLoader = new FusionDataLoader(cardList);
				IEnumerable<Fusion> fusions = fusionLoader.LoadDataIntoMemory();
				fusionLoader.LogAnomalies(_fileLogger);
				#endregion

				#region Character Loader
				CharacterDataLoader characterLoader = new CharacterDataLoader();
				IEnumerable<Character> characterList = characterLoader.LoadDataIntoMemory();
				#endregion

				#region Character Image Loader
				CharacterImageDataLoader characterImageLoader = new CharacterImageDataLoader();
				IEnumerable<GameImage> characterImages = characterImageLoader.LoadDataIntoMemory();
				#endregion

				#region Drop Rate Loader
				CardDropDataLoader dropRateLoader = new CardDropDataLoader(cardList, characterList);
				IEnumerable<CardPercentage> dropRates = dropRateLoader.LoadDataIntoMemory();
				dropRateLoader.LogAnomalies(_fileLogger);
				#endregion

				#region Equipment Loader
				EquipmentDataLoader equipmentLoader = new EquipmentDataLoader(cardList);
				IEnumerable<Equippable> equipment = equipmentLoader.LoadDataIntoMemory();
				equipmentLoader.LogAnomalies(_fileLogger);
				#endregion

				LoggingUtility.LogInfo(MessageConstants.DATA_LOADING_SUCCESSFUL);
			}
			catch (Exception ex)
			{
				//Treat any exception that bubbled up to here as unhandled and fatal
				LoggingUtility.LogError(ex.Message);
				LoggingUtility.LogError(MessageConstants.DATA_LOADING_FAILED);
				_exitCode = -1;
			}
			finally
			{
				if (_waitOnExit)
				{
					LoggingUtility.LogInfo(MessageConstants.EXIT_PROMPT);
					Console.Read();
				}

				_fileLogger.Dispose();
				PubSubManager.Unsubscribe<LogMessageEvent>();
				Environment.Exit(_exitCode);
			}
		}
		#endregion



		#region Private Methods
		private static void PrepareConsole(string[] args)
		{
			IEnumerable<string> argsList = args.Select(arg => arg.ToLower());

			if(argsList.Contains("/v") || argsList.Contains("-v") 
				|| argsList.Contains("/verbose") || argsList.Contains("-verbose"))
			{
				_useVerboseOutput = true;
			}

			if (argsList.Contains("/w") || argsList.Contains("-w")
				|| argsList.Contains("/wait") || argsList.Contains("-wait")
				|| argsList.Contains("/waitonexit") || argsList.Contains("-waitonexit"))
			{
				_waitOnExit = true;
			}

			Console.BufferHeight = ConsoleConstants.BUFFER_HEIGHT;
			Console.WindowWidth = ConsoleConstants.CONSOLE_WIDTH;
			Console.Title = ConsoleConstants.DATA_LOADER_APP_NAME;
		}
		#endregion



		#region PubSub Event Handlers
		private static void HandleLogMessageEvent(LogMessageEvent e)
		{
			ConsoleColor messageColor = ConsoleColor.White;
			string prefix = $"[{e.Level.ToString()}] ";

			switch(e.Level)
			{
				case LogLevel.INFO:
				case LogLevel.VERBOSE:
				{
					messageColor = ConsoleColor.Cyan;
					break;
				}
				case LogLevel.WARN:
				case LogLevel.DEBUG:
				{
					messageColor = ConsoleColor.Yellow;
					break;
				}
				case LogLevel.ERROR:
				{
					messageColor = ConsoleColor.Red;
					break;
				}
				default:
				{
					break;
				}
			}

			Console.ForegroundColor = messageColor;

			if (_useVerboseOutput || e.Level > LogLevel.VERBOSE)
			{
				Console.WriteLine($"{prefix}{e.Message}");
			}
		}
		#endregion
	}
}
