using System;

namespace _Project.Scripts.Network.Connection
{
    [Serializable]
    public struct Server
    {
        public int buildConfigurationID;
        public string buildConfigurationName;
        public string buildName;
        public bool deleted;
        public string fleetID;
        public string fleetName;
        public string hardwareType;
        public int id;
        public string ip;
        public int locationID;
        public string locationName;
        public int machineID;
        public int port;
        public string status; 
    }
}