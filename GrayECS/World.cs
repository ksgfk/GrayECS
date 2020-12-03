using System.Collections.Generic;
using System.Linq;

namespace KSGFK
{
    public class World
    {
        private readonly EntityManager _entityManager;
        private readonly SystemManager _systemManager;
        private readonly List<List<Archetype>> _sysExeTable;

        public EntityManager EntityManager => _entityManager;
        public SystemManager SystemManager => _systemManager;

        public World()
        {
            _entityManager = new EntityManager();
            _systemManager = new SystemManager();
            _sysExeTable = new List<List<Archetype>>();
        }

        public void BuildDependence()
        {
            var e = _entityManager.Archetypes;
            var sys = _systemManager.Systems;

            _sysExeTable.Clear();
            foreach (var s in sys)
            {
                var req = s.RequiredComponent;
                var ie = from arch in e
                    let coms = arch.Components
                    let isMatch = req.All(coms.Contains)
                    where isMatch select arch;
                var exe = new List<Archetype>(ie);
                _sysExeTable.Add(exe);
            }
        }

        public void Update()
        {
            for (var i = 0; i < _systemManager.Systems.Count; i++)
            {
                var sys = _systemManager.Systems[i];
                var exe = _sysExeTable[i];
                sys.Manager = EntityManager;
                foreach (var archetype in exe)
                {
                    foreach (var entity in _entityManager.GetArchetypeGenEntities(archetype))
                    {
                        sys.UpdatedEntity = entity;
                        sys.Update();
                    }
                }
            }
        }
    }
}