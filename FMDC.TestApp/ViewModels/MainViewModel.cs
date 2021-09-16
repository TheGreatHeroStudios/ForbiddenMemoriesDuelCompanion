using FMDC.Model;
using FMDC.Model.Models;
using FMDC.TestApp.Enums;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FMDC.TestApp.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		#region Public Propert(ies)
		private FeatureSelection _currentFeature;
		public FeatureSelection CurrentFeature
		{
			get => _currentFeature;
			set
			{
				_currentFeature = value;
				RaisePropertyChanged(nameof(CurrentFeature));
			}
		}
		#endregion



		#region 'INotifyPropertyChanged' Implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion



		#region Public Method(s)
		public void SaveCardConfiguration
		(
			IEnumerable<CardCount> cardCounts,
			IEnumerable<Card> cardData
		)
		{
			//Serialize the supplied card counts to JSON
			string cardConfigurationJson =
				JsonConvert
					.SerializeObject
					(
						cardCounts,
						Formatting.Indented,
						new JsonSerializerSettings
						{
							ReferenceLoopHandling = ReferenceLoopHandling.Ignore
						}
					);

			//Open a save dialog to save the serialized card counts
			SaveFileDialog saveDialog = new SaveFileDialog();
			saveDialog.AddExtension = true;
			saveDialog.DefaultExt = "*.fmdc";
			saveDialog.Filter = "Forbidden Memories Duel Companion Card Configuration (*.fmdc)|*.fmdc";
			saveDialog.InitialDirectory = ApplicationConstants.APPLICATION_DATA_FOLDER;

			if (saveDialog.ShowDialog() ?? false)
			{
				File.WriteAllText(saveDialog.FileName, cardConfigurationJson);
			}
		}


		public IEnumerable<CardCount> LoadCardConfiguration(IEnumerable<Card> cardData)
		{
			//Open a save dialog to save the serialized card counts
			OpenFileDialog openDialog = new OpenFileDialog();
			openDialog.AddExtension = true;
			openDialog.DefaultExt = "*.fmdc";
			openDialog.Filter = "Forbidden Memories Duel Companion Card Configuration (*.fmdc)|*.fmdc";
			openDialog.InitialDirectory = ApplicationConstants.APPLICATION_DATA_FOLDER;

			if (openDialog.ShowDialog() ?? false)
			{
				//Open and deserialize the file to a collection of card counts
				string cardConfigurationJson = File.ReadAllText(openDialog.FileName);

				List<CardCount> cardCounts =
					JsonConvert.DeserializeObject<List<CardCount>>(cardConfigurationJson);

				return
					cardCounts
						.Join
						(
							cardData,
							cardCount => cardCount.CardId,
							card => card.CardId,
							(cardCount, card) =>
							{
								cardCount.Card = card;
								return cardCount;
							}
						);
			}
			else
			{
				return null;
			}
		}
		#endregion



		#region Private Method(s)
		private void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
