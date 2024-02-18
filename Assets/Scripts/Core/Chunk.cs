using Shafter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static readonly int CHUNK_SIZE = 32;
    public static readonly int UNLOAD_TIME = 5;

    #region Prefabs 
    [SerializeField] private GameObject m_blockPrefab;
    #endregion

    #region Components
    public WorldPosition WorldPosition { get; private set; }
    #endregion

    #region State
    public enum LoadState { Unloaded = 0, Loaded = 1 };

    [System.Serializable]
    public class State
    {
        [field: SerializeField] public LoadState LoadState { get; set; } = LoadState.Unloaded;
        [field: SerializeField] public Block[,,] Blocks;
    }

    [SerializeField]
    private State m_state;
    public State CurrentState => m_state;
    #endregion

    #region Load and unload
    public bool IsLoaded => m_state.LoadState == LoadState.Loaded;
    private HashSet<WorldLoader> m_worldLoaders = new();

    public void StartLoader(WorldLoader worldLoader) => m_worldLoaders.Add(worldLoader);

    public IEnumerator ICheckLoaders()
    {
        HashSet<WorldLoader> unloadPlayers = new();
        foreach (WorldLoader loader in m_worldLoaders)
            if (Mathf.Abs(loader.WorldPosition.Position.x - WorldPosition.Position.x) > loader.LoadDistance 
             || Mathf.Abs(loader.WorldPosition.Position.y - WorldPosition.Position.y) > loader.LoadDistance
             || Mathf.Abs(loader.WorldPosition.Position.z - WorldPosition.Position.z) > loader.LoadDistance)
                unloadPlayers.Add(loader);

        foreach (WorldLoader unload in unloadPlayers)
        {
            m_worldLoaders.Remove(unload);
        }

        if(m_worldLoaders.Count == 0)
            World.TryDespawnChunk(WorldPosition.ChunkPosition);
        else
        {
            yield return new WaitForSeconds(UNLOAD_TIME);
            StartCoroutine(ICheckLoaders());
        }
    }

    public void Load(Shafter.Material[,,] materials)
    {
        List<MegaBlock> megaBlocks = CalculateMegaBlocks(materials);
        foreach (MegaBlock megaBlock in megaBlocks)
        {
            Block block = Instantiate(
                m_blockPrefab,
                WorldPosition.Position + megaBlock.Center - Vector3.one * CHUNK_SIZE / 2,
                Quaternion.identity,
                transform
            ).GetComponent<Block>();

            block.transform.localScale = (Vector3)megaBlock.Size;
        }

        m_state.LoadState = LoadState.Loaded;

        /*
        for (int x = 0; x < CHUNK_SIZE; x++)
            for (int y = 0; y < CHUNK_SIZE; y++)
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    if (materials[x, y, z] == null) continue;

                    Block block = Instantiate(
                        m_blockPrefab,
                        WorldPosition.Position + new Vector3Int(x, y, z) - Vector3.one * CHUNK_SIZE / 2,
                        Quaternion.identity,
                        transform
                    ).GetComponent<Block>();
                }
        */
    }

    public class MegaBlock
    {
        public Vector3 Center { get; set; }
        public Vector3Int Size { get; set; }
    }

    public List<MegaBlock> CalculateMegaBlocks(Shafter.Material[,,] materials)
    {
        bool[,,] used = new bool[materials.GetLength(0), materials.GetLength(1), materials.GetLength(2)];
        List<MegaBlock> result = new(); 

        for (int x = 0; x < materials.GetLength(0); x++)
            for (int y = 0; y < materials.GetLength(1); y++)
                for (int z = 0; z < materials.GetLength(2); z++)
                {
                    Vector3Int size = Vector3Int.zero;
                    if (materials[x, y, z] == null || used[x, y, z]) continue;
                    used[x, y, z] = true;
                    size += Vector3Int.right;

                    int xi = x + 1;
                    while (xi < materials.GetLength(0))
                    {
                        if (materials[xi, y, z] == null || used[xi, y, z]) break;

                        used[xi, y, z] = true;
                        size += Vector3Int.right;
                        xi++;
                    }

                    if (size.x == 0) continue;

                    int yi = y + 1;
                    size += Vector3Int.up;
                    bool yCompleted = false;
                    while (yi < materials.GetLength(1) && !yCompleted)
                    {
                        for (int i = x; i < xi && !yCompleted; i++)
                            if (materials[i, yi, z] == null || used[i, yi, z])
                                yCompleted = true;

                        if(!yCompleted)
                        {
                            for (int i = x; i < xi; i++)
                                used[i, yi, z] = true;

                            size += Vector3Int.up;
                            yi++;
                        }
                    }

                    int zi = z + 1;
                    size += Vector3Int.forward;
                    bool zCompleted = false;
                    while (zi < materials.GetLength(2) && !zCompleted)
                    {
                        for (int i = x; i < xi && !zCompleted; i++)
                            for (int j = y; j < yi && !zCompleted; j++)
                            {
                                if (materials[i, j, zi] == null || used[i, j, zi])
                                    zCompleted = true;
                            }

                        if(!zCompleted)
                        {
                            for (int i = x; i < xi; i++)
                                for (int j = y; j < yi; j++)
                                    used[i, j, zi] = true;

                            size += Vector3Int.forward;
                            zi++;
                        }
                    }

                    Debug.Log(new Vector3(x, y, z) + (Vector3)size / 2);
                    Debug.Log((Vector3)size);
                    result.Add(
                        new MegaBlock() 
                        {
                            Center = new Vector3(x, y, z) + (Vector3)size / 2,
                            Size = size
                        }
                    );
                }

        return result;
    }
    #endregion

    private void Awake()
    {
        WorldPosition = new(transform);
    }

    private void Start()
    {
        StartCoroutine(ICheckLoaders());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = m_state.LoadState == LoadState.Unloaded ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, CHUNK_SIZE * Vector3.one);
    }
}
