using UnityEngine;

namespace _Project.Scripts.Game.Data
{
    [CreateAssetMenu(menuName = "Game/Map Generator Data")]
    public class MapGeneratorData : ScriptableObject
    {
        [Header("General Settings")]
        public float GameAreaRadius = 1000f;

        [Space(10)]
        [Header("AI Settings")]
        public GameObject[] AIPrefabs;
        public Vector2 AIFuelStationCountRange = new (1, 2);
        public float AIFuelStationSpawnRange;
        public Vector2 AIAreaCountRange = new (10, 20);
        public float AIAreaPatronRange = 200f;
        public float AIFuelStationPatronRange = 100f;

        [Space(10)]
        [Header("Meteor Settings")]
        public GameObject[] MeteorPrefabs;
        public int MeteorCount = 300;

        [Space(10)]
        [Header("Fuel Station Settings")]
        public GameObject[] FuelStationPrefabs;
        public int FuelStationCount = 10;

        [Space(10)]
        [Header("Feature Settings")]
        public GameObject[] FeaturePrefabs;
        public int FeatureCount = 10;
    }
}