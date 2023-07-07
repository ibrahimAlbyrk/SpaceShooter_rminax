using UnityEngine;

namespace _Project.Scripts.Features
{
    using Spaceship;
    
    [CreateAssetMenu(menuName = "Features/Invisible Feature", order = 2)]
    public class InvisibleFeature_SO : Feature_SO
    {
        public override void OnStart(SpaceshipController ownedController)
        {
            OwnedController = ownedController;
            
            //TODO: Add Feature
        }

        public override void OnUpdate()
        {
        }

        public override void OnEnd()
        {
            //TODO: Remove Feature
        }
    }
}