using System;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Spaceship
{
    public class Health : NetworkBehaviour
    {
        public event EventHandler<DeathEventArgs> OnDeath;
        public event EventHandler<HealthChangedEventArgs> OnHealthChanged;

        [SerializeField] private float _maxHealth = 100;

        public bool isDamageable = true;

        public float GetHealth() => _currentHealth;
        
        public bool IsDead => _currentHealth <= 0f;

        #region Sync Variables

        [SyncVar(hook = nameof(OnHealthUpdated))] [SerializeField]
        private float _currentHealth;

        private void OnHealthUpdated(float _, float __)
        {
            OnHealthChanged?.Invoke(this, new HealthChangedEventArgs
            {
                Health = _currentHealth,
                MaxHealth = _maxHealth
            });
        }

        #endregion

        #region Server Callbacks

        public override void OnStartServer()
        {
            _currentHealth = _maxHealth;
        }

        #endregion

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
            
            OnDeath?.Invoke(this, new DeathEventArgs { ConnectionToClient = connectionToClient });
        }

        #region Base methods

        [Client]
        private void OnDestroy()
        {
            OnDeath?.Invoke(this, new DeathEventArgs { ConnectionToClient = connectionToClient });
        }

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