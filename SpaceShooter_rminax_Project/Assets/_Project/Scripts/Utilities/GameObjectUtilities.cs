using UnityEngine;

namespace _Project.Scripts.Utilities
{
    public static class GameObjectUtilities
    {
        public static void Destroy(this GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    }
}