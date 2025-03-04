﻿using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TGH.Common.Utilities.DataLoader.Implementations;
using TGH.Common.Utilities.Logging;

namespace FMDC.DataLoader.Implementations
{
	public class CardDataLoader : DataLoader<Card>
	{
		#region Constructor(s)
		public CardDataLoader() : base(URLConstants.YUGIOH_FANDOM_URL) { }
		#endregion



		#region Abstract Base Class Implementations
		public override Func<Card, int> KeySelector => card => card.CardId;

		public override IEnumerable<Card> ReadDataIntoMemory()
		{
			if (ActuaRecordCount == ExpectedRecordCount)
			{
				//If the correct count of card records has already been loaded into the database, 
				//skip the entire data load process and return the entities from the database.
				Logger.LogInfo(MessageConstants.CARD_LOADING_SKIPPED);

				return
					_context.Read<Card>(entity => true);

			}

			Task<HttpResponseMessage> cardListResponse = null;

			try
			{
				if (!File.Exists(FileConstants.CARD_DATA_FILEPATH))
				{
					Logger.LogInfo(MessageConstants.BEGIN_CARD_LOADING);
					cardListResponse = GetRemoteContentAsync(URLConstants.CARD_LIST_PATH);
					cardListResponse.Wait();
				}
			}
			catch (Exception)
			{
				Logger.LogError(MessageConstants.CARD_REPO_ACCESS_FAILURE);
				throw;
			}

			try
			{
				Logger.LogInfo(MessageConstants.PARSING_CARD_DATA);

				IEnumerable<Card> cards;

				if (File.Exists(FileConstants.CARD_DATA_FILEPATH))
				{
					cards = 
						JsonConvert
							.DeserializeObject<List<Card>>
							(
								File.ReadAllText(FileConstants.CARD_DATA_FILEPATH)
							);
				}
				else
				{
					string cardListHTML = cardListResponse.Result.Content.ReadAsStringAsync().Result;
					IEnumerable<Task<Card>> cardTasks = ParseCardData(cardListHTML);

					Task<Card>[] cardTaskArray = cardTasks?.ToArray();
					Task.WaitAll(cardTaskArray, -1);

					cards = cardTaskArray
						.Select(task => task.Result)
						.Where(card => card != null);

					//Cache the card data in flat-file format so we don't have to web scrape it next time
					File.WriteAllText
					(
						FileConstants.CARD_DATA_FILEPATH,
						JsonConvert.SerializeObject(cards.ToList())
					);
				}

				if (cards.Count() != DataLoaderConstants.TOTAL_CARD_COUNT)
				{
					throw new Exception(MessageConstants.CARD_COUNT_MISMATCH);
				}
				else
				{
					Logger.LogInfo(MessageConstants.CARD_RETRIEVAL_SUCCESSFUL);
					return cards;
				}
			}
			catch (Exception)
			{
				Logger.LogError(MessageConstants.CARD_RETRIEVAL_FAILED);
				throw;
			}
		}
		#endregion



		#region Override(s)
		public override int ExpectedRecordCount => DataLoaderConstants.TOTAL_CARD_COUNT;
		#endregion



		#region Private Methods        
		private IEnumerable<Task<Card>> ParseCardData(string cardDataHtml)
		{
			try
			{
				//Load the HTML Document
				HtmlDocument document = new HtmlDocument();
				document.LoadHtml(cardDataHtml);

				if (document == null || document.DocumentNode == null)
				{
					throw new Exception(MessageConstants.CARD_DATA_DOM_ERROR);
				}

				//Get the table elements from the DOM corresponding to the card list (the list is split into two tables because fuck you)
				IEnumerable<Task<Card>> cardList =
					document.DocumentNode.Descendants()
						.Where(node => node.Name == "table" && node.HasClass("card-list"))
						.Select
						(
							//For each table found, get the data rows corresponding to each individual card
							table =>
								table.Descendants()
									.Where
									(
										td => td.Name == "tr" && td.ParentNode.Name != "thead"
									)
						)
						.Aggregate
						(
							//Since there are two seperate collections of table row nodes (from the two initial card tables), 
							//we need to aggregate them into a single, cohesive collection of table row nodes.
							//Once this is done, Aggregate() also lets us specify a selector function to project the rows 
							//into a new collection.  We can use this to build our card object for each row.
							new List<HtmlNode>(),
							(accumulator, tableRows) =>
							{
								accumulator.AddRange(tableRows);
								return accumulator;
							},
							rows => rows.Select
							(
								async row =>
								{
									Card card = await BuildCardObject(row);
									if (card != null)
									{
										Logger.LogVerbose
										(
											string.Format
											(
												MessageConstants.CARD_LOADED_TEMPLATE,
												card.Name,
												card.CardId.ToString("000")
											)
										);
									}
									return card;
								}
							)
						);

				return cardList;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format(MessageConstants.CARD_SITE_PARSING_ERROR_TEMPLATE, ex.Message));
			}
		}


		private async Task<Card> BuildCardObject(HtmlNode rowNode)
		{
			string cardDetailRelativePath = null;

			try
			{
				//Load the fields for each data row
				HtmlNode[] data = rowNode.ChildNodes.AsEnumerable().ToArray();

				//Determine Card Type
				CardType cardType = CardType.Unknown;
				Enum.TryParse(data[2].InnerText, out cardType);

				if (cardType == CardType.Unknown)
				{
					throw new FormatException("Unable to parse card from provided row data.");
				}

				//Determine Monster Type
				MonsterType monsterType = MonsterType.Unknown;
				Enum.TryParse(data[3].InnerText.Replace(" ", "").Replace("-", ""), out monsterType);

				cardDetailRelativePath = data[1].FirstChild.GetAttributeValue("href", null);

				HttpResponseMessage cardDetailResponse = await GetRemoteContentAsync(cardDetailRelativePath);
				string cardDetailHTML = await cardDetailResponse.Content.ReadAsStringAsync();

				//Parse Necessary info from the card's detail HTML
				string cardDescription = ParseCardDescription(cardDetailHTML);

				IEnumerable<GuardianStar?> guardianStars = null;

				if (monsterType != MonsterType.Unknown)
				{
					guardianStars = ParseGuardianStars(cardDetailHTML);
				}

				//Build the new card object
				return new Card()
				{
					CardId = int.Parse(data[0].InnerText),
					Name = HttpUtility.HtmlDecode(data[1].FirstChild.InnerText),
					CardType = cardType,
					MonsterType = monsterType == MonsterType.Unknown ? (MonsterType?)null : monsterType,
					Description = cardDescription,
					FirstGuardianStar = guardianStars?.FirstOrDefault(),
					SecondGuardianStar = guardianStars?.Skip(1).FirstOrDefault(),
					Level = monsterType == MonsterType.Unknown ? (int?)null : int.Parse(data[4].InnerText),
					AttackPoints = monsterType == MonsterType.Unknown ? (int?)null : int.Parse(data[5].InnerText),
					DefensePoints = monsterType == MonsterType.Unknown ? (int?)null : int.Parse(data[6].InnerText),
					Password = string.IsNullOrEmpty(data[7].InnerText) ? null : data[7].InnerText,
					StarchipCost = string.IsNullOrEmpty(data[8].InnerText) ? (int?)null : int.Parse(data[8].InnerText, NumberStyles.Any)
				};
			}
			catch (Exception ex)
			{
				//If we failed to parse data for an individual card, return null (rather than bubbling-up the exception)
				//At the program level, we will check to make sure we have the correct number of non-null card objects.
				if (cardDetailRelativePath != null && cardDetailRelativePath.Length >= 7)
				{
					Logger.LogError
					(
						string.Format
						(
							MessageConstants.CARD_OBJECT_CONSTRUCTION_ERROR_TEMPLATE,
							cardDetailRelativePath?.Substring(6)
						)
					);
				}
				else
				{
					Logger.LogError(MessageConstants.UNKNOWN_CARD_OBJECT_CONSTRUCTION_ERROR);
				}

				Logger.LogError(ex.Message);
				return null;
			}
		}


		private static string ParseCardDescription(string cardDetailHTML)
		{
			try
			{
				//Load the HTML Document
				HtmlDocument document = new HtmlDocument();
				document.LoadHtml(cardDetailHTML);

				//Find the first div with the 'lore' class.  
				//It's child should be a paragraph tag containing the card's description
				return HttpUtility.HtmlDecode
				(
					document.DocumentNode.Descendants()
						.Where(node => node.Name == "div" && node.GetAttributeValue("class", null) == "lore")
						.First()
						.InnerText
						.Replace("\n", "")
				);
			}
			catch (Exception ex)
			{
				Logger.LogError
				(
					string.Format
					(
						MessageConstants.CARD_DESCRIPTION_RETRIEVAL_ERROR_TEMPLATE,
						ex.Message
					)
				);
				return null;
			}
		}


		private static IEnumerable<GuardianStar?> ParseGuardianStars(string cardDetailHTML)
		{
			//Load the HTML Document
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(cardDetailHTML);

			//Find and parse the nodes containing the guardian star text
			IEnumerable<GuardianStar?> guardianStars =
				document.DocumentNode.Descendants()
					.Where(node => node.Name == "table" && node.GetAttributeValue("class", null) == "innertable")
					.First()
					.LastChild
					.ChildNodes[1]
					.ChildNodes[2]
					.ChildNodes[1]
					.ChildNodes[1]
					.InnerText
					.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
					.Select
					(
						guardianStarText =>
						{
							//Build a regex to cleanse non-alphabetical characters from the HTML text
							Regex alphaRegex = new Regex(RegexConstants.ALPHA_REGEX);

							string cleansedText =
								alphaRegex.Replace(guardianStarText, string.Empty);

							//Attempt to parse an enum value from the cleansed guardian star text
							GuardianStar star = GuardianStar.Unknown;
							Enum.TryParse(cleansedText, out star);

							return star == GuardianStar.Unknown ? null : (GuardianStar?)star;
						}
					);

			return guardianStars;
		}
		#endregion
	}
}
