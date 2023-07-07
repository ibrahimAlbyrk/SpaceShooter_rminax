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

    public class FeatureSystem : NetIdentity
    {
        [SerializeField] private float _featureDetectionRange = 20f;

        [SerializeField] private LayerMask _detectionLayer;

        private event Action<Collider> OnEntered;

        private readonly Dictionary<Feature_SO, float> _features = new();

        private readonly List<Collider> _collisions = new();

        private SpaceshipController _thisController;
        
        private void OnColliderEntered(Collider coll)
        {
            if (!isOwned) return;

            var featureHandler = coll.GetComponent<FeatureHandler>();

            var feature = featureHandler.GetFeature();

            if (featureHandler == null || feature == null) return;

            if (_features.ContainsKey(feature))
            {
                CMD_UpdateFeatureTime(feature.Name);
            }
            else
            {
                CMD_AddFeature(feature.Name, SpaceshipController.instance);
            }
        }

        #region Command Methods

        [Command]
        private void CMD_AddFeature(string featureName, SpaceshipController ownedController) => RPC_AddFeature(featureName, ownedController);

        [Command]
        private void CMD_UpdateFeatureTime(string featureName) => RPC_UpdateFeatureTime(featureName);

        [Command]
        private void CMD_RemoveFeature(string featureName) => RPC_RemoveFeature(featureName);

        #endregion

        #region Client Methods

        [ClientRpc]
        private void RPC_AddFeature(string featureName, SpaceshipController ownedController)
        {
            var feature = Resources.Load<Feature_SO>($"FeaturesSO/{featureName}");
            feature = Instantiate(feature);

            if (feature == null) return;
            
            var endTime = Time.time + feature.Duration;

            feature.OnStart(ownedController);

            _features.Add(feature, endTime);
            
            print($"<color=green>{featureName}</color> added to <color=orange>{ownedController.gameObject.name}</color>");
        }

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
        }

        #endregion

        #region Base Methods

        [Client]
        private void Start()
        {
            _thisController = GetComponent<SpaceshipController>();
            
            if (!isOwned) return;
            
            OnEntered += OnColliderEntered;
        }

        [Client]
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
                if (Time.time < endTime) return;
                
                endedFeatures.Add(feature);
            }

            //Remove expired ones
            foreach (var endedFeature in endedFeatures)
            {
                CMD_RemoveFeature(endedFeature.Name);
            }

            //Check Colliders and do action
            var colls = Physics.OverlapSphere(transform.position, _featureDetectionRange, _detectionLayer);

            foreach (var coll in colls)
            {
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