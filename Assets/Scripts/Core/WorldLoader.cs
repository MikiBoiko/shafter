using Shafter;
using System.Collections;
using UnityEngine;

public class WorldLoader : MonoBehaviour
{
    public static readonly float SPAWN_CHUNKS_AROUND_PERIOD = 5f;
    public static readonly float LOAD_CHUNKS_AROUND_PERIOD = 5f;

    #region Attributes
    [field: SerializeField] public float SpawnDistance { get; private set; }
    [field: SerializeField] public float LoadDistance { get; private set; }
    #endregion

    #region Components
    public WorldPosition WorldPosition { get; private set; }
    #endregion

    public IEnumerator ITrySpawnChunksAround()
    {
        Vector3Int chunkPosition = WorldPosition.ChunkPosition;
        Vector3 offset = Vector3.one * SpawnDistance / Chunk.CHUNK_SIZE / 2;
        Vector3Int startPosition = Vector3Int.RoundToInt(chunkPosition - offset);
        Vector3Int endPosition = Vector3Int.RoundToInt(chunkPosition + offset);

        for (int x = startPosition.x; x <= endPosition.x; x++)
            for (int y = startPosition.y; y <= endPosition.y; y++)
                for (int z = startPosition.z; z <= endPosition.z; z++)
                    World.TrySpawnChunk(new Vector3Int(x, y, z), this);

        yield return new WaitForSeconds(SPAWN_CHUNKS_AROUND_PERIOD);

        StartCoroutine(ITrySpawnChunksAround());
    }

    public IEnumerator ITryLoadChunksAround()
    {
        Vector3Int chunkPosition = WorldPosition.ChunkPosition;
        Vector3 offset = Vector3.one * LoadDistance / Chunk.CHUNK_SIZE / 2;
        Vector3Int startPosition = Vector3Int.RoundToInt(chunkPosition - offset);
        Vector3Int endPosition = Vector3Int.RoundToInt(chunkPosition + offset);

        for (int x = startPosition.x; x <= endPosition.x; x++)
            for (int y = startPosition.y; y <= endPosition.y; y++)
                for (int z = startPosition.z; z <= endPosition.z; z++)
                    World.TryLoadChunk(new Vector3Int(x, y, z), this);

        yield return new WaitForSeconds(SPAWN_CHUNKS_AROUND_PERIOD);

        StartCoroutine(ITryLoadChunksAround());
    }

    private void Awake()
    {
        WorldPosition = new(transform);
    }

    private void Start()
    {
        StartCoroutine(ITrySpawnChunksAround());
        StartCoroutine(ITryLoadChunksAround());
    }
}
