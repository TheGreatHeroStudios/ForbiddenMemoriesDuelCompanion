using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FMDC.Data.DataLoader.Implementations
{
	public class CharacterImageDataLoader : DataLoader<GameImage>
	{
		#region Constructor
		public CharacterImageDataLoader() : base(URLConstants.YUGIPEDIA_IMAGE_URL)
		{

		}
		#endregion



		#region Abstract Base Class Implementations
		public override IEnumerable<GameImage> LoadDataIntoMemory()
		{
			try
			{
				LoggingUtility.LogInfo(MessageConstants.LOADING_CHARACTER_IMAGES);
				IEnumerable<CharacterLoadingInfo> imageRelativePaths = ReadCharacterImageInfo();

				IEnumerable<Task<GameImage>> characterImageTasks = 
					imageRelativePaths
						.Select
						(
							async imageInfo =>
							{
								try
								{
									HttpResponseMessage imageResponse = await GetRemoteContentAsync(imageInfo.CharacterImagePath);
									string base64 = Convert.ToBase64String(await imageResponse.Content.ReadAsByteArrayAsync());

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
										ImageBase64 = base64
									};
								}
								catch(Exception ex)
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
			catch(Exception ex)
			{
				LoggingUtility.LogWarning(MessageConstants.CHARACTER_IMAGE_RETRIEVAL_FAILURE);
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
		private IEnumerable<CharacterLoadingInfo> ReadCharacterImageInfo()
		{
			return
				LoadDataFile(FileConstants.CHARACTER_LOADING_INFO_FILEPATH)
					.Select
					(
						row =>
						{
							string[] dataFields = row.Split(',').Select(field => field.Trim()).ToArray();
							return new CharacterLoadingInfo()
							{
								CharacterId = int.Parse(dataFields[0]),
								CharacterName = dataFields[1],
								CharacterImagePath = dataFields[4]
							}; 
						}
					);
		}
		#endregion
	}
}
