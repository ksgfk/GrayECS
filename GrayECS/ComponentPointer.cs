using System;

namespace KSGFK
{
    public readonly struct ComponentPointer
    {
        public readonly Memory<byte> Memory;
        public readonly Type Type;

        public ComponentPointer(Memory<byte> memory, Type type)
        {
            Memory = memory;
            Type = type;
        }
    }
}