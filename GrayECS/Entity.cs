using System;

namespace KSGFK
{
    public readonly struct Entity : IEquatable<Entity>
    {
        public readonly int Id;
        public readonly int Generation;

        public Entity(int id, int generation)
        {
            Id = id;
            Generation = generation;
        }

        public bool Equals(Entity other) { return Id == other.Id && Generation == other.Generation; }

        public override bool Equals(object obj) { return obj is Entity other && Equals(other); }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ Generation;
            }
        }

        public override string ToString() { return $"[{Id}:{Generation}]"; }
    }
}