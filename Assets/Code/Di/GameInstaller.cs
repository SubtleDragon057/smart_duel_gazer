﻿using Zenject;
using AssemblyCSharp.Assets.Code.Core.Dialog.Impl;
using AssemblyCSharp.Assets.Code.Core.Dialog.Interface;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using AssemblyCSharp.Assets.Code.Core.Screen.Impl;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.Navigation.Impl;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Impl;
using AssemblyCSharp.Assets.Code.Core.DataManager.Impl.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Connection;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Impl;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Interface;
using AssemblyCSharp.Assets.Code.Features.Connection.Helpers;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Impl;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardModel;
using AssemblyCSharp.Assets.Code.Core.DataManager.Impl.CardModel;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler;
using AssemblyCSharp.Assets.Core.DataManager.Impl.ModelRecycler;
using AssemblyCSharp.Assets.Code.Core.Models.Impl;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Features.SpeedDuel;

namespace AssemblyCSharp.Assets.Code.Di
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            #region Core

            Container.Bind<IDialogService>().To<DialogService>().AsSingle();
            Container.Bind<IScreenService>().To<ScreenService>().AsSingle();
            Container.Bind<INavigationService>().To<NavigationService>().AsSingle();

            Container.Bind<IDataManager>().To<DataManager>().AsSingle();
            Container.Bind<IConnectionDataManager>().To<ConnectionDataManager>().AsSingle();
            Container.Bind<ICardModelDataManager>().To<CardModelDataManager>().AsSingle();
            Container.Bind<IModelRecycler>().To<ModelRecycler>().AsSingle();
            
            Container.Bind<ISmartDuelServer>().To<SmartDuelServer>().AsSingle();

            Container.Bind<IPlayerPrefsProvider>().To<PlayerPrefsProvider>().AsSingle();
            Container.Bind<IConnectionStorageProvider>().To<ConnectionStorageProvider>().AsSingle();

            Container.Bind<ModelEventHandler>().AsSingle();
            Container.BindFactory<GameObject, ModelComponentsManager, ModelComponentsManager.Factory>()
                .FromFactory<PrefabFactory<ModelComponentsManager>>();

            #endregion

            #region Features

            Container.Bind<ConnectionFormValidators>().AsSingle();
            Container.BindFactory<GameObject, DestructionParticles, DestructionParticles.Factory>()
                .FromFactory<PrefabFactory<DestructionParticles>>();

            #endregion
        }
    }
}
