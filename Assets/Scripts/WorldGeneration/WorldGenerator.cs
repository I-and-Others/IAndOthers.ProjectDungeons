using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    [Header("Hex Map Settings")]
    public int mapRadius = 15;       // hex radius
    public float hexSize = 1.15f;   // how large each hex is

    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int customSeed = 197221;   // try 197221 or any other

    [Header("Noise Settings")]
    public float elevScale = 12f;
    public float tempScale = 15f;
    public float moistScale = 10f;

    private float elevOffsetX, elevOffsetY;
    private float tempOffsetX, tempOffsetY;
    private float moistOffsetX, moistOffsetY;

    [Header("Thresholds")]
    [Range(0f, 1f)] public float seaLevel = 0.35f;
    [Range(0f, 1f)] public float mountainLevel = 0.75f;

    // Terrain Prefabs
    [Header("Prefabs")]
    public GameObject waterPrefab;     // WaterHex
    public GameObject mountainPrefab;  // MountainHex
    public GameObject snowPrefab;      // SnowHex
    public GameObject desertPrefab;    // DesertHex
    public GameObject forestPrefab;    // ForestHex
    public GameObject grassPrefab;     // GrassHex

    private List<HexCell> mapCells = new List<HexCell>();

    public BiomeSettings biomeSettings;

    void Start()
    {
        GenerateMap();
    }

    public void ClearMap()
    {
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
        mapCells.Clear();
    }

    private void GenerateMap()
    {
        ClearMap();

        // Validate required components
        if (waterPrefab == null || mountainPrefab == null || snowPrefab == null || 
            desertPrefab == null || forestPrefab == null || grassPrefab == null)
        {
            Debug.LogError("Some prefabs are not assigned in the WorldGenerator!");
            return;
        }

        if (biomeSettings == null)
        {
            Debug.LogError("BiomeSettings not assigned!");
            return;
        }

        // Setup seed
        int seed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : customSeed;
        System.Random rng = new System.Random(seed);

        // Generate noise offsets
        SetupNoiseOffsets(rng);

        // Generate the base map
        for (int q = -mapRadius; q <= mapRadius; q++)
        {
            for (int r = -mapRadius; r <= mapRadius; r++)
            {
                int s = -q - r;
                if (Mathf.Abs(s) <= mapRadius)
                {
                    GenerateHexAt(q, r);
                }
            }
        }

        // Apply special rules
        if (biomeSettings.enableSeaBorder)
            GenerateSeaBorder();
        if (biomeSettings.enableIceCaps)
            GenerateIceCaps();
        if (biomeSettings.enableMountainRanges)
            GenerateMountainRanges();
    }

    private void GenerateHexAt(int q, int r)
    {
        Vector3 worldPos = AxialToWorldPosition(q, r);
        
        float elevation = GetElevation(q, r);
        float temperature = GetTemperature(q, r);
        float moisture = GetMoisture(q, r);

        // Determine biome based on settings and noise values
        BiomeDefinition biome = DetermineBiome(elevation, temperature, moisture);
        
        // Instantiate hex
        GameObject hexGO = Instantiate(biome.prefab, worldPos, Quaternion.identity, transform);
        
        // Setup cell
        HexCell cell = hexGO.GetComponent<HexCell>();
        if (cell == null)
            cell = hexGO.AddComponent<HexCell>();
        
        cell.Initialize(q, r, biome.terrainType, biome.movementCost);
        mapCells.Add(cell);
    }

    private BiomeDefinition DetermineBiome(float elevation, float temperature, float moisture)
    {
        BiomeDefinition result = new BiomeDefinition();
        
        // First check if we should force water based on biome settings
        if (elevation < seaLevel || (biomeSettings.enableSeaBorder && GetDistanceFromMapEdge(0, 0) < biomeSettings.coastalBiomeWidth))
        {
            result.terrainType = TerrainType.Water;
            result.prefab = waterPrefab;
            result.movementCost = 3;
            return result;
        }

        // Get the normalized position for biome distribution
        float normalizedTemp = (temperature + 1f) / 2f; // Convert from -1..1 to 0..1
        float totalWeight = 0f;
        
        // Calculate weights based on environmental factors and biome settings
        Dictionary<BiomeType, float> weights = new Dictionary<BiomeType, float>();
        
        foreach (var biome in biomeSettings.biomeDistributions)
        {
            if (!biome.enabled) continue;
            
            float weight = biome.targetPercentage / 100f; // Base weight from distribution settings
            
            // Modify weight based on environmental factors
            switch (biome.biomeType)
            {
                case BiomeType.Desert:
                    weight *= (normalizedTemp * (1f - moisture)); // More likely in hot, dry areas
                    break;
                case BiomeType.Snow:
                    weight *= (1f - normalizedTemp); // More likely in cold areas
                    break;
                case BiomeType.Forest:
                    weight *= moisture; // More likely in wet areas
                    break;
                case BiomeType.Mountain:
                    weight *= elevation; // More likely in high elevation
                    break;
                case BiomeType.Water:
                    weight *= (1f - elevation); // More likely in low elevation
                    break;
            }
            
            weights[biome.biomeType] = weight;
            totalWeight += weight;
        }
        
        // Normalize weights
        if (totalWeight > 0)
        {
            float random = Random.value * totalWeight;
            float currentSum = 0f;
            
            foreach (var kvp in weights)
            {
                currentSum += kvp.Value;
                if (random <= currentSum)
                {
                    // Set the biome based on the selected type
                    switch (kvp.Key)
                    {
                        case BiomeType.Desert:
                            result.terrainType = TerrainType.Ground;
                            result.prefab = desertPrefab;
                            result.movementCost = 1;
                            break;
                        case BiomeType.Snow:
                            result.terrainType = TerrainType.Ice;
                            result.prefab = snowPrefab;
                            result.movementCost = 2;
                            break;
                        case BiomeType.Forest:
                            result.terrainType = TerrainType.Forest;
                            result.prefab = forestPrefab;
                            result.movementCost = 2;
                            break;
                        case BiomeType.Mountain:
                            result.terrainType = TerrainType.Mountain;
                            result.prefab = mountainPrefab;
                            result.movementCost = 3;
                            break;
                        default:
                            result.terrainType = TerrainType.Ground;
                            result.prefab = grassPrefab;
                            result.movementCost = 1;
                            break;
                    }
                    return result;
                }
            }
        }
        
        // Fallback to grass if something goes wrong
        result.terrainType = TerrainType.Ground;
        result.prefab = grassPrefab;
        result.movementCost = 1;
        
        return result;
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

    private void SetupNoiseOffsets(System.Random rng)
    {
        elevOffsetX = (float)rng.NextDouble() * 99999f;
        elevOffsetY = (float)rng.NextDouble() * 99999f;
        tempOffsetX = (float)rng.NextDouble() * 99999f;
        tempOffsetY = (float)rng.NextDouble() * 99999f;
        moistOffsetX = (float)rng.NextDouble() * 99999f;
        moistOffsetY = (float)rng.NextDouble() * 99999f;
    }

    private void GenerateSeaBorder()
    {
        float borderWidth = biomeSettings.coastalBiomeWidth;
        foreach (var cell in mapCells)
        {
            float distanceFromEdge = GetDistanceFromMapEdge(cell.q, cell.r);
            if (distanceFromEdge < borderWidth)
            {
                // Convert to water
                ReplaceHexWithWater(cell);
            }
        }
    }

    private void GenerateIceCaps()
    {
        foreach (var cell in mapCells)
        {
            float normalizedLatitude = GetNormalizedLatitude(cell.q, cell.r);
            if (normalizedLatitude > 0.8f || normalizedLatitude < -0.8f)
            {
                // Convert to ice
                ReplaceHexWithIce(cell);
            }
        }
    }

    private void GenerateMountainRanges()
    {
        // Use a different noise scale for mountain ranges
        float mountainNoiseScale = 8f;
        float mountainThreshold = 1f - biomeSettings.mountainRangeDensity;
        
        foreach (var cell in mapCells.ToList()) // Use ToList() to avoid collection modification issues
        {
            if (cell.terrain == TerrainType.Water || cell.terrain == TerrainType.Ice)
                continue;

            float nx = (cell.q + elevOffsetX) / mountainNoiseScale;
            float ny = (cell.r + elevOffsetY) / mountainNoiseScale;
            float mountainNoise = Mathf.PerlinNoise(nx, ny);

            if (mountainNoise > mountainThreshold)
            {
                GameObject oldHex = cell.gameObject;
                Vector3 position = oldHex.transform.position;
                
                GameObject mountainHex = Instantiate(mountainPrefab, position, Quaternion.identity, transform);
                HexCell newCell = mountainHex.GetComponent<HexCell>();
                if (newCell == null) newCell = mountainHex.AddComponent<HexCell>();
                
                newCell.Initialize(cell.q, cell.r, TerrainType.Mountain, 3);
                
                mapCells.Remove(cell);
                mapCells.Add(newCell);
                
                DestroyImmediate(oldHex);
            }
        }
    }

    private float GetDistanceFromMapEdge(int q, int r)
    {
        // Calculate distance from hex to map edge
        float maxDistance = mapRadius;
        float distance = Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(-q-r));
        return 1 - (distance / maxDistance);
    }

    private float GetNormalizedLatitude(int q, int r)
    {
        // Convert hex coordinates to a latitude value between -1 and 1
        return r / (float)mapRadius;
    }

    private void ReplaceHexWithWater(HexCell cell)
    {
        // Destroy existing hex
        GameObject oldHex = cell.gameObject;
        Vector3 position = oldHex.transform.position;
        
        // Create new water hex
        GameObject waterHex = Instantiate(waterPrefab, position, Quaternion.identity, transform);
        HexCell newCell = waterHex.GetComponent<HexCell>();
        if (newCell == null) newCell = waterHex.AddComponent<HexCell>();
        
        // Initialize with same coordinates but water terrain
        newCell.Initialize(cell.q, cell.r, TerrainType.Water, 3);
        
        // Update map cells list
        mapCells.Remove(cell);
        mapCells.Add(newCell);
        
        // Destroy old hex
        DestroyImmediate(oldHex);
    }

    private void ReplaceHexWithIce(HexCell cell)
    {
        GameObject oldHex = cell.gameObject;
        Vector3 position = oldHex.transform.position;
        
        GameObject iceHex = Instantiate(snowPrefab, position, Quaternion.identity, transform);
        HexCell newCell = iceHex.GetComponent<HexCell>();
        if (newCell == null) newCell = iceHex.AddComponent<HexCell>();
        
        newCell.Initialize(cell.q, cell.r, TerrainType.Ice, 2);
        
        mapCells.Remove(cell);
        mapCells.Add(newCell);
        
        DestroyImmediate(oldHex);
    }
}

