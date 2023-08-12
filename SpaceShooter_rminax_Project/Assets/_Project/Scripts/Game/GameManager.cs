using Mirror;
using UnityEngine;
using System.Threading.Tasks;

namespace _Project.Scripts.Game
{
    using Data;
    using Room;
    using Network.Managers.Room;

    public class GameManager : RoomSingleton<GameManager>
    {
        [Header("Mod Settings")]
        [SerializeField] private ModManager _modManager;
        
        private ModType _modType;

        public ModType GetModType() => _modType;
        
        public MapGeneratorData GetData() => _modManager?.GetMapData(); 
        
        public override async void OnStartServer()
        {
            await Task.Delay(100);
            
            var room = SpaceRoomManager.Instance.GetRoomOfScene(gameObject.scene);

            _modType = room.IsServer ? ModType.OpenWorld : ModType.ShrinkingArea;
            
            _modManager?.Init(_modType);
            
            _modManager?.StartGameModOnServer();
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