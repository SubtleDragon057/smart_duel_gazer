using Zenject;
using UnityEngine;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.UIComponents.Constants;
using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class SetCard : MonoBehaviour
    {
        [SerializeField]
        private Renderer _image;
        [SerializeField]
        private List<Texture> _errorImages = new List<Texture>();

        private IDataManager _dataManager;
        private ModelEventHandler _modelEventHandler;

        private Animator _animator;
        private string _zone;
        private CurrentState currentState;

        #region Constructors

        [Inject]
        public void Construct(IDataManager dataManager,
                              ModelEventHandler modelEventHandler)
        {
            _dataManager = dataManager;
            _modelEventHandler = modelEventHandler;
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            _zone = null;
            currentState = CurrentState.FaceDown;
            _animator.SetBool(AnimatorParameters.RemoveCardBool, false);

            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subcriptions

        private void SubscribeToEvents()
        {
            _modelEventHandler.OnSummonSetCard += OnSummonEvent;

            _modelEventHandler.OnSpellTrapActivate += OnSpellTrapActivate;
            _modelEventHandler.OnReturnToFaceDown += OnReturnToFaceDown;
            _modelEventHandler.OnSetCardRemove += OnSpellTrapRemove;
            _modelEventHandler.OnRevealSetMonster += SetMonsterEvent;
            _modelEventHandler.OnDestroySetMonster += DestroySetMonster;

            _modelEventHandler.OnActivatePlayfield += ActivatePlayfield;
            _modelEventHandler.OnPickupPlayfield += PickupPlayfield;
        }

        private void UnsubscribeFromEvents()
        {
            _modelEventHandler.OnSummonSetCard -= OnSummonEvent;

            _modelEventHandler.OnSpellTrapActivate -= OnSpellTrapActivate;
            _modelEventHandler.OnReturnToFaceDown -= OnReturnToFaceDown;
            _modelEventHandler.OnSetCardRemove -= OnSpellTrapRemove;
            _modelEventHandler.OnRevealSetMonster -= SetMonsterEvent;
            _modelEventHandler.OnDestroySetMonster -= DestroySetMonster;
            _modelEventHandler.OnChangeMonsterVisibility -= HideSetCardImage;

            _modelEventHandler.OnActivatePlayfield -= ActivatePlayfield;
            _modelEventHandler.OnPickupPlayfield -= PickupPlayfield;
        }

        #endregion

        private async void OnSummonEvent(string zone, string modelName, bool isSetMonster)
        {
            if (transform.gameObject.activeSelf == false)
            {
                return;
            }

            if (isSetMonster)
            {
                SetMonster();
            }

            _zone = zone;
            _modelEventHandler.OnSummonSetCard -= OnSummonEvent;
            
            await GetAndDisplayCardImage(modelName);
            _modelEventHandler.OnChangeMonsterVisibility += HideSetCardImage;
        }

        #region Playfield Events

        private void ActivatePlayfield(GameObject playfield)
        {
            switch (currentState) 
            {
                case CurrentState.FaceDown:
                    _animator.SetTrigger(AnimatorParameters.FadeInSetCardTrigger);
                    break;
                case CurrentState.SpellActivated:
                    _animator.SetTrigger(AnimatorParameters.ActivateSpellOrTrapTrigger);
                    break;
                case CurrentState.SetMonsterRevealed:
                    _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
                    break;
            }
        }

        private void PickupPlayfield()
        {
            _animator.SetTrigger(AnimatorParameters.RemoveSetCardTrigger);
        }

        #endregion

        #region Spell/Trap Events

        private void OnSpellTrapActivate(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.ActivateSpellOrTrapTrigger);
            currentState = CurrentState.SpellActivated;
        }

        private void OnReturnToFaceDown(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.ReturnSpellTrapToFaceDown);
            currentState = CurrentState.FaceDown;
        }

        private void OnSpellTrapRemove(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.RemoveSetCardTrigger);
        }

        #endregion

        #region Set Monster Events

        private void SetMonster()
        {           
            transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
        
        private void SetMonsterEvent(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
            currentState = CurrentState.SetMonsterRevealed;
        }

        private void HideSetCardImage(string zone, bool state)
        {
            if (_zone == zone && state == false)
            {
                _animator.SetTrigger(AnimatorParameters.HideSetMonsterImageTrigger);
                currentState = CurrentState.FaceDown;
            }
        }

        private void DestroySetMonster(string zone)
        {
            if (_zone != zone)
            {
                return;
            }

            _animator.SetTrigger(AnimatorParameters.RevealSetMonsterTrigger);
            _animator.SetBool(AnimatorParameters.RemoveCardBool, true);
        }

        #endregion

        #region Card Image

        private async Task GetAndDisplayCardImage(string cardId)
        {
            var image = await _dataManager.GetCardImage(cardId.RemoveCloneSuffix());
            if (image == null)
            {
                SetRandomErrorImage();
                return;
            }

            _image.material.SetTexture("_MainTex", image);
        }

        private void SetRandomErrorImage()
        {
            var randomNum = Random.Range(0, _errorImages.Count);
            _image.material.SetTexture("_MainTex", _errorImages[randomNum]);
        }

        #endregion

        public class Factory : PlaceholderFactory<GameObject, SetCard>
        {
        }

        public enum CurrentState
        {
            FaceDown,
            SpellActivated,
            SetMonsterRevealed,
        }
    }
}
