using UnityEngine;

namespace _Project.Scripts
{
    public static class SpawnUtilities
    {
        public static Vector3 GetSpawnPosition(float maxRange)
        {
            return Random.insideUnitSphere * maxRange;
        }
    }
}