namespace _Project.Scripts.Network.Managers.Room
{
    [System.Serializable]
    public enum ServerRoomState : byte
    {
        Create,
        Join,
        Remove,
        Exit,
    }
    
    [System.Serializable]
    public enum ClientRoomState : byte
    {
        Created,
        Joined,
        Removed,
        Exited,
        Fail
    }
}