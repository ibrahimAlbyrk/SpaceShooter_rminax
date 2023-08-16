using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class LobbyPlayer : NetIdentity
    {
        public static LobbyPlayer LocalLobbyPlayer;
        
        public GameObject ShipPrefab;
        
        [ClientCallback]
        private void Start()
        {
            if (isLocalPlayer)
                LocalLobbyPlayer = this;
            
            CMD_SetShip("Ship_1_Controller");
        }

        [Command(requiresAuthority = false)]
        public void CMD_SetShip(string shipName)
        {
            ShipPrefab = Resources.Load<GameObject>($"SpawnablePrefabs/Characters/Ships/{shipName}");
        }
    }
}