﻿using _Project.Scripts.Game.Mod.ShrinkingArea;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Spaceship
{
    using Game;
    using Network;
    
    public class DeadManager : NetIdentity
    {
        [Header("Text Settings")]
        [SerializeField] private GameObject _deadText;
        [SerializeField] private GameObject _respawnText;

        [Space(20)]
        [Header("Button Settings")]
        [SerializeField] private Button _exitGameButton;

        [Space(20)]
        [Header("Content Settings")]
        [SerializeField] private GameObject _deadTitleContent;

        private SpaceshipController _controller;
        
        public void CloseGameOver() => CMD_CloseGameOver();

        [Command(requiresAuthority = false)]
        private void CMD_CloseGameOver() => RPC_CloseGameOver();

        [TargetRpc]
        private void RPC_CloseGameOver()
        {
            _deadTitleContent.SetActive(false);
            
            _deadText.SetActive(false);
            _respawnText.SetActive(false);
            
            _exitGameButton.onClick.RemoveAllListeners();
        }
        
        public void ShowGameOver(ModType modType) => CMD_ShowGameOver(modType);

        [Command(requiresAuthority = false)]
        private void CMD_ShowGameOver(ModType modType) => RPC_ShowGameOver(modType);

        [TargetRpc]
        private void RPC_ShowGameOver(ModType modType)
        {
            _deadTitleContent.SetActive(true);
            
            _deadText.SetActive(true);
            _respawnText.SetActive(modType == ModType.OpenWorld);
            
            _exitGameButton.onClick.AddListener(() =>
            {
                if (LeaderboardManager.Instance != null)
                    LeaderboardManager.Instance.CMD_RemovePlayer(_controller.Username);
            
                if (ShrinkingAreaSystem.Instance != null)
                    ShrinkingAreaSystem.Instance.CMD_RemovePlayer(_controller);
                
                NetworkClient.Disconnect();
            });
        }

        private void Awake() => _controller = GetComponent<SpaceshipController>();
    }
}