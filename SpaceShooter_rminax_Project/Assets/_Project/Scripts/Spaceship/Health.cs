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

        public bool IsDead => _currentHealth <= 0f;

        #region Sync Variables

        [SyncVar(hook = nameof(OnHealthUpdated))]
        private float _currentHealth;
        
        private void OnHealthUpdated(float _, float newHealth)
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

        [Server]
        public void Add(float value)
        {
            value = Mathf.Max(value, 0);

            _currentHealth = Mathf.Min(_currentHealth + value, _maxHealth);
        }
        
        [Server]
        public void Remove(float value)
        {
            value = Mathf.Max(value, 0);

            _currentHealth = Mathf.Max(_currentHealth - 0, 0);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke(this, new DeathEventArgs{ConnectionToClient = connectionToClient});

                RPC_HandleDeath();
            }
        }

        private void RPC_HandleDeath()
        {
            gameObject.SetActive(false);
        }

        #region Base methods

        [ServerCallback]
        private void OnDestroy()
        {
            OnDeath?.Invoke(this, new DeathEventArgs{ConnectionToClient = connectionToClient});
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