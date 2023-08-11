using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Game
{
    public static class GameContainer
    {
        private static readonly List<ContainerData> _data = new();

        public static bool Add<T>(Scene scene, T element) where T : class
        {
            var container = _data.FirstOrDefault(d => d.HasScene(scene));
            if (container.Scene == default)
            {
                var containerData = new ContainerData
                (
                    scene,
                    new List<object> { element }
                );

                _data.Add(containerData);
                return true;
            }

            if (container.HasSameTypeObject<T>()) return false;

            container.Objects.Add(element);

            return true;
        }

        public static bool Remove<T>(Scene scene) where T : class
        {
            var container = _data.FirstOrDefault(d => d.HasScene(scene));

            var obj = container.GetObjectOfSameType<T>();

            return container.Scene != default && container.Objects.Remove(obj);
        }

        public static T Get<T>(Scene scene) where T : class
        {
            var container = _data.FirstOrDefault(d => d.Scene == scene);

            if (container.Scene == default) return default;

            var obj = container.GetObjectOfSameType<T>();

            return obj as T;
        }
    }

    public class GameContainerHelper
    {
        private readonly Scene _scene;

        public GameContainerHelper(Scene scene)
        {
            _scene = scene;
        }

        public bool Add<T>(T value) where T : class
        {
            return GameContainer.Add(_scene, value);
        }

        public bool Remove<T>(T element = null) where T : class
        {
            return GameContainer.Remove<T>(_scene);
        }

        public T Get<T>() where T : class
        {
            return GameContainer.Get<T>(_scene);
        }
    }
}