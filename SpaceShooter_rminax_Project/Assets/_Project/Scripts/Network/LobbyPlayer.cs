using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network
{
    [RequireComponent(typeof(NetworkMatch))]
    public class LobbyPlayer : NetIdentity
    {
        public static LobbyPlayer LocalLobbyPlayer;
        
        [SyncVar] public string Username;

        [Command]
        public void CMD_SetUsername(string username) => Username = username;

        private void Start()
        {
            if (isLocalPlayer)
                LocalLobbyPlayer = this;
        }
    }
}