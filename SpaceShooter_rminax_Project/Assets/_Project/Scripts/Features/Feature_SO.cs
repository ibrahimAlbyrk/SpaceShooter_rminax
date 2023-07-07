using UnityEngine;

namespace _Project.Scripts.Features
{
    using Spaceship;
    
    public abstract class Feature_SO : ScriptableObject
    {
        [Header("Feature Settings")]
        public string Name = "New Feature";
        public float Duration = 5f;
        
        public SpaceshipController OwnedController { get; set; }

        public abstract void OnStart(SpaceshipController ownedController);

        public abstract void OnUpdate();

        public abstract void OnEnd();
    }
}