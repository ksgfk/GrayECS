using System;
using System.Collections.Generic;
using System.Linq;

namespace KSGFK
{
    public class EntityManager
    {
        private class EntityInfo
        {
            public Archetype Arch;
            public int IdInArch;
            public int Generation;

            public EntityInfo(Archetype arch, int idInArch, int generation)
            {
                Arch = arch;
                IdInArch = idInArch;
                Generation = generation;
            }

            public void Clear()
            {
                Arch = null;
                IdInArch = 0;
                Generation++;
            }
        }

        private readonly List<Archetype> _archetypes;
        private readonly List<SortedSet<int>> _entityPerArchGen;
        private readonly List<EntityInfo> _activeEntity;
        private readonly Queue<int> _emptyIdx;

        public int EntityCount => _activeEntity.Count;
        public IReadOnlyList<Archetype> Archetypes => _archetypes;

        public EntityManager()
        {
            _archetypes = new List<Archetype>();
            _entityPerArchGen = new List<SortedSet<int>>();
            _activeEntity = new List<EntityInfo>();
            _emptyIdx = new Queue<int>();
        }

        public Archetype CreateArchetype(params Type[] components)
        {
            var arch = new Archetype(_archetypes.Count, components);
            _archetypes.Add(arch);
            _entityPerArchGen.Add(new SortedSet<int>());
            return arch;
        }

        public Entity CreateEntity(Archetype archetype)
        {
            var idInArch = archetype.NewEntity();
            var idx = GetEmptyEntry();
            _activeEntity[idx].Arch = archetype;
            _activeEntity[idx].IdInArch = idInArch;
            _entityPerArchGen[archetype.UniqueId].Add(idx);
            return new Entity(idx, _activeEntity[idx].Generation);
        }

        public bool DestroyEntity(in Entity entity)
        {
            if (!CheckEntity(in entity)) return false;
            var info = _activeEntity[entity.Id];
            _entityPerArchGen[info.Arch.UniqueId].Remove(entity.Id);
            info.Arch.DeleteEntity(info.IdInArch);
            info.Clear();
            _emptyIdx.Enqueue(entity.Id);
            return true;
        }

        private int GetEmptyEntry()
        {
            int idx;
            if (_emptyIdx.Count == 0)
            {
                idx = _activeEntity.Count;
                _activeEntity.Add(new EntityInfo(null, -1, 0));
            }
            else
            {
                idx = _emptyIdx.Dequeue();
            }

            return idx;
        }

        public ComponentPointer[] GetComponents(in Entity entity)
        {
            if (!CheckEntity(in entity)) throw new ArgumentException();
            var info = _activeEntity[entity.Id];
            var arch = info.Arch;
            var ptrs = new ComponentPointer[arch.Components.Count];
            for (var i = 0; i < arch.Components.Count; i++)
            {
                var mem = arch.GetSharedData(info.IdInArch, i);
                var type = arch.Components[i];
                ptrs[i] = new ComponentPointer(mem, type);
            }

            return ptrs;
        }

        private bool CheckEntity(in Entity entity)
        {
            if (entity.Id < 0 || entity.Id >= _activeEntity.Count) return false;
            return entity.Generation == _activeEntity[entity.Id].Generation;
        }

        public IEnumerable<Entity> GetArchetypeGenEntities(Archetype archetype)
        {
            foreach (var idx in _entityPerArchGen[archetype.UniqueId])
            {
                yield return new Entity(idx, _activeEntity[idx].Generation);
            }
        }

        public T GetData<T>(in Entity entity) where T : struct
        {
            if (!CheckEntity(in entity)) throw new ArgumentException();
            var info = _activeEntity[entity.Id];
            return info.Arch.GetData<T>(info.IdInArch);
        }

        public void SetData<T>(in Entity entity, T data) where T : struct
        {
            if (!CheckEntity(in entity)) throw new ArgumentException();
            var info = _activeEntity[entity.Id];
            info.Arch.SetData(data, info.IdInArch);
        }
    }
}