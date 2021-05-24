using Zenject;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel.EventHandlers
{
    public class SmartDuelEventHandler : MonoBehaviour, ISmartDuelEventListener
    {
        private const float RemoveCardDurationInSeconds = 7;

        private const string PlaymatZonesPath = "Playmat/Zones";
        private const string SetCardKey = "SetCard";
        private const string ParticlesKey = "Particles";
        private const string FusionParticlesKey = "FusionParticles";

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;
        private ModelEventHandler _modelEventHandler;
        private ModelComponentsManager.Factory _modelFactory;

        private GameObject _speedDuelField;
        private bool _isFusionSummon = false;

        #region Properties

        // TODO: work more OO: create PlayMat/Field entity with zones which contain models
        private Dictionary<string, GameObject> InstantiatedModels { get; } = new Dictionary<string, GameObject>();

        #endregion

        #region Constructors

        [Inject]
        public void Construct(
            ISmartDuelServer smartDuelServer,
            IDataManager dataManager,
            IScreenService screenService,
            ModelEventHandler modelEventHandler,
            ModelComponentsManager.Factory modelFactory)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;
            _modelEventHandler = modelEventHandler;
            _modelFactory = modelFactory;

            screenService.UseAutoOrientation();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            _smartDuelServer.Connect(this);
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _modelEventHandler.OnFusionSummon += OnFusionSummon;
            _modelEventHandler.OnFinishFusion += FinishFusion;
        }

        private void OnDestroy()
        {
            _smartDuelServer?.Dispose();
        }

        #endregion

        #region ISmartDuelEventListener

        public void OnSmartDuelEventReceived(SmartDuelEvent smartDuelEvent)
        {
            FetchSpeedDuelField();

            if (_speedDuelField == null)
            {
                Debug.LogWarning("Speed Duel Field isn't placed yet");
                return;
            }

            if (smartDuelEvent is PlayCardEvent playCardEvent)
            {
                OnPlayCardEventReceived(playCardEvent);
            }
            else if (smartDuelEvent is RemoveCardEvent removeCardEvent)
            {
                OnRemoveCardEventReceived(removeCardEvent);
            }
        }

        private void FetchSpeedDuelField()
        {
            if (_speedDuelField == null)
            {
                _speedDuelField = GetComponent<PlacementEventHandler>().SpeedDuelField;
            }
        }

        #region PlayCardEvent

        private void OnPlayCardEventReceived(PlayCardEvent playCardEvent)
        {
            var zone = _speedDuelField.transform.Find($"{PlaymatZonesPath}/{playCardEvent.ZoneName}");
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(playCardEvent.CardId);
            if (cardModel == null)
            {
                // TODO: create a function which returns either an instantiated 3D model of the monster
                // or an instantiated model of the card, and then handle the position
                PlayCardImage(playCardEvent, zone);
                return;
            }

            cardModel.SetActive(true);
            PlayCardModel(playCardEvent, zone, cardModel);
        }

        #region Card image

        // TODO: Test with monster for which there's no 3D model
        private void PlayCardImage(PlayCardEvent playCardEvent, Transform zone)
        {
            var setCardKey = $"{zone.name}:{SetCardKey}";
            if (!InstantiatedModels.ContainsKey(setCardKey))
            {
                var setCard = GetGameObject(SetCardKey, zone.position, zone.rotation);
                InstantiatedModels.Add(setCardKey, setCard);

                if (playCardEvent.CardPosition == CardPosition.FaceDown)
                {
                    HandlePlayCardModelEvents(default, zone.name, playCardEvent.CardId, false);
                    return;
                }
            }

            switch (playCardEvent.CardPosition)
            {
                case CardPosition.FaceUp:
                    HandlePlayCardModelEvents(ModelEvent.SpellTrapActivate, zone.name, playCardEvent.CardId, false);
                    break;
                case CardPosition.FaceDown:
                    HandlePlayCardModelEvents(ModelEvent.ReturnToFaceDown, zone.name, playCardEvent.CardId, false);
                    break;
                case CardPosition.FaceUpDefence:
                    HandlePlayCardModelEvents(ModelEvent.RevealSetMonster, zone.name, playCardEvent.CardId, true);
                    break;
                case CardPosition.FaceDownDefence:
                    HandlePlayCardModelEvents(default, zone.name, playCardEvent.CardId, true);
                    break;
            }
        }

        #endregion

        #region Card model

        private void PlayCardModel(PlayCardEvent playCardEvent, Transform zone, GameObject cardModel)
        {
            var instantiatedModel = GetInstantiatedModel(cardModel, zone);
            var hasSetCard = InstantiatedModels.TryGetValue($"{zone.name}:{SetCardKey}", out var setCardModel);

            switch (playCardEvent.CardPosition)
            {
                case CardPosition.FaceUp:
                    HandleFaceUpPosition(instantiatedModel, zone, hasSetCard, setCardModel);
                    break;
                case CardPosition.FaceDown:
                    HandleFaceDownPosition(zone, hasSetCard, instantiatedModel.name);
                    break;
                case CardPosition.FaceUpDefence:
                    HandleFaceUpDefencePosition(instantiatedModel, zone, hasSetCard, setCardModel);
                    break;
                case CardPosition.FaceDownDefence:
                    HandleFaceDownDefencePosition(zone, hasSetCard, instantiatedModel.name);
                    break;
            }
        }

        private GameObject GetInstantiatedModel(GameObject cardModel, Transform zone)
        {
            var alreadyInstantiated = InstantiatedModels.TryGetValue(zone.name, out var instantiatedModel);
            if (alreadyInstantiated)
            {
                return instantiatedModel;
            }

            return InstantiateModel(cardModel, zone);
        }

        private GameObject InstantiateModel(GameObject cardModel, Transform zone)
        {
            Debug.Log($"InstantiateModel(cardModel: {cardModel}, zone: {zone})");

            var instantiatedModel = cardModel.IsClone()
                ? cardModel
                : _modelFactory.Create(cardModel).gameObject.transform.parent.gameObject;

            instantiatedModel.transform.SetParent(_speedDuelField.transform);
            instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);

            _modelEventHandler.ActivateModel(zone.name);
            InstantiatedModels.Add(zone.name, instantiatedModel);

            return instantiatedModel;
        }

        private void HandleFaceUpPosition(GameObject instantiatedModel, Transform zone, bool hasSetCard, GameObject setCardModel)
        {
            _modelEventHandler.RaiseEventByEventName(ModelEvent.SummonMonster, zone.name);
            instantiatedModel.transform.position = zone.position;

            if (hasSetCard)
            {
                _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, zone.name);
                InstantiatedModels.Remove($"{zone.name}:{SetCardKey}");
                setCardModel.SetActive(false);
                _dataManager.SaveGameObject(SetCardKey, setCardModel);
            }
        }

        private void HandleFaceDownPosition(Transform zone, bool hasSetCard, string cardId)
        {
            if (!hasSetCard)
            {
                var setCard = GetGameObject(SetCardKey, zone.position, zone.rotation);
                if (setCard == null)
                {
                    Debug.LogWarning("The setCard queue is empty :(");
                    return;
                }

                InstantiatedModels.Add($"{zone.name}:{SetCardKey}", setCard);

                HandlePlayCardModelEvents(default, zone.name, cardId, false);
            }

            _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, false);
        }

        private void HandleFaceDownDefencePosition(Transform zone, bool hasSetCard, string cardId)
        {
            if (!hasSetCard)
            {
                var setCard = GetGameObject(SetCardKey, zone.position, zone.rotation);
                if (setCard == null)
                {
                    Debug.LogWarning("The setCard queue is empty :(");
                    return;
                }

                InstantiatedModels.Add($"{zone.name}:{SetCardKey}", setCard);

                HandlePlayCardModelEvents(default, zone.name, cardId, true);
            }

            _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, false);
        }

        private void HandleFaceUpDefencePosition(GameObject model, Transform zone, bool hasSetCard, GameObject setCardModel)
        {
            if (hasSetCard)
            {
                HandlePlayCardModelEvents(ModelEvent.RevealSetMonster, zone.name, model.name, true);
                _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, true);

                // This puts the model on top of the set card rather than clipping through it.
                model.transform.position = setCardModel.transform.GetChild(0).GetChild(0).position;
                return;
            }

            var setCard = GetGameObject(SetCardKey, zone.position, zone.rotation);
            HandlePlayCardModelEvents(ModelEvent.RevealSetMonster, zone.name, model.name, true);

            // This puts the model on top of the set card rather than clipping through it.
            model.transform.position = setCard.transform.GetChild(0).GetChild(0).position;

            InstantiatedModels.Add($"{zone.name}:{SetCardKey}", setCard);
            _modelEventHandler.RaiseChangeVisibilityEvent(zone.name, true);
        }

        #endregion

        private void HandlePlayCardModelEvents(ModelEvent modelEvent, string zone, string cardId, bool isMonster)
        {
            _modelEventHandler.RaiseSummonSetCardEvent(zone, cardId, isMonster);

            switch (modelEvent)
            {
                case ModelEvent.SpellTrapActivate:
                    _modelEventHandler.RaiseEventByEventName(ModelEvent.SpellTrapActivate, zone);
                    break;
                case ModelEvent.RevealSetMonster:
                    _modelEventHandler.RaiseEventByEventName(ModelEvent.RevealSetMonster, zone);
                    break;
                case ModelEvent.ReturnToFaceDown:
                    _modelEventHandler.RaiseEventByEventName(ModelEvent.ReturnToFaceDown, zone);
                    break;
            }
        }

        #endregion

        #region RemoveCardEvent

        private void OnRemoveCardEventReceived(RemoveCardEvent removeCardEvent)
        {
            if (_isFusionSummon)
            {
                HandleFusionSummon(removeCardEvent);
                return;
            }

            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                RemoveSetCard(removeCardEvent);
                return;
            }

            var destructionParticles = GetGameObject(ParticlesKey, model.transform.position, model.transform.rotation);

            _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroyMonster, removeCardEvent.ZoneName);

            StartCoroutine(RecycleGameObject(ParticlesKey, destructionParticles));
            StartCoroutine(RecycleGameObject(model.name, model));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            if (InstantiatedModels.ContainsKey($"{removeCardEvent.ZoneName}:{SetCardKey}"))
            {
                _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroySetMonster, removeCardEvent.ZoneName);
                RemoveSetCard(removeCardEvent);
            }
        }

        private void RemoveSetCard(RemoveCardEvent removeCardEvent)
        {
            var isCardSet = InstantiatedModels.TryGetValue($"{removeCardEvent.ZoneName}:{SetCardKey}", out var setCard);
            if (!isCardSet)
            {
                return;
            }

            _modelEventHandler.RaiseEventByEventName(ModelEvent.SetCardRemove, removeCardEvent.ZoneName);
            InstantiatedModels.Remove($"{removeCardEvent.ZoneName}:{SetCardKey}");
            StartCoroutine(RecycleGameObject(SetCardKey, setCard));
        }

        #region SpecialSummonEvents

        private void HandleFusionSummon(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if(!modelExists)
            {
                return;
            }
            
            var fusionParticles = GetGameObject(FusionParticlesKey, model.transform.position, model.transform.rotation);
            _modelEventHandler.RaiseChangeVisibilityEvent(removeCardEvent.ZoneName, false);
            _modelEventHandler.FuseMonsters(removeCardEvent.ZoneName);

            StartCoroutine(RecycleGameObject(model.name, model, 2));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);

            if (InstantiatedModels.ContainsKey($"{removeCardEvent.ZoneName}:{SetCardKey}"))
            {
                _modelEventHandler.RaiseEventByEventName(ModelEvent.DestroySetMonster, removeCardEvent.ZoneName);
                RemoveSetCard(removeCardEvent);
            }
        }

        #endregion

        #endregion

        private GameObject GetGameObject(string key, Vector3 position, Quaternion rotation)
        {
            var model = _dataManager.GetGameObject(key);

            model.transform.SetPositionAndRotation(position, rotation);
            model.SetActive(true);

            return model;
        }

        private IEnumerator RecycleGameObject(string key, GameObject model, float time = RemoveCardDurationInSeconds)
        {
            yield return new WaitForSeconds(time);

            model.SetActive(false);

            _dataManager.SaveGameObject(key.RemoveCloneSuffix(), model);
        }

        private void OnFusionSummon()
        {
            _isFusionSummon = true;
        }

        private void FinishFusion()
        {
            _isFusionSummon = false;
        }

        #endregion
    }
}
