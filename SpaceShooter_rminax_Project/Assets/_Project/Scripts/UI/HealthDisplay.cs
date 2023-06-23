﻿using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace _Project.Scripts.UI
{
    using Spaceship;
    
    public class HealthDisplay : MonoBehaviour
    {
        [SerializeField] private NetworkIdentity _identity;

        [SerializeField] private Transform targetTransform;

        [SerializeField] private float _height = 1;
        
        [SerializeField] private Health _health;

        [SerializeField] private Image _healthBarImage;

        [SerializeField] private float _healthShowTime;
        
        private float _healthShowTimer;

        private Coroutine _showCor;

        private void Awake()
        {
            _health.OnHealthChanged += OnHealthUpdated;
            _healthBarImage.gameObject.SetActive(false);
        }

        private void Update()
        {
            var cam = Camera.current;
            
            if (cam == null) return;
            
            transform.position = targetTransform.position + cam.transform.up * _height;
        }
        
        private void OnDestroy()
        {
            _health.OnHealthChanged -= OnHealthUpdated;
        }

        private void OnHealthUpdated(object sender,HealthChangedEventArgs args)
        {
            if (_identity.isOwned) return;
            
            _healthBarImage.fillAmount = args.Health / args.MaxHealth;
            
            if(_showCor != null)
                StopCoroutine(_showCor);
            
            _showCor = StartCoroutine(ShowHealth());
        }

        private IEnumerator ShowHealth()
        {
            _healthBarImage.gameObject.SetActive(true);

            var endTime = Time.time + _healthShowTime;
            while (Time.time < endTime)
            {
                yield return new WaitForEndOfFrame();
            }

            _healthBarImage.gameObject.SetActive(false);
            yield return null;
        }
    }
}