namespace _Project.Scripts.Network.Connection
{
    [System.Serializable]
    public class QueueAllocationRequest {
        public string allocationId;
        public int buildConfigurationId;
        public string payload;
        public string regionId;
        public bool restart;
    }
}