using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public UnityEvent onTurnStart = new UnityEvent();
    public UnityEvent onTurnEnd = new UnityEvent();
    
    private List<Character> characters = new List<Character>();
    private int currentCharacterIndex = 0;
    private Character activeCharacter;
    
    [SerializeField] private UIManager uiManager;

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
        activeCharacter = characters[currentCharacterIndex];
        activeCharacter.StartTurn();
        
        // Update UI
        uiManager.UpdateCharacterInfo(activeCharacter);
        
        onTurnStart.Invoke();
    }

    public void EndTurn()
    {
        onTurnEnd.Invoke();
        
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;
        StartTurn();
    }

    public Character GetActiveCharacter()
    {
        return activeCharacter;
    }
} 