using Zenject;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts
{
    public class DestructionParticles : MonoBehaviour
    {
        private ModelEventHandler _eventHandler;
        private ParticleSystem _particleSystem;

        #region Constructor

        [Inject]
        public void Construct(ModelEventHandler modelEventHandler)
        {
            _eventHandler = modelEventHandler;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            _eventHandler.OnMonsterRemoval += OnMonsterDestruction;
        }

        #endregion

        private void OnMonsterDestruction(SkinnedMeshRenderer[] renderers)
        {
            if(renderers == null)
            {
                return;
            }

            SetMeshShape(renderers[0]);
            _particleSystem.Play();
            _eventHandler.OnMonsterRemoval -= OnMonsterDestruction;
        }

        private void SetMeshShape(SkinnedMeshRenderer _skinnedMesh)
        {
            var shape = _particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            shape.skinnedMeshRenderer = _skinnedMesh;
        }

        public class Factory : PlaceholderFactory<GameObject, DestructionParticles>
        {
        }
    }
}
