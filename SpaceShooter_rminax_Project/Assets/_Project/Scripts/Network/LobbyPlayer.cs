using System;
using _Project.Scripts.Network.Managers;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network
{
    [RequireComponent(typeof(NetworkMatch))]
    public class LobbyPlayer : NetworkBehaviour
    {
        public static LobbyPlayer localLobbyPlayer;

        private NetworkMatch _networkMatch;
        
        [SyncVar] public string Username;
        
        public Guid RoomID;
        
        [Command]
        public void CMD_SetRoomID(Guid roomID) => RPC_SetRoomID(roomID);

        [ClientRpc]
        private void RPC_SetRoomID(Guid roomID)
        {
            RoomID = roomID;
            _networkMatch.matchId = roomID;
        }

        [Command]
        public void CMD_SetUsername(string username) => Username = username;
        
        private void Start()
        {
            if (isLocalPlayer)
                localLobbyPlayer = this;

            _networkMatch = GetComponent<NetworkMatch>();
        }
    }
}