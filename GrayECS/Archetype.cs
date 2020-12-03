using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KSGFK
{
    public class Archetype
    {
        public const int ChunkSize = 16384;

        public class Chunk
        {
            public readonly byte[] Data;

            public Chunk() { Data = new byte[ChunkSize]; }
        }

        private readonly int _uniqueId;
        private readonly List<Chunk> _chunks;
        private readonly Type[] _types;
        private readonly int[] _offsets;
        private readonly int[] _sizes;
        private readonly int _chunkCapacity;
        private readonly int _entitySize;

        private int _entityCount;

        public int UniqueId => _uniqueId;
        public int ChunkCapacity => _chunkCapacity;
        public int ActiveEntityCount => _entityCount;
        public int ChunkCount => _chunks.Count;
        public IReadOnlyList<Chunk> Chunks => _chunks;
        public IReadOnlyList<Type> Components => _types;

        public Archetype(int id, IEnumerable<Type> components)
        {
            _uniqueId = id;
            _chunks = new List<Chunk>();
            _types = components.ToArray();
            _offsets = new int[_types.Length];
            _sizes = new int[_types.Length];

            for (var i = 0; i < _types.Length; i++)
            {
                _sizes[i] = Marshal.SizeOf(_types[i]);
            }

            _entitySize = _sizes.Sum();
            _chunkCapacity = ChunkSize / _entitySize;
            if (_chunkCapacity <= 0) throw new ArgumentException();
            var currentOffset = 0;
            for (var i = 0; i < _types.Length; i++)
            {
                _offsets[i] = currentOffset;
                currentOffset += _sizes[i] * _chunkCapacity;
            }

            _entityCount = 0;
        }

        /// <summary>
        /// Allocate a new entity
        /// </summary>
        /// <returns>Entity Index</returns>
        public int NewEntity()
        {
            if (_entityCount >= _chunks.Count * _chunkCapacity)
            {
                _chunks.Add(new Chunk());
            }

            return _entityCount++;
        }

        public void DeleteEntity(int index)
        {
            if (index < 0 || index >= _entityCount) throw new ArgumentOutOfRangeException();
            var lastIdx = _entityCount - 1;
            if (index == lastIdx)
            {
                _entityCount--;
                return;
            }

            var tarCnkIdx = index % _chunkCapacity;
            var tarCnk = _chunks[index / _chunkCapacity].Data;
            var lastCnkIdx = lastIdx % _chunkCapacity;
            var lastCnk = _chunks[lastIdx / _chunkCapacity].Data;
            for (var i = 0; i < _types.Length; i++)
            {
                var tarPtr = _offsets[i] + _sizes[i] * tarCnkIdx;
                var lastPtr = _offsets[i] + _sizes[i] * lastCnkIdx;
                Array.Copy(lastCnk, lastPtr, tarCnk, tarPtr, _sizes[i]);
            }

            _entityCount--;
        }

        public Span<byte> GetData(int entityIndex, int comIndex)
        {
            if (entityIndex < 0 || entityIndex >= _entityCount) throw new ArgumentOutOfRangeException();
            if (comIndex < 0 || comIndex >= _types.Length) throw new ArgumentOutOfRangeException();
            var dataIdx = entityIndex % _chunkCapacity;
            var dataCnk = _chunks[entityIndex / _chunkCapacity].Data;
            return new Span<byte>(dataCnk, _offsets[comIndex] + _sizes[comIndex] * dataIdx, _sizes[comIndex]);
        }

        public Memory<byte> GetSharedData(int entityIndex, int comIndex)
        {
            if (entityIndex < 0 || entityIndex >= _entityCount) throw new ArgumentOutOfRangeException();
            if (comIndex < 0 || comIndex >= _types.Length) throw new ArgumentOutOfRangeException();
            var dataIdx = entityIndex % _chunkCapacity;
            var dataCnk = _chunks[entityIndex / _chunkCapacity].Data;
            return new Memory<byte>(dataCnk, _offsets[comIndex] + _sizes[comIndex] * dataIdx, _sizes[comIndex]);
        }

        public ref T GetDataRef<T>(int entityIndex) where T : struct
        {
            var comIndex = FindComIndex(typeof(T));
            if (comIndex == -1) throw new ArgumentException(nameof(T));
            var data = GetData(entityIndex, comIndex);
            return ref MemoryMarshal.GetReference(MemoryMarshal.Cast<byte, T>(data));
        }

        public ref T GetDataRefUnsafe<T>(int entityIndex, int comIndex) where T : struct
        {
            return ref MemoryMarshal.GetReference(MemoryMarshal.Cast<byte, T>(GetData(entityIndex, comIndex)));
        }

        public T GetData<T>(int entityIndex) where T : struct
        {
            var comIndex = FindComIndex(typeof(T));
            if (comIndex == -1) throw new ArgumentException(nameof(T));
            var data = GetData(entityIndex, comIndex);
            return MemoryMarshal.Cast<byte, T>(data)[0];
        }

        public T GetDataUnsafe<T>(int entityIndex, int comIndex) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(GetData(entityIndex, comIndex))[0];
        }

        public void SetData(Span<byte> data, int entityIndex, int comIndex)
        {
            var span = GetData(entityIndex, comIndex);
            data.CopyTo(span);
        }

        public void SetData<T>(T data, int entityIndex) where T : struct { GetDataRef<T>(entityIndex) = data; }

        public void SetDataUnsafe<T>(T data, int entityIndex, int comIndex) where T : struct
        {
            GetDataRefUnsafe<T>(entityIndex, comIndex) = data;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("EntitySize:").Append(_entitySize).Append('\n');
            sb.Append("ChunkCapacity:").Append(_chunkCapacity).Append('\n');
            sb.Append('[');
            for (var index = 0; index < _types.Length; index++)
            {
                sb.Append('{').Append(_types[index].Name);
                sb.Append('|').Append("Size:").Append(_sizes[index]);
                sb.Append('|').Append("Offset:").Append(_offsets[index]);
                sb.Append('}');
            }

            sb.Append(']');
            return sb.ToString();
        }

        public int FindComIndex(Type comType)
        {
            var comIndex = -1;
            for (var i = 0; i < _types.Length; i++)
            {
                if (comType == _types[i])
                {
                    comIndex = i;
                    break;
                }
            }

            return comIndex;
        }
    }
}