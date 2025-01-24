using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button nextTurnButton;
    [SerializeField] private TextMeshProUGUI characterInfoText;

    private void Start()
    {
        nextTurnButton.onClick.AddListener(OnNextTurnClicked);
    }

    private void OnNextTurnClicked()
    {
        GameManager.Instance.EndTurn();
    }

    public void UpdateCharacterInfo(Character character)
    {
        characterInfoText.text = $"Character: {character.data.characterName}\n" +
                               $"HP: {character.CurrentHealth}/{character.data.maxHealth}\n" +
                               $"Physical Armor: {character.CurrentPhysicalArmor}\n" +
                               $"Magic Armor: {character.CurrentMagicArmor}\n" +
                               $"Movement Points: {character.MovementPoints}";
    }
} 