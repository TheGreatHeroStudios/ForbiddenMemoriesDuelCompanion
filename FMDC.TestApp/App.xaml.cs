using FMDC.BusinessLogic;
using FMDC.Model;
using FMDC.Model.Enums;
using FMDC.Model.Models;
using FMDC.Persistence;
using FMDC.TestApp.Pages;
using FMDC.TestApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TGH.Common.Extensions;
using TGH.Common.Patterns.IoC;
using TGH.Common.Persistence.Interfaces;
using TGH.Common.Repository.Implementations;
using TGH.Common.Repository.Interfaces;

namespace FMDC.TestApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		#region Non-Public Member(s)
		private IGenericRepository _cardRepository;
		private List<Card> _cardList;
		private List<Fusion> _fusionList;
		private List<Equippable> _equippableList;
		private List<GameImage> _gameImages;
		#endregion



		#region Constructor(s)
		public App()
		{
			RegisterDependencies();
			LoadCardData();
		}
		#endregion



		#region Public Propertie(s)
		public List<Card> CardList => _cardList;
		public List<Fusion> FusionList => _fusionList;
		public List<Equippable> EquippableList => _equippableList;
		public List<GameImage> GameImages => _gameImages;
		#endregion



		#region Non-Public Method(s)
		private void RegisterDependencies()
		{
			//Register Persistence Layer Type(s)
			DependencyManager
				.RegisterService<IDatabaseContext, ForbiddenMemoriesDbContext>
				(
					() =>
						new ForbiddenMemoriesDbContext
						(
							PersistenceConstants.SQLITE_DB_TARGET_FILEPATH
						),
					ServiceScope.Singleton
				);

			//Register Repository Layer Type(s)
			DependencyManager
				.RegisterService<IGenericRepository, GenericRepository>
				(
					ServiceScope.Singleton
				);

			//Register Service Layer Type(s)
			DependencyManager
				.RegisterService<IFusionService, FusionService>
				(
					() => new FusionService(FusionList),
					ServiceScope.Singleton
				);

			//Register Application
			DependencyManager
				.RegisterService<App, App>
				(
					() => (App)Current,
					ServiceScope.Singleton
				);

			//Register View Model Layer Type(s)
			DependencyManager
				.RegisterService<TrunkViewModel, TrunkViewModel>
				(
					ServiceScope.Singleton
				);

			DependencyManager
				.RegisterService<PlayOptimizerViewModel, PlayOptimizerViewModel>
				(
					ServiceScope.Singleton
				);

			//Register View Layer Type(s)
			DependencyManager
				.RegisterService<Trunk, Trunk>
				(
					ServiceScope.Singleton
				);

			DependencyManager
				.RegisterService<PlayOptimizer, PlayOptimizer>
				(
					ServiceScope.Singleton
				);
		}


		private void LoadCardData()
		{
			//Retrieve a repository instance from the dependency manager 
			//which will be used to load card data from the database.
			_cardRepository =
				DependencyManager.ResolveService<IGenericRepository>
				(
					ServiceScope.Singleton
				);

			//Retrieve card images and secondary types
			//which will be associated to the card list
			_gameImages =
				_cardRepository
					.RetrieveEntities<GameImage>
					(
						gameImage => true
					)
					.ToList();

			IEnumerable<SecondaryType> secondaryTypes =
				_cardRepository
					.RetrieveEntities<SecondaryType>
					(
						secondaryType => true
					);

			//Initialize the card list and add a placeholder for no selection.
			_cardList = new List<Card>();
			_cardList.Add
			(
				new Card
				{
					CardId = -1,
					Name = "None"
				}
			);

			//Join the card images and secondary types to their appropriate card
			//and use the retrieved list of cards to hydrate the in-memory collection 
			_cardList.AddRange
			(
				_cardRepository
					.RetrieveEntities<Card>(card => true)
					.Join
					(
						_gameImages
							.Where
							(
								gameImage =>
									gameImage.EntityType == ImageEntityType.Card
							),
						card => card.CardImageId,
						thumbnail => thumbnail.GameImageId,
						(card, thumbnail) =>
						{
							card.CardImage = thumbnail;
							return card;
						}
					)
					.Join
					(
						_gameImages
							.Where
							(
								gameImage =>
									gameImage.EntityType == ImageEntityType.CardDetails
							),
						card => card.CardDescriptionImageId,
						thumbnail => thumbnail.GameImageId,
						(card, detailImage) =>
						{
							card.CardDescriptionImage = detailImage;
							return card;
						}
					)
					.LeftJoin
					(
						secondaryTypes
							.GroupBy
							(
								secondaryType =>
									secondaryType.CardId
							),
						card => card.CardId,
						secondaryTypes => secondaryTypes.Key,
						(card, secondaryTypeList) =>
						{
							card.SecondaryTypes = secondaryTypeList?.ToList();
							return card;
						}
					)
					.OrderBy(card => card.Name)
					.ToList()
			);

			//Load the list of available fusions and associate each fusion with its resultant card
			_fusionList =
				_cardRepository
					.RetrieveEntities<Fusion>(fusion => true)
					.Join
					(
						_cardList,
						fusion => fusion.ResultantCardId,
						resultantCard => resultantCard.CardId,
						(fusion, resultantCard) =>
						{
							fusion.ResultantCard = resultantCard;
							return fusion;
						}
					)
					.ToList();

			//Load the list of equipment and its associated cards
			_equippableList =
				_cardRepository
					.RetrieveEntities<Equippable>(equippable => true)
					.Join
					(
						_cardList,
						equippable => equippable.EquipCardId,
						equipCard => equipCard.CardId,
						(equippable, equipCard) =>
						{
							equippable.EquipCard = equipCard;
							return equippable;
						}
					)
					.Join
					(
						_cardList,
						equippable => equippable.TargetCardId,
						targetCard => targetCard.CardId,
						(equippable, targetCard) =>
						{
							equippable.TargetCard = targetCard;
							return equippable;
						}
					)
					.ToList();
		}
		#endregion
	}
}
