using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Network.Managers;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Game
{
    public class Leaderboard : NetworkBehaviour
    {
        #region Serialize Variables

        [SerializeField] private GameObject _leaderboardPlayerPrefab;
        
        [SerializeField] private Transform _playersContent;

        #endregion

        #region Private Veriables

        private Dictionary<NetworkConnectionToClient, PlayerLeaderboardField> _leaderboard;

        #endregion

        #region Command Methods

        [Command(requiresAuthority = false)]
        public void CMD_AddScoreToConnection(NetworkConnectionToClient conn, int value) => RPC_AddScoreToConnection(conn, value);
        
        [Command(requiresAuthority = false)]
        public void CMD_SetScoreToConnection(NetworkConnectionToClient conn, int value) => RPC_SetScoreToConnection(conn, value);
        
        [Command(requiresAuthority = false)]
        public void CMD_RemoveScoreToConnection(NetworkConnectionToClient conn, int value) => RPC_RemoveScoreToConnection(conn, value);

        #endregion

        #region Client Methods

        private void RPC_AddScoreToConnection(NetworkConnectionToClient conn, int value)
        {
            var leaderboardField = _leaderboard[conn];
            
            if (leaderboardField == null) return;

            var currentScore = leaderboardField.Score;
            
            leaderboardField.SetScore(currentScore + value);
        }
        
        private void RPC_SetScoreToConnection(NetworkConnectionToClient conn, int value)
        {
            var leaderboardField = _leaderboard[conn];
            
            if (leaderboardField == null) return;

            leaderboardField.SetScore(value);
        }
        
        private void RPC_RemoveScoreToConnection(NetworkConnectionToClient conn, int value)
        {
            var leaderboardField = _leaderboard[conn];
            
            if (leaderboardField == null) return;

            var currentScore = leaderboardField.Score;

            var score = Mathf.Max(currentScore - value, 0);
            
            leaderboardField.SetScore(score);
        }

        #endregion

        #region Private Methods

        private void AddConnectionToLeaderboard(NetworkConnectionToClient conn)
        {
            if (_leaderboard.ContainsKey(conn)) return;

            var playerFieldInstant = Instantiate(_leaderboardPlayerPrefab, _playersContent);

            var playerLeaderboardField = playerFieldInstant.GetComponent<PlayerLeaderboardField>();
            
            playerLeaderboardField.Init($"Player {_leaderboard.Count}");
            playerLeaderboardField.UpdateUI();
            
            _leaderboard.Add(conn, playerLeaderboardField);
        }
        
        private void RemoveConnectionToLeaderboard(NetworkConnectionToClient conn)
        {
            if (!_leaderboard.Keys.Contains(conn)) return;

            if (!_leaderboard.Remove(conn, out var leaderboardField)) return;
            
            Destroy(leaderboardField.gameObject);
        }

        #endregion

        #region Base Methods

        private void Awake()
        {
            SpaceLobbyManager.Instance.OnConnectedGamePlayer += AddConnectionToLeaderboard;
            SpaceLobbyManager.Instance.OnDisconnectedGamePlayer += RemoveConnectionToLeaderboard;
        }

        private void OnDestroy()
        {
            SpaceLobbyManager.Instance.OnConnectedGamePlayer -= AddConnectionToLeaderboard;
            SpaceLobbyManager.Instance.OnDisconnectedGamePlayer -= RemoveConnectionToLeaderboard;
        }

        #endregion
    }
}