using Zenject;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.UIComponents.Constants;

namespace AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(ModelSettings))]
    public class ModelComponentsManager : MonoBehaviour, IModelComponentsManager
    {
        private ModelEventHandler _eventHandler;

        private Animator _animator;
        private SkinnedMeshRenderer[] _skinnedRenderers;
        private MeshRenderer[] _meshRenderers;
        private ModelSettings _settings;
        private string _zone;
        private bool areRenderersEnabled;

        #region Properties

        public ModelEventHandler EventHandler { get => _eventHandler; }
        public string Zone { get => _zone; }

        #endregion

        #region Constructor

        [Inject]
        public void Construct(ModelEventHandler modelEventHandler)
        {
            _eventHandler = modelEventHandler;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            _skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            if(_skinnedRenderers.Length == 0)
            {
                _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            }

            _settings = GetComponent<ModelSettings>();            
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _zone = null;
            UnsubscribeToEvents();
        }

        #endregion

        #region Event Subscriptions

        public void SubscribeToEvents()
        {
            _eventHandler.OnActivateModel += ActivateModel;
            _eventHandler.OnSummonMonster += SummonMonster;
            _eventHandler.OnRevealSetMonster += RevealSetMonster;
            _eventHandler.OnChangeMonsterVisibility += SetMonsterVisibility;
            _eventHandler.OnDestroyMonster += DestroyMonster;

            _eventHandler.OnFuseMonsters += FuseMonsters;

            _eventHandler.OnActivatePlayfield += ActivatePlayfield;
            _eventHandler.OnPickupPlayfield += PickupPlayfield;
        }

        public void UnsubscribeToEvents()
        {
            _eventHandler.OnActivateModel -= ActivateModel;
            _eventHandler.OnSummonMonster -= SummonMonster;
            _eventHandler.OnRevealSetMonster -= RevealSetMonster;
            _eventHandler.OnChangeMonsterVisibility -= SetMonsterVisibility;
            _eventHandler.OnDestroyMonster -= DestroyMonster;

            _eventHandler.OnFuseMonsters -= FuseMonsters;

            _eventHandler.OnActivatePlayfield -= ActivatePlayfield;
            _eventHandler.OnPickupPlayfield -= PickupPlayfield;
        }

        #endregion

        private void ActivateModel(string zone)
        {
            _zone = zone;
            ScaleModel();

            _eventHandler.OnActivateModel -= ActivateModel;
        }

        private void ScaleModel()
        {
            transform.parent.transform.localScale = _settings.ModelScale;
        }

        private void SummonMonster(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetBool(AnimatorParameters.DefenceBool, false);
            _animator.SetTrigger(AnimatorParameters.SummoningTrigger);

            if(_settings.MonsterType == MonsterType.Fusion)
            {
                _eventHandler.FinishFusion();
            }

            if(_skinnedRenderers.Length == 0)
            {
                _meshRenderers.SetRendererVisibility(true);
                areRenderersEnabled = true;
                return;
            }
            
            _skinnedRenderers.SetRendererVisibility(true);
            areRenderersEnabled = true;
        }

        private void RevealSetMonster(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetBool(AnimatorParameters.DefenceBool, true);
        }

        private void SetMonsterVisibility(string zone, bool state)
        {
            if (zone != _zone)
            {
                return;
            }

            if (_skinnedRenderers.Length == 0)
            {
                _meshRenderers.SetRendererVisibility(state);
                areRenderersEnabled = state;
                return;
            }

            _skinnedRenderers.SetRendererVisibility(state);
            areRenderersEnabled = state;
        }

        private void FuseMonsters(string zone)
        {
            if(_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.FusionTrigger);
        }

        private void DestroyMonster(string zone)
        {
            if (zone != _zone)
            {
                return;
            }

            if (_animator.HasState(0, AnimatorParameters.DeathTrigger))
            {
                _animator.SetTrigger(AnimatorParameters.DeathTrigger);
                return;
            }
            
            ActivateParticlesAndRemoveModel();
        }

        private void ActivateParticlesAndRemoveModel()
        {
            _eventHandler.OnDestroyMonster -= DestroyMonster;

            if (_skinnedRenderers.Length == 0)
            {
                _meshRenderers.SetRendererVisibility(true);
                return;
            }

            _eventHandler.RaiseMonsterRemovalEvent(_skinnedRenderers);
            _skinnedRenderers.SetRendererVisibility(false);
        }

        private void ActivatePlayfield(GameObject playfield)
        {
            if (_skinnedRenderers.Length == 0)
            {
                _meshRenderers.SetRendererVisibility(areRenderersEnabled);
                return;
            }

            _skinnedRenderers.SetRendererVisibility(areRenderersEnabled);
        }
        
        private void PickupPlayfield()
        {
            if (_skinnedRenderers.Length == 0)
            {
                _meshRenderers.SetRendererVisibility(true);
                return;
            }

            _skinnedRenderers.SetRendererVisibility(false);
        }

        public class Factory : PlaceholderFactory<GameObject, ModelComponentsManager>
        {
        }
    }

    public static class ModelComponentUtilities
    {
        public static void SetRendererVisibility(this Renderer[] renderers, bool visibility)
        {
            foreach (Renderer item in renderers)
            {
                item.enabled = visibility;
            }
        }
    }
}
