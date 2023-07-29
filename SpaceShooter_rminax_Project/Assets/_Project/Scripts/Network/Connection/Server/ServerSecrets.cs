using System;
using System.Text;

namespace _Project.Scripts.Network.Connection
{
    public static class ServerSecrets
    {
        private static readonly byte[] keyByteArray = Encoding.UTF8.GetBytes($"{keyId}:{keySecret}");

        private const string keyId = "64bd2fdf-c508-406d-acc8-3914ef74d47d";
        private const string keySecret = "MmqLPuNkYjtwv2hyKznSBPXS5O5eXjtP";

        private const string projectId = "fd649886-e50c-40f8-9b6f-44d242c1fce8";
        private const string environmentId = "c1cfa2ac-01c2-4632-9810-7c13b32b5c71";

        public static readonly string REGION_ID = "1623dec4-214d-43a3-9507-83641f2a62fe";
        public static readonly string ALLOCATION_ID = "0ac699d1-9a0f-450d-8a1c-165687eb1860";

        public static readonly string FLEET_ID = "19112259-f7e9-4d35-86e4-f9417b8ea54b";

        public static readonly string KEY_BASE_64 = Convert.ToBase64String(keyByteArray);

        public static readonly string SERVER_URL =
            $"https://services.api.unity.com/multiplay/servers/v1/projects/{projectId}/environments/{environmentId}/servers";

        public static readonly string EXCHANGE_URL =
            $"https://services.api.unity.com/auth/v1/token-exchange?projectId={projectId}&environmentId={environmentId}";

        public static readonly string ALLOCATIONS_URL =
            $"https://multiplay.services.api.unity.com/v1/allocations/projects/{projectId}/environments/{environmentId}/fleets/{FLEET_ID}/allocations";
    }
}