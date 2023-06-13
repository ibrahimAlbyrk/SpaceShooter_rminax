using System;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network
{
    [RequireComponent(typeof(NetworkMatch))]
    public class Player_NETWORK : NetworkBehaviour
    {
        public static Player_NETWORK LocalPlayer;

        private NetworkMatch _networkMatch;

        #region Sync Variables

        [SyncVar] public string Username;

        [SyncVar(hook = nameof(OnChangedRoomID))] public Guid RoomID;

        private void OnChangedRoomID(Guid _, Guid newRoomID)
        {
            _networkMatch.matchId = newRoomID;
        }

        #endregion

        [Command]
        public void SetRoomID(Guid roomID)
        {
            RoomID = roomID;
        }
        
        private void Start()
        {
            if (isLocalPlayer)
                LocalPlayer = this;

            _networkMatch = GetComponent<NetworkMatch>();
        }
    }
}