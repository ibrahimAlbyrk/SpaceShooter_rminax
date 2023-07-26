using System;
using Mirror;
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
        public void SetDamageable(bool state) => isDamageable = state;
        
        [Command(requiresAuthority = false)]
        public void Reset()
        {
            _currentHealth = _maxHealth;
        }
        
        [Command(requiresAuthority = false)]
        public void Add(float value)
        {
            value = Mathf.Max(value, 0);

            _currentHealth = Mathf.Min(_currentHealth + value, _maxHealth);
        }

        [Command(requiresAuthority = false)]
        public void Remove(float value)
        {
            if (!isDamageable) return;
            
            value = Mathf.Max(value, 0);

            _currentHealth = Mathf.Max(_currentHealth - value, 0);

            if (_currentHealth > 0) return;
            
            _controller.RPC_OnDeath();
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