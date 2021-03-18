using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp.Assets.Core.DataManager.Interface.ModelRecycler;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler.Entities;

namespace AssemblyCSharp.Assets.Core.DataManager.Impl.ModelRecycler
{
    public class ModelRecycler : IModelRecycler
    {
        private readonly Dictionary<int, Queue<GameObject>> _generalRecycler = new Dictionary<int, Queue<GameObject>>();
        private readonly Dictionary<string, Queue<GameObject>> _modelRecycler = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, SkinnedMeshRenderer[]> _renderers = new Dictionary<string, SkinnedMeshRenderer[]>();

        public void CreateRecycler()
        {
            _generalRecycler.Add((int)RecyclerKeys.DestructionParticles, new Queue<GameObject>());
            _generalRecycler.Add((int)RecyclerKeys.SetCard, new Queue<GameObject>());
        }

        public void AddToQueue(int key, GameObject model)
        {
            _generalRecycler[key].Enqueue(model);
            model.SetActive(false);
        }

        public GameObject UseFromQueue(int key, Vector3 position, Quaternion rotation, Transform parent)
        {
            var model = _generalRecycler[key].Dequeue();
            model.transform.SetPositionAndRotation(position, rotation);
            model.transform.parent = parent;
            model.SetActive(true);
            return model;
        }
        public GameObject UseFromQueue(int key, 
                                       Vector3 position, 
                                       Quaternion rotation, 
                                       Transform parent, 
                                       SkinnedMeshRenderer meshToDestroy)
        {
            var model = _generalRecycler[key].Dequeue();
            var mesh = model.GetComponent<ISetMeshCharacter>();
            mesh.GetCharacterMesh(meshToDestroy);
            model.transform.SetPositionAndRotation(position, rotation);
            model.transform.parent = parent;
            model.SetActive(true);
            return model;
        }
        public GameObject UseFromQueue(int key, Transform parent)
        {
            var model = _generalRecycler[key].Dequeue();
            model.transform.parent = parent;
            model.SetActive(true);
            return model;
        }

        public SkinnedMeshRenderer[] GetMeshRenderers(string key, GameObject obj)
        {
            bool modelExists = _renderers.TryGetValue(key, out var renderers);
            if (!modelExists)
            {
                renderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                _renderers.Add(key, renderers);
                return renderers;
            }

            return renderers;
        }

        public bool CheckForExistingModel(string key)
        {
            bool modelExists = _modelRecycler.TryGetValue(key, out _);
            if (!modelExists) 
            {
                return false;
            }

            return _modelRecycler.ContainsKey(key);
        }

        public GameObject GetExistingModel(string key, Transform parent)
        {
            var model = _modelRecycler[key].Dequeue();
            var renderers = GetMeshRenderers(key, model);

            foreach (SkinnedMeshRenderer item in renderers)
            {
                item.enabled = true;
            }

            model.transform.parent = parent;
            model.SetActive(true);
            return model;
        }
        
        public void RecycleModel(string key, GameObject model)
        {
            if(!_modelRecycler.ContainsKey(key))
            {
                _modelRecycler.Add(key, new Queue<GameObject>());
            }

            _modelRecycler[key].Enqueue(model);
            model.SetActive(false);
        }
    }
}
