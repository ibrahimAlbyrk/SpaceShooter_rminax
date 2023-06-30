using System;
using Mirror;
using UnityEngine;
using System.Collections;

namespace _Project.Scripts.Features
{
    [RequireComponent(typeof(NetworkIdentity))]
    public abstract class SpaceshipFeature : NetworkBehaviour
    {
        public event Action<SpaceshipFeature> OnFinished; 
        
        [Header("Feature Settings")]
        [SerializeField] protected float _duration = 5f;

        protected Coroutine _featureCoroutine;

        public abstract void AddFeature();
        
        public abstract void RemoveFeature();
        
        public void ResetFeatureDuration()
        {
            if (_featureCoroutine == null) return;
            
            StopCoroutine(_featureCoroutine);
            _featureCoroutine = StartCoroutine(RemoveFeatureToDuration());
        }

        protected IEnumerator RemoveFeatureToDuration()
        {
            yield return new WaitForSeconds(_duration);
            
            RemoveFeature();
            
            OnFinished?.Invoke(this);
        }
    }
}