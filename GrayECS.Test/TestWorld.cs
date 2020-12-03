using System;
using System.Collections.Generic;
using System.Numerics;
using KSGFK;
using NUnit.Framework;

namespace GrayECS.Test
{
    public class PrintInfo : SystemBase
    {
        private readonly Type[] _reqCom = {typeof(int)};

        public override IReadOnlyList<Type> RequiredComponent => _reqCom;
        public override string Name => nameof(PrintInfo);

        public override void Update()
        {
            var data = Manager.GetData<int>(UpdatedEntity);
            Console.WriteLine($"[{Name}],{UpdatedEntity.ToString()}:{data.ToString()}");
        }
    }

    public class PrintVec3 : SystemBase
    {
        private readonly Type[] _reqCom = {typeof(Vector3)};

        public override IReadOnlyList<Type> RequiredComponent => _reqCom;
        public override string Name => nameof(PrintVec3);

        public override void Update()
        {
            var data = Manager.GetData<Vector3>(UpdatedEntity);
            Console.WriteLine($"[{Name}],{UpdatedEntity.ToString()}:{data.ToString()}");
        }
    }

    public class TestWorld
    {
        [Test]
        public void Test()
        {
            var world = new World();
            var a = world.EntityManager.CreateArchetype(typeof(int), typeof(Vector3));
            var b = world.EntityManager.CreateArchetype(typeof(int), typeof(Matrix4x4));
            const int cnt = 2;
            for (var i = 0; i < cnt; i++)
            {
                var e1 = world.EntityManager.CreateEntity(a);
                world.EntityManager.SetData(in e1, i);
                world.EntityManager.SetData(in e1, new Vector3(0, 0, i));

                var e2 = world.EntityManager.CreateEntity(b);
                world.EntityManager.SetData(in e2, int.MaxValue - i);
            }

            world.SystemManager.RegisterSystem<PrintInfo>();
            world.SystemManager.RegisterSystem<PrintVec3>();
            world.BuildDependence();
            world.Update();
        }
    }
}