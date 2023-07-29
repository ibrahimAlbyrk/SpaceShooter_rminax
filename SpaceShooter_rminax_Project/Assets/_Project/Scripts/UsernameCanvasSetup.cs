﻿using UnityEngine;

namespace _Project.Scripts
{
    public class UsernameCanvasSetup : MonoBehaviour
    {
        [SerializeField] private Transform _targetTransform;

        [SerializeField] private float _distance;
        
        private void LateUpdate()
        {
            var cam = Camera.current;

            if (cam == null) return;
            
            transform.position = _targetTransform.position + cam.transform.up * _distance;
        }
    }
}