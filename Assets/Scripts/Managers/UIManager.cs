using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI characterInfoText;
    [SerializeField] private TextMeshProUGUI turnOrderText;
    [SerializeField] private CharacterStatusUI characterStatusUI;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI countdownText;

    private void Start()
    {
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
        UpdateEndTurnButton();

        // Hide game over panel initially
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Subscribe to game over event
        GameManager.Instance.onGameOver.AddListener(ShowGameOver);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onGameOver.RemoveListener(ShowGameOver);
        }
    }

    private void ShowGameOver(string message, float countdown)
    {
        // Show game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Set game over message
        if (gameOverText != null)
            gameOverText.text = message;

        // Start countdown
        StartCoroutine(UpdateCountdown(countdown));
    }

    private IEnumerator UpdateCountdown(float duration)
    {
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            if (countdownText != null)
                countdownText.text = $"Restarting in {timeLeft:F1} seconds...";

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        if (countdownText != null)
            countdownText.text = "Restarting...";
    }

    private void OnEndTurnClicked()
    {
        GameManager.Instance.EndTurn();
    }

    private void UpdateEndTurnButton()
    {
        Character activeCharacter = GameManager.Instance.GetActiveCharacter();
        endTurnButton.interactable = (activeCharacter != null);
        
        TextMeshProUGUI buttonText = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = "End Turn";
        }
    }

    public void UpdateCharacterInfo(Character character)
    {
        characterInfoText.text = $"Active Character: {character.data.characterName}\n" +
                               $"HP: {character.CurrentHealth}/{character.data.maxHealth}\n" +
                               $"Physical Armor: {character.CurrentPhysicalArmor}\n" +
                               $"Magic Armor: {character.CurrentMagicArmor}\n" +
                               $"Movement Points: {character.MovementPoints}\n" +
                               $"Initiative: {character.data.initiative}";
                               
        // Update the character status UI
        characterStatusUI.UpdateForCharacter(character);
        UpdateEndTurnButton();
    }

    public void UpdateTurnOrder(List<Character> turnOrder, int currentIndex)
    {
        string turnOrderStr = "Turn Order:\n";
        for (int i = 0; i < turnOrder.Count; i++)
        {
            string prefix = i == currentIndex ? "► " : "   ";
            turnOrderStr += $"{prefix}{turnOrder[i].data.characterName}\n";
        }
        turnOrderText.text = turnOrderStr;
    }
} 