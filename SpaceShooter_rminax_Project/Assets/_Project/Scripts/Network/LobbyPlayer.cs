using System;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network
{
    using Managers.Room;
    
    [RequireComponent(typeof(NetworkMatch))]
    public class LobbyPlayer : NetIdentity
    {
        public static LobbyPlayer localLobbyPlayer;
        
        [SyncVar] public string Username;
        
        public Guid RoomID;
        
        [Command]
        public void CMD_SetRoomID(Guid roomID) => RPC_SetRoomID(roomID);

        [ClientRpc]
        private void RPC_SetRoomID(Guid roomID)
        {
            RoomID = roomID;
        }

        [Command]
        public void CMD_SetUsername(string username) => Username = username;

        private void Start()
        {
            if (isLocalPlayer)
                localLobbyPlayer = this;
        }
    }
}