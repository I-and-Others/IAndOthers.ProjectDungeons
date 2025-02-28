using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputManager : MonoBehaviour
{
    [SerializeField] private SkillBarUI skillBarUI;

    private void Awake()
    {
        if (skillBarUI == null)
        {
            skillBarUI = FindObjectOfType<SkillBarUI>();
        }
    }

    private void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["Cancel"].performed += OnCancel;
            playerInput.actions["Skill1"].performed += ctx => OnSkillSelect(0);
            playerInput.actions["Skill2"].performed += ctx => OnSkillSelect(1);
            playerInput.actions["Skill3"].performed += ctx => OnSkillSelect(2);
        }
    }

    private void OnDisable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions["Cancel"].performed -= OnCancel;
            playerInput.actions["Skill1"].performed -= ctx => OnSkillSelect(0);
            playerInput.actions["Skill2"].performed -= ctx => OnSkillSelect(1);
            playerInput.actions["Skill3"].performed -= ctx => OnSkillSelect(2);
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        skillBarUI.CancelSkillSelection();
    }

    private void OnSkillSelect(int skillIndex)
    {
        skillBarUI.SelectSkill(skillIndex);
    }
} 