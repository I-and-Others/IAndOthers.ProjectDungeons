using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI characterInfoText;
    [SerializeField] private TextMeshProUGUI turnOrderText;
    [SerializeField] private TextMeshProUGUI activeCharacterDetailsText;

    private void Start()
    {
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
        UpdateEndTurnButton();
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
                               $"Movement Points: {character.MovementPoints}/{character.MaxMovementPoints}\n" +
                               $"Initiative: {character.data.initiative}";
                               
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