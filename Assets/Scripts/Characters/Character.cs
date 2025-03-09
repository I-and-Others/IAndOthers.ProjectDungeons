using UnityEngine;

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
                break;
            case 1:
                Debug.Log($"Using skill 1: {data.skill1Name}");
                skill1CurrentCooldown = data.skill1Cooldown;
                break;
            case 2:
                Debug.Log($"Using skill 2: {data.skill2Name}");
                skill2CurrentCooldown = data.skill2Cooldown;
                break;
        }
    }

    private void ApplySkill1(Character target)
    {
        ApplyDamage(target, data.skill1Damage, data.skill1Type);
    }

    private void ApplySkill2(Character target)
    {
        ApplyDamage(target, data.skill2Damage, data.skill2Type);
    }

    private void ApplyDamage(Character target, int amount, SkillType type)
    {
        switch (type)
        {
            case SkillType.PhysicalAttack:
                if (target.CurrentPhysicalArmor > 0)
                {
                    target.CurrentPhysicalArmor -= amount;
                    if (target.CurrentPhysicalArmor < 0)
                    {
                        target.CurrentHealth += target.CurrentPhysicalArmor;
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
                        target.CurrentHealth += target.CurrentMagicArmor;
                        target.CurrentMagicArmor = 0;
                    }
                }
                else
                {
                    target.CurrentHealth -= amount;
                }
                break;

            case SkillType.Heal:
                target.CurrentHealth = Mathf.Min(target.CurrentHealth + amount, target.data.maxHealth);
                break;

            case SkillType.Debuff:
                // For debuff, we'll reduce physical armor
                target.CurrentPhysicalArmor = Mathf.Max(0, target.CurrentPhysicalArmor - amount);
                break;
        }

        // Check for death
        if (target.CurrentHealth <= 0)
        {
            target.Die();
        }
    }

    private void Die()
    {
        // Handle character death
        gameObject.SetActive(false);
        // You might want to add more death handling logic here
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