using System;
using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace _Project.Scripts.Game
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class LeaderboardManager : NetworkBehaviour
    {
        public event Action OnLeaderboardUpdated;
        
        [SerializeField] private int maxRanking = 10;
        
        private static List<PlayerScoreData> scoreEntries = new();

        public static LeaderboardManager Instance;

        private void Awake()
        {
            print("test");
            
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        public List<PlayerScoreData> GetLeaderboard() => scoreEntries;

        #region Command Methods
        
        [Command(requiresAuthority = false)]
        public void CMD_AddPlayer(string username) => AddPlayer(username);
        
        [Command(requiresAuthority = false)]
        public void CMD_RemovePlayer(string username) => RemovePlayer(username);

        [Command(requiresAuthority = false)]
        public void CMD_AddScore(string username, int value) => AddScore(username, value);

        [Command(requiresAuthority = false)]
        public void CMD_SetScore(string username, int value) => SetScore(username, value);

        [Command(requiresAuthority = false)]
        public void CMD_SubtractScore(string username, int value) => SubtractScore(username, value);

        #endregion

        #region Server methods

        [ServerCallback]
        private void AddPlayer(string username)
        {
            if (CheckEntry(username)) return;
            
            scoreEntries.Add(new PlayerScoreData{Username = username, Score =  0});
            
            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }
        
        [ServerCallback]
        private void RemovePlayer(string username)
        {
            if (!CheckEntry(username, out var data)) return;

            scoreEntries.Remove(data);
            
            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }

        [ServerCallback]
        private void AddScore(string username, int value)
        {
            if (!CheckEntry(username, out var data)) return;
            
            var index = scoreEntries.IndexOf(data);

            scoreEntries[index] = new PlayerScoreData
            {
                Username = username, Score = data.Score + value
            };

            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }

        [ServerCallback]
        private void SetScore(string username, int value)
        {
            if (!CheckEntry(username, out var data)) return;
            
            var index = scoreEntries.IndexOf(data);

            scoreEntries[index] = new PlayerScoreData
            {
                Username = username, Score = Mathf.Max(value, 0)
            };

            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }

        [ServerCallback]
        private void SubtractScore(string username, int value)
        {
            if (!CheckEntry(username, out var data)) return;

            var index = scoreEntries.IndexOf(data);

            scoreEntries[index] = new PlayerScoreData
            {
                Username = username, Score = Mathf.Max(data.Score - value, 0)
            };
            
            OrderToScoreEntries();
            
            OnLeaderboardUpdated?.Invoke();
        }

        #endregion

        #region Utilities

        private bool CheckEntry(string username)
        {
            return CheckEntry(username, out _);
        }
        
        private bool CheckEntry(string username, out PlayerScoreData data)
        {
            data = scoreEntries.FirstOrDefault(entry => entry.Username == username);

            return data != null;
        }

        private void OrderToScoreEntries()
        {
            scoreEntries = scoreEntries.OrderByDescending(data => data.Score).Take(maxRanking).ToList();
        }

        #endregion
    }

    public class PlayerScoreData
    {
        public string Username;
        public int Score;
    }
}