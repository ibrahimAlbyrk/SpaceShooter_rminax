using Mirror;
using UnityEngine;

namespace _Project.Scripts.Game
{
    using Data;
    using Room;
    using Network;

    [RoomSingleton]
    public class GameManager : NetIdentity
    {
        [Header("Mod Settings")]
        [SerializeField] private ModManager _modManager;
        [SerializeField] private ModType _modType;

        public ModType GetModType() => _modType;
        
        public MapGeneratorData GetData() => _modManager?.GetMapData();
        
        public override void OnStartServer()
        {
            _modManager?.Init(_modType);
            
            _modManager?.StartGameModOnServer();

            RPC_StartOnClient();
        }

        [Command(requiresAuthority = false)]
        private void RPC_StartOnClient()
        {
            _modManager?.StartGameModOnClient();
        }

        [ServerCallback]
        private void Update()
        {
            _modManager.RunGameMod();
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