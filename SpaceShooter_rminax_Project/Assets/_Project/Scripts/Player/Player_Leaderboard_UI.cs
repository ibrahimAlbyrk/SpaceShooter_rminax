using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace _Project.Scripts.Player
{
    using Game;
    
    [RequireComponent(typeof(NetworkIdentity))]
    public class Player_Leaderboard_UI : NetworkBehaviour
    {
        [SerializeField] private GameObject _leaderboardFieldPrefab;

        [SerializeField] private Transform _leaderboardContent;
        
        private readonly Dictionary<string, PlayerLeaderboardField> _leaderboard = new();

        [Command]
        private void CMD_RequestLeaderboard()
        {
            T_RPC_ReceiveLeaderboard(LeaderboardManager.Instance.GetLeaderboard());
        }

        [TargetRpc]
        private void T_RPC_ReceiveLeaderboard(List<PlayerScoreData> leaderboard)
        {
            RenderLeaderboard(leaderboard);
        }

        private void RenderLeaderboard(List<PlayerScoreData> leaderboard)
        {
            foreach (var username in _leaderboard.Keys.ToArray())
            {
                var field = _leaderboard[username];

                Destroy(field.gameObject);
                
                _leaderboard.Remove(username);
            }

            foreach (var data in leaderboard)
            {
                var field = Instantiate(_leaderboardFieldPrefab, _leaderboardContent).GetComponent<PlayerLeaderboardField>();
                field.Init(data.Username, data.Score);
                
                _leaderboard.Add(data.Username, field);
            }
        }

        private void OpenLeaderboard()
        {
            CMD_RequestLeaderboard();
            
            _leaderboardContent.gameObject.SetActive(true);
        }

        private void CloseLeaderboard()
        {
            _leaderboardContent.gameObject.SetActive(false);
        }

        private void UpdateLeaderboard()
        {
            if (!_leaderboardContent.gameObject.activeSelf) return;
            
            CMD_RequestLeaderboard();
        }
        
        [ClientCallback]
        private void OnDestroy()
        {
            LeaderboardManager.Instance.OnLeaderboardUpdated -= UpdateLeaderboard;
        }

        [ClientCallback]
        private void Start()
        {
            CloseLeaderboard();

            LeaderboardManager.Instance.OnLeaderboardUpdated += UpdateLeaderboard;
        }

        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;
            
            if(Input.GetKeyDown(KeyCode.Tab))
                OpenLeaderboard();
            
            if(Input.GetKeyUp(KeyCode.Tab))
                CloseLeaderboard();
        }
    }
}