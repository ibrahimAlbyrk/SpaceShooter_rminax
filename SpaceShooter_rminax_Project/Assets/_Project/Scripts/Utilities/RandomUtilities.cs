using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace _Project.Scripts.Utilities
{
    public static class RandomUtilities
    {
        public static T GetRandomElement<T>(this IEnumerable<T> collection)
        {
            var array = collection as T[] ?? collection.ToArray();
            
            var collectionLenght = array.Length;
            
            var randomIndex = Random.Range(0, collectionLenght);

            return array[randomIndex];
        }
    }
}