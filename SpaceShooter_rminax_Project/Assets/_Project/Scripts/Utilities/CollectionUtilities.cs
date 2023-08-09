﻿using System;
using Mirror;
using System.Linq;
using System.Collections.Generic;

namespace _Project.Scripts.Utilities
{
    public static class CollectionUtilities
    {
        #region List

        public static void RemoveAll<T>(this List<T> list)
        {
            foreach (var element in list.ToList())
            {
                list.Remove(element);
            }
        }

        #endregion
        
        #region Array

        public static void RemoveAll<T>(this T[] array)
        {
            var list = array.ToList();

            for (var i = 0; i < list.Count; i++)
            {
                list.RemoveAt(i);
            }

            // ReSharper disable once RedundantAssignment
            array = list.ToArray();
        }

        #endregion

        #region IEnumerable

        public static T GetElement<T>(this IEnumerable<T> collection, int atIndex)
        {
            var index = 0;
            foreach (var element in collection)
            {
                if (index == atIndex) return element;
                
                index++;
            }
            
            throw new ArgumentOutOfRangeException(nameof(atIndex), $"Expected value less then {index}");
        }

        public static int GetLenght<T>(this IEnumerable<T> collection)
        {
            var lenght = 0;
            
            using var enumerator = collection.GetEnumerator();
            
            while (enumerator.MoveNext())
                checked { ++lenght; }

            return lenght;
        }

        public static SyncList<T> ToSyncList<T>(this IEnumerable<T> collection)
        {
            var syncList = new SyncList<T>();

            foreach (var element in collection)
            {
                syncList.Add(element);
            }

            return syncList;
        }

        #endregion
    }
}