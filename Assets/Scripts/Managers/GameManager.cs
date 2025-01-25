using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public UnityEvent onTurnStart = new UnityEvent();
    public UnityEvent onTurnEnd = new UnityEvent();
    
    private List<Character> characters = new List<Character>();
    private List<Character> turnOrder = new List<Character>();
    private int currentTurnIndex = 0;
    private Character activeCharacter;
    
    [SerializeField] public UIManager uiManager;

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
        activeCharacter = turnOrder[currentTurnIndex];
        activeCharacter.StartTurn();
        
        // Update UI
        uiManager.UpdateCharacterInfo(activeCharacter);
        uiManager.UpdateTurnOrder(turnOrder, currentTurnIndex);
        
        onTurnStart.Invoke();
    }

    public void EndTurn()
    {
        // No need to check movement points anymore - players can end turn whenever they want
        onTurnEnd.Invoke();
        
        currentTurnIndex++;
        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
            StartNewRound();
        }
        else
        {
            StartTurn();
        }
    }

    private void StartNewRound()
    {
        Debug.Log("Starting new round");
        foreach (var character in characters)
        {
            // Reset any round-based stats here if needed
        }
        StartTurn();
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
} 