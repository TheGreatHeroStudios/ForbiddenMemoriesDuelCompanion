using FMDC.DataLoader.Implementations;
using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TGH.Common.Patterns.IoC;
using TGH.Common.Persistence.Interfaces;
using TGH.Common.Repository.Implementations;
using TGH.Common.Repository.Interfaces;
using TGH.Common.Utilities.Logging;
using TGH.Common.Utilities.Logging.Providers;

namespace FMDC.DataLoader
{
	class Program
	{
		#region Fields
		private static bool _useVerboseOutput = false;
		private static bool _waitOnExit = false;
		private static bool _rebuildDatabase = false;

		private static int _exitCode = 0;

		private static CardDataLoader _cardLoader;
		private static SecondaryTypeDataLoader _secondaryTypeLoader;
		private static CardImageDataLoader _cardImageLoader;
		private static FusionDataLoader _fusionLoader;
		private static CharacterDataLoader _characterLoader;
		private static CharacterImageDataLoader _characterImageLoader;
		private static CardPercentageDataLoader _cardPercentageLoader;
		private static EquipmentDataLoader _equipmentLoader;

		private static IEnumerable<Card> _cardList;
		private static IEnumerable<SecondaryType> _secondaryTypes;
		private static IEnumerable<GameImage> _cardImages;
		private static IEnumerable<Fusion> _fusions;
		private static IEnumerable<Character> _characterList;
		private static IEnumerable<GameImage> _characterImages;
		private static IEnumerable<CardPercentage> _cardPercentages;
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
				LoadDatabase();
			}
			catch (Exception ex)
			{
				//Treat any exception that bubbled up to here as unhandled and fatal
				Logger.LogError(ex.Message);
				Logger.LogError(MessageConstants.DATA_LOADING_FAILED);
				_exitCode = -1;
			}
			finally
			{
				if (_waitOnExit)
				{
					Logger.LogInfo(MessageConstants.EXIT_PROMPT);
					Console.Read();
				}

				Logger.UnregisterLoggingProviders();
				Environment.Exit(_exitCode);
			}
		}
		#endregion



		#region Private Methods
		private static void ConfigureMiddleware()
		{
			//Register logging provider(s)
			Logger.RegisterLoggingProviders
			(
				new LoggingProvider[]
				{
					new FileLoggingProvider
					(
						FileConstants.DATA_LOADER_LOG_FILE_NAME,
						ApplicationConstants.APPLICATION_DATA_FOLDER
					),
					new ConsoleLoggingProvider()
				}
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

			if (argsList.Contains("/r") || argsList.Contains("-r")
				|| argsList.Contains("/rebuild") || argsList.Contains("-rebuild")
				|| argsList.Contains("/rebuilddatabase") || argsList.Contains("-rebuilddatabase"))
			{
				_rebuildDatabase = true;
			}

			Console.BufferHeight = ConsoleConstants.BUFFER_HEIGHT;
			Console.WindowWidth = ConsoleConstants.CONSOLE_WIDTH;
			Console.Title = ConsoleConstants.DATA_LOADER_APP_NAME;
		}


		private static void LoadDataIntoMemory()
		{
			if
			(
				_rebuildDatabase &&
				File.Exists(PersistenceConstants.SQLITE_DB_TARGET_FILEPATH) &&
				File.Exists(PersistenceConstants.SQLITE_TEMPLATE_FILEPATH)
			)
			{
				//If the database exists but should be rebuilt, delete it from the 
				//target filepath and re-copy it from the template database filepath.
				Logger.LogInfo(MessageConstants.BEGIN_REBUILD_DATABASE);

				File.Delete(PersistenceConstants.SQLITE_DB_TARGET_FILEPATH);

				File.Copy
				(
					PersistenceConstants.SQLITE_TEMPLATE_FILEPATH,
					PersistenceConstants.SQLITE_DB_TARGET_FILEPATH
				);
			}

			Logger.LogInfo(MessageConstants.BEGIN_DATA_LOAD_PROCESS);

			#region Character Image Loader
			_characterImageLoader = new CharacterImageDataLoader();
			_characterImages = _characterImageLoader.ReadDataIntoMemory();
			#endregion

			#region Card Image Loader
			_cardImageLoader = new CardImageDataLoader();
			_cardImages = _cardImageLoader.ReadDataIntoMemory();
			#endregion

			#region Card Loader
			_cardLoader = new CardDataLoader();
			_cardList = _cardLoader.ReadDataIntoMemory();
			#endregion

			#region Secondary Type Loader
			_secondaryTypeLoader = new SecondaryTypeDataLoader(_cardList);
			_secondaryTypes = _secondaryTypeLoader.ReadDataIntoMemory();
			#endregion

			#region Fusion Loader
			_fusionLoader = new FusionDataLoader(_cardList);
			_fusions = _fusionLoader.ReadDataIntoMemory();
			_fusionLoader.LogAnomalies();
			#endregion

			#region Equipment Loader
			_equipmentLoader = new EquipmentDataLoader(_cardList);
			_equipment = _equipmentLoader.ReadDataIntoMemory();
			_equipmentLoader.LogAnomalies();
			#endregion

			#region Character Loader
			_characterLoader = new CharacterDataLoader();
			_characterList = _characterLoader.ReadDataIntoMemory();
			#endregion

			#region Card Percentage Loader
			_cardPercentageLoader = new CardPercentageDataLoader(_cardList, _characterList);
			_cardPercentages = _cardPercentageLoader.ReadDataIntoMemory();
			_cardPercentageLoader.LogAnomalies();
			#endregion

			Logger.LogInfo(MessageConstants.DATA_LOADING_SUCCESSFUL);
		}


		private static void LoadDatabase()
		{
			Logger.LogInfo(MessageConstants.BEGIN_DATABASE_LOADING);

			_cardImages = _cardImageLoader.LoadDataIntoDatabase(_cardImages);
			_characterImages = _characterImageLoader.LoadDataIntoDatabase(_characterImages);

			//Associate loaded card images with their corresponding card
			_cardList =
				_cardList
					.Join
					(
						_cardImages
							.Where
							(
								cardImage =>
									cardImage.EntityType == ImageEntityType.Card
							),
						card => card.CardId,
						cardImage => cardImage.EntityId,
						(card, cardImage) => (card, cardImageId: cardImage.GameImageId)
					)
					.Join
					(
						_cardImages
							.Where
							(
								cardDetailImage =>
									cardDetailImage.EntityType == ImageEntityType.CardDetails
							),
						cardTuple => cardTuple.card.CardId,
						cardDetailImage => cardDetailImage.EntityId,
						(cardTuple, cardDetailImage) =>
							(
								cardTuple.card,
								cardTuple.cardImageId,
								cardDetailImageId: cardDetailImage.GameImageId
							)
					)
					.ToList()
					.Select
					(
						cardImageTuple =>
						{
							cardImageTuple.card.CardImageId =
								cardImageTuple.cardImageId;

							cardImageTuple.card.CardDescriptionImageId =
								cardImageTuple.cardDetailImageId;

							return cardImageTuple.card;
						}
					);

			_cardList = _cardLoader.LoadDataIntoDatabase(_cardList);

			_secondaryTypes = _secondaryTypeLoader.LoadDataIntoDatabase(_secondaryTypes);

			_fusions = _fusionLoader.LoadDataIntoDatabase(_fusions);

			_equipment = _equipmentLoader.LoadDataIntoDatabase(_equipment);

			//Associate loaded character images with their corresponding 
			//character and extract deck inclusion percentage rates.
			List<CardPercentage> deckInclusionPercentages = new List<CardPercentage>();

			_characterList =
				_characterList
					.Join
					(
						_characterImages,
						character => character.CharacterId,
						characterImage => characterImage.EntityId,
						(character, characterImage) =>
						{
							character.CharacterImageId = characterImage.GameImageId;

							if (character.CardPercentages != null)
							{
								deckInclusionPercentages.AddRange(character.CardPercentages);
							}

							return character;
						}
					);

			_characterList = _characterLoader.LoadDataIntoDatabase(_characterList);

			_cardPercentages =
				_cardPercentageLoader
					.LoadDataIntoDatabase
					(
						_cardPercentages
							.Concat(deckInclusionPercentages)
					);

			Logger.LogInfo(MessageConstants.DATABASE_LOADING_SUCCESSFUL);
		}
		#endregion
	}
}
