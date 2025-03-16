using UnityEngine;
using System;
using Scripts.Entities.Class;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    public CharacterData data;
    
    // Current stats with properties to handle UI updates
    private int currentHealth;
    private int currentPhysicalArmor;
    private int currentMagicArmor;
    private int currentActionPoints;
    private int currentMovementPoints;
    private int maxMovementPoints;  // Store the max movement points
    
    // Skill cooldowns
    private int skill1CurrentCooldown;
    private int skill2CurrentCooldown;

    // Add the private field for basic attack cooldown
    private int basicAttackCurrentCooldown;

    public bool IsDead { get; private set; }

    public HexCell CurrentTile { get; set; }
    public int MovementPoints
    {
        get => currentMovementPoints;
        set
        {
            currentMovementPoints = Mathf.Clamp(value, 0, maxMovementPoints);
        }
    }

    public int MaxMovementPoints => maxMovementPoints;  // Add getter for max movement points

    // Public properties with UI update triggers
    public int CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0, data.maxHealth);
    }

    public int CurrentPhysicalArmor
    {
        get => currentPhysicalArmor;
        set => currentPhysicalArmor = Mathf.Clamp(value, 0, data.maxPhysicalArmor);
    }

    public int CurrentMagicArmor
    {
        get => currentMagicArmor;
        set => currentMagicArmor = Mathf.Clamp(value, 0, data.maxMagicArmor);
    }

    private const int MOVEMENT_POINTS_PER_TURN = 2; // Now characters can only move 2 hexes per turn

    void Awake()
    {
        InitializeCharacter();
    }

    public virtual void Initialize(CharacterData characterData)
    {
        data = characterData;
        InitializeCharacter();
    }

    public void InitializeCharacter()
    {
        // Initialize all stats first
        currentHealth = data.maxHealth;
        currentPhysicalArmor = data.maxPhysicalArmor;
        currentMagicArmor = data.maxMagicArmor;
        maxMovementPoints = data.movementPoints;
        currentMovementPoints = maxMovementPoints;
        currentActionPoints = 4;
        
        // Initialize all cooldowns
        basicAttackCurrentCooldown = 0;
        skill1CurrentCooldown = 0;
        skill2CurrentCooldown = 0;

        // Then update the properties to ensure proper clamping
        CurrentHealth = currentHealth;
        CurrentPhysicalArmor = currentPhysicalArmor;
        CurrentMagicArmor = currentMagicArmor;
        MovementPoints = currentMovementPoints;
    }

    public void UseSkill(int skillIndex, Character target)
    {
        switch (skillIndex)
        {
            case 0: // Basic attack
                Debug.Log($"Using basic attack: {data.attackName}");
                basicAttackCurrentCooldown = 1;
                ApplyDamage(target, data.attackDamage, data.attackType);
                break;
            case 1:
                Debug.Log($"Using skill 1: {data.skill1Name}");
                skill1CurrentCooldown = data.skill1Cooldown;
                ApplyDamage(target, data.skill1Damage, data.skill1Type);
                break;
            case 2:
                Debug.Log($"Using skill 2: {data.skill2Name}");
                skill2CurrentCooldown = data.skill2Cooldown;
                ApplyDamage(target, data.skill2Damage, data.skill2Type);
                break;
        }
    }

    private void ApplyDamage(Character target, int amount, SkillType type)
    {
        if (target == null) return;

        switch (type)
        {
            case SkillType.PhysicalAttack:
                if (target.CurrentPhysicalArmor > 0)
                {
                    target.CurrentPhysicalArmor -= amount;
                    if (target.CurrentPhysicalArmor < 0)
                    {
                        target.CurrentHealth += target.CurrentPhysicalArmor; // Apply overflow damage to health
                        target.CurrentPhysicalArmor = 0;
                    }
                }
                else
                {
                    target.CurrentHealth -= amount;
                }
                break;

            case SkillType.MagicalAttack:
                if (target.CurrentMagicArmor > 0)
                {
                    target.CurrentMagicArmor -= amount;
                    if (target.CurrentMagicArmor < 0)
                    {
                        target.CurrentHealth += target.CurrentMagicArmor; // Apply overflow damage to health
                        target.CurrentMagicArmor = 0;
                    }
                }
                else
                {
                    target.CurrentHealth -= amount;
                }
                break;

            case SkillType.Heal:
                target.CurrentHealth = Mathf.Min(target.CurrentHealth + amount, data.maxHealth);
                break;

            case SkillType.Debuff:
                target.CurrentPhysicalArmor = Mathf.Max(0, target.CurrentPhysicalArmor - amount);
                break;
        }

        // Log the damage application for debugging
        Debug.Log($"{name} used {type} on {target.name} for {amount} damage/effect");

        // Check for death
        if (target.CurrentHealth <= 0)
        {
            target.Die();
        }

        // Trigger stat change event to update UI
        EventManager.Instance.Trigger(GameEvents.ON_CHARACTER_STAT_INFO_CHANGED, this, EventArgs.Empty);
    }

    private void Die()
    {
        if (IsDead) return; // Prevent multiple deaths
        
        IsDead = true;

        // Play death animation if available
        CharacterAnimator animator = GetComponent<CharacterAnimator>();
        if (animator != null)
        {
            animator.Die();
        }

        // Disable character components but keep the GameObject
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        var movement = GetComponent<CharacterMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // Remove outline if present
        var outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }

        // Notify GameManager about death
        GameManager.Instance.HandleCharacterDeath(this);

        // Set a death visual state (e.g., make the character lie down or fade out)
        Transform modelTransform = transform.GetChild(0); // Assuming the model is the first child
        if (modelTransform != null)
        {
            // Rotate the model to lie down
            modelTransform.localRotation = Quaternion.Euler(90, 0, 0);
            
            // Optional: Start a coroutine to fade out the model
            StartCoroutine(FadeOutModel(modelTransform.gameObject));
        }
    }

    private System.Collections.IEnumerator FadeOutModel(GameObject model)
    {
        // Get all renderers in the model
        var renderers = model.GetComponentsInChildren<Renderer>();
        var materials = new List<Material>();

        // Collect all materials and make them transparent
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_Mode"))
                {
                    material.SetFloat("_Mode", 2); // Set to fade mode
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                }
                materials.Add(material);
            }
        }

        // Fade out over 2 seconds
        float duration = 2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);

            foreach (var material in materials)
            {
                Color color = material.color;
                color.a = alpha;
                material.color = color;
            }

            yield return null;
        }

        // After fade out, disable the model
        model.SetActive(false);
    }

    public void StartTurn()
    {
        MovementPoints = maxMovementPoints;
        
        // Reduce cooldowns
        if (basicAttackCurrentCooldown > 0) basicAttackCurrentCooldown--;
        if (skill1CurrentCooldown > 0) skill1CurrentCooldown--;
        if (skill2CurrentCooldown > 0) skill2CurrentCooldown--;
    }

    public bool CanUseSkill(int skillNumber)
    {
        if (skillNumber == 0) return currentActionPoints >= 2 && basicAttackCurrentCooldown <= 0;
        if (skillNumber == 1) return currentActionPoints >= data.skill1ActionPointCost && skill1CurrentCooldown <= 0;
        if (skillNumber == 2) return currentActionPoints >= data.skill2ActionPointCost && skill2CurrentCooldown <= 0;
        return false;
    }

    public int GetSkillCooldown(int skillIndex)
    {
        switch (skillIndex)
        {
            case 0:
                return basicAttackCurrentCooldown;
            case 1:
                return skill1CurrentCooldown;
            case 2:
                return skill2CurrentCooldown;
            default:
                return 0;
        }
    }
} 