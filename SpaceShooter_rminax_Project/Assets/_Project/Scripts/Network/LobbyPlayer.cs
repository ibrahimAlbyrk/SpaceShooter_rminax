using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class LobbyPlayer : NetIdentity
    {
        public static LobbyPlayer LocalLobbyPlayer;

        [SyncVar(hook = nameof(OnChangedShipName))]
        public string ShipName;

        public GameObject ShipPrefab;
        
        [ClientCallback]
        private void Start()
        {
            if (isLocalPlayer)
                LocalLobbyPlayer = this;

            var shipName = PlayerPrefs.HasKey("ShipName") ? PlayerPrefs.GetString("ShipName") : null;
            
            CMD_SetShip(shipName);
        }

        private void OnChangedShipName(string _, string newName)
        {
            print(newName);
            ShipPrefab = Resources.Load<GameObject>($"SpawnablePrefabs/Characters/Ships/{newName}");
        }

        [Command]
        private void CMD_SetShip(string shipName)
        {
            ShipName = shipName;
        }
    }
}