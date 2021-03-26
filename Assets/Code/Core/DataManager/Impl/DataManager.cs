﻿using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardModel;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities;
using AssemblyCSharp.Assets.Core.DataManager.Interface.ModelRecycler;
using UnityEngine;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl
{
    public class DataManager : IDataManager
    {
        private readonly IConnectionDataManager _connectionDataManager;
        private readonly ICardModelDataManager _cardModelDataManager;
        private readonly IModelRecycler _modelRecycler;

        [Inject]
        public DataManager(
            IConnectionDataManager connectionDataManager,
            ICardModelDataManager cardModelDataManager,
            IModelRecycler modelRecycler)
        {
            _connectionDataManager = connectionDataManager;
            _cardModelDataManager = cardModelDataManager;
            _modelRecycler = modelRecycler;
        }

        #region Connection

        public ConnectionInfo GetConnectionInfo()
        {
            return _connectionDataManager.GetConnectionInfo();
        }

        public void SaveConnectionInfo(ConnectionInfo connectionInfo)
        {
            _connectionDataManager.SaveConnectionInfo(connectionInfo);
        }

        #endregion

        #region CardModel

        public GameObject GetCardModel(string cardId)
        {
            return _cardModelDataManager.GetCardModel(cardId);
        }

        #endregion

        #region ModelRecycler

        public void CreateRecycler()
        {
            _modelRecycler.CreateRecycler();
        }

        public void AddToQueue(int key, GameObject model)
        {
            _modelRecycler.AddToQueue(key, model);
        }

        public GameObject UseFromQueue(int key, Vector3 position, Quaternion rotation)
        {
            return _modelRecycler.UseFromQueue(key, position, rotation);
        }
        public GameObject UseFromQueue(int key,
                                       Vector3 position,
                                       Quaternion rotation,
                                       SkinnedMeshRenderer meshToDestroy)
        {
            return _modelRecycler.UseFromQueue(key, position, rotation, meshToDestroy);
        }
        public GameObject UseFromQueue(int key)
        {
            return _modelRecycler.UseFromQueue(key);
        }

        public SkinnedMeshRenderer[] GetMeshRenderers(string key, GameObject obj)
        {
            return _modelRecycler.GetMeshRenderers(key, obj);
        }

        public bool CheckForExistingModel(string key)
        {
            return _modelRecycler.CheckForExistingModel(key);
        }


        public GameObject GetExistingModel(string key)
        {
            return _modelRecycler.GetExistingModel(key);
        }
        public GameObject GetExistingModel(string key, Transform parent)
        {
            return _modelRecycler.GetExistingModel(key, parent);
        }

        public void RecycleModel(string key, GameObject model)
        {
            _modelRecycler.RecycleModel(key, model);
        }

        public void CacheImage(string key, Texture texture)
        {
            _modelRecycler.CacheImage(key, texture);
        }

        public bool CheckForCachedImage(string key)
        {
            return _modelRecycler.CheckForCachedImage(key);
        }

        public Texture GetCachedImage(string key)
        {
            return _modelRecycler.GetCachedImage(key);
        }

        #endregion
    }
}
