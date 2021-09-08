using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FMDC.Model
{
	public static class DataLoaderConstants
	{
		/// <summary>
		///		Total number of cards in the game
		/// </summary>
		public const int TOTAL_CARD_COUNT = 722;

		/// <summary>
		///		Total images to be loaded for cards
		/// </summary>
		public const int CARD_IMAGE_COUNT = 1444;

		/// <summary>
		///		Total number of NPCs in the game
		/// </summary>
		public const int TOTAL_CHARACTER_COUNT = 39;

		/// <summary>
		///		Represents the max of a random number generated to determine random drop rates
		/// </summary>
		public const int DROP_RATE_DENOMINATOR = 2048;
	}



	public static class RegexConstants
	{
		/// <summary>
		///		Regex to match non-case-sensitive alphabetical characters (a-z)
		/// </summary>
		public const string ALPHA_REGEX = @"[^a-zA-Z]";

		/// <summary>
		///		Regex to match '(attack/defense)' syntax
		/// </summary>
		public const string PARENTHESES_REGEX = @"\([A-z0-9/]*\)";

		/// <summary>
		///		Regex to match generic types (in square brackets)
		/// </summary>
		public const string GENERAL_TYPE_REGEX = @"\[[A-z]*\]";
	}



	public static class PropertyConstants
	{
		public const string FUSION_ANOMALY_PROPERTY_NAME_TEMPLATE = "{0}FusionAnomalies";
		public const string DROPRATE_ANOMALY_PROPERTY_NAME_TEMPLATE = "{0}DropRateAnomalies";
		public const string BASE64_PNG_PREFIX = "data:image/png;base64, ";
	}



	public static class ConsoleConstants
	{
		public static readonly int BUFFER_HEIGHT = Int16.MaxValue - 1;
		public const int CONSOLE_WIDTH = 100;
		public const string DATA_LOADER_APP_NAME = "Duel Companion Data Loader";
	}



	public static class FileConstants
	{
		//File Paths
		public static readonly string EXECUTABLE_FILEPATH = Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.LastIndexOf('\\'));
		public static readonly string GENERAL_FUSION_FILEPATH = $@"{EXECUTABLE_FILEPATH}\Files\GeneralFusions.txt";
		public static readonly string SPECIFIC_FUSION_FILEPATH = $@"{EXECUTABLE_FILEPATH}\Files\SpecificFusions.txt";
		public static readonly string SECONDARY_TYPE_FILEPATH = $@"{EXECUTABLE_FILEPATH}\Files\SecondaryTypes.txt";
		public static readonly string CHARACTER_LOADING_INFO_FILEPATH = $@"{EXECUTABLE_FILEPATH}\Files\NPCLinks.csv";
		public static readonly string DROP_RATE_FILEPATH_TEMPLATE = $"{EXECUTABLE_FILEPATH}\\Files\\{{0}}Drops.txt";
		public static readonly string EQUIPMENT_FILEPATH = $@"{EXECUTABLE_FILEPATH}\Files\Equips.txt";

		//Log File Constants
		public static readonly string DEFAULT_LOG_FILEPATH = $@"{EXECUTABLE_FILEPATH}";
		public const string DATA_LOADER_LOG_FILE_NAME = "DataLoaderLog.txt";
		public const string LOG_FILE_HEADER_DIVIDER = "--------------------------------------------------------------";
		public const string LOG_FILE_SESSION_STAMP_TEMPLATE = "Logging Session Started ({0})";
		public const string GENERAL_FUSION_ANOMALY_COUNT_TEMPLATE = "General Fusion Anomalies ({0} found): ";
		public const string SPECIFIC_FUSION_ANOMALY_COUNT_TEMPLATE = "Specific Fusion Anomalies ({0} found): ";
		public const string EQUIPMENT_ANOMALY_COUNT_TEMPLATE = "Equipment Anomalies ({0} found): ";
		public const string GENERIC_ANOMALY_COUNT_TEMPLATE = "{0} ({1} found): ";
		public const string ANOMALY_LOG_TEMPLATE = "    (Line {0})   {1}";
	}


	public static class URLConstants
	{
		public const string YUGIOH_FANDOM_URL = "https://yugioh.fandom.com";
		public const string YUGIPEDIA_URL = "https://yugipedia.com";
		public const string YUGIPEDIA_IMAGE_URL = "https://ms.yugipedia.com";
		public const string CARD_LIST_PATH = "/wiki/List_of_Yu-Gi-Oh!_Forbidden_Memories_cards";
		public const string CARD_GALLERY_PATH = "/wiki/Gallery_of_Yu-Gi-Oh!_Forbidden_Memories_cards_(North_American_English)";
	}



	public static class MessageConstants
	{
		//Success Messages
		public const string CARD_RETRIEVAL_SUCCESSFUL = "Card Retrieval Successful!";
		public const string CARD_IMAGE_LOADING_SUCCESSFUL = "Card Images Loaded Successfully!";
		public const string FUSION_LOADING_SUCCESSFUL_TEMPLATE = "{0} Fusions Loaded Successfully!";
		public const string SECONDARY_TYPE_LOADING_SUCCESSFUL = "Secondary Types Loaded Successfully!";
		public const string CHARACTER_LOADING_SUCCESSFUL = "Characters Loaded Successfully!";
		public const string CHARACTER_IMAGE_LOADING_SUCCESSFUL = "Character Images Loaded Successfully!";
		public const string DROPRATE_LOADING_SUCCESSFUL_TEMPLATE = "{0} Drop Rates Loaded Successfully!";
		public const string EQUIPMENT_LOADING_SUCCESSFUL = "Equipment Loaded Successfully!";
		public const string DATA_LOADING_SUCCESSFUL = "Data Loading Completed Successfully!";

		//Failure Messages
		public const string ADD_REQUEST_HEADER_FAILURE = "Failed to add request header.";
		public const string REMOTE_CONTENT_ACCESS_FAILURE = "Failed to get remote content.";
		public const string HTTP_CLIENT_NOT_SPECIFIED = "No HTTP client was specified for the target data loader.";
		public const string DATA_LOADING_FAILED = "Data Loading Failed.  Duel Companion application may not function as expected.";
		public const string CARD_REPO_ACCESS_FAILURE = "Error Accessing Card Repository Site.";
		public const string CARD_COUNT_MISMATCH = "Total number of cards loaded did not match the expected value.";
		public const string CARD_RETRIEVAL_FAILED = "Card Retrieval Failed.";
		public const string CARD_DATA_DOM_ERROR = "Could not load DOM for card data site.";
		public const string CARD_SITE_PARSING_ERROR_TEMPLATE = "Failed to parse card site data:  {0}";
		public const string CARD_OBJECT_CONSTRUCTION_ERROR_TEMPLATE = "Error building in-memory object for card '{0}':";
		public const string UNKNOWN_CARD_OBJECT_CONSTRUCTION_ERROR = "Error building in-memory card object. (Unknown Card):";
		public const string CARD_DESCRIPTION_RETRIEVAL_ERROR_TEMPLATE = "Error parsing card description: {0}";
		public const string CARD_IMAGE_REPO_ACCESS_FAILURE = "Error Accessing Card Image Gallery.";
		public const string CARD_IMAGE_RETRIEVAL_FAILURE = "Failed to Retrieve Card Images From Gallery.";
		public const string CARD_IMAGE_DOM_ERROR = "Could not load DOM for card image site.";
		public const string CARD_GALLERY_PARSING_ERROR_TEMPLATE = "Failed to parse gallery site data: {0}";
		public const string CARD_IMAGE_LOAD_FAILURE_TEMPLATE = "Failed to load card image for card '{0}':";
		public const string UNKNOWN_CARD_IMAGE_LOAD_FAILURE = "Failed to load card image data. (Unknown Card):";
		public const string FUSION_LOADER_CARD_LIST_NULL = "Card List for cross-referencing fusions was not initialized.  Fusions can not be loaded.";
		public const string SECONDARY_TYPE_LOAD_FAILURE = "Error Loading Secondary Type Data.";
		public const string SECONDARY_TYPE_PARSING_ERROR = "Error Parsing Secondary Type Record.";
		public const string HTTP_REQUEST_RETRY_FAILURE = "Could not execute GET request.  Exceeded the maximum number of retries.";
		public const string BASE64_CONVERSION_ERROR_TEMPLATE = "Error converting base64 to bitmap: {0}";
		public const string BITMAP_CONVERSION_ERROR_TEMPLATE = "Error converting bitmap to base64: {0}";
		public const string FUSION_LOADING_ERROR_TEMPLATE = "Error Loading {0} Fusion Data.";
		public const string CHARACTER_LOAD_FAILURE = "Error Loading Character Data into memory.";
		public const string CHARACTER_RETRIEVAL_FAILURE_TEMPLATE = "Failed to load character information for '{0}'";
		public const string CHARACTER_INFO_DOM_ERROR = "Could not load DOM for character info site.";
		public const string CHARACTER_BIO_PARSING_ERROR_TEMPLATE = "Error parsing character bio: {0}";
		public const string CHARACTER_DECKLIST_PARSING_ERROR_TEMPLATE = "Error parsing character deck list: {0}";
		public const string CHARACTER_IMAGE_RETRIEVAL_FAILURE = "Failed to Retrieve Character Images.";
		public const string CHARACTER_IMAGE_LOAD_FAILURE_TEMPLATE = "Failed to load character image for character '{0}':";
		public const string DROP_LOADER_CARD_LIST_NULL = "Card List for cross-referencing card drops was not initialized.  Drop rates can not be loaded.";
		public const string DROP_LOADER_CHARACTER_LIST_NULL = "Character List for correlating drop rates was not initialized.  Drop rates can not be loaded.";
		public const string DROP_PERCENTAGE_LOADING_FAILURE_TEMPLATE = "Failed to load drop rates for {0} wins.";
		public const string TYPE_LOADER_CARD_LIST_NULL = "Card List for cross-referencing secondary type loader was not initialized.  Secondary types can not be loaded.";
		public const string EQUIPMENT_CARD_LIST_NULL = "Card List for cross-referencing equipment was not initialized.  Equipment can not be loaded.";
		public const string EQUIPMENT_LOAD_FAILURE = "Error loading equipment data.";

		//Warning Messages
		public const string CARD_IMAGE_COUNT_MISMATCH = "Total number of card images loaded did not match the expected value.";
		public const string IMAGE_DISPLAY_WARNING = "Some images may not display correctly in the app.";
		public const string FUSION_LOADER_CARD_LIST_INCOMPLETE = "Card List for cross-referencing fusions was incomplete.  Some fusions may not load correctly.";
		public const string FUSION_LOAD_FAILURE_WARNING_TEMPLATE = "Some {0} fusions failed to load.  See log file for more details.";
		public const string CHARACTER_COUNT_MISMATCH = "Total number of characters loaded did not match the expected value.";
		public const string CHARACTER_IMAGE_COUNT_MISMATCH = "Total number of character images loaded did not match the expected value.";
		public const string CHARACTER_DISPLAY_WARNING = "Some characters may not display correctly in the app.";
		public const string DROP_LOADER_CARD_LIST_INCOMPLETE = "Card List for cross-referencing card drops was incomplete.";
		public const string DROP_LOADER_CHARACTER_LIST_INCOMPLETE = "Character List for correlating drop rates was incomplete.";
		public const string DROP_RATE_DISPLAY_WARNING = "Drop Rates may not display correctly in the application.";
		public const string DROPRATE_LOAD_FAILURE_WARNING_TEMPLATE = "Some {0} drop rates failed to load.  See log file for more details.";
		public const string TYPE_LOADER_CARD_LIST_INCOMPLETE = "Card List for cross-referencing secondary types was incomplete.";
		public const string SECONDARY_TYPE_LOAD_FAILURE_WARNING = "Some Secondary Types failed to load.";
		public const string SECONDARY_TYPE_FUNCTION_WARNING = "Secondary Types may not function correctly in the application.";
		public const string EQUIPMENT_CARD_LIST_INCOMPLETE = "Card List for cross-referencing equipment was incomplete.";
		public const string EQUIPMENT_FUNCTION_WARNING = "Equipment may not function correctly in the application.";
		public const string EQUIPMENT_LOAD_FAILURE_WARNING_TEMPLATE = "Some equipment failed to load.  See log file for more details.";

		//Info Messages
		public const string EXIT_PROMPT = "Press any key to exit...";
		public static readonly string BEGIN_CARD_LOADING = $"Loading Cards from Remote Repository ({URLConstants.YUGIOH_FANDOM_URL})...";
		public const string PARSING_CARD_DATA = "Parsing Card Data...";
		public const string CARD_LOADED_TEMPLATE = "Loaded '{0}' (#{1})";
		public static readonly string BEGIN_CARD_IMAGE_LOADING = $"Loading Card Image Gallery ({URLConstants.YUGIPEDIA_URL})...";
		public const string PARSING_IMAGE_DATA = "Parsing Image Data...";
		public const string CARD_IMAGE_LOADED_TEMPLATE = "Loaded Image Data for '{0}' (#{1})";
		public const string LOADING_FUSION_DATA_TEMPLATE = "Loading {0} Fusion Data...";
		public const string FUSION_PARSED_TEMPLATE = "Parsed Specific Fusion: ({0} + {1} = {2})";
		public const string BEGIN_LOADING_SECONDARY_TYPES = "Loading Secondary Type List...";
		public const string LOADING_CHARACTER_LIST = "Loading Character List...";
		public const string CHARACTER_LOADED_TEMPLATE = "Loaded Character '{0}'";
		public const string LOADING_CHARACTER_IMAGES = "Loading Character Images...";
		public const string CHARACTER_IMAGE_LOADED_TEMPLATE = "Loaded Image Data for '{0}'";
		public const string LOADING_DROPRATE_DATA_TEMPLATE = "Loading Drop Rates for {0} Wins...";
		public const string DROPRATE_PARSED_TEMPLATE = "Parsed {0} Drop Rate for '{1}': '{2}' ({3}%)";
		public const string SECONDARY_TYPES_LOADED_TEMPLATE = "Loaded Secondary Types for Monster #{0} ({1})";
		public const string BEGIN_EQUIPMENT_LOADING = "Loading Equipment Data...";
		public const string EQUIPMENT_PARSED_TEMPLATE = "Equip card '{0}' parsed for target #{1} ({2})";
	}



	public static class AnomalyConstants
	{
		public const string INCORRECT_NUM_OPERANDS = "Incorrect number of operands for fusion row.";
		public const string INCORRECT_NUM_COMPONENTS = "Incorrect number of fusion components.";
		public const string INVALID_TYPE_TEMPLATE = "Invalid type operand '{0}'";
		public const string INVALID_OPERATION = "Invalid Left-Hand Operand";
		public const string TARGET_CARD_PARSING_ERROR = "Target card ID could not be parsed or does not exist.";
		public const string TARGET_NO_DELIMITER = "Target was not preceeded by a delimiter.";
		public const string DIVIDER_NO_TARGET = "Divider was not preceeded by a target card.";
		public const string DELIMITER_NO_FUSION = "Delimiter was not preceeded by a valid fusion or blank target.";
		public const string NO_VALID_TARGET_FOR_FUSION = "No valid target card was set for the fusion record";
		public const string UNKNOWN_ROW_TYPE = "Could not determine row type.";
		public const string CARD_NAME_NOT_FOUND_TEMPLATE = "Card name '{0}' was not found in the card list.";
		public const string CARD_ID_NOT_FOUND_TEMPLATE = "Card with Id #{0} was not found in the card list.";
		public const string DROPPED_CARD_NAME_DISCREPANCY_TEMPLATE = "The provided card name '{0}' does not match the identified card: (#{1}) '{2}'";
		public const string CHARACTER_NOT_FOUND_TEMPLATE = "The character '{0}' specified in the data file does not match a valid character.";
		public const string DIVIDER_NO_CHARACTER = "Divider was not preceeded by a valid character.";
		public const string CHARACTER_NO_DELIMITER = "Character was not preceeded by a delimiter.";
		public const string DELIMITER_NO_DROP = "Delimiter was not preceeded by a valid drop target.";
		public const string DELIMITER_NO_VALID_PRECEEDING = "Delimiter was not preceeded by a valid equip target or primary delimiter.";
		public const string DELIMITER_NO_EQUIP = "Delimiter was not preceeded by a valid equip target.";
		public const string SECOND_DELIMITER_NO_FIRST = "Secondary Delimiter was not preceeded by a valid primary delimiter";
		public const string PRIMARY_DELIMITER_INVALID = "Primary Delimiter was not valid for preceeding equip.";
		public const string NO_VALID_CHARACTER = "No valid character was set for the drop target.";
		public const string CARD_ID_PARSING_ERROR = "Could not parse a card Id from the start of the target row.";
		public const string DROP_RATE_PARSING_ERROR = "Could not parse a drop rate from the end of the target row.";
		public const string EQUIP_INVALID_PRECEEDING = "Equipment was not preceeded by another equip or divider row.";
		public const string EQUIP_PARSING_ERROR = "Could not parse an equip card Id from the start of the equip row.";
		public const string NO_VALID_TARGET_FOR_EQUIP = "No valid target card was set for the equip record";
	}
}
