using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Extensions
{
    using Game;
    
    public static class GameObjectExtensions
    {
        private static Scene _scene;

        public static GameContainerHelper GameContainer(this GameObject gameObject)
        {
            return new GameContainerHelper(gameObject.scene);
        }
    }
}