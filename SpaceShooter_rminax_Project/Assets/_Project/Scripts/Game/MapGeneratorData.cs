using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Game.Data
{
    [CreateAssetMenu(menuName = "Game/Map Generator Data")]
    public class MapGeneratorData : ScriptableObject
    {
        [BoxGroup("General Settings")] public float GameAreaRadius = 1000f;

        [BoxGroup("AI Settings")] public GameObject[] AIPrefabs;
        
        [Space(10)]
        [Title("Fuel Station")]
        [BoxGroup("AI Settings")] public Vector2 AIFuelStationCountRange = new (1, 2);
        [BoxGroup("AI Settings")] public float AIFuelStationSpawnRange;
        [BoxGroup("AI Settings")] public float AIFuelStationPatrolRange = 100f;
        [BoxGroup("AI Settings")] public float AIFuelStationDetectionRange = 200f;
        
        [Space(10)]
        [Title("Area")]
        [BoxGroup("AI Settings")] public Vector2 AIAreaCountRange = new (10, 20);
        [BoxGroup("AI Settings")] public float AIAreaPatronRange = 200f;
        [BoxGroup("AI Settings")] public float AIAreaDetectionRange = 300f;
        
        [BoxGroup("Environment Settings")] public GameObject[] EnvironmentPrefabs;
        [BoxGroup("Environment Settings")] public float EnvironmentMinSpawnRange = 1000;
        [BoxGroup("Environment Settings")] public float EnvironmentMaxSpawnRange = 3000;
        [BoxGroup("Environment Settings")] public int EnvironmentCount = 10;

        [BoxGroup("Meteor Settings")] public GameObject[] MeteorPrefabs;
        [BoxGroup("Meteor Settings")] public int MeteorCount = 300;

        [BoxGroup("Fuel Station Settings")] public GameObject[] FuelStationPrefabs;
        [BoxGroup("Fuel Station Settings")] public int FuelStationCount = 10;

        [BoxGroup("Feature Settings")] public GameObject[] FeaturePrefabs;
        [BoxGroup("Feature Settings")] public int FeatureCount = 10;
    }
}