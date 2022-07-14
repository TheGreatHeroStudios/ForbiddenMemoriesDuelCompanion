using FMDC.DataLoader.Exceptions;
using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TGH.Common.Utilities.DataLoader.Implementations;
using TGH.Common.Utilities.Logging;

namespace FMDC.DataLoader.Implementations
{
	public class FusionDataLoader : DataLoader<Fusion>
	{
		#region Properties
		public List<(int LineNumber, string Info)> GeneralFusionAnomalies { get; } = new List<(int lineNumber, string content)>();
		public List<(int LineNumber, string Info)> SpecificFusionAnomalies { get; } = new List<(int lineNumber, string content)>();
		#endregion



		#region Fields
		private IEnumerable<Card> _cardList = null;
		private Fusion _lastGeneralFusion = null;
		private int _lastSpecificTargetId = 0;
		private DataRowType _lastSpecificFusionRowType = DataRowType.Unknown;
		#endregion



		#region Constructor(s)
		public FusionDataLoader(IEnumerable<Card> cardList)
		{
			if (cardList == null)
			{
				throw new ArgumentException(MessageConstants.FUSION_LOADER_CARD_LIST_NULL);
			}
			else if (cardList.Count() != DataLoaderConstants.TOTAL_CARD_COUNT)
			{
				Logger.LogWarning(MessageConstants.FUSION_LOADER_CARD_LIST_INCOMPLETE);
			}
			_cardList = cardList;
		}
		#endregion



		#region Abstract Base Class Implementations
		public override Func<Fusion, int> KeySelector => fusion => fusion.FusionId;

		public override IEnumerable<Fusion> ReadDataIntoMemory()
		{
			if (ActualRecordCount == ExpectedRecordCount)
			{
				//If the correct count of fusion records has already been loaded into the  
				//database, skip the entire data load process and return the entities from the database.
				Logger.LogInfo(MessageConstants.FUSION_LOADING_SKIPPED);

				return
					_repository
						.RetrieveEntities<Fusion>
						(
							entity => true
						);

			}

			return
				LoadFusions(FusionType.General)
					.Concat(LoadFusions(FusionType.Specific))
					.Where(fusion => fusion != null);
		}
		#endregion



		#region Override(s)
		public override int ExpectedRecordCount => DataLoaderConstants.TOTAL_FUSION_COUNT;
		#endregion



		#region Public Methods
		public void LogAnomalies()
		{
			if (GeneralFusionAnomalies.Any())
			{
				Logger.WriteLine("");
				Logger.WriteLine
				(
					string.Format
					(
						FileConstants.GENERAL_FUSION_ANOMALY_COUNT_TEMPLATE,
						GeneralFusionAnomalies.Count()
					)
				);

				GeneralFusionAnomalies
					.ForEach
					(
						anomaly =>
							Logger.WriteLine
							(
								string.Format
								(
									FileConstants.ANOMALY_LOG_TEMPLATE,
									anomaly.LineNumber,
									anomaly.Info
								)
							)
					);

				Logger.WriteLine("");
			}

			if (SpecificFusionAnomalies.Any())
			{
				Logger.WriteLine("");
				Logger.WriteLine
				(
					string.Format
					(
						FileConstants.SPECIFIC_FUSION_ANOMALY_COUNT_TEMPLATE,
						SpecificFusionAnomalies.Count()
					)
				);

				SpecificFusionAnomalies
					.ForEach
					(
						anomaly =>
							Logger.WriteLine
							(
								string.Format
								(
									FileConstants.ANOMALY_LOG_TEMPLATE,
									anomaly.LineNumber,
									anomaly.Info
								)
							)
					);
				Logger.WriteLine("");
			}
		}
		#endregion



