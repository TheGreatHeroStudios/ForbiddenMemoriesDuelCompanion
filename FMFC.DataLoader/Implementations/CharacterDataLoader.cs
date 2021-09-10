using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Utility;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FMDC.DataLoader.Implementations
{
	public class CharacterDataLoader : DataLoader<Character>
	{
		#region Constructor(s)
		public CharacterDataLoader() : base(URLConstants.YUGIPEDIA_URL)
		{

		}
		#endregion



		#region Abstract Base Class Implementations
		public override Func<Character, int> KeySelector => character => character.CharacterId;

		public override IEnumerable<Character> LoadDataIntoMemory()
		{
			try
			{
				if (ActualRecordCount == ExpectedRecordCount)
				{
					//If the correct count of character records has already been loaded into the  
					//database, skip the entire data load process and return the entities from the database.
					LoggingUtility.LogInfo(MessageConstants.CHARACTER_LOADING_SKIPPED);

					return
						_cardRepository
							.RetrieveEntities<Character>
							(
								entity => true
							);

				}

				//Load the info required to instantiate character objects
				LoggingUtility.LogInfo(MessageConstants.LOADING_CHARACTER_LIST);
				IEnumerable<CharacterLoadingInfo> characterLoadingInfo = ReadCharacterLoadingInfo();

				//Load each character's data into memory
				IEnumerable<Task<Character>> characterTasks =
					characterLoadingInfo
						.Select(loadingInfo => LoadCharacter(loadingInfo))
						.Where(character => character != null);

				//Wait for all character loading tasks to complete before returning the character collection
				Task<Character>[] characterTaskArray = characterTasks.ToArray();
				Task.WaitAll(characterTaskArray, -1);

				IEnumerable<Character> characters =
					characterTaskArray
						.Select(characterTask => characterTask.Result)
						.Where(character => character != null);

				//Make sure that the correct number of chracters were loaded successfully
				if (characters.Count() != DataLoaderConstants.TOTAL_CHARACTER_COUNT)
				{
					LoggingUtility.LogWarning(MessageConstants.CHARACTER_COUNT_MISMATCH);
					LoggingUtility.LogWarning(MessageConstants.CHARACTER_DISPLAY_WARNING);
				}
				else
				{
					LoggingUtility.LogInfo(MessageConstants.CHARACTER_LOADING_SUCCESSFUL);
				}

				return characters;
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError(MessageConstants.CHARACTER_LOAD_FAILURE);
				throw ex;
			}
		}
		#endregion



		#region Override(s)
		public override int ExpectedRecordCount => DataLoaderConstants.TOTAL_CHARACTER_COUNT;
		#endregion



		#region Private Methods
		private IEnumerable<CharacterLoadingInfo> ReadCharacterLoadingInfo()
		{
			//Load each line of the file containing the data needed to retrieve character information
			IEnumerable<string> loadingInfo = LoadDataFile(FileConstants.CHARACTER_LOADING_INFO_FILEPATH);

			//For each line, parse the information into a struct to store the info needed to load the character
			return
				loadingInfo
					.Select
					(
						row =>
						{
							string[] dataFields = row.Split(',').Select(field => field.Trim()).ToArray();
							return new CharacterLoadingInfo()
							{
								CharacterId = int.Parse(dataFields[0]),
								CharacterName = dataFields[1],
								InfoRelativePath = dataFields[2],
								DeckListOrdinal = dataFields[3] == "N/A" ? -1 : int.Parse(dataFields[3])
							};
						}
					);
		}


		private async Task<Character> LoadCharacter(CharacterLoadingInfo loadingInfo)
		{
			try
			{
				//Retrieve HTML from the character's information URL to parse description and deck list
				HttpResponseMessage characterInfoResponse = await GetRemoteContentAsync(loadingInfo.InfoRelativePath);
				string infoContent = await characterInfoResponse.Content.ReadAsStringAsync();

				//Load the HTML Document
				HtmlDocument infoHTML = new HtmlDocument();
				infoHTML.LoadHtml(infoContent);

				if (infoHTML == null || infoHTML.DocumentNode == null)
				{
					throw new Exception(MessageConstants.CHARACTER_INFO_DOM_ERROR);
				}

				string biography = ParseCharacterBiography(infoHTML);

				LoggingUtility.LogVerbose
				(
					string.Format
					(
						MessageConstants.CHARACTER_LOADED_TEMPLATE,
						loadingInfo.CharacterName
					)
				);

				Character character =
					new Character()
					{
						CharacterId = loadingInfo.CharacterId,
						Name = loadingInfo.CharacterName,
						Biography = biography
						//NOTE: Character image and drop rates are
						//loaded and associated to the character as
						//part of separate data loaders
					};

				List<CardPercentage> deckCards =
					loadingInfo.DeckListOrdinal > 0 ?
						ParseCharacterDeckList
						(
							character,
							infoHTML, 
							loadingInfo.DeckListOrdinal
						)
						.ToList() :
						null;

				character.CardPercentages = deckCards;

				return character;
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError
				(
					string.Format
					(
						MessageConstants.CHARACTER_RETRIEVAL_FAILURE_TEMPLATE,
						loadingInfo.CharacterName
					)
				);
				LoggingUtility.LogError(ex.Message);

				return null;
			}
		}


		private string ParseCharacterBiography(HtmlDocument infoHTML)
		{
			try
			{
				StringBuilder characterBio = new StringBuilder();

				//Biography nodes are grandchildren of the "mw-content-text" tag
				IEnumerable<HtmlNode> contentNodes = infoHTML.DocumentNode.Descendants()
					.Where(node => node.ParentNode?.ParentNode?.Id == "mw-content-text");

				//Read all paragraph nodes leading up to the 'Deck' header node
				foreach (HtmlNode currentNode in contentNodes)
				{
					if (currentNode.Name == "p")
					{
						characterBio.AppendLine(currentNode.InnerText);
					}
					else if (currentNode.Name == "h2" && currentNode.FirstChild.Id == "Deck")
					{
						break;
					}
				}

				return characterBio.ToString();
			}
			catch (Exception ex)
			{
				throw new Exception
				(
					string.Format
					(
						MessageConstants.CHARACTER_BIO_PARSING_ERROR_TEMPLATE,
						ex.Message
					)
				);
			}
		}


		private IEnumerable<CardPercentage> ParseCharacterDeckList
		(
			Character targetCharacter,
			HtmlDocument infoHTML, 
			int deckListTableOrdinal
		)
		{
			//Get the card-list table corresponding to the character's deck loadout (as determined by the ordinal)
			//Get all table rows (not including header row) and parse the relevant card data and percentages out of it
			return
				infoHTML.DocumentNode.Descendants()
					.Where(node => node.Name == "table" && node.HasClass("card-list"))
					.Skip(deckListTableOrdinal - 1)
					.First()
					.Descendants()
					.Where(node => node.Name == "tr")
					.Skip(1)
					.Select
					(
						rowNode =>
						{
							try
							{
								List<HtmlNode> dataNodes = rowNode.ChildNodes.Where(node => node.Name == "td").ToList();
								double generationPercentage = double.Parse(dataNodes[8].InnerText) / DataLoaderConstants.DROP_RATE_DENOMINATOR;
								int generationRate = int.Parse(dataNodes[8].InnerText);

								CardPercentage cardPercentage = 
									new CardPercentage
									{
										CharacterId = targetCharacter.CharacterId,
										CardId = int.Parse(dataNodes[0].InnerText),
										PercentageType = PercentageType.DeckInclusion,
										GenerationPercentage = generationPercentage * 100,
										GenerationRatePer2048 = generationRate,
										Character = targetCharacter
									};

								return cardPercentage;
							}
							catch (Exception ex)
							{
								throw new Exception
								(
									string.Format
									(
										MessageConstants.CHARACTER_DECKLIST_PARSING_ERROR_TEMPLATE,
										ex.Message
									)
								);
							}
						}
					);
		}
		#endregion
	}
}
