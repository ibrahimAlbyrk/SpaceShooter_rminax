namespace _Project.Scripts.Features
{
    public class InvisibleFeature : SpaceshipFeature
    {
        public override void AddFeature()
        {
            //TODO: Add Feature
            
            _featureCoroutine = StartCoroutine(RemoveFeatureToDuration());
        }

        public override void RemoveFeature()
        {
            //TODO: Remove Feature
        }
    }
}