using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class WorldSettingManager : MonoBehaviour
{
    public static WorldSettingManager Instance { get; private set; }
    
    [SerializeField] private WorldGenerator worldGenerator;
    
    [Header("Character Spawning")]
    [SerializeField] private CharacterData[] heroesToSpawn;
    [SerializeField] private EnemyData[] enemiesToSpawn;
    [SerializeField] private GameObject enemyPrefab;  // Base enemy prefab
    [SerializeField] private int minEnemyCount = 1;
    [SerializeField] private int maxEnemyCount = 3;
    
    private List<Character> spawnedCharacters = new List<Character>();
    
    public UnityEvent<List<Character>> onCharactersSpawned = new UnityEvent<List<Character>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Wait a frame to ensure world generation is complete
        Invoke(nameof(SpawnCharacters), 0.1f);
    }

    private void SpawnCharacters()
    {
        var availableTiles = FindSpawnableTiles();
        
        // Spawn heroes
        foreach (var heroData in heroesToSpawn)
        {
            if (availableTiles.Count == 0)
            {
                Debug.LogError("Not enough spawn tiles!");
                break;
            }

            SpawnCharacter(heroData, availableTiles);
        }

        // Spawn enemies
        int enemyCount = Random.Range(minEnemyCount, maxEnemyCount + 1);
        for (int i = 0; i < enemyCount; i++)
        {
            if (availableTiles.Count == 0 || enemiesToSpawn.Length == 0)
            {
                break;
            }

            // Randomly select an enemy type
            EnemyData enemyData = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Length)];
            SpawnCharacter(enemyData, availableTiles, enemyPrefab);
        }

        // Notify that characters are spawned
        onCharactersSpawned.Invoke(spawnedCharacters);
        
        // Initialize game
        GameManager.Instance.InitializeGame(spawnedCharacters);
    }

    private void SpawnCharacter(CharacterData characterData, List<HexCell> availableTiles, GameObject overridePrefab = null)
    {
        // Get random spawn position
        int randomIndex = Random.Range(0, availableTiles.Count);
        HexCell spawnTile = availableTiles[randomIndex];
        availableTiles.RemoveAt(randomIndex);

        // Use override prefab if provided, otherwise use character's default prefab
        GameObject prefabToSpawn = overridePrefab != null ? overridePrefab : characterData.characterPrefab;

        // Spawn character
        GameObject characterObj = Instantiate(prefabToSpawn, 
            spawnTile.transform.position + Vector3.up * 0.0f,
            Quaternion.identity);
        
        Character character = characterObj.GetComponent<Character>();
        character.data = characterData;
        character.CurrentTile = spawnTile;
        
        spawnedCharacters.Add(character);
    }

    private List<HexCell> FindSpawnableTiles()
    {
        return FindObjectsOfType<HexCell>()
            .Where(hex => hex.terrain == TerrainType.Ground)
            .ToList();
    }
} 