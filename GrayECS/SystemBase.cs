using System;
using System.Collections.Generic;

namespace KSGFK
{
    public abstract class SystemBase
    {
        protected internal EntityManager Manager { get; internal set; }
        protected internal Entity UpdatedEntity { get; internal set; }

        public abstract IReadOnlyList<Type> RequiredComponent { get; }
        public abstract string Name { get; }

        public abstract void Update();
    }
}