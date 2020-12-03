using System;
using System.Collections.Generic;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using KSGFK;

namespace GrayECS.Benchmark
{
    public struct Rotation
    {
        public Quaternion Rot;
    }

    public struct Position
    {
        public Vector3 Pos;

        public static Position GetRand(Random rand)
        {
            var v = new Vector3(rand.Next(-100, 101), rand.Next(-100, 101), rand.Next(-100, 101));
            return new Position {Pos = v};
        }
    }

    public struct Velocity
    {
        public Vector3 V;

        public static Velocity GetRand(Random rand)
        {
            var v = new Vector3((float) rand.NextDouble() * 10 * rand.Next(-1, 2),
                (float) rand.NextDouble() * 10 * rand.Next(-1, 2),
                (float) rand.NextDouble() * 10 * rand.Next(-1, 2));
            return new Velocity {V = v};
        }
    }

    public class GameObject
    {
        public Position Pos { get; set; }
        public Rotation Rot { get; set; }
        private byte[] _fill = new byte[137];
        private byte[] _fill2 = new byte[13];
    }

    public class Player : GameObject
    {
        public Velocity V { get; set; }

        public void Move()
        {
            var mov = Vector3.Normalize(V.V) * BenchmarkEcs.DeltaTime;
            var p = Pos.Pos;
            p += mov;
            Pos = new Position {Pos = p};
        }
    }

    public class MoveSystem : SystemBase
    {
        private readonly Type[] _reqCom = {typeof(Position), typeof(Velocity)};
        public override IReadOnlyList<Type> RequiredComponent => _reqCom;
        public override string Name => nameof(MoveSystem);

        public override void Update()
        {
            var p = Manager.GetData<Position>(UpdatedEntity);
            var v = Manager.GetData<Velocity>(UpdatedEntity);
            var mov = Vector3.Normalize(v.V) * BenchmarkEcs.DeltaTime;
            p.Pos += mov;
            Manager.SetData(UpdatedEntity, p);
        }
    }

    public class BenchmarkEcs
    {
        public static float DeltaTime => 0.02f;
        public static int EntityCount => 10000;
        public static Random Rand { get; } = new Random();

        private World _world;
        private List<Player> _p;

        [GlobalSetup]
        public void Startup()
        {
            _world = new World();
            var go = _world.EntityManager.CreateArchetype(typeof(Position), typeof(Rotation), typeof(Velocity));
            for (var i = 0; i < EntityCount; i++)
            {
                var e = _world.EntityManager.CreateEntity(go);
                _world.EntityManager.SetData(e, Position.GetRand(Rand));
                _world.EntityManager.SetData(e, Velocity.GetRand(Rand));
            }

            _world.SystemManager.RegisterSystem<MoveSystem>();
            _world.BuildDependence();

            //------------------------

            var l = new List<Player>(EntityCount);
            for (var i = 0; i < EntityCount; i++)
            {
                var p = new Player {Pos = Position.GetRand(Rand), V = Velocity.GetRand(Rand)};
                l.Add(p);
            }

            _p = l;
        }

        [Benchmark]
        public void ECS() { _world.Update(); }

        [Benchmark]
        public void OOP()
        {
            foreach (var player in _p)
            {
                player.Move();
            }
        }
    }
}