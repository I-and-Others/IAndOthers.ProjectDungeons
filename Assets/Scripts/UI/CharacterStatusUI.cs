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
    [SerializeField] private GameObject movementPointsContainer;

    [Header("Colors")]
    [SerializeField] private Color healthColor = Color.green;
    [SerializeField] private Color physicalArmorColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color magicalArmorColor = new Color(0.4f, 0.6f, 1f);

    [Header("Visibility")]
    [SerializeField] private float visibilityDistance = 20f;
    [SerializeField] private CanvasGroup canvasGroup;

    private Character character;
    private Camera mainCamera;
    private RectTransform rectTransform;
    private Vector3 offset = new Vector3(0, 2f, 0); // Adjust this to position the UI above the character

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    public void Initialize(Character character)
    {
        this.character = character;
        characterIcon.sprite = character.data.characterIcon;
        UpdateUI();
    }

    private void LateUpdate()
    {
        if (character != null)
        {
            // Update position to follow character
            Vector3 worldPosition = character.transform.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            
            if (screenPosition.z < 0)
            {
                screenPosition *= -1;
            }

            rectTransform.position = screenPosition;
            
            // Update stats
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // Update health bar
        float healthPercent = (float)character.CurrentHealth / character.data.maxHealth;
        healthBarFill.fillAmount = healthPercent;

        // Update armor bars
        float physicalArmorPercent = (float)character.CurrentPhysicalArmor / character.data.maxPhysicalArmor;
        physicalArmorBarFill.fillAmount = physicalArmorPercent;

        float magicalArmorPercent = (float)character.CurrentMagicArmor / character.data.maxMagicArmor;
        magicalArmorBarFill.fillAmount = magicalArmorPercent;

        // Update movement points
        movementPointsText.text = character.MovementPoints.ToString();
        movementPointsContainer.SetActive(GameManager.Instance.IsCharacterTurn(character));
    }

    private void UpdateVisibility()
    {
        if (canvasGroup != null)
        {
            float distanceToCamera = Vector3.Distance(mainCamera.transform.position, character.transform.position);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, (distanceToCamera - visibilityDistance) / 5f);
        }
    }
} 