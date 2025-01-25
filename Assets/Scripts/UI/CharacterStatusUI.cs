using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStatusUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image characterIcon;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image physicalArmorBarFill;
    [SerializeField] private Image magicalArmorBarFill;
    [SerializeField] private TextMeshProUGUI movementPointsText;
    
    [SerializeField] private Character targetCharacter; // Assign this in inspector
    
    private RectTransform rectTransform;
    private Vector3 offset = new Vector3(0, 2f, 0);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (targetCharacter != null)
        {
            Initialize(targetCharacter);
        }
    }

    public void Initialize(Character character)
    {
        targetCharacter = character;
        
        if (character.data.characterIcon != null)
        {
            characterIcon.sprite = character.data.characterIcon;
        }
        UpdateUI();
    }

    private void LateUpdate()
    {
        if (targetCharacter != null)
        {
            // Update position to follow character
            Vector3 worldPosition = targetCharacter.transform.position + offset;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            rectTransform.position = screenPosition;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (targetCharacter == null) return;

        // Update bars
        healthBarFill.fillAmount = (float)targetCharacter.CurrentHealth / targetCharacter.data.maxHealth;
        physicalArmorBarFill.fillAmount = (float)targetCharacter.CurrentPhysicalArmor / targetCharacter.data.maxPhysicalArmor;
        magicalArmorBarFill.fillAmount = (float)targetCharacter.CurrentMagicArmor / targetCharacter.data.maxMagicArmor;

        // Update movement points text
        movementPointsText.text = $"{targetCharacter.MovementPoints}";

        // Optional: Add visual feedback for active character
        bool isActive = GameManager.Instance.GetActiveCharacter() == targetCharacter;
        // You could add some visual indication that this is the active character
    }

    public void UpdateForCharacter(Character character)
    {
        if (character == null) return;

        // Update character icon
        characterIcon.sprite = character.data.characterIcon;

        // Update bars
        healthBarFill.fillAmount = (float)character.CurrentHealth / character.data.maxHealth;
        physicalArmorBarFill.fillAmount = (float)character.CurrentPhysicalArmor / character.data.maxPhysicalArmor;
        magicalArmorBarFill.fillAmount = (float)character.CurrentMagicArmor / character.data.maxMagicArmor;

        // Update movement points text
        movementPointsText.text = $"{character.MovementPoints}";
    }
} 