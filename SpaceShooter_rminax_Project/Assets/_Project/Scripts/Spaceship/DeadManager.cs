using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Spaceship
{
    using Game;
    using Network;
    using Game.Mod.ShrinkingArea;
    
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
        
        [ServerCallback]
        public void CloseGameOver() => RPC_CloseGameOver();

        [TargetRpc]
        private void RPC_CloseGameOver()
        {
            _deadTitleContent.SetActive(false);
            
            _deadText.SetActive(false);
            _respawnText.SetActive(false);
            
            _exitGameButton.onClick.RemoveAllListeners();
        }
        
        [ServerCallback]
        public void ShowGameOver(ModType modType) => RPC_ShowGameOver(modType);

        [TargetRpc]
        private void RPC_ShowGameOver(ModType modType)
        {
            _deadTitleContent.SetActive(true);
            
            _deadText.SetActive(true);
            _respawnText.SetActive(modType == ModType.OpenWorld);
            
            _exitGameButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance.GetModType() != ModType.OpenWorld)
                {
                    if (LeaderboardManager.Instance != null)
                        LeaderboardManager.Instance.CMD_RemovePlayer(_controller.Username);
            
                    if (ShrinkingAreaSystem.Instance != null)
                        ShrinkingAreaSystem.Instance.CMD_RemovePlayer(_controller);   
                }
                
                NetworkClient.Disconnect();
            });
        }

        private void Awake() => _controller = GetComponent<SpaceshipController>();
    }
}