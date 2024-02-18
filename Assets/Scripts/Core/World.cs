using Shafter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    #region Prefabs
    [SerializeField] private GameObject m_chunkPrefab;
    #endregion

    #region Singleton
    public static World Current { get; private set; }
    #endregion

    #region State
    [System.Serializable]
    public class State
    {
        public Dictionary<Vector3Int, Chunk> loadedChunks = new();
    }

    [SerializeField] private State m_state;
    public State CurrentState => m_state;
    #endregion

    #region Chunks
    public static bool TrySpawnChunk(Vector3Int at, WorldLoader worldLoader)
    {
        if (Current.m_state.loadedChunks.ContainsKey(at))
        {
            Current.m_state.loadedChunks[at].StartLoader(worldLoader);
            return false;
        }

        Chunk chunck = Instantiate(
            Current.m_chunkPrefab, 
            at * Chunk.CHUNK_SIZE, 
            Quaternion.identity, 
            Current.transform
        ).GetComponent<Chunk>();

        chunck.StartLoader(worldLoader);
        Current.m_state.loadedChunks.Add(at, chunck);
        return true;
    }

    public static bool TryDespawnChunk(Vector3Int at)
    {
        if (!Current.m_state.loadedChunks.ContainsKey(at)) return false;

        Chunk chunk = Current.m_state.loadedChunks[at];
        Current.m_state.loadedChunks.Remove(at);
        Destroy(chunk.gameObject);

        return true;
    }

    [SerializeField]
    private WorldGeneration m_worldGeneration = new() 
    { 
        HeightGenerator = new() 
        { 
            Algorithm = new Noise.SimplePerlinAlgorithm() 
        } 
    };

    public static bool TryLoadChunk(Vector3Int at, WorldLoader worldLoader)
    {
        if (!Current.m_state.loadedChunks.ContainsKey(at)) return false;

        Chunk chunk = Current.m_state.loadedChunks[at];

        if (chunk.IsLoaded) return false;

        chunk.Load(Current.m_worldGeneration.GenerateChunk(at));

        return true;
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        #region Singleton
        if(Current != null && Current != this)
        {
            Debug.LogError("Two world instances in scene.");
            Destroy(gameObject);
            return;
        }

        Current = this;
        #endregion
    }
    #endregion
}
