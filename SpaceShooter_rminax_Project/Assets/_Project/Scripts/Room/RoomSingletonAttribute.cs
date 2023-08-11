using System;

namespace _Project.Scripts.Room
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]

    public sealed class RoomSingletonAttribute : Attribute
    {
    }
}