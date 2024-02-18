using UnityEngine;

namespace Shafter
{
    public class WorldPosition
    {
        public Transform Transform { get; private set; }
        public Vector3 Position => Transform.position;
        public Vector3Int ChunkPosition => Vector3Int.RoundToInt((Position) / Chunk.CHUNK_SIZE);
        
        public WorldPosition(Transform transform)
        {
            Transform = transform;
        }
    }
}
