using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FMDC.DataLoader.Implementations
{
	public class CharacterImageDataLoader : DataLoader<GameImage>
	{
		#region Class-Specific Constant(s)
		private static readonly string CHARACTER_IMAGE_ROOTED_DIRECTORY =
			$"{ApplicationConstants.APPLICATION_DATA_FOLDER}" +
			$"{ApplicationConstants.CHARACTER_IMAGE_SUBDIRECTORY}";
		#endregion



		#region Constructor(s)
		public CharacterImageDataLoader() : base(URLConstants.YUGIPEDIA_IMAGE_URL)
		{

		}
		#endregion



		#region Abstract Base Class Implementations
		public override Func<GameImage, int> KeySelector => gameImage => gameImage.GameImageId;

		public override IEnumerable<GameImage> LoadDataIntoMemory()
		{
			try
			{
				if (ActualRecordCount == ExpectedRecordCount)
				{
					//If the correct count of character image records has already been loaded into the  
					//database, skip the entire data load process and return the entities from the database.
					LoggingUtility.LogInfo(MessageConstants.CHARACTER_IMAGE_LOADING_SKIPPED);

					return
						_cardRepository
							.RetrieveEntities<GameImage>
							(
								entity => entity.EntityType == ImageEntityType.Character
							);

				}

				LoggingUtility.LogInfo(MessageConstants.LOADING_CHARACTER_IMAGES);

				IEnumerable<CharacterLoadingInfo> imageRelativePaths = ReadCharacterImageInfo();

				IEnumerable<Task<GameImage>> characterImageTasks =
					imageRelativePaths
						.Select
						(
							async imageInfo =>
								await LoadCharacterImageAsync(imageInfo)
					);

				Task<GameImage>[] characterImageTaskArray = characterImageTasks.ToArray();
				Task.WaitAll(characterImageTaskArray);

				IEnumerable<GameImage> images = characterImageTaskArray
					.Select(task => task.Result)
					.Where(image => image != null);

				if (images.Count() != DataLoaderConstants.TOTAL_CHARACTER_COUNT)
				{
					LoggingUtility.LogWarning(MessageConstants.CHARACTER_IMAGE_COUNT_MISMATCH);
					LoggingUtility.LogWarning(MessageConstants.IMAGE_DISPLAY_WARNING);
				}
				else
				{
					LoggingUtility.LogInfo(MessageConstants.CHARACTER_IMAGE_LOADING_SUCCESSFUL);
				}

				return images;
			}
			catch (Exception ex)
			{
				LoggingUtility.LogWarning(MessageConstants.CHARACTER_IMAGE_RETRIEVAL_FAILURE);
				LoggingUtility.LogWarning(ex.Message);
				LoggingUtility.LogWarning(MessageConstants.IMAGE_DISPLAY_WARNING);
				return null;
			}
		}
		#endregion



		#region Override(s)
		public override int ExpectedRecordCount => DataLoaderConstants.TOTAL_CHARACTER_COUNT;

		public override Func<GameImage, bool> RecordCountPredicate =>
			gameImage => gameImage.EntityType == ImageEntityType.Character;
		#endregion



		#region Private Methods
		private IEnumerable<CharacterLoadingInfo> ReadCharacterImageInfo()
		{
			return
				LoadDataFile(FileConstants.CHARACTER_LOADING_INFO_FILEPATH)
					.Select
					(
						row =>
						{
							string[] dataFields = 
								row
									.Split(',')
									.Select(field => field.Trim())
									.ToArray();

							return new CharacterLoadingInfo()
							{
								CharacterId = int.Parse(dataFields[0]),
								CharacterName = dataFields[1],
								CharacterImagePath = dataFields[4]
							};
						}
					);
		}


		private async Task<GameImage> LoadCharacterImageAsync(CharacterLoadingInfo imageInfo)
		{
			try
			{
				//Build strings for easy reference to the character image
				string characterImageFileName = $"{imageInfo.CharacterName}.png";

				string characterImageRootedFilePath =
					CHARACTER_IMAGE_ROOTED_DIRECTORY + characterImageFileName;

				string characterImageRelativePath =
					ApplicationConstants.CHARACTER_IMAGE_SUBDIRECTORY + characterImageFileName;

				if (!File.Exists(characterImageRootedFilePath))
				{
					//If the image file does not yet exist locally, 
					//scrape it from the web and save it to the file system.
					HttpResponseMessage imageResponse =
						await GetRemoteContentAsync(imageInfo.CharacterImagePath);

					byte[] imageBytes =
						await imageResponse.Content.ReadAsByteArrayAsync();

					SaveCharacterImageFile(imageBytes, characterImageRootedFilePath);
				}

				LoggingUtility.LogVerbose
				(
					string.Format
					(
						MessageConstants.CHARACTER_IMAGE_LOADED_TEMPLATE,
						imageInfo.CharacterName
					)
				);

				return new GameImage()
				{
					EntityType = ImageEntityType.Character,
					EntityId = imageInfo.CharacterId,
					ImageRelativePath = characterImageRelativePath
				};
			}
			catch (Exception ex)
			{
				LoggingUtility.LogError
				(
					string.Format
					(
						MessageConstants.CHARACTER_IMAGE_LOAD_FAILURE_TEMPLATE,
						imageInfo.CharacterName
					)
				);
				LoggingUtility.LogError(ex.Message);

				return null;
			}
		}


		private void SaveCharacterImageFile
		(
			byte[] imageBytes, 
			string imageFilePath
		)
		{
			//Create the directory for character images (if it doesn't already exist)
			Directory.CreateDirectory(CHARACTER_IMAGE_ROOTED_DIRECTORY);

			//Create an image file for the character and save it to the file ssytem
			File.WriteAllBytes(imageFilePath, imageBytes);
		}
		#endregion
	}
}
