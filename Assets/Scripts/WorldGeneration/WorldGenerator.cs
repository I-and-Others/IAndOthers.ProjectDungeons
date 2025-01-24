using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    [Header("Hex Map Settings")]
    public int mapRadius = 5;       // hex radius
    public float hexSize = 1.15f;   // how large each hex is

    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int customSeed = 197221;   // try 197221 or any other

    [Header("Noise Settings")]
    public float elevScale = 20f;
    public float tempScale = 20f;
    public float moistScale = 20f;

    private float elevOffsetX, elevOffsetY;
    private float tempOffsetX, tempOffsetY;
    private float moistOffsetX, moistOffsetY;

    [Header("Thresholds")]
    [Range(0f, 1f)] public float seaLevel = 0.3f;
    [Range(0f, 1f)] public float mountainLevel = 0.8f;

    // Terrain Prefabs
    [Header("Prefabs")]
    public GameObject waterPrefab;     // WaterHex
    public GameObject mountainPrefab;  // MountainHex
    public GameObject snowPrefab;      // SnowHex
    public GameObject desertPrefab;    // DesertHex
    public GameObject forestPrefab;    // ForestHex
    public GameObject grassPrefab;     // GrassHex

    private List<HexCell> mapCells = new List<HexCell>();

    void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        // Clear old
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        mapCells.Clear();

        // 1) Determine Seed
        int seed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : customSeed;
        System.Random rng = new System.Random(seed);

        // 2) Generate random offsets for each noise
        elevOffsetX = (float)rng.NextDouble() * 99999f;
        elevOffsetY = (float)rng.NextDouble() * 99999f;
        tempOffsetX = (float)rng.NextDouble() * 99999f;
        tempOffsetY = (float)rng.NextDouble() * 99999f;
        moistOffsetX = (float)rng.NextDouble() * 99999f;
        moistOffsetY = (float)rng.NextDouble() * 99999f;

        // 3) Loop over axial coords in a hex shape
        for (int q = -mapRadius; q <= mapRadius; q++)
        {
            for (int r = -mapRadius; r <= mapRadius; r++)
            {
                int s = -q - r;
                if (Mathf.Abs(s) <= mapRadius)
                {
                    // Convert to world position
                    Vector3 worldPos = AxialToWorldPosition(q, r);

                    // 4) Sample three noises
                    float elevation = GetElevation(q, r);
                    float temperature = GetTemperature(q, r);
                    float moisture = GetMoisture(q, r);

                    // 5) Decide which biome
                    GameObject chosenPrefab;
                    TerrainType chosenTerrain;
                    int movementCost;

                    if (elevation < seaLevel)
                    {
                        // Water
                        chosenPrefab = waterPrefab;
                        chosenTerrain = TerrainType.Water;
                        movementCost = 3; // or impassable
                    }
                    else if (elevation > mountainLevel)
                    {
                        // Mountain
                        chosenPrefab = mountainPrefab;
                        chosenTerrain = TerrainType.Mountain;
                        movementCost = 3;
                    }
                    else
                    {
                        // Land biomes
                        // Example logic:
                        if (temperature < -0.3f)
                        {
                            // Snow
                            chosenPrefab = snowPrefab;
                            chosenTerrain = TerrainType.Ice;
                            movementCost = 2;
                        }
                        else if (temperature > 0.4f && moisture < 0.3f)
                        {
                            // Desert
                            chosenPrefab = desertPrefab;
                            chosenTerrain = TerrainType.Ground; // or Desert
                            movementCost = 1;
                        }
                        else if (moisture >= 0.6f)
                        {
                            // Forest
                            chosenPrefab = forestPrefab;
                            chosenTerrain = TerrainType.Forest;
                            movementCost = 2;
                        }
                        else
                        {
                            // Grassland (default fallback)
                            chosenPrefab = grassPrefab;
                            chosenTerrain = TerrainType.Ground;
                            movementCost = 1;
                        }
                    }

                    // 6) Instantiate the chosen prefab
                    GameObject hexGO = Instantiate(
                        chosenPrefab,
                        worldPos,
                        Quaternion.identity,
                        this.transform
                    );

                    // 7) Setup HexCell
                    HexCell cell = hexGO.GetComponent<HexCell>();
                    if (cell == null)
                        cell = hexGO.AddComponent<HexCell>();

                    cell.Initialize(q, r, chosenTerrain, movementCost);
                    mapCells.Add(cell);
                }
            }
        }
    }

    private Vector3 AxialToWorldPosition(int q, int r)
    {
        float x = hexSize * Mathf.Sqrt(3f) * (q + (r * 0.5f));
        float z = hexSize * (1.5f * r);
        return new Vector3(x, 0f, z);
    }

    // Elevation: 0..1
    private float GetElevation(int q, int r)
    {
        float nx = (q + elevOffsetX) / elevScale;
        float ny = (r + elevOffsetY) / elevScale;
        return Mathf.PerlinNoise(nx, ny); // 0..1
    }

    // Temperature: -1..1
    private float GetTemperature(int q, int r)
    {
        float nx = (q + tempOffsetX) / tempScale;
        float ny = (r + tempOffsetY) / tempScale;
        float val = Mathf.PerlinNoise(nx, ny); // 0..1
        return val * 2f - 1f;                  // Map to -1..1
    }

    // Moisture: 0..1
    private float GetMoisture(int q, int r)
    {
        float nx = (q + moistOffsetX) / moistScale;
        float ny = (r + moistOffsetY) / moistScale;
        return Mathf.PerlinNoise(nx, ny);      // 0..1
    }
}
