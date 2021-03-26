using UnityEngine;

namespace AssemblyCSharp.Assets.Core.DataManager.Interface.ModelRecycler
{
    public interface IModelRecycler
    {
        public void CreateRecycler();
        public void AddToQueue(int key, GameObject model);
        public GameObject UseFromQueue(int key, Vector3 position, Quaternion rotation);
        public GameObject UseFromQueue(int key,
                                       Vector3 position,
                                       Quaternion rotation,
                                       SkinnedMeshRenderer meshToDestroy);
        public GameObject UseFromQueue(int key);
        public SkinnedMeshRenderer[] GetMeshRenderers(string key, GameObject obj);
        public bool CheckForExistingModel(string key);
        public GameObject GetExistingModel(string key);
        public GameObject GetExistingModel(string key, Transform parent);
        public void RecycleModel(string key, GameObject model);
        public void CacheImage(string key, Texture texture);
        public bool CheckForCachedImage(string key);
        public Texture GetCachedImage(string key);
    }
}
