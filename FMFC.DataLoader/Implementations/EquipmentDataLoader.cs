using FMDC.DataLoader.Exceptions;
using FMDC.Model;
using FMDC.Model.Models;
using FMDC.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FMDC.DataLoader.Implementations
{
	public class EquipmentDataLoader : DataLoader<Equippable>
	{
		#region Properties
		public List<(int LineNumber, string Info)> EquipmentAnomalies { get; set; } = new List<(int LineNumber, string Info)>();
		#endregion



		#region Fields
		private IEnumerable<Card> _cardList = null;
		private int _lastTargetId = 0;
		private DataRowType _lastEquipRowType = DataRowType.Unknown;
		#endregion



		#region Constructor(s)
		public EquipmentDataLoader(IEnumerable<Card> cardList)
		{
			if (cardList == null)
			{
				throw new ArgumentException(MessageConstants.EQUIPMENT_CARD_LIST_NULL);
			}
			else if (cardList.Count() != DataLoaderConstants.TOTAL_CARD_COUNT)
			{
				LoggingUtility.LogWarning(MessageConstants.EQUIPMENT_CARD_LIST_INCOMPLETE);
				LoggingUtility.LogWarning(MessageConstants.EQUIPMENT_FUNCTION_WARNING);
			}
			_cardList = cardList;
		}
		#endregion



		#region Abstract Base Class Implementations
		public override Func<Equippable, int> KeySelector => equippable => equippable.EquippableId;

		public override IEnumerable<Equippable> LoadDataIntoMemory()
		{
			LoggingUtility.LogInfo(MessageConstants.BEGIN_EQUIPMENT_LOADING);

			try
			{
				int currentRowNum = 0;

				//Load equipment data from file
				IEnumerable<string> equipmentData = LoadDataFile(FileConstants.EQUIPMENT_FILEPATH);

				List<Equippable> equips = equipmentData
					.Select
					(
						record =>
						{
							currentRowNum++;
							return ParseEquipmentRecord(record, currentRowNum);
						}
					)
					.ToList();

				if (EquipmentAnomalies.Any())
				{
					LoggingUtility.LogWarning(MessageConstants.EQUIPMENT_LOAD_FAILURE_WARNING_TEMPLATE);
				}
				else
				{
					LoggingUtility.LogInfo(MessageConstants.EQUIPMENT_LOADING_SUCCESSFUL);
				}

				return equips.Where(equip => equip != null);
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError(MessageConstants.EQUIPMENT_LOAD_FAILURE);
				throw ex;
			}
		}
		#endregion



		#region Override(s)
		public override int ExpectedRecordCount => DataLoaderConstants.TOTAL_EQUIPPABLE_COUNT;
		#endregion



		#region Public Methods
		public void LogAnomalies(FileLogger logger)
		{
			if (EquipmentAnomalies.Any())
			{
				logger.WriteLine("");

				logger.WriteLine
				(
					string.Format
					(
						FileConstants.EQUIPMENT_ANOMALY_COUNT_TEMPLATE,
						EquipmentAnomalies.Count
					)
				);
				EquipmentAnomalies
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
		#endregion



		#region Private Methods
		private Equippable ParseEquipmentRecord(string rowData, int lineNumber)
		{
			DataRowType rowType = DataRowType.Unknown;

			rowData = rowData.Trim();

			try
			{
				rowType = DetermineEquipmentRowType(rowData);

				switch (rowType)
				{
					//If the row contains a target for equipment, cross-reference it with the card list and store the target id
					case DataRowType.Target:
					{
						//Verify that the target is either the first record, or it is preceeded by a delimiter
						if (lineNumber != 1 && _lastEquipRowType != DataRowType.Delimiter)
						{
							throw new FileParsingAnomalyException(AnomalyConstants.TARGET_NO_DELIMITER);
						}

						//Parse the card Id and name from the start of the row data
						string cardIdString = rowData.Substring(1, 3);
						string cardName = rowData.Substring(5, rowData.IndexOf('(') - 6).Trim();

						if (!int.TryParse(cardIdString, out int targetCardId))
						{
							throw new FileParsingAnomalyException(AnomalyConstants.CARD_ID_PARSING_ERROR);
						}

						//Verify that the card Id parsed matches the name of the card provided
						VerifyCard(targetCardId, cardName);

						//Store the target card's Id to associate with upcoming equips
						_lastTargetId = targetCardId;

						//Target rows do not constitute their own equips, so return null
						return null;
					}

					case DataRowType.Divider:
					{
						//If the current row is a divider, ensure that the previous row was a Target.
						if (_lastEquipRowType != DataRowType.Target)
						{
							//If it wasn't, log an anomaly and invalidate the last target id (so we don't associate an equip with the wrong target)
							_lastTargetId = 0;
							throw new FileParsingAnomalyException(AnomalyConstants.DIVIDER_NO_TARGET);
						}

						//Divider rows do not constitute their own drop, so return null
						return null;
					}

					case DataRowType.Delimiter:
					{
						//If the current row is a delimiter, invalidate the last target id.  (The next row should also be the second delimiter or new target.)
						_lastTargetId = 0;

						if (!new[] { DataRowType.Delimiter, DataRowType.Equip }.Contains(_lastEquipRowType))
						{
							//If the previous row was not an equip or other delimiter, it is an anomaly
							throw new FileParsingAnomalyException(AnomalyConstants.DELIMITER_NO_VALID_PRECEEDING);
						}
						else if (_lastEquipRowType == DataRowType.Delimiter && !string.IsNullOrEmpty(rowData))
						{
							//If the previous row was a delimeter and this delimiter is more than just whitespace, log an anomaly
							throw new FileParsingAnomalyException(AnomalyConstants.SECOND_DELIMITER_NO_FIRST);
						}
						else if (_lastEquipRowType == DataRowType.Equip && rowData.Any(c => c != '='))
						{
							//If the previous row was not a delimiter, but the current row is not the primary delimiter, log an anomaly
							throw new FileParsingAnomalyException(AnomalyConstants.PRIMARY_DELIMITER_INVALID);
						}

						//Delimiters do not constitute their own drop, so return null
						return null;
					}

					//If the row contains an equipment card, cross-reference it with the card list and build the equippable record
					case DataRowType.Equip:
					{
						//Verify that the equipment card is preceeded either by another equip card or a divider
						if (!new[] { DataRowType.Equip, DataRowType.Divider }.Contains(_lastEquipRowType))
						{
							throw new FileParsingAnomalyException(AnomalyConstants.EQUIP_INVALID_PRECEEDING);
						}

						//Parse the equip card Id and name from the start of the row data
						string cardIdString = rowData.Substring(1, 3);
						string cardName = rowData.Substring(5);

						if (!int.TryParse(cardIdString, out int equipCardId))
						{
							throw new FileParsingAnomalyException(AnomalyConstants.EQUIP_PARSING_ERROR);
						}

						//Verify that the equip card Id parsed matches the name of the card provided
						VerifyCard(equipCardId, cardName);

						//Ensure that a valid target card is set for the equipemnt
						Card lastTargetCard = _cardList.Where(card => card.CardId == _lastTargetId).FirstOrDefault();

						if (lastTargetCard == null)
						{
							throw new FileParsingAnomalyException(AnomalyConstants.NO_VALID_TARGET_FOR_EQUIP);
						}

						//If we got this far, create an equippable record and return it
						Equippable equipment = new Equippable()
						{
							EquipCardId = equipCardId,
							TargetCardId = _lastTargetId
						};

						LoggingUtility.LogVerbose
						(
							string.Format
							(
								MessageConstants.EQUIPMENT_PARSED_TEMPLATE,
								_cardList.First(card => card.CardId == equipCardId).Name,
								_lastTargetId.ToString("000"),
								lastTargetCard.Name
							)
						);

						return equipment;
					}

					default:
					{
						throw new FileParsingAnomalyException(AnomalyConstants.UNKNOWN_ROW_TYPE);
					}
				}
			}
			catch (FileParsingAnomalyException ex)
			{
				EquipmentAnomalies.Add((lineNumber, ex.Message));
				return null;
			}
			finally
			{
				_lastEquipRowType = rowType;
			}
		}


		private DataRowType DetermineEquipmentRowType(string rowData)
		{
			DataRowType rowType = DataRowType.Unknown;

			rowData = rowData.Trim();

			if (string.IsNullOrEmpty(rowData) || !rowData.Any(c => c != '='))
			{
				//If the row data has only whitespace or equals signs it is a delimiting row
				rowType = DataRowType.Delimiter;
			}
			else if (rowData.Contains('('))
			{
				//If the row data contains an open parentheses it denotes a target with a certain number of equips
				rowType = DataRowType.Target;
			}
			else if (!rowData.Any(c => c != '-'))
			{
				//If the row data contains only dashes, it is a divider between targets and their equips
				rowType = DataRowType.Divider;
			}
			else if (rowData.StartsWith("#"))
			{
				//If all else fails, and the row starts with a number sign (#), treat it as an equip for the former target
				rowType = DataRowType.Equip;
			}

			return rowType;
		}


		private void VerifyCard(int cardId, string cardName)
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
		#endregion
	}
}
