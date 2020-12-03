using System;
using System.Collections.Generic;

namespace KSGFK
{
    public class SystemManager
    {
        private readonly List<SystemBase> _systems;
        private readonly Dictionary<string, int> _name2IdX;

        public IReadOnlyList<SystemBase> Systems => _systems;

        public SystemBase this[string name] => _systems[_name2IdX[name]];

        public SystemManager()
        {
            _systems = new List<SystemBase>();
            _name2IdX = new Dictionary<string, int>();
        }

        public void RegisterSystem<T>() where T : SystemBase, new()
        {
            var instance = new T();
            if (_name2IdX.ContainsKey(instance.Name)) throw new ArgumentException();
            var idx = _systems.Count;
            _systems.Add(instance);
            _name2IdX.Add(instance.Name, idx);
        }
    }
}