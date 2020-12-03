using System;
using System.Numerics;
using KSGFK;
using NUnit.Framework;

namespace GrayECS.Test
{
    public class TestArchetype
    {
        [Test]
        public unsafe void Test()
        {
            var arch = new Archetype(-1,
                new[]
                {
                    typeof(int),
                    typeof(double),
                    typeof(Vector4)
                });
            Console.WriteLine(arch);
            var chunk = arch.Chunks;

            const int newCount = 512;
            for (var i = 0; i < newCount; i++)
            {
                var idx = arch.NewEntity();
                arch.GetDataRefUnsafe<int>(idx, 0) = i;
            }

            Assert.True(arch.ActiveEntityCount == newCount);
            for (var i = 0; i < arch.ActiveEntityCount; i++)
            {
                const int j = 0;
                var v = i * 4;
                var x = chunk[j].Data[v];
                var y = chunk[j].Data[v + 1];
                var z = chunk[j].Data[v + 2];
                var w = chunk[j].Data[v + 3];
                var value = (x & 0xFF) | ((y & 0xFF) << 8) | ((z & 0xFF) << 16) | ((w & 0xFF) << 24);
                Assert.True(value == v / 4);
            }

            arch.DeleteEntity(256);
            Assert.True(arch.GetData<int>(256) == 511);
            Assert.True(arch.GetData<int>(arch.ActiveEntityCount - 1) == 510);

            arch.DeleteEntity(arch.ActiveEntityCount - 1);
            Assert.True(arch.GetData<int>(arch.ActiveEntityCount - 1) == 509);
            Assert.True(arch.GetData<int>(256) == 511);
        }
    }
}