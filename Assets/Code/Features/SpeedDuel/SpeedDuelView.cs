using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.ModelRecycler.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.Entities;
using AssemblyCSharp.Assets.Code.Core.Models.Impl;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class SpeedDuelView : MonoBehaviour, ISmartDuelEventListener
    {
        private static readonly string SET_CARD = "SetCard";
        private static readonly string PLAYMAT_ZONES = "Playmat/Zones/";

        [SerializeField]
        private GameObject _objectToPlace;
        [SerializeField]
        private GameObject _placementIndicator;
        [SerializeField]
        private GameObject _particles;
        [SerializeField]
        private ModelEventHandler _eventHandler;

        private ISmartDuelServer _smartDuelServer;
        private IDataManager _dataManager;

        private ApiWebRequest _webRequest;
        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private List<ARRaycastHit> _hits;
        private Pose _placementPose;
        private WaitForSeconds _waitTime = new WaitForSeconds(10);
        private bool _placementPoseIsValid = false;
        private bool _objectPlaced = false;

        #region Properties

        private GameObject SpeedDuelField { get; set; }
        private Dictionary<string, GameObject> InstantiatedModels { get; } = new Dictionary<string, GameObject>();

        #endregion

        #region Constructors

        [Inject]
        public void Construct(
            ISmartDuelServer smartDuelServer,
            IDataManager dataManager,
            IScreenService screenService)
        {
            _smartDuelServer = smartDuelServer;
            _dataManager = dataManager;

            screenService.UseAutoOrientation();
            ConnectToServer();
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            GetObjectReferences();
        }

        private void Start()
        {
            _dataManager.CreateRecycler();
            InstantiateObjectPool("Particles", (int)RecyclerKeys.DestructionParticles, _particles, 6);
            InstantiateObjectPool("SetCards", (int)RecyclerKeys.SetCard, _dataManager.GetCardModel(SET_CARD), 6);
        }

        private void Update()
        {
            UpdatePlacementIndicatorIfNecessary();
        }

        private void OnDestroy()
        {
            _smartDuelServer?.Dispose();
        }

        #endregion

        #region Placement indicator

        private void GetObjectReferences()
        {
            _arRaycastManager = FindObjectOfType<ARRaycastManager>();
            _arPlaneManager = FindObjectOfType<ARPlaneManager>();
            _webRequest = GetComponent<ApiWebRequest>();
        }

        private void InstantiateObjectPool(string parentName, int key, GameObject prefab, int amount)
        {
            var parent = new GameObject(parentName + " Pool");

            for (int i = 0; i < amount; i++)
            {
                var obj = Instantiate(prefab, parent.transform);
                _dataManager.AddToQueue(key, obj);
            }
        }

        private void UpdatePlacementIndicatorIfNecessary()
        {

#if UNITY_EDITOR
            if (!_objectPlaced && Input.GetKeyDown(KeyCode.Space))
            {
                PlaceObject();
            }

            return;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            if (_objectPlaced)
#pragma warning restore CS0162 // Unreachable code detected
            {
                return;
            }

            _hits = UpdatePlacementPose();
            UpdatePlacementIndicator();

            if (_placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceObject();
                SetPlaymatScale(_hits);
            }
        }

        private List<ARRaycastHit> UpdatePlacementPose()
        {
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hits = new List<ARRaycastHit>();
            _arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

            _placementPoseIsValid = hits.Count > 0;
            if (_placementPoseIsValid)
            {
                _placementPose = hits[hits.Count-1].pose;

                var cameraForward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                _placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }

            return hits;
        }

        private void UpdatePlacementIndicator()
        {
            if (_placementPoseIsValid)
            {
                _placementIndicator.SetActive(true);
                _placementIndicator.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
            }
            else
            {
                _placementIndicator.SetActive(false);
            }
        }

        private void PlaceObject()
        {
            _objectPlaced = true;
            _placementIndicator.SetActive(false);
            SpeedDuelField = Instantiate(_objectToPlace, _placementPose.position, _placementPose.rotation);
        }

        private void SetPlaymatScale(List<ARRaycastHit> hits)
        {
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            _arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds);

            if (hits == null)
            {
                return;
            }

            var scalePlane = GetCameraOrientation(_arPlaneManager.GetPlane(hits[hits.Count].trackableId));

            if (scalePlane <= 0)
            {
                return;
            }

            SpeedDuelField.transform.localScale = new Vector3(scalePlane, scalePlane, scalePlane);

            _arPlaneManager.SetTrackablesActive(false);
            _arPlaneManager.enabled = false;
        }

        private float GetCameraOrientation(ARPlane plane)
        {
            float scaleAmount;
            var cameraOriantation = Camera.current.transform.rotation.y;

            if (cameraOriantation.IsWithinRange(45, 135)   || 
                cameraOriantation.IsWithinRange(225, 315)  ||
                cameraOriantation.IsWithinRange(-45, -135) || 
                cameraOriantation.IsWithinRange(-225, -315))
            {
                scaleAmount = plane.size.y;
            }
            else
            {
                scaleAmount = plane.size.x;
            }

            return scaleAmount;
        }

        private void OnPlaymatDestroyed()
        {
            _objectPlaced = false;
            _placementIndicator.SetActive(true);
            _arPlaneManager.enabled = true;
        }

        #endregion

        #region Smart duel events

        private void ConnectToServer()
        {
            _smartDuelServer.Connect(this);
        }

        public void onSmartDuelEventReceived(SmartDuelEvent smartDuelEvent)
        {
            if (smartDuelEvent is SummonEvent summonEvent)
            {
                OnSummonEventReceived(summonEvent);
            }
            else if (smartDuelEvent is RemoveCardEvent removeCardEvent)
            {
                OnRemovecardEventReceived(removeCardEvent);
            }
            else if (smartDuelEvent is PositionChangeEvent positionChangeEvent)
            {
                OnPositionChangeEventRecieved(positionChangeEvent);
            }
            else if (smartDuelEvent is SpellTrapSetEvent spellTrapSetEvent)
            {
                OnSpellTrapSetEventRecieved(spellTrapSetEvent);
            }
        }

        private void OnSummonEventReceived(SummonEvent summonEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + summonEvent.ZoneName);
            if (zone == null)
            {
                return;
            }

            var cardModel = _dataManager.GetCardModel(summonEvent.CardId);
            if (cardModel == null)
            {
                return;
            }

            GameObject instantiatedModel;
            if (_dataManager.CheckForExistingModel(cardModel.name + "(Clone)"))
            {
                instantiatedModel = _dataManager.GetExistingModel(cardModel.name + "(Clone)", SpeedDuelField.transform);
                instantiatedModel.transform.SetPositionAndRotation(zone.position, zone.rotation);
            }
            else
            {
                instantiatedModel = Instantiate(cardModel, zone.transform.position, zone.transform.rotation, SpeedDuelField.transform);
            }

            _eventHandler.RaiseEvent(EventNames.SummonMonster, summonEvent.ZoneName);
            InstantiatedModels.Add(summonEvent.ZoneName, instantiatedModel);

            bool isSet = summonEvent.IsSet;
            if (isSet)
            {
                _webRequest.GetImageFromAPI(cardModel.name);

                var setCardImage = _dataManager.GetCardModel(SET_CARD);
                if (setCardImage == null)
                {
                    return;
                }

                _eventHandler.RaiseEvent(EventNames.ChangeMonsterVisibility, summonEvent.ZoneName, false);

                var setCardModel = _dataManager.UseFromQueue((int)RecyclerKeys.SetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                if (!_dataManager.CheckForCachedImage(cardModel.name))
                {
                    Debug.LogError("No Cached Image");
                    return;
                }
                var imageSetter = setCardModel.GetComponentInChildren<IImageSetter>();
                imageSetter.ChangeImageFromAPI(cardModel.name);

                InstantiatedModels.Add(summonEvent.ZoneName + "SetCard", setCardModel);
            }
        }

        private void OnRemovecardEventReceived(RemoveCardEvent removeCardEvent)
        {
            var modelExists = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName, out var model);
            if (!modelExists)
            {
                var modelIsSet = InstantiatedModels.TryGetValue(removeCardEvent.ZoneName + "SetCard", out var setCardBack);
                if (!modelIsSet)
                {
                    return;
                }
                var animator = setCardBack.GetComponent<Animator>();
                animator.SetTrigger(AnimatorIDSetter.Animator_Remove_Spell_Or_Trap);

                InstantiatedModels.Remove(removeCardEvent.ZoneName + "SetCard");
                StartCoroutine(WaitToProceed((int)RecyclerKeys.SetCard, setCardBack));

                return;
            }

            _eventHandler.RaiseEvent(EventNames.DestroyMonster, removeCardEvent.ZoneName, false);

            var destructionParticles = _dataManager.UseFromQueue(
                (int)RecyclerKeys.DestructionParticles, SpeedDuelField.transform);

            StartCoroutine(WaitToProceed((int)RecyclerKeys.DestructionParticles, destructionParticles));
            StartCoroutine(WaitToProceed(model.name, model));

            InstantiatedModels.Remove(removeCardEvent.ZoneName);
        }

        private void OnPositionChangeEventRecieved(PositionChangeEvent positionChangeEvent)
        {
            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + positionChangeEvent.ZoneName);

            var modelExists = InstantiatedModels.TryGetValue(positionChangeEvent.ZoneName, out var model);
            if (!modelExists)
            {
                return;
            }

            if (!positionChangeEvent.IsSet)
            {
                _eventHandler.RaiseEvent(EventNames.ChangeMonsterVisibility, zone.name, true);
                return;
            }

            var cardBackExists = InstantiatedModels.TryGetValue(positionChangeEvent.ZoneName + "SetCard", out var _);
            if (!cardBackExists)
            {
                var cardBack = _dataManager.GetCardModel(SET_CARD);
                if (cardBack == null)
                {
                    return;
                }

                _eventHandler.RaiseEvent(EventNames.ChangeMonsterVisibility, zone.name, false);

                var setCardBackModel = _dataManager.UseFromQueue((int)RecyclerKeys.SetCard, zone.position, zone.rotation, SpeedDuelField.transform);
                InstantiatedModels.Add(positionChangeEvent.ZoneName + "SetCard", setCardBackModel);
                return;
            }
            
            _eventHandler.RaiseEvent(EventNames.ChangeMonsterVisibility, zone.name, false);
        }

        private void OnSpellTrapSetEventRecieved(SpellTrapSetEvent spellTrapSetEvent)
        {
            var modelName = spellTrapSetEvent.CardId;
            if (modelName == null)
            {
                return;
            }
            
            _webRequest.GetImageFromAPI(modelName);

            var zone = SpeedDuelField.transform.Find(PLAYMAT_ZONES + spellTrapSetEvent.ZoneName);
            if (zone == null)
            {
                return;
            }

            var spellRotation = Quaternion.Euler(0, -90, 0);            
            var setCardModel = _dataManager.UseFromQueue((int)RecyclerKeys.SetCard, zone.position, spellRotation, SpeedDuelField.transform);
            
            if (!_dataManager.CheckForCachedImage(modelName))
            {
                StartCoroutine(AwaitImage(setCardModel, modelName));
                if (InstantiatedModels.ContainsKey(spellTrapSetEvent.ZoneName + "SetCard"))
                {
                    //TODO: Add error handler
                    Debug.LogWarning("Recycling Old Resources");
                    InstantiatedModels.TryGetValue(spellTrapSetEvent.ZoneName + "SetCard", out var model);
                    _dataManager.AddToQueue((int)RecyclerKeys.SetCard, model);
                    InstantiatedModels.Remove(spellTrapSetEvent.ZoneName + "SetCard");
                }
                InstantiatedModels.Add(spellTrapSetEvent.ZoneName + "SetCard", setCardModel);
                return;
            }

            var imageSetter = setCardModel.GetComponentInChildren<IImageSetter>();
            var texture = _dataManager.GetCachedImage(modelName);
            imageSetter.ChangeImage(texture);
            
            if (InstantiatedModels.ContainsKey(spellTrapSetEvent.ZoneName + "SetCard"))
            {
                //TODO: Add error handler
                Debug.LogWarning("Recycling Old Resources");
                InstantiatedModels.TryGetValue(spellTrapSetEvent.ZoneName + "SetCard", out var model);
                _dataManager.AddToQueue((int)RecyclerKeys.SetCard, model);
                InstantiatedModels.Remove(spellTrapSetEvent.ZoneName + "SetCard");
            }
            InstantiatedModels.Add(spellTrapSetEvent.ZoneName + "SetCard", setCardModel); 
        }

        #endregion

        #region Coroutines

        private IEnumerator WaitToProceed(int key, GameObject model)
        {
            yield return _waitTime;
            _dataManager.AddToQueue(key, model);
        }
        private IEnumerator WaitToProceed(string key, GameObject model)
        {
            yield return _waitTime;
            _dataManager.RecycleModel(key, model);
        }

        private IEnumerator AwaitImage(GameObject model, string cardID)
        {
            yield return new WaitForSeconds(0.75f);

            var imageSetter = model.GetComponentInChildren<IImageSetter>();
            var texture = _dataManager.GetCachedImage(cardID);
            imageSetter.ChangeImage(texture);
        }

        #endregion
    }
}
