using FMDC.DataLoader.Exceptions;
using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FMDC.DataLoader.Implementations
{
	public class SecondaryTypeDataLoader : DataLoader<SecondaryType>
	{
		#region Fields
		private IEnumerable<Card> _cardList = null;
		#endregion



		#region Constructor
		public SecondaryTypeDataLoader(IEnumerable<Card> cardList)
		{
			if (cardList == null)
			{
				throw new ArgumentException(MessageConstants.TYPE_LOADER_CARD_LIST_NULL);
			}
			else if (cardList.Count() != DataLoaderConstants.TOTAL_CARD_COUNT)
			{
				LoggingUtility.LogWarning(MessageConstants.TYPE_LOADER_CARD_LIST_INCOMPLETE);
				LoggingUtility.LogWarning(MessageConstants.SECONDARY_TYPE_FUNCTION_WARNING);
			}
			_cardList = cardList;
		}
		#endregion



		#region Abstract Base Class Implementations
		public override IEnumerable<SecondaryType> LoadDataIntoMemory()
		{
			LoggingUtility.LogInfo(MessageConstants.BEGIN_LOADING_SECONDARY_TYPES);

			try
			{
				//Read in the table of monsters and their secondary data types
				IEnumerable<string> secondaryTypeData = LoadDataFile(FileConstants.SECONDARY_TYPE_FILEPATH);

				var secondaryTypes = secondaryTypeData
					.Skip(2)    //Skip the header rows 
					.SelectMany
					(
						//Each row of the data file may contain multiple secondary types for the target monster
						//As such, we will use SelectMany to build and coalesce collections of secondary types for each monster
						row =>
						{
							try
							{
								//Split the row into individual cells on the delimiter ('|') and trim excess whitespace
								List<string> cells = row.Split('|').Select(cell => cell.Trim()).ToList();

								//The first cell in the row should contain the Monster's name.
								//Cross reference this with the card list to get a valid card Id
								string monsterName = cells[0];
								int monsterId = CrossReferenceMonsterCard(monsterName);

								//The third cell in the row contains secondary types for the monster, delimeted by commas.
								//Split these on the delimiter, and parse each one into an individual secondary type for the target monster
								IEnumerable<SecondaryType> monsterSecondaryTypes =
									cells[2].Replace(" ", "").Replace("-", "").Split(',')
										.Select
										(
											type =>
												new SecondaryType()
												{
													CardId = monsterId,
													Type = (MonsterType)Enum.Parse(typeof(MonsterType), type)
												}
										);

								LoggingUtility.LogVerbose
								(
									string.Format
									(
										MessageConstants.SECONDARY_TYPES_LOADED_TEMPLATE,
										monsterId.ToString("000"),
										monsterName
									)
								);

								return monsterSecondaryTypes;
							}
							catch (Exception ex)
							{
								LoggingUtility.LogError(MessageConstants.SECONDARY_TYPE_PARSING_ERROR);
								LoggingUtility.LogError(ex.Message);

								//When using SelectMany, we can't simply return a null for bad records, 
								//as this will cause the final unioning of the data to fail.  
								//Instead, we can return a new collection with a single 'null' record to
								//indicate that there were bad records while parsing secondary types
								return new List<SecondaryType> { null };
							}
						}
					);

				//If there were any null records in the final result set, that indicates there was a problem 
				//parsing some secondary types, so warn the user that some secondary types were not loaded successfully
				if (secondaryTypes.Any(type => type == null))
				{
					LoggingUtility.LogWarning(MessageConstants.SECONDARY_TYPE_LOAD_FAILURE_WARNING);
					LoggingUtility.LogWarning(MessageConstants.SECONDARY_TYPE_FUNCTION_WARNING);
				}
				else
				{
					LoggingUtility.LogInfo(MessageConstants.SECONDARY_TYPE_LOADING_SUCCESSFUL);
				}

				return secondaryTypes.Where(type => type != null);
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError(MessageConstants.SECONDARY_TYPE_LOAD_FAILURE);
				throw ex;
			}
		}


		public override int LoadDataIntoDatabase(IEnumerable<SecondaryType> data)
		{
			throw new NotImplementedException();
		}
		#endregion



		#region Private Methods
		private int CrossReferenceMonsterCard(string monsterName)
		{
			Card referencedCard = _cardList.FirstOrDefault(card => card.Name == monsterName);

			if (referencedCard == null)
			{
				throw new FileParsingAnomalyException
				(
					string.Format
					(
						AnomalyConstants.CARD_NAME_NOT_FOUND_TEMPLATE,
						monsterName
					)
				);
			}

			return referencedCard.CardId;
		}
		#endregion
	}
}
