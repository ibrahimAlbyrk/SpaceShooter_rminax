using System;
using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace _Project.Scripts.Spaceship
{
    using Network;
    using UI.Fields;
    
    public class FuelSystem : NetIdentity
    {
        [Header("Detection Settings")]
        [SerializeField] private LayerMask _stationLayer;
        [SerializeField] private float _stationDetectionRange = 20f;

        [Header("Fuel Settings")]
        [SerializeField] private float _maxFuel = 100;
        [SerializeField] private float _fuelDrainAmount = .3f;
        [SerializeField] private float _fuelFastDrainAmount = .6f;
        [SerializeField] private float _fuelAddAmount = 5f;

        [SerializeField] private float _currentFuel;
        
        [SerializeField] private Bar_UI _barUI;
        
        private event Action<Collider> OnEntered;
        private event Action<Collider> OnExited;
        
        private readonly List<Collider> _collisions = new();

        private bool _onStation;

        private SpaceshipController _controller;
        
        #region Base Methods

        [Client]
        private void OnDestroy()
        {
            OnEntered -= OnColliderEntered;
            OnExited -= OnColliderExited;
        }

        [Client]
        private void Start()
        {
            if (!isOwned) return;
            
            OnEntered += OnColliderEntered;
            OnExited += OnColliderExited;

            _controller = GetComponent<SpaceshipController>();
            
            _currentFuel = _maxFuel;
            
            _barUI.SetValue(_currentFuel, _maxFuel);
        }

        [Client]
        private void Update()
        {
            if (!isOwned) return;
            
            CheckCollision();

            FuelHandler();
        }

        #endregion

        #region Collision Methods

        private void OnColliderEntered(Collider _)
        {
            if (!isOwned) return;

            _onStation = true;
        }
        
        private void OnColliderExited(Collider _)
        {
            if (!isOwned) return;

            _onStation = false;
        }

        private void CheckCollision()
        {
            //Check Colliders and do action
            var colls = Physics.OverlapSphere(transform.position, _stationDetectionRange, _stationLayer);

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
                
                OnExited?.Invoke(coll);
            }
        }

        #endregion

        #region Fuel Methods

        private void FuelHandler()
        {
            if(_onStation) OnRefuel();
            else OnDrainFuel();

            if (_currentFuel <= 0 && _controller.IsRunningMotor)
            {
                _controller.IsRunningMotor = false;
            }
        }
        
        private void OnRefuel()
        {
            var amount = _fuelAddAmount * Time.fixedDeltaTime;

            _currentFuel = Mathf.Min(_currentFuel + amount, _maxFuel);
            
            _barUI.SetValue(_currentFuel, _maxFuel);
            
            if (_currentFuel > 0 && !_controller.IsRunningMotor)
            {
                _controller.IsRunningMotor = true;
            }
        }

        private void OnDrainFuel()
        {
            var isThrottle = _controller.RawInput.w > 0f;

            var drain = isThrottle ? _fuelFastDrainAmount : _fuelDrainAmount;
            
            var amount = drain * Time.fixedDeltaTime;

            _currentFuel = Mathf.Max(_currentFuel - amount, 0f);
            
            _barUI.SetValue(_currentFuel, _maxFuel);
        }

        #endregion
    }
}