using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public UnityEvent onTurnStart = new UnityEvent();
    public UnityEvent onTurnEnd = new UnityEvent();
    public UnityEvent<Character> onCharacterDeath = new UnityEvent<Character>();
    public UnityEvent<string, float> onGameOver = new UnityEvent<string, float>(); // Message and countdown duration
    
    private List<Character> characters = new List<Character>();
    private List<Character> turnOrder = new List<Character>();
    private int currentTurnIndex = 0;
    private Character activeCharacter;
    private bool isGameOver = false;
    
    [SerializeField] public UIManager uiManager;
    [SerializeField] private float gameOverCountdown = 5f;

    public bool canEndTurn => characters.All(c => c.MovementPoints == 0);

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

    public void InitializeGame(List<Character> spawnedCharacters)
    {
        characters = spawnedCharacters;
        
        // Sort characters by initiative
        characters.Sort((a, b) => b.data.initiative.CompareTo(a.data.initiative));
        turnOrder = new List<Character>(characters);
        
        // Ensure we have characters before proceeding
        if (characters.Count > 0)
        {
            activeCharacter = characters[0];
            uiManager.UpdateCharacterInfo(activeCharacter);
            
            // Wait a frame to ensure all components are properly initialized
            StartCoroutine(StartGameDelayed());
        }
        else
        {
            Debug.LogError("No characters available to start the game!");
        }
    }

    private IEnumerator StartGameDelayed()
    {
        // Wait for next frame to ensure all components are initialized
        yield return null;
        StartGame();
    }

    private void StartGame()
    {
        if (characters.Count > 0)
        {
            StartTurn();
        }
    }

    public void StartTurn()
    {
        if (isGameOver) return;

        // Skip turns of dead characters
        while (currentTurnIndex < turnOrder.Count && turnOrder[currentTurnIndex].IsDead)
        {
            currentTurnIndex++;
        }

        // If we've reached the end of the turn order, start a new round
        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
            StartNewRound();
            return;
        }

        activeCharacter = turnOrder[currentTurnIndex];
        activeCharacter.StartTurn();
            
        onTurnStart.Invoke();
        uiManager.UpdateCharacterInfo(activeCharacter);
        uiManager.UpdateTurnOrder(turnOrder, currentTurnIndex);

        // Check if it's an enemy's turn
        Enemy enemy = activeCharacter as Enemy;
        if (enemy != null && !enemy.IsDead)
        {
            // Execute enemy turn automatically
            enemy.ExecuteTurn();
        }
        else if (!activeCharacter.IsDead)
        {
            // Automatically select the active character if it's a player character
            SelectionManager.Instance.SelectCharacter(activeCharacter);
        }
        else
        {
            // If the character is dead, skip their turn
            EndTurn();
        }
    }

    public void EndTurn()
    {
        if (isGameOver) return;

        onTurnEnd.Invoke();
        
        currentTurnIndex++;
        StartTurn();
    }

    private void StartNewRound()
    {
        if (isGameOver) return;

        // Remove dead characters from turn order
        turnOrder.RemoveAll(character => character.IsDead);
        
        // Check if game should end
        CheckGameEndConditions();
        
        if (!isGameOver)
        {
            foreach (var character in turnOrder)
            {
                if (!character.IsDead)
                {
                    // Reset any round-based stats here if needed
                }
            }
            StartTurn();
        }
    }

    public Character GetActiveCharacter()
    {
        return activeCharacter;
    }

    // Add this method to check if it's a specific character's turn
    public bool IsCharacterTurn(Character character)
    {
        return character == activeCharacter;
    }

    public void HandleCharacterDeath(Character character)
    {
        // Trigger death event
        onCharacterDeath.Invoke(character);

        // Remove from lists
        characters.Remove(character);
        turnOrder.Remove(character);

        // Adjust current turn index if necessary
        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
        }

        // If it was the active character's turn, end their turn
        if (character == activeCharacter)
        {
            EndTurn();
        }

        // Update UI
        uiManager.UpdateTurnOrder(turnOrder, currentTurnIndex);

        // Check win/lose conditions
        CheckGameEndConditions();
    }

    private void CheckGameEndConditions()
    {
        if (isGameOver) return;

        // Count remaining heroes and enemies
        int heroCount = characters.Count(c => !(c is Enemy));
        int enemyCount = characters.Count(c => c is Enemy);

        if (heroCount == 0)
        {
            StartGameOver("Game Over - Heroes Defeated!");
        }
        else if (enemyCount == 0)
        {
            StartGameOver("Victory - All Enemies Defeated!");
        }
    }

    private void StartGameOver(string message)
    {
        isGameOver = true;
        
        // Disable all character controls
        foreach (var character in FindObjectsOfType<Character>())
        {
            var movement = character.GetComponent<CharacterMovement>();
            if (movement != null)
                movement.enabled = false;
        }

        // Trigger game over event with message and countdown duration
        onGameOver.Invoke(message, gameOverCountdown);
        
        // Start countdown to restart
        StartCoroutine(GameOverCountdown());
    }

    private IEnumerator GameOverCountdown()
    {
        yield return new WaitForSeconds(gameOverCountdown);
        
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
} 