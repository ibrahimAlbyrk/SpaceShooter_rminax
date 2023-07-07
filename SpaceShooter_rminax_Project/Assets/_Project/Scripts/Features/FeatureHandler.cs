using UnityEngine;

namespace _Project.Scripts.Features
{
    public class FeatureHandler : MonoBehaviour
    {
        [SerializeField] private Feature_SO _feature;

        public Feature_SO GetFeature() => _feature;
    }
}