		#region Private Methods
		private IEnumerable<Fusion> LoadFusions(FusionType fusionType)
		{
			Logger.LogInfo(string.Format(MessageConstants.LOADING_FUSION_DATA_TEMPLATE, fusionType));

			try
			{
				int currentRowNum = 0;
				bool loadSpecificFusions = fusionType == FusionType.Specific;

				//Load either specific or general fusion data depending on how the method is called
				string fusionDataFilePath = loadSpecificFusions ?
					FileConstants.SPECIFIC_FUSION_FILEPATH :
					FileConstants.GENERAL_FUSION_FILEPATH;

				//Parse each row of the data file to build a list of fusion records.
				//NOTE: 'ToList()' must be called to materialize the collection so that any anomalies can be logged and notified
				IEnumerable<string> fusionData = LoadDataFile(fusionDataFilePath);
				List<Fusion> fusions = fusionData
					.Select
					(
						record =>
						{
							currentRowNum++;
							return loadSpecificFusions ?
								ParseSpecificFusionRecord(record, currentRowNum) :
								ParseGeneralFusionRecord(record, currentRowNum);
						}
					)
					.ToList();

				//Use reflection to get the anomalies property for the type of fusions loaded
				var anomalies = (IEnumerable<(int, string)>)GetType()
					.GetProperty(string.Format(PropertyConstants.FUSION_ANOMALY_PROPERTY_NAME_TEMPLATE, fusionType))
					.GetValue(this);

				//If any anomalies exist, log a warning message to the console.  These should also be dumped to a file.
				if (anomalies.Any())
				{
					Logger.LogWarning(string.Format(MessageConstants.FUSION_LOAD_FAILURE_WARNING_TEMPLATE, fusionType));
				}
				else
				{
					Logger.LogInfo(string.Format(MessageConstants.FUSION_LOADING_SUCCESSFUL_TEMPLATE, fusionType));
				}

				return fusions;
			}
			catch (Exception ex)
			{
				Logger.LogError(string.Format(MessageConstants.FUSION_LOADING_ERROR_TEMPLATE, fusionType));
				throw ex;
			}
		}


		private Fusion ParseGeneralFusionRecord(string rowData, int lineNumber)
		{
			try
			{
				Fusion generalFusion = new Fusion();
				generalFusion.FusionType = FusionType.General;

				//Split the fusion 'equation' into left and right operands
				string[] operands = rowData.Split('=');

				if (operands.Count() != 2)
				{
					//If there was no equal sign in the line, there is no fusion result
					throw new FileParsingAnomalyException(AnomalyConstants.INCORRECT_NUM_OPERANDS);
				}

				//Lookup the resultantCard from the name in the right operand
				Card resultantCard = LookupCard(operands[1]);
				generalFusion.ResultantCardId = resultantCard.CardId;

				if (operands[0].Contains('+'))
				{
					//If the left operand contains a plus, the row contains a new fusion, 
					//so split the two components of the fusion
					string[] fusionComponents = operands[0].Split('+').Select(component => component.Trim()).ToArray();

					//There should be exactly 2 components for fusion.  If there are more/fewer, the fusion can't be parsed
					if (fusionComponents.Count() != 2)
					{
						throw new FileParsingAnomalyException(AnomalyConstants.INCORRECT_NUM_COMPONENTS);
					}

					//Parse the target and fusionComponent cards from the two components in the row...
					for (int i = 0; i < fusionComponents.Count(); i++)
					{
						if (Regex.IsMatch(fusionComponents[i].Replace("-", "").Replace(" ", ""), RegexConstants.GENERAL_TYPE_REGEX))
						{
							//...If either component matches the generic type syntax, try to parse just the type...
							fusionComponents[i] = fusionComponents[i].Replace("[", "").Replace("]", "").Replace("-", "").Replace(" ", "");

							if (!Enum.TryParse(fusionComponents[i], out MonsterType componentType))
							{
								throw new FileParsingAnomalyException(string.Format(AnomalyConstants.INVALID_TYPE_TEMPLATE, fusionComponents[i]));
							}

							if (i == 0)
							{
								generalFusion.TargetMonsterType = componentType;
							}
							else
							{
								generalFusion.FusionMaterialMonsterType = componentType;
							}
						}
						else
						{
							//...Otherwise, parse the whole card to get its ID.
							Card componentCard = LookupCard(fusionComponents[i]);

							if (i == 0)
							{
								generalFusion.TargetCardId = componentCard.CardId;
							}
							else
							{
								generalFusion.FusionMaterialCardId = componentCard.CardId;
							}
						}
					}

					//Since subsequent rows may reference the same fusion to form a different card (the left operand is blank),
					//We will store the results of this fusion to re-use for subsequent rows if needed
					_lastGeneralFusion = generalFusion;
				}
				else if (string.IsNullOrEmpty(operands[0].Trim()))
				{
					//If the left operand is empty, the right operand is a different result of the last fusion, 
					//so copy the data from the last fusion
					generalFusion.TargetCardId = _lastGeneralFusion.TargetCardId;
					generalFusion.TargetMonsterType = _lastGeneralFusion.TargetMonsterType;
					generalFusion.FusionMaterialCardId = _lastGeneralFusion.FusionMaterialCardId;
					generalFusion.FusionMaterialMonsterType = _lastGeneralFusion.FusionMaterialMonsterType;
				}
				else
				{
					//If the left hand operand is not empty but contains no plus, it's a bad row
					throw new FileParsingAnomalyException(AnomalyConstants.INVALID_OPERATION);
				}

				return generalFusion;
			}
			catch (FileParsingAnomalyException ex)
			{
				GeneralFusionAnomalies.Add((lineNumber, ex.Message));
				return null;
			}
		}


		private Fusion ParseSpecificFusionRecord(string rowData, int lineNumber)
		{
			DataRowType rowType = DataRowType.Unknown;

			try
			{
				rowType = DetermineSpecificFusionRowType(rowData);

				switch (rowType)
				{
					case DataRowType.Target:
					{
						//If the row contains a target, try to parse the target card id from the last 3 characters of the row
						rowData = rowData.Trim();
						string cardIdString = rowData.Substring(rowData.Length - 3, 3);

						if
						(
							//Store this ID, as it will apply to any fusions that follow it
							!int.TryParse(cardIdString, out _lastSpecificTargetId) ||
							!_cardList.Where(card => card.CardId == _lastSpecificTargetId).Any()
						)
						{
							//Throw an exception if the ID could not be parsed or does not exist in the card list
							throw new FileParsingAnomalyException(AnomalyConstants.TARGET_CARD_PARSING_ERROR);
						}

						//If the preceeding row was not a delimiter, log an anomaly
						if (lineNumber != 1 && _lastSpecificFusionRowType != DataRowType.Delimiter)
						{
							throw new FileParsingAnomalyException(AnomalyConstants.TARGET_NO_DELIMITER);
						}

						//If the current record is a target, it does not constitute a fusion of its own, so return null
						return null;
					}

					case DataRowType.Divider:
					{
						//If the current record is a divider, ensure that the previous row type was a target.
						if (_lastSpecificFusionRowType != DataRowType.Target)
						{
							//If it wasn't, log an anomaly and invalidate the last target id (so we don't associate a fusion with the wrong target)
							_lastSpecificTargetId = 0;
							throw new FileParsingAnomalyException(AnomalyConstants.DIVIDER_NO_TARGET);
						}

						return null;
					}

					case DataRowType.Delimiter:
					{
						//If the current row is a delimiter, invalidate the last target id.  (The next row should also be a new target.)
						_lastSpecificTargetId = 0;

						//If the previous row was a target (or additional delimiter), log an anomaly
						if (_lastSpecificFusionRowType == DataRowType.Target || _lastSpecificFusionRowType == DataRowType.Delimiter)
						{
							throw new FileParsingAnomalyException(AnomalyConstants.DELIMITER_NO_FUSION);
						}

						return null;
					}

					case DataRowType.Fusion:
					{
						//Ensure that a valid target card is set before attempting to create the fusion
						if (!_cardList.Where(card => card.CardId == _lastSpecificTargetId).Any())
						{
							throw new FileParsingAnomalyException(AnomalyConstants.NO_VALID_TARGET_FOR_FUSION);
						}

						Fusion specificFusion = new Fusion();
						specificFusion.FusionType = FusionType.Specific;

						//Format each operand by trimming whitespace and removing anything in parentheses.  What remains should be two card names
						string[] operands = Regex.Replace(rowData, RegexConstants.PARENTHESES_REGEX, "").Split('=')
							.Select(operand => operand.Trim())
							.ToArray();

						//Ensure that there are exactly 2 operands (a fusion material and a result)
						if (operands.Count() != 2)
						{
							throw new FileParsingAnomalyException(AnomalyConstants.INCORRECT_NUM_OPERANDS);
						}

						//For each of the two operands, parse a card, and use it to set the fusion data
						Card[] parsedCards = operands
							.Select(operand => LookupCard(operand))
							.ToArray();

						specificFusion.TargetCardId = _lastSpecificTargetId;
						specificFusion.FusionMaterialCardId = parsedCards[0].CardId;
						specificFusion.ResultantCardId = parsedCards[1].CardId;

						Logger.LogVerbose
						(
							string.Format
							(
								MessageConstants.FUSION_PARSED_TEMPLATE,
								_cardList.Where(card => card.CardId == _lastSpecificTargetId).First().Name,
								parsedCards[0].Name,
								parsedCards[1].Name
							)
						);

						return specificFusion;
					}

					case DataRowType.Equip:
					{
						//If the card is an equip, do nothing.  (These will be handled by a different data loader)
						return null;
					}

					default:
					{
						throw new FileParsingAnomalyException(AnomalyConstants.UNKNOWN_ROW_TYPE);
					}
				}
			}
			catch (FileParsingAnomalyException ex)
			{
				SpecificFusionAnomalies.Add((lineNumber, ex.Message));
				return null;
			}
			finally
			{
				_lastSpecificFusionRowType = rowType;
			}
		}


		private Card LookupCard(string cardName)
		{
			//Remove attack and defense from the card name and trim any whitespace
			string cleansedName = Regex.Replace(cardName, RegexConstants.PARENTHESES_REGEX, "").Trim();

			Card referencedCard = _cardList.FirstOrDefault(card => card.Name.ToLower() == cleansedName.ToLower());

			if (referencedCard == null)
			{
				//If we could not parse a card object from the name of the referenced card in the file, it is not a valid card
				throw new FileParsingAnomalyException(string.Format(AnomalyConstants.CARD_NAME_NOT_FOUND_TEMPLATE, cleansedName));
			}

			return referencedCard;
		}


		private DataRowType DetermineSpecificFusionRowType(string rowData)
		{
			DataRowType rowType = DataRowType.Unknown;

			if (string.IsNullOrEmpty(rowData.Trim()))
			{
				//If the row data contains only whitespace, it is a delimiting row
				rowType = DataRowType.Delimiter;
			}
			else if (string.IsNullOrEmpty(rowData.Trim().Replace("-", "")))
			{
				//If the row data contains only dashes, it is a divider between the target and fusions materials
				rowType = DataRowType.Divider;
			}
			else if (rowData.Contains('='))
			{
				//If the row data contains an equals sign, it is a fusion row
				rowType = DataRowType.Fusion;
			}
			else if (int.TryParse(rowData.Trim().Substring(rowData.Trim().Length - 3, 3), out int result) && result > 0 && result <= DataLoaderConstants.TOTAL_CARD_COUNT)
			{
				//If we were able to parse a valid card id from the right of the data row, the card is a target
				rowType = DataRowType.Target;
			}
			else if (rowData.Contains("(Equip)"))
			{
				//If all else fails and the row contains the term '(Equip)', treat the row as an 'equip' fusion and ignore it
				rowType = DataRowType.Equip;
			}


			return rowType;
		}
		#endregion
	}
}
