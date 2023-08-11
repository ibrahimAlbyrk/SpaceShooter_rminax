using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Game
{
    public readonly struct ContainerData
    {
        public readonly Scene Scene;
        public readonly List<object> Objects;
        
        public ContainerData(Scene scene, List<object> objects)
        {
            Scene = scene;

            Objects = objects;
        }
        
        public bool HasSameTypeObject<T>() => Objects.Any(obj => obj.GetType() == typeof(T));

        public object GetObjectOfSameType<T>() => Objects.FirstOrDefault(obj => obj.GetType() == typeof(T));
        
        public bool HasScene(Scene scene) => scene == Scene;
    }
}