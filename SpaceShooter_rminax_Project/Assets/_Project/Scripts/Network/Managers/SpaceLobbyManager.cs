using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Managers
{
    public class SpaceLobbyManager : NetworkBehaviour
    {
        public static SpaceLobbyManager Instance;

        public event Action<NetworkConnectionToClient> OnConnectedNetworkPlayer; 
        public event Action<NetworkConnectionToClient> OnDisconnectedNetworkPlayer; 
        
        public event Action<NetworkConnectionToClient> OnConnectedGamePlayer; 
        public event Action<NetworkConnectionToClient> OnDisconnectedGamePlayer; 
        
        public List<NetworkConnectionToClient> NetworkPlayers = new();
        public List<NetworkConnectionToClient> GamePlayers = new();

        [Header("Singleton Settings")]
        [SerializeField] private bool _DontDestroy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;
            
            if(_DontDestroy)
                DontDestroyOnLoad(gameObject);
        }
        
        [Command(requiresAuthority = false)]
        public void AddNetworkPlayer(NetworkConnectionToClient conn)
        {
            if (NetworkPlayers.Contains(conn)) return;
            
            NetworkPlayers.Add(conn);
            
            OnConnectedNetworkPlayer?.Invoke(conn);
        }
        
        [Command(requiresAuthority = false)]
        public void RemoveNetworkPlayer(NetworkConnectionToClient conn)
        {
            NetworkPlayers.Remove(conn);
            
            OnDisconnectedNetworkPlayer?.Invoke(conn);
        }
        
        [Command(requiresAuthority = false)]
        public void AddGamePlayer(NetworkConnectionToClient conn)
        {
            if(GamePlayers.Contains(conn)) return;
            
            GamePlayers.Add(conn);
            
            OnConnectedGamePlayer?.Invoke(conn);
        }
        
        [Command(requiresAuthority = false)]
        public void RemoveGamePlayer(NetworkConnectionToClient conn)
        {
            GamePlayers.Remove(conn);
            
            OnDisconnectedGamePlayer?.Invoke(conn);
        }
    }
}