using UnityEngine;

namespace _Project.Scripts
{
    public static class SpawnUtilities
    {
        public static Vector3 GetSpawnPosition(float maxRange) => Random.insideUnitSphere * maxRange;

        public static Quaternion GetSpawnRotation() => Random.rotation;
    }
}