﻿using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Utility;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace FMDC.DataLoader.Implementations
{
	public class CardImageDataLoader : DataLoader<GameImage>
	{
		#region Constructor
		public CardImageDataLoader() : base(URLConstants.YUGIPEDIA_URL)
		{

		}
		#endregion



		#region Abstract Base Class Implementations
		public override IEnumerable<GameImage> LoadDataIntoMemory()
		{
			Task<HttpResponseMessage> cardGalleryResponse = null;

			//Retrieve the HTML for the card gallery
			try
			{
				LoggingUtility.LogInfo(MessageConstants.BEGIN_CARD_IMAGE_LOADING);

				cardGalleryResponse = GetRemoteContentAsync(URLConstants.CARD_GALLERY_PATH);
				cardGalleryResponse.Wait();
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError(MessageConstants.CARD_IMAGE_REPO_ACCESS_FAILURE);
				throw ex;
			}

			try
			{
				LoggingUtility.LogInfo(MessageConstants.PARSING_IMAGE_DATA);

				//Parse the HTML to get the URLs for each card's images.  Each card parsed should return two images--
				//The card image itself, and the image representing the card's description
				string cardGalleryHTML = cardGalleryResponse.Result.Content.ReadAsStringAsync().Result;
				IEnumerable<Task<IEnumerable<GameImage>>> cardImagesTasks = ParseCardImageData(cardGalleryHTML);

				//Each pair of card images represents a new HTTP request that needs to be awaited
				//before the returned image data can be stored.  We can begin making these calls
				//asynchronously, but must await completion of all of them before they can be loaded
				//into a single collection
				Task<IEnumerable<GameImage>>[] cardImagesTaskArray = cardImagesTasks.ToArray();
				Task.WaitAll(cardImagesTaskArray, -1);

				//Once each of the tasks completes, what we have is a collection of images for each card
				//(containing the card's image and description image).  These collections should then
				//be coalesced into a single collection of images
				IEnumerable<GameImage> images = cardImagesTaskArray
					.SelectMany(task => task.Result)
					.Where(image => image != null);

				//Each card should have yielded two images.  If not, display a warning that not all images loaded correctly
				if (images.Count() != DataLoaderConstants.TOTAL_CARD_COUNT * 2)
				{
					LoggingUtility.LogWarning(MessageConstants.CARD_IMAGE_COUNT_MISMATCH);
					LoggingUtility.LogWarning(MessageConstants.IMAGE_DISPLAY_WARNING);
				}
				else
				{
					LoggingUtility.LogInfo(MessageConstants.CARD_IMAGE_LOADING_SUCCESSFUL);
				}

				return images;
			}
			catch (Exception ex)
			{
				LoggingUtility.LogWarning(MessageConstants.CARD_IMAGE_RETRIEVAL_FAILURE);
				LoggingUtility.LogWarning(ex.Message);
				LoggingUtility.LogWarning(MessageConstants.IMAGE_DISPLAY_WARNING);
				return null;
			}
		}


		public override int LoadDataIntoDatabase(IEnumerable<GameImage> data)
		{
			throw new NotImplementedException();
		}
		#endregion



		#region Private Methods
		private IEnumerable<Task<IEnumerable<GameImage>>> ParseCardImageData(string galleryHTML)
		{
			try
			{
				//Load the HTML Document
				HtmlDocument document = new HtmlDocument();
				document.LoadHtml(galleryHTML);

				if (document == null || document.DocumentNode == null)
				{
					throw new Exception(MessageConstants.CARD_IMAGE_DOM_ERROR);
				}

				//Get all image elements in the gallery whose parent has the 'image' class on it.
				//These correspond to the gallery thumbnails of each card
				IEnumerable<Task<IEnumerable<GameImage>>> cardImages = document.DocumentNode.Descendants()
					.Where
					(
						node =>
							node.Ancestors().Where(ancestor => ancestor.HasClass("gallery")).Any() &&
							node.Name == "img" &&
							node.ParentNode.HasClass("image")
					)
					.Select(async node => await ParseImageNode(node));

				return cardImages;
			}
			catch (Exception ex)
			{
				throw new Exception
				(
					string.Format
					(
						MessageConstants.CARD_GALLERY_PARSING_ERROR_TEMPLATE,
						ex.Message
					)
				);
			}
		}


		private async Task<IEnumerable<GameImage>> ParseImageNode(HtmlNode node)
		{
			string cardName = "";

			try
			{
				//Clean the image url to get the non-thumbnail version.  Remove '/thumb/' from the route
				//and remove additional file info after the image extension
				string imgURL = node.GetAttributeValue("src", null).Replace("/thumb/", "");
				imgURL = imgURL.Substring(0, imgURL.IndexOf(".png") + 4);

				//Traverse the DOM from the image element to get to the node containing the card's identifying information
				//From here, we can get the card's id (number) and its name.
				HtmlNode imageInfoNode = node.ParentNode.ParentNode.ParentNode.ParentNode.ChildNodes[3].ChildNodes[1];
				int cardId = int.Parse(imageInfoNode.FirstChild.InnerText.Substring(1), NumberStyles.Any);
				cardName = imageInfoNode.ChildNodes[3].InnerText.Replace("&amp;", "&");

				//Make a request to get the image data from the source of the image node
				HttpResponseMessage imageResponse = await GetRemoteContentAsync(imgURL);
				byte[] imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();

				//Build the two images for the respective card
				IEnumerable<GameImage> cardImages = BuildCardImages(cardId, cardName, imageBytes);

				LoggingUtility.LogVerbose
				(
					string.Format
					(
						MessageConstants.CARD_IMAGE_LOADED_TEMPLATE,
						cardName,
						cardId.ToString("000")
					)
				);

				return cardImages;
			}
			catch (Exception ex)
			{
				//If we failed to parse an individual card image, return null (rather than bubbling-up the exception)
				//At the program level, we will check to make sure we have the correct number of non-null card images.
				if (!string.IsNullOrEmpty(cardName))
				{
					LoggingUtility.LogError
					(
						string.Format
						(
							MessageConstants.CARD_IMAGE_LOAD_FAILURE_TEMPLATE,
							cardName
						)
					);
				}
				else
				{
					LoggingUtility.LogError(MessageConstants.UNKNOWN_CARD_IMAGE_LOAD_FAILURE);
				}

				LoggingUtility.LogError(ex.Message);
				return null;
			}
		}


		private IEnumerable<GameImage> BuildCardImages(int cardId, string cardName, byte[] cardDetailsImageBytes)
		{
			//Grab subset of card details image to get just the thumbnail of the card itself 
			Bitmap cardDetailsBitmap = cardDetailsImageBytes.ConvertToBitmap();

			Rectangle cardThumbnailRect = new Rectangle(3, 5, 138, 194);

			Bitmap cardThumbnailBitmap = 
				cardDetailsBitmap.Clone(cardThumbnailRect, cardDetailsBitmap.PixelFormat);

			//Create the directories for the card images (if they don't already exist)
			string detailImageRootedDirectory =
				$"{ApplicationConstants.APPLICATION_DATA_FOLDER}" +
				$"{ApplicationConstants.DETAIL_IMAGE_SUBDIRECTORY}";

			string thumbnailImageRootedDirectory =
				$"{ApplicationConstants.APPLICATION_DATA_FOLDER}" +
				$"{ApplicationConstants.THUMBNAIL_IMAGE_SUBDIRECTORY}";

			Directory.CreateDirectory(detailImageRootedDirectory);
			Directory.CreateDirectory(thumbnailImageRootedDirectory);

			//Build the file paths for the images being generated from the card's name and id
			string cardFileName = $"{cardId}_{cardName}.png";

			string cardDetailsRelativePath = 
				$"{ApplicationConstants.DETAIL_IMAGE_SUBDIRECTORY}{cardFileName}";

			string cardThumbnailRelativePath = 
				$"{ApplicationConstants.THUMBNAIL_IMAGE_SUBDIRECTORY}{cardFileName}";

			//Create an image file for both the card information and card thumbnail
			File.WriteAllBytes
			(
				$"{detailImageRootedDirectory}{cardFileName}", 
				cardDetailsImageBytes
			);

			File.WriteAllBytes
			(
				$"{thumbnailImageRootedDirectory}{cardFileName}",
				cardThumbnailBitmap.ConvertToByteArray()
			);

			return
				new List<GameImage>
				{
					//Card Details Image
					new GameImage()
					{
						EntityType = ImageEntityType.CardDetails,
						EntityId = cardId,
						ImageRelativePath = cardDetailsRelativePath
					},

					//Card Image
					new GameImage()
					{
						EntityType = ImageEntityType.Card,
						EntityId = cardId,
						ImageRelativePath = cardThumbnailRelativePath
					}
				};
		}
		#endregion
	}
}
