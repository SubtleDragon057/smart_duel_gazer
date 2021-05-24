using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.UIComponents.Constants;
using System.Collections;
using UnityEngine;

public class Polymerization : MonoBehaviour
{
    private const string Zone = "Playmat/Zones/fusionZone";
    private Transform _model;
    private Transform _fusionZone;
    private Animator _animator;
    private string _zone;

    private ModelComponentsManager _manager;
    private ModelEventHandler _modelEventHandler;

    #region Lifecycle

    void Awake()
    {
        _fusionZone = GameObject.Find(Zone).transform;
        _animator = GetComponent<Animator>();

        _model = transform.parent;

        _manager = GetComponent<ModelComponentsManager>();
        _modelEventHandler = _manager.EventHandler;

        _modelEventHandler.FusionActivated();
    }

    private void OnEnable()
    {
        _modelEventHandler.OnActivateModel += ActivateModel;
        _modelEventHandler.OnSummonMonster += SummonModel;
        _modelEventHandler.OnFinishFusion += FinishFusion;
        _modelEventHandler.OnChangeMonsterVisibility += ChangeVisibilityAndStopFusionSummon;
    }

    private void OnDisable()
    {
        _zone = null;
        
        _modelEventHandler.OnFinishFusion -= FinishFusion;
        _modelEventHandler.OnChangeMonsterVisibility -= ChangeVisibilityAndStopFusionSummon;
    }

    #endregion

    private void ActivateModel(string zone)
    {
        _modelEventHandler.FusionSummon();
        _modelEventHandler.OnActivateModel -= ActivateModel;

        _zone = _manager.Zone;
    }

    private void SummonModel(string zone)
    {
        if (_zone != zone)
        {
            return;
        }

        _modelEventHandler.FusionSummon();
        _modelEventHandler.OnSummonMonster -= SummonModel;
        StartCoroutine(SetPosition());
    }

    private IEnumerator SetPosition()
    {
        yield return new WaitForEndOfFrame();
        _model.position = _fusionZone.position;
    }

    private void ChangeVisibilityAndStopFusionSummon(string zone, bool state)
    {
        if (_zone != zone || state)
        {
            return;
        }

        _modelEventHandler.FinishFusion();
        _animator.ResetTrigger(AnimatorParameters.DeathTrigger);
    }
    
    private void FinishFusion()
    {
        _animator.SetTrigger(AnimatorParameters.DeathTrigger);
    }
}
