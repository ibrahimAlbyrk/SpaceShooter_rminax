using System;
using System.Text;

namespace _Project.Scripts.Network.Connection
{
    public static class ServerSecrets
    {
        private const string keyId = "64bd2fdf-c508-406d-acc8-3914ef74d47d";
        private const string keySecret = "MmqLPuNkYjtwv2hyKznSBPXS5O5eXjtP";
        
        private static readonly byte[] keyByteArray = Encoding.UTF8.GetBytes($"{keyId}:{keySecret}");

        private const string projectId = "fd649886-e50c-40f8-9b6f-44d242c1fce8";
        private const string environmentId = "c1cfa2ac-01c2-4632-9810-7c13b32b5c71";
        
        public static readonly string KEY_BASE_64 = Convert.ToBase64String(keyByteArray);

        public static readonly string SERVER_URL =
            $"https://services.api.unity.com/multiplay/servers/v1/projects/{projectId}/environments/{environmentId}/servers";
        
        public static string GetExchangeURL(string fleetId) =>
            $"https://multiplay.services.api.unity.com/v1/allocations/projects/{projectId}/environments/{environmentId}/fleets/{fleetId}/allocations";
    }
}