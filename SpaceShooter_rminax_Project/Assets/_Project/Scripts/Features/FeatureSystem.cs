using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace _Project.Scripts.Features
{
    public class FeatureSystem : NetworkBehaviour
    {
        [SerializeField] private float _featureDetectionRange = 20f;
        [SerializeField] private LayerMask _detectionLayer;

        private event Action<Collider> OnEntered; 
        
        private readonly List<SpaceshipFeature> _features = new();

        private readonly List<Collider> _collisions = new();
        
        private void OnColliderEntered(Collider coll)
        {
            var feature = coll.GetComponent<SpaceshipFeature>();

            if (feature == null) return;
            
            if(_features.Contains(feature))
                feature.ResetFeatureDuration();
            else
            {
                feature.AddFeature();
                
                _features.Add(feature);
                feature.OnFinished += RemoveFeature;
            }
        }
        
        private void RemoveFeature(SpaceshipFeature feature)
        {
            _features.Remove(feature);

            feature.OnFinished -= RemoveFeature;
        }

        [Client]
        private void Start()
        {
            if (!isOwned) return;
            
            OnEntered += OnColliderEntered;
        }
        
        [Client]
        private void Update()
        {
            if (!isOwned) return;
            
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
                
                if(colls.Any(c => c == coll)) continue;

                _collisions.Remove(coll);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(transform.position, _featureDetectionRange);
        }
    }
}