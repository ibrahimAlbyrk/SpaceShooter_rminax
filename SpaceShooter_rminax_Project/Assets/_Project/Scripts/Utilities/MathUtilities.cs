using Unity.Mathematics;
using UnityEngine;

namespace _Project.Scripts.Utilities
{
    public static class MathUtilities
    {
        public static bool InDistance(float pointA, float pointB, float distance) =>
            math.abs(pointA - pointB) < distance;
        
        public static bool OutDistance(float pointA, float pointB, float distance) =>
            math.abs(pointA - pointB) > distance;

        public static bool InDistance(Vector3 pointA, Vector3 pointB, float distance) =>
            (pointA - pointB).sqrMagnitude < distance * distance;
        
        public static bool OutDistance(Vector3 pointA, Vector3 pointB, float distance) =>
            (pointA - pointB).sqrMagnitude > distance * distance;
    }
}