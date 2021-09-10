using FMDC.DataLoader.Implementations;
using FMDC.Model;
using FMDC.Model.Models;
using FMDC.Utility;
using FMDC.Utility.PubSubUtility;
using FMDC.Utility.PubSubUtility.PubSubEventTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FMDC.DataLoader
{
	class Program
	{
		#region Fields
		private static bool _useVerboseOutput = false;
		private static bool _waitOnExit = false;
		private static int _exitCode = 0;
		private static FileLogger _fileLogger;

		private static IEnumerable<Card> _cardList;
		private static IEnumerable<SecondaryType> _secondaryTypes;
		private static IEnumerable<GameImage> _cardImages;
		private static IEnumerable<Fusion> _fusions;
		private static IEnumerable<Character> _characterList;
		private static IEnumerable<GameImage> _characterImages;
		private static IEnumerable<CharacterCardPercentage> _dropRates;
		private static IEnumerable<Equippable> _equipment;
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
						ApplicationConstants.APPLICATION_DATA_FOLDER
					);

				PrepareConsole(args);

				LoadDataIntoMemory();

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

			if (argsList.Contains("/v") || argsList.Contains("-v")
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
		
		
		private static void LoadDataIntoMemory()
		{
			#region Card Loader
			CardDataLoader cardLoader = new CardDataLoader();
			_cardList = cardLoader.LoadDataIntoMemory();
			#endregion

			#region Secondary Type Loader
			SecondaryTypeDataLoader secondaryTypeLoader = new SecondaryTypeDataLoader(_cardList);
			_secondaryTypes = secondaryTypeLoader.LoadDataIntoMemory();
			#endregion
			
			#region Card Image Loader
			CardImageDataLoader cardImageLoader = new CardImageDataLoader();
			_cardImages = cardImageLoader.LoadDataIntoMemory();
			#endregion
			
			#region Fusion Loader
			FusionDataLoader fusionLoader = new FusionDataLoader(_cardList);
			_fusions = fusionLoader.LoadDataIntoMemory();
			fusionLoader.LogAnomalies(_fileLogger);
			#endregion

			#region Character Loader
			CharacterDataLoader characterLoader = new CharacterDataLoader();
			_characterList = characterLoader.LoadDataIntoMemory();
			#endregion

			#region Character Image Loader
			CharacterImageDataLoader characterImageLoader = new CharacterImageDataLoader();
			_characterImages = characterImageLoader.LoadDataIntoMemory();
			#endregion

			#region Drop Rate Loader
			CardPercentageDataLoader dropRateLoader = new CardPercentageDataLoader(_cardList, _characterList);
			_dropRates = dropRateLoader.LoadDataIntoMemory();
			dropRateLoader.LogAnomalies(_fileLogger);
			#endregion

			#region Equipment Loader
			EquipmentDataLoader equipmentLoader = new EquipmentDataLoader(_cardList);
			_equipment = equipmentLoader.LoadDataIntoMemory();
			equipmentLoader.LogAnomalies(_fileLogger);
			#endregion
		}
		#endregion



		#region PubSub Event Handlers
		private static void HandleLogMessageEvent(LogMessageEvent e)
		{
			ConsoleColor messageColor = ConsoleColor.White;
			string prefix = $"[{e.Level.ToString()}] ";

			switch (e.Level)
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
