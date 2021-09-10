using FMDC.DataLoader.Implementations;
using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Persistence;
using FMDC.Utility;
using FMDC.Utility.PubSubUtility;
using FMDC.Utility.PubSubUtility.PubSubEventTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using TGH.Common.Patterns.IoC;
using TGH.Common.Persistence.Interfaces;
using TGH.Common.Repository.Implementations;
using TGH.Common.Repository.Interfaces;

namespace FMDC.DataLoader
{
	class Program
	{
		#region Fields
		private static bool _useVerboseOutput = false;
		private static bool _waitOnExit = false;
		private static int _exitCode = 0;
		private static FileLogger _fileLogger;

		private static IGenericRepository _cardRepository;

		private static CardDataLoader _cardLoader;
		private static SecondaryTypeDataLoader _secondaryTypeLoader;
		private static CardImageDataLoader _cardImageLoader;
		private static FusionDataLoader _fusionLoader;
		private static CharacterDataLoader _characterLoader;
		private static CharacterImageDataLoader _characterImageLoader;
		private static CardPercentageDataLoader _dropRateLoader;
		private static EquipmentDataLoader _equipmentLoader;

		private static IEnumerable<Card> _cardList;
		private static IEnumerable<SecondaryType> _secondaryTypes;
		private static IEnumerable<GameImage> _cardImages;
		private static IEnumerable<Fusion> _fusions;
		private static IEnumerable<Character> _characterList;
		private static IEnumerable<GameImage> _characterImages;
		private static IEnumerable<CardPercentage> _dropRates;
		private static IEnumerable<Equippable> _equipment;
		#endregion



		#region Entry Point
		static void Main(string[] args)
		{
			try
			{
				ConfigureMiddleware();
				PrepareConsole(args);

				LoadDataIntoMemory();
				PrepareRelationalDataForStorage();

				PersistData();
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
		private static void ConfigureMiddleware()
		{
			PubSubManager.Subscribe<LogMessageEvent>(HandleLogMessageEvent);
			_fileLogger =
				new FileLogger
				(
					FileConstants.DATA_LOADER_LOG_FILE_NAME,
					ApplicationConstants.APPLICATION_DATA_FOLDER
				);

			//Register singleton middleware instances for the necessary data layers
			DependencyManager
				.RegisterService<IDatabaseContext, ForbiddenMemoriesDbContext>
				(
					() => 
						new ForbiddenMemoriesDbContext
						(
							PersistenceConstants.SQLITE_DB_TARGET_FILEPATH
						),
					ServiceScope.Singleton
				);

			DependencyManager
				.RegisterService<IGenericRepository, GenericRepository>
				(
					ServiceScope.Singleton
				);

			//Immediately resolve an instance of the repository to serve as the card repository
			_cardRepository = 
				DependencyManager.ResolveService<IGenericRepository>(ServiceScope.Singleton);
		}


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
			_cardLoader = new CardDataLoader();
			_cardList = _cardLoader.LoadDataIntoMemory();
			#endregion

			#region Secondary Type Loader
			_secondaryTypeLoader = new SecondaryTypeDataLoader(_cardList);
			_secondaryTypes = _secondaryTypeLoader.LoadDataIntoMemory();
			#endregion
			
			#region Card Image Loader
			_cardImageLoader = new CardImageDataLoader();
			_cardImages = _cardImageLoader.LoadDataIntoMemory();
			#endregion
			
			#region Fusion Loader
			_fusionLoader = new FusionDataLoader(_cardList);
			_fusions = _fusionLoader.LoadDataIntoMemory();
			_fusionLoader.LogAnomalies(_fileLogger);
			#endregion

			#region Character Loader
			_characterLoader = new CharacterDataLoader();
			_characterList = _characterLoader.LoadDataIntoMemory();
			#endregion

			#region Character Image Loader
			_characterImageLoader = new CharacterImageDataLoader();
			_characterImages = _characterImageLoader.LoadDataIntoMemory();
			#endregion

			#region Drop Rate Loader
			_dropRateLoader = new CardPercentageDataLoader(_cardList, _characterList);
			_dropRates = _dropRateLoader.LoadDataIntoMemory();
			_dropRateLoader.LogAnomalies(_fileLogger);
			#endregion

			#region Equipment Loader
			_equipmentLoader = new EquipmentDataLoader(_cardList);
			_equipment = _equipmentLoader.LoadDataIntoMemory();
			_equipmentLoader.LogAnomalies(_fileLogger);
			#endregion

			LoggingUtility.LogInfo(MessageConstants.DATA_LOADING_SUCCESSFUL);
		}
		
		
		private static void PrepareRelationalDataForStorage()
		{
			
		}


		private static void PersistData()
		{
			LoggingUtility.LogInfo(MessageConstants.BEGIN_DATABASE_LOADING);

			_cardImageLoader.LoadDataIntoDatabase(_cardImages);
			_characterImageLoader.LoadDataIntoDatabase(_characterImages);

			LoggingUtility.LogInfo(MessageConstants.DATABASE_LOADING_SUCCESSFUL);
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
