using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler
{
    public interface IModelRecycler
    {
        public bool IsGameObjectRecyclable(string key);
        public bool IsPlayfieldRecyclable();
        public void AddGameObjectToQueue(string key, UnityEngine.GameObject model);
        public UnityEngine.GameObject GetGameObjectFromQueue(string key, Vector3 position, Quaternion rotation, Transform parent);
        public void RemoveGameObjectQueue(string key);
    }
}
