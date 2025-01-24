using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class WorldSettingManager : MonoBehaviour
{
    public static WorldSettingManager Instance { get; private set; }
    
    [SerializeField] private WorldGenerator worldGenerator;
    [SerializeField] private CharacterData[] charactersToSpawn;
    
    private List<Character> spawnedCharacters = new List<Character>();
    
    // Add this event
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
        
        foreach (var characterData in charactersToSpawn)
        {
            if (availableTiles.Count == 0)
            {
                Debug.LogError("Not enough spawn tiles!");
                break;
            }

            // Get random spawn position
            int randomIndex = Random.Range(0, availableTiles.Count);
            HexCell spawnTile = availableTiles[randomIndex];
            availableTiles.RemoveAt(randomIndex);

            // Spawn character
            GameObject characterObj = Instantiate(characterData.characterPrefab, 
                spawnTile.transform.position + Vector3.up * 0.1f, // Slight offset to prevent z-fighting
                Quaternion.identity);
            
            Character character = characterObj.GetComponent<Character>();
            character.data = characterData;
            character.CurrentTile = spawnTile;
            
            spawnedCharacters.Add(character);
        }

        // Notify that characters are spawned
        onCharactersSpawned.Invoke(spawnedCharacters);
        
        // Initialize game
        GameManager.Instance.InitializeGame(spawnedCharacters);
    }

    private List<HexCell> FindSpawnableTiles()
    {
        return FindObjectsOfType<HexCell>()
            .Where(hex => hex.terrain == TerrainType.Ground)
            .ToList();
    }
} 