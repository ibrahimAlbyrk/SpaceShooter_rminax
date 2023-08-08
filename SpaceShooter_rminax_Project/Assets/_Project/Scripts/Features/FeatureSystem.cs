using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

namespace _Project.Scripts.Features
{
    using Network;
    using Spaceship;
    using Utilities;

    public class FeatureSystem : NetIdentity
    {
        [SerializeField] private Transform _featureContent;
        [SerializeField] private GameObject _featureUIPrefab;

        [SerializeField] private float _featureDetectionRange = 20f;

        [SerializeField] private LayerMask _detectionLayer;

        private readonly List<Feature_UI> _featureUIs = new();

        private event Action<Collider> OnEntered;

        private readonly Dictionary<Feature_SO, float> _features = new();

        private readonly List<Collider> _collisions = new();

        private PhysicsScene _physicsScene;

        [ClientCallback]
        private void OnColliderEntered(Collider coll)
        {
            if (!isOwned) return;

            var featureHandler = coll.GetComponent<FeatureHandler>();

            var feature = featureHandler?.GetFeature();

            if (feature == null) return;

            featureHandler.Destroy();

            var selectedFeature = _features.Keys.FirstOrDefault(f => f.Name == feature.Name);

            if (selectedFeature != null)
            {
                var index = _features.Keys.ToList().IndexOf(selectedFeature);

                CMD_UpdateFeatureUI(index);
                CMD_UpdateFeatureTime(feature.Name);
            }
            else
            {
                CMD_AddFeature(feature.Name, SpaceshipController.instance);

                CMD_CreateFeatureUI(feature.name);
            }
        }

        #region Command Methods

        [Command]
        private void CMD_CreateFeatureUI(string featureName) => RPC_CreateFeatureUI(featureName);

        [Command]
        private void CMD_UpdateFeatureUI(int index) => RPC_UpdateFeatureUI(index);

        [Command]
        private void CMD_AddFeature(string featureName, SpaceshipController ownedController) =>
            RPC_AddFeature(featureName, ownedController);

        [Command]
        private void CMD_UpdateFeatureTime(string featureName) => RPC_UpdateFeatureTime(featureName);

        [Command]
        private void CMD_RemoveFeature(string featureName) => RPC_RemoveFeature(featureName);

        #endregion

        #region Client Methods

        [TargetRpc]
        private void RPC_CreateFeatureUI(string featureName)
        {
            var feature = _features.Keys.FirstOrDefault(f => f.Name == featureName);

            if (feature == null) return;

            var _featureUI = Instantiate(_featureUIPrefab, _featureContent).GetComponent<Feature_UI>();

            if (_featureUI == null) return;

            _featureUI.Init(feature.Icon, feature.Duration);

            _featureUIs.Add(_featureUI);
        }

        [TargetRpc]
        private void RPC_UpdateFeatureUI(int index)
        {
            if (index >= _featureUIs.Count) return;

            _featureUIs[index].UpdateDuration();
        }

        [ClientRpc]
        private void RPC_AddFeature(string featureName, SpaceshipController ownedController)
        {
            var feature = Resources.Load<Feature_SO>($"FeaturesSO/{featureName}");
            feature = Instantiate(feature);

            if (feature == null) return;

            var endTime = Time.time + feature.Duration;

            _features.Add(feature, endTime);
            
            CMD_AddToList(featureName);
            
            SetSettingFeature(ownedController, feature);
        }

        [Command(requiresAuthority = false)]
        private void CMD_AddToList(string featureName)
        {
            var feature = Resources.Load<Feature_SO>($"FeaturesSO/{featureName}");
            feature = Instantiate(feature);
            
            var endTime = Time.time + feature.Duration;

            _features.Add(feature, endTime);
        }
        
        [Command(requiresAuthority = false)]
        private void CMD_RemoveToList(string featureName)
        {
            var feature = _features.Keys.FirstOrDefault(f => f.Name == featureName);

            if (feature == null) return;

            _features.Remove(feature);
        }

        #region Fire Feature Methods

        private void SetSettingFeature(SpaceshipController ownedController, Feature_SO feature)
        {
            if (feature is not FireFeature_SO)
            {
                feature.OnStart(ownedController);
                
                return;
            }

            CMD_SetFireFeature(feature.Name, ownedController);
        }

        [Command(requiresAuthority = false)]
        private void CMD_SetFireFeature(string featureName, SpaceshipController ownedController)
        {
            var feature = _features.Keys.FirstOrDefault(f => f.Name == featureName);

            if (feature is not FireFeature_SO fireFeature) return;
            
            var bulletCount = fireFeature.IncreaseBulletCountRange.GetRandomValue();
            var targetDistance = fireFeature.IncreaseTargetDistanceRange.GetRandomValue();
            var speed = fireFeature.IncreaseSpeedRange.GetRandomValue();

            RPC_SetSettingForFireFeature(ownedController, featureName, bulletCount, targetDistance, speed);
        }

        [ClientRpc]
        private void RPC_SetSettingForFireFeature(SpaceshipController ownedController, string featureName,
            float bulletCount, float targetDistance, float speed)
        {
            var feature = _features.Keys.FirstOrDefault(f => f.Name == featureName);

            if (feature == null) return;

            if (feature is FireFeature_SO fireFeature)
            {
                fireFeature.SetBulletSettings(bulletCount, targetDistance, speed);
            }

            feature.OnStart(ownedController);
        }

        #endregion

        [ClientRpc]
        private void RPC_UpdateFeatureTime(string featureName)
        {
            var feature = _features.Keys.FirstOrDefault(feature => feature.Name == featureName);

            if (feature == null) return;

            var endTime = Time.time + feature.Duration;

            _features[feature] = endTime;
        }

        [ClientRpc]
        private void RPC_RemoveFeature(string featureName)
        {
            var feature = _features.Keys.FirstOrDefault(feature => feature.Name == featureName);

            if (feature == null) return;

            feature.OnEnd();

            _features.Remove(feature);
            
            CMD_RemoveToList(featureName);
        }

        #endregion

        #region Base Methods

        [ClientCallback]
        private void Start()
        {
            if (!isOwned) return;

            OnEntered += OnColliderEntered;

            _physicsScene = gameObject.scene.GetPhysicsScene();
        }

        [ClientCallback]
        private void Update()
        {
            //Run features' update function 
            foreach (var feature in _features.Keys)
            {
                feature.OnUpdate();
            }

            if (!isOwned) return;

            var endedFeatures = new List<Feature_SO>();

            //check if expired
            foreach (var (feature, endTime) in _features)
            {
                if (Time.time < endTime) continue;

                endedFeatures.Add(feature);
            }

            //Remove expired ones
            foreach (var endedFeature in endedFeatures)
            {
                CMD_RemoveFeature(endedFeature.Name);
                //CMD_RemoveAddedFeature(endedFeature.name);
            }

            //Check Colliders and do action
            var colls = new Collider[2];
            _physicsScene.OverlapSphere(transform.position, _featureDetectionRange, colls, _detectionLayer,
                QueryTriggerInteraction.UseGlobal);

            foreach (var coll in colls)
            {
                if (coll == null) continue;

                if (_collisions.Contains(coll)) continue;

                _collisions.Add(coll);

                OnEntered?.Invoke(coll);
            }

            for (var i = 0; i < _collisions.Count; i++)
            {
                var coll = _collisions[i];

                if (colls.Any(c => c == coll)) continue;

                _collisions.Remove(coll);
            }
        }

        #endregion

        #region Editor

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, _featureDetectionRange);
        }

        #endregion
    }
}