using System;
using Mirror;
using UnityEngine;

namespace _Project.Scripts.Network.Messages
{
    public struct ServerRoomMessage : NetworkMessage
    {
        public ServerRoomOperation ServerRoomOperation;
        public Guid RoomID;
    }

    [Serializable]
    public struct ClientRoomMessage : NetworkMessage
    {
        public ClientRoomOperation ClientRoomOperation;
        public Guid RoomID;
        public RoomInfo[] RoomInfos;
        public PlayerInfo[] PlayerInfos;
    }
    
    [Serializable]
    public struct RoomInfo
    {
        public Guid RoomID;
        public byte Players;
        public byte MaxPlayers;
        public bool IsServer;
    }
    
    [Serializable]
    public struct PlayerInfo
    {
        public string Username;
        public bool IsReady;
        public Guid RoomID;
    }
    
    [Serializable]
    public struct RoomPlayerData
    {
        public string Username;
        public int Score;
    }
    
    /// <summary>
    /// Match operation to execute on the server
    /// </summary>
    public enum ServerRoomOperation : byte
    {
        None,
        Create,
        Cancel,
        Start,
        Join,
        Leave,
        Ready
    }
    
    /// <summary>
    /// Match operation to execute on the client
    /// </summary>
    public enum ClientRoomOperation : byte
    {
        None,
        List,
        Created,
        Cancelled,
        Joined,
        Departed,
        UpdateRoom,
        Started
    }
}