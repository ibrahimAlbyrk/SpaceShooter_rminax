using System;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

namespace _Project.Scripts.Spaceship
{
    public class Health : NetworkBehaviour
    {
        public event Action<float, float> OnHealthChanged;

        [SerializeField] private float _maxHealth = 100;

        [SyncVar] public bool isDamageable = true;

        private SpaceshipController _controller;

        public float GetHealth() => _currentHealth;
        
        public bool IsDead => _currentHealth <= 0f;

        #region Sync Variables

        [SyncVar(hook = nameof(OnHealthUpdated))] [SerializeField]
        private float _currentHealth;

        private void OnHealthUpdated(float _, float __)
        {
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        #endregion

        #region Server Callbacks

        public override void OnStartServer()
        {
            _currentHealth = _maxHealth;
        }

        #endregion

        [Command(requiresAuthority = false)]
        public void CMD_SetDamageable(bool state) => SetDamageable(state);

        [Command(requiresAuthority = false)]
        public void CMD_Reset() => Reset();

        [Command(requiresAuthority = false)]
        public void CMD_Add(float value) => Add(value);
        
        [Command(requiresAuthority = false)]
        public void CMD_Remove(float value) => Remove(value);
        
        [ServerCallback]
        public void SetDamageable(bool state) => isDamageable = state;
        
        [ServerCallback]
        public void Reset()
        {
            _currentHealth = _maxHealth;
        }

        [ServerCallback]
        public void Add(float value)
        {
            value = Mathf.Max(value, 0);

            _currentHealth = Mathf.Min(_currentHealth + value, _maxHealth);
        }

        [ServerCallback]
        public void Remove(float value)
        {
            if (!isDamageable) return;
            
            value = Mathf.Max(value, 0);

            _currentHealth = Mathf.Max(_currentHealth - value, 0);

            if (_currentHealth > 0) return;
            
            _controller.OnDeath();
        }

        #region Base methods

        private void Awake() => _controller = GetComponent<SpaceshipController>();

        #endregion
    }

    public class DeathEventArgs
    {
        public NetworkConnection ConnectionToClient;
    }

    public class HealthChangedEventArgs
    {
        public float Health;
        public float MaxHealth;
    }
}