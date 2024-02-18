using UnityEngine;

namespace Shafter
{
    [System.Serializable]
    public class WorldGeneration
    {
        public static readonly int MAX_HEIGHT_RANDOM_OFFSET;

        [field: SerializeField] public int Seed {  get; set; }
        public Noise.Generator HeightGenerator { get; set; }

        public Shafter.Material[,,] GenerateChunk(Vector3Int at)
        {
            Random.InitState(Seed);

            Shafter.Material[,,] result = new Shafter.Material[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];

            #region Height
            float[,] heights = HeightGenerator.Generate2D(
                new(
                    at.x * Chunk.CHUNK_SIZE + Random.Range(-MAX_HEIGHT_RANDOM_OFFSET, MAX_HEIGHT_RANDOM_OFFSET), 
                    at.z * Chunk.CHUNK_SIZE + Random.Range(-MAX_HEIGHT_RANDOM_OFFSET, MAX_HEIGHT_RANDOM_OFFSET)
                ), 
                Chunk.CHUNK_SIZE
            );

            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        if (at.y * Chunk.CHUNK_SIZE + y > heights[x, z] * 20)
                            result[x, y, z] = null;
                        else
                            result[x, y, z] = new Shafter.Material();
                    }
            #endregion

            return result;
        }
    }
}
