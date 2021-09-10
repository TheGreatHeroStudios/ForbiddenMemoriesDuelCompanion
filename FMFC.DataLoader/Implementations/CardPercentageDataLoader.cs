using FMDC.DataLoader.Exceptions;
using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FMDC.DataLoader.Implementations
{
	public class CardPercentageDataLoader : DataLoader<CardPercentage>
	{
		#region Properties
		public List<(int LineNumber, string Info)> SA_POWDropRateAnomalies { get; set; } = new List<(int LineNumber, string Info)>();
		public List<(int LineNumber, string Info)> SA_TECDropRateAnomalies { get; set; } = new List<(int LineNumber, string Info)>();
		public List<(int LineNumber, string Info)> BCD_POW_TECDropRateAnomalies { get; set; } = new List<(int LineNumber, string Info)>();
		#endregion



		#region Fields
		private IEnumerable<Card> _cardList = null;
		private IEnumerable<Character> _characterList = null;
		private int _lastCharacterId = 0;
		private DataRowType _lastDropRateRowType = DataRowType.Unknown;
		#endregion



		#region Constructor(s)
		public CardPercentageDataLoader
		(
			IEnumerable<Card> cardList, 
			IEnumerable<Character> characterList
		)
		{
			if (cardList == null)
			{
				throw new ArgumentException(MessageConstants.DROP_LOADER_CARD_LIST_NULL);
			}
			else if (cardList.Count() != DataLoaderConstants.TOTAL_CARD_COUNT)
			{
				LoggingUtility.LogWarning(MessageConstants.DROP_LOADER_CARD_LIST_INCOMPLETE);
				LoggingUtility.LogWarning(MessageConstants.DROP_RATE_DISPLAY_WARNING);
			}
			_cardList = cardList;

			if (characterList == null)
			{
				throw new ArgumentException(MessageConstants.DROP_LOADER_CHARACTER_LIST_NULL);
			}
			else if (characterList.Count() != DataLoaderConstants.TOTAL_CHARACTER_COUNT)
			{
				LoggingUtility.LogWarning(MessageConstants.DROP_LOADER_CHARACTER_LIST_INCOMPLETE);
				LoggingUtility.LogWarning(MessageConstants.DROP_RATE_DISPLAY_WARNING);
			}
			_characterList = characterList;
		}
		#endregion



		#region Abstract Base Class Implementations
		public override Func<CardPercentage, int> KeySelector => 
			cardPercentage => cardPercentage.CardPercentageId;

		public override IEnumerable<CardPercentage> LoadDataIntoMemory()
		{
			if (ActualRecordCount == ExpectedRecordCount)
			{
				//If the correct count of card percentage records has already been loaded into the  
				//database, skip the entire data load process and return the entities from the database.
				LoggingUtility.LogInfo(MessageConstants.CARD_PERCENTAGE_LOADING_SKIPPED);

				return
					_cardRepository
						.RetrieveEntities<CardPercentage>
						(
							entity => true
						);

			}

			return
				LoadDropPercentages(PercentageType.SA_POW)
					.Concat(LoadDropPercentages(PercentageType.SA_TEC))
					.Concat(LoadDropPercentages(PercentageType.BCD_POW_TEC))
					.Where(percentage => percentage != null);
		}
		#endregion



		#region Override(s)
		public override int ExpectedRecordCount => DataLoaderConstants.TOTAL_CARD_PERCENTAGE_COUNT;
		#endregion



		#region Public Methods
		public void LogAnomalies(FileLogger logger)
		{
			//Get the properties logging anomalies for each type of drop
			foreach (PropertyInfo anomalyPropertyInfo in GetType().GetProperties())
			{
				//Get the value of each anomaly list property
				List<(int LineNumber, string Info)> anomalyList =
					anomalyPropertyInfo.GetValue(this) as List<(int LineNumber, string Info)>;

				//If the property was not an anomaly list, do nothing else for the current property
				if (anomalyList == null)
				{
					continue;
				}

				//If any anomalies were logged, write them to a file
				if (anomalyList.Any())
				{
					logger.WriteLine("");

					logger.WriteLine
					(
						string.Format
						(
							FileConstants.GENERIC_ANOMALY_COUNT_TEMPLATE,
							anomalyPropertyInfo.Name,
							anomalyList.Count()
						)
					);

					anomalyList
						.ForEach
						(
							anomaly =>
								logger.WriteLine
								(
									string.Format
									(
										FileConstants.ANOMALY_LOG_TEMPLATE,
										anomaly.LineNumber,
										anomaly.Info
									)
								)
						);
				}
			}
		}
		#endregion



		#region Private Methods
		private IEnumerable<CardPercentage> LoadDropPercentages
		(
			PercentageType dropPercentageType
		)
		{
			LoggingUtility.LogInfo
			(
				string.Format
				(
					MessageConstants.LOADING_DROPRATE_DATA_TEMPLATE, 
					dropPercentageType
				)
			);

			try
			{
				int currentRowNum = 0;

				//Load the file for the specified drop
				//percentage type (POW/TEC and ranking)
				IEnumerable<string> dropRateData = 
					LoadDataFile
					(
						string.Format
						(
							FileConstants.DROP_RATE_FILEPATH_TEMPLATE, 
							dropPercentageType
						)
					);

				//Parse each row of the data file to build a list of drop percentages.
				//NOTE: 'ToList()' must be called to materialize the collection so that
				//any anomalies can be logged and notified
				List<CardPercentage> dropRates = 
					dropRateData
						.Select
						(
							record =>
							{
								currentRowNum++;
								return ParseCardDropPercentageRecord
								(
									record, 
									currentRowNum, 
									dropPercentageType
								);
							}
						)
						.ToList();

				//Use reflection to get the anomalies property for the type of card drops loaded
				//If any anomalies exist, log a warning message to the console.  These should also be dumped to a file.
				if (GetAnomalyLoggingProperty(dropPercentageType).Any())
				{
					LoggingUtility
						.LogWarning
						(
							string.Format
							(
								MessageConstants.DROPRATE_LOAD_FAILURE_WARNING_TEMPLATE, 
								dropPercentageType
							)
						);
				}
				else
				{
					LoggingUtility
						.LogInfo
						(
							string.Format
							(
								MessageConstants.DROPRATE_LOADING_SUCCESSFUL_TEMPLATE, 
								dropPercentageType
							)
						);
				}

				return dropRates;
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError
				(
					string.Format
					(
						MessageConstants.DROP_PERCENTAGE_LOADING_FAILURE_TEMPLATE,
						dropPercentageType
					)
				);
				throw ex;
			}
		}


		private CardPercentage ParseCardDropPercentageRecord
		(
			string rowData, 
			int lineNumber, 
			PercentageType dropPercentageType
		)
		{
			DataRowType rowType = DataRowType.Unknown;

			try
			{
				rowType = DetermineDropRateRowType(rowData);

				switch (rowType)
				{
					//If the row contains the name of a character, cross-reference it with the character list to get and store the character's Id
					case DataRowType.Character:
					{
						Character currentCharacter = 
							_characterList
								.Where(c => c.Name == rowData.Trim())
								.FirstOrDefault();

						if (currentCharacter == null)
						{
							//If a valid character could not be found based on the name provided, log an anomaly and invalidate the last character
							_lastCharacterId = 0;
							throw new FileParsingAnomalyException
							(
								string.Format
								(
									AnomalyConstants.CHARACTER_NOT_FOUND_TEMPLATE,
									rowData
								)
							);
						}

						_lastCharacterId = currentCharacter.CharacterId;

						//If the preceeding row was not a delimiter, log an anomaly
						if (lineNumber != 1 && _lastDropRateRowType != DataRowType.Delimiter)
						{
							throw new FileParsingAnomalyException
							(
								AnomalyConstants.CHARACTER_NO_DELIMITER
							);
						}

						//Character rows do not constitute their own drop, so return null
						return null;
					}

					case DataRowType.Divider:
					{
						//If the current row is a divider, ensure that the previous row was a Character.
						if (_lastDropRateRowType != DataRowType.Character)
						{
							//If it wasn't, log an anomaly and invalidate the last character id (so we don't associate a drop with the wrong character)
							_lastCharacterId = 0;
							throw new FileParsingAnomalyException
							(
								AnomalyConstants.DIVIDER_NO_CHARACTER
							);
						}

						//Divider rows do not constitute their own drop, so return null
						return null;
					}

					case DataRowType.Delimiter:
					{
						//If the current row is a delimiter, invalidate the last character id.  (The next row should also be a new target.)
						_lastCharacterId = 0;

						//If the delimiter was not preceeded by a target card drop, log an anomaly
						if (_lastDropRateRowType != DataRowType.Target)
						{
							throw new FileParsingAnomalyException
							(
								AnomalyConstants.DELIMITER_NO_DROP
							);
						}

						//Delimiters do not constitute their own drop, so return null
						return null;
					}

					case DataRowType.Target:
					{
						try
						{
							rowData = rowData.Trim();

							//If the current row is a target for dropping, first, ensure that a valid character is set for the drop
							Character targetCharacter = 
								_characterList
									.FirstOrDefault
									(
										character => 
											character.CharacterId == _lastCharacterId
									);

							if (targetCharacter == null)
							{
								throw new FileParsingAnomalyException
								(
									AnomalyConstants.NO_VALID_CHARACTER
								);
							}

							//Next, attempt to parse the card Id from the start of the row data, 
							//the drop rate from the end of the row data, and the card name from the middle
							IEnumerable<string> targetParams = rowData.Split(new[] { ' ' }, StringSplitOptions.None);

							string cardIdString = targetParams.First();
							string dropRateString = targetParams.Last();

							string cardName = targetParams
								.Skip(1)
								.Take(targetParams.Count() - 2)
								.Aggregate
								(
									new StringBuilder(),
									(sb, word) => sb.Append($"{word} "),
									sb => sb.ToString().Trim()
								);

							if (!int.TryParse(dropRateString, out int dropRate))
							{
								throw new FileParsingAnomalyException
								(
									AnomalyConstants.DROP_RATE_PARSING_ERROR
								);
							}

							if (!int.TryParse(cardIdString, out int targetCardId))
							{
								throw new FileParsingAnomalyException
								(
									AnomalyConstants.CARD_ID_PARSING_ERROR
								);
							}

							//Verify that the card Id and name parsed from the row data are correctly correlated
							VerifyDroppedCard(targetCardId, cardName);

							//If all the above validation has passed, we should have all the information we need 
							//to build the CardPercentage object
							CardPercentage cardPercentage = 
								new CardPercentage()
								{
									CharacterId = targetCharacter.CharacterId,
									CardId = targetCardId,
									PercentageType = dropPercentageType,
									GenerationPercentage = 
										dropRate / (double)DataLoaderConstants.DROP_RATE_DENOMINATOR * 100,
									GenerationRatePer2048 = dropRate,
									Character = targetCharacter,

								};

							LoggingUtility.LogVerbose
							(
								string.Format
								(
									MessageConstants.DROPRATE_PARSED_TEMPLATE,
									dropPercentageType,
									targetCharacter.Name,
									cardName,
									Math.Round(cardPercentage.GenerationPercentage, 3)
								)
							);

							return cardPercentage;
						}
						catch (Exception ex)
						{
							throw new FileParsingAnomalyException(ex.Message);
						}
					}

					default:
					{
						throw new FileParsingAnomalyException(AnomalyConstants.UNKNOWN_ROW_TYPE);
					}
				}
			}
			catch (FileParsingAnomalyException ex)
			{
				GetAnomalyLoggingProperty(dropPercentageType).Add((lineNumber, ex.Message));
				return null;
			}
			finally
			{
				_lastDropRateRowType = rowType;
			}
		}


		private DataRowType DetermineDropRateRowType(string rowData)
		{
			DataRowType rowType = DataRowType.Unknown;

			rowData = rowData.Trim();

			if (string.IsNullOrEmpty(rowData))
			{
				//If the row data contains only whitespace, it is a delimiting row
				rowType = DataRowType.Delimiter;
			}
			else if (!rowData.Any(c => c != '-'))
			{
				//If the row data contains only dashes, it is a divider between the character and target drops
				rowType = DataRowType.Divider;
			}
			else if (char.IsLetter(rowData[0]))
			{
				//If the first character in the row data is a letter, the row data is a character
				rowType = DataRowType.Character;
			}
			else if (char.IsDigit(rowData[0]))
			{
				//If the first character in the row data is a digit, the row data is a target card dropped by the character
				rowType = DataRowType.Target;
			}

			return rowType;
		}


		private void VerifyDroppedCard(int cardId, string cardName)
		{
			Card referencedCard = _cardList.FirstOrDefault(card => card.CardId == cardId);

			if (referencedCard == null)
			{
				//If we could not parse a card object from the id of the referenced card in the file, it is not a valid card
				throw new FileParsingAnomalyException(string.Format(AnomalyConstants.CARD_ID_NOT_FOUND_TEMPLATE, cardId.ToString("000")));
			}
			else if (!string.Equals(cardName, referencedCard.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				//If the name provided for the drop does not match that of the referenced card, log an anomaly
				throw new FileParsingAnomalyException
				(
					string.Format
					(
						AnomalyConstants.DROPPED_CARD_NAME_DISCREPANCY_TEMPLATE,
						cardName,
						referencedCard.CardId.ToString("000"),
						referencedCard.Name
					)
				);
			}
		}


		private List<(int LineNumber, string Info)> GetAnomalyLoggingProperty
		(
			PercentageType anomalyType
		)
		{
			return (List<(int LineNumber, string Info)>)GetType()
				.GetProperty(string.Format(PropertyConstants.DROPRATE_ANOMALY_PROPERTY_NAME_TEMPLATE, anomalyType))
				.GetValue(this);
		}
		#endregion
	}
}
