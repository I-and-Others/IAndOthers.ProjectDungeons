using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class SkillBarUI : MonoBehaviour
{
    [System.Serializable]
    public class SkillButton
    {
        public Button button;
        public Image skillIcon;
        public Image cooldownOverlay;
        public TextMeshProUGUI cooldownText;
    }

    [SerializeField] private List<SkillButton> skillButtons;
    private Character currentCharacter;
    private int selectedSkillIndex = -1;
    private const int SKILL_RANGE = 3; // 3 hex range for all skills for now

    private void Start()
    {
        // Setup button listeners
        for (int i = 0; i < skillButtons.Count; i++)
        {
            int index = i; // Capture the index for the lambda
            skillButtons[i].button.onClick.AddListener(() => OnSkillButtonClicked(index));
        }

        GameManager.Instance.onTurnStart.AddListener(OnTurnStart);
        GameManager.Instance.onTurnEnd.AddListener(OnTurnEnd);

        // Add listener for the Cancel action
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["Cancel"].performed += ctx => CancelSkillSelection();
        }
    }

    private void OnTurnStart()
    {
        UpdateSkillBar(GameManager.Instance.GetActiveCharacter());
    }

    private void OnTurnEnd()
    {
        selectedSkillIndex = -1; // Reset selected skill
    }

    public void UpdateSkillBar(Character character)
    {
        currentCharacter = character;
        if (character == null) return;

        // Basic Attack (Skill 0)
        skillButtons[0].skillIcon.sprite = character.data.attackIcon;
        
        // Skill 1
        skillButtons[1].skillIcon.sprite = character.data.skill1Icon;
        
        // Skill 2
        skillButtons[2].skillIcon.sprite = character.data.skill2Icon;

        UpdateCooldowns();
    }

    private void UpdateCooldowns()
    {
        if (currentCharacter == null) return;

        for (int i = 0; i < skillButtons.Count; i++)
        {
            bool isOnCooldown = currentCharacter.GetSkillCooldown(i) > 0;
            skillButtons[i].cooldownOverlay.gameObject.SetActive(isOnCooldown);
            
            if (isOnCooldown)
            {
                skillButtons[i].cooldownText.text = currentCharacter.GetSkillCooldown(i).ToString();
            }
            
            skillButtons[i].button.interactable = currentCharacter.CanUseSkill(i) && 
                                                GameManager.Instance.IsCharacterTurn(currentCharacter);
        }
    }

    private void OnSkillButtonClicked(int skillIndex)
    {
        if (selectedSkillIndex == skillIndex)
        {
            // Deselect the skill if it's already selected
            CancelSkillSelection();
        }
        else
        {
            selectedSkillIndex = skillIndex;
            Debug.Log($"Selected skill {skillIndex}");
            
            // Update selection manager to show skill range
            SelectionManager.Instance.SetSelectionMode(SelectionMode.TargetSelect, SKILL_RANGE);
        }
    }

    public void CancelSkillSelection()
    {
        if (selectedSkillIndex != -1)
        {
            Debug.Log("Skill selection canceled");
            selectedSkillIndex = -1;
            SelectionManager.Instance.SetSelectionMode(SelectionMode.Move);
        }
    }

    public void TryUseSkillOnCell(HexCell targetCell)
    {
        Debug.Log($"Attempting to use skill {selectedSkillIndex} on cell {targetCell.q}, {targetCell.r}");
        if (selectedSkillIndex == -1 || currentCharacter == null) return;

        // Check if target is in range
        int distance = HexDistance(currentCharacter.CurrentTile, targetCell);
        if (distance > SKILL_RANGE)
        {
            Debug.Log($"Target is too far! Distance: {distance}, Max Range: {SKILL_RANGE}");
            return;
        }

        // Update cooldown and UI
        currentCharacter.UseSkill(selectedSkillIndex, null); // We'll add target later
        UpdateCooldowns();
        
        // Reset selection
        selectedSkillIndex = -1;
        SelectionManager.Instance.SetSelectionMode(SelectionMode.Move);
    }

    private int HexDistance(HexCell a, HexCell b)
    {
        return (Mathf.Abs(a.q - b.q) 
                + Mathf.Abs(a.q + a.r - b.q - b.r)
                + Mathf.Abs(a.r - b.r)) / 2;
    }

    public void SelectSkill(int skillIndex)
    {
        if (selectedSkillIndex == skillIndex)
        {
            CancelSkillSelection();
        }
        else
        {
            selectedSkillIndex = skillIndex;
            Debug.Log($"Selected skill {skillIndex}");
            SelectionManager.Instance.SetSelectionMode(SelectionMode.TargetSelect, SKILL_RANGE);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onTurnStart.RemoveListener(OnTurnStart);
            GameManager.Instance.onTurnEnd.RemoveListener(OnTurnEnd);
        }
    }
} 