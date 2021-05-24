using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts
{
    public class FusionParticles : MonoBehaviour
    {
        public List<Color> _colours = new List<Color>();

        private const string FusionParticlesKey = "FusionParticles";
        private ParticleSystem _particles;
        private List<Transform> routes = new List<Transform>();
        private Transform _spline;

        private int numberOfRoutes;
        private int nextRoute = 0;
        private bool coroutineAllowed = false;
        private bool hasRoutes = false;
        private float speedModifier;

        private ModelEventHandler _modelEventHandler;
        private IDataManager _dataManager;

        #region Constructor

        [Inject]
        public void Construct(ModelEventHandler modelEventHandler,
                              IDataManager dataManager)
        {
            _modelEventHandler = modelEventHandler;
            _dataManager = dataManager;
        }

        #endregion

        #region LifeCycle

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();

            _modelEventHandler.OnFusionActivated += FindRoutes;
            _modelEventHandler.OnFuseMonsters += FuseMonsters;
            _modelEventHandler.OnFinishFusion += FinishFusion;
        }

        private void Update()
        {
            if (coroutineAllowed)
            {
                StartCoroutine(TravelAlongRoute(nextRoute));
            }
        }

        #endregion

        private void FindRoutes()
        {
            _spline = GameObject.Find("Spline").transform;
            numberOfRoutes = _spline.childCount;

            for (int i = 0; i < numberOfRoutes; i++)
            {
                routes.Add(_spline.GetChild(i));
            }

            hasRoutes = true;
        }
        
        private void FuseMonsters(string zone)
        {
            if(!isActiveAndEnabled)
            {
                return;
            }
            
            speedModifier = Random.Range(0.2f, 0.5f);
            var randomColour = _colours[Random.Range(0, _colours.Count)];

            var main = _particles.main;
            main.startColor = randomColour;

            StartCoroutine(TravelToRoute());
            _modelEventHandler.OnFuseMonsters -= FuseMonsters;
        }

        private void FinishFusion()
        {
            _particles.Stop();
            _modelEventHandler.OnFuseMonsters += FuseMonsters;
            _dataManager.SaveGameObject(FusionParticlesKey, gameObject);
        }

        private IEnumerator TravelToRoute()
        {            
            while(!hasRoutes)
            {
                yield return new WaitForEndOfFrame();
            }
            
            var time = 0f;
            var routeStartPosition = routes[0].GetChild(0).position;

            while (transform.position != routeStartPosition)
            {
                time += Time.deltaTime * 0.05f;
                transform.position = Vector3.Lerp(transform.position, routeStartPosition, time);

                yield return new WaitForEndOfFrame();
            }

            coroutineAllowed = true;
        }
        
        private IEnumerator TravelAlongRoute(int routeNumber)
        {
            coroutineAllowed = false;

            var time = 0f;

            Vector3 pointA = routes[routeNumber].GetChild(0).position;
            Vector3 pointB = routes[routeNumber].GetChild(1).position;
            Vector3 control1 = routes[routeNumber].GetChild(2).position;
            Vector3 control2 = routes[routeNumber].GetChild(3).position;

            while (time < 1)
            {
                time += Time.deltaTime * speedModifier;

                //Bezier Curve Formula
                var newPosition = Mathf.Pow(1 - time, 3) * pointA +
                    3 * Mathf.Pow(1 - time, 2) * time * control1 +
                    3 * (1 - time) * Mathf.Pow(time, 2) * control2 +
                    Mathf.Pow(time, 3) * pointB;

                transform.position = newPosition;
                yield return new WaitForEndOfFrame();
            }

            nextRoute += 1;
            coroutineAllowed = true;

            if (nextRoute >= numberOfRoutes)
            {
                nextRoute = 0;
            }
        }

        public class Factory : PlaceholderFactory<GameObject, FusionParticles>
        {
        }
    }
}

