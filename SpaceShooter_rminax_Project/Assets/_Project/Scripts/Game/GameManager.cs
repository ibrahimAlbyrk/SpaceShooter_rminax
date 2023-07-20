using Mirror;
using UnityEngine;

namespace _Project.Scripts.Game
{
    using Data;
    using Network;

    public class GameManager : NetIdentity
    {
        public static GameManager Instance;
        
        [Header("Mod Settings")]
        [SerializeField] private ModManager _modManager;
        [SerializeField] private ModType _modType;

        public MapGeneratorData GetData() => _modManager?.GetMapData();
        
        public override void OnStartServer()
        {
            _modManager.Init(_modType);
            
            _modManager.StartGameMod();
        }

        private void Awake()
        {
            Instance = this;
        }
        
        [ServerCallback]
        private void FixedUpdate()
        {
            _modManager.FixedRunGameMod();
        }
    }

    public enum ModType
    {
        OpenWorld,
        ShrinkingArea
    }
}