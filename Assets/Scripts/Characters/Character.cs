using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterData data;
    
    // Current stats
    private int currentHealth;
    private int currentPhysicalArmor;
    private int currentMagicArmor;
    private int currentActionPoints;
    private int currentMovementPoints;
    
    // Skill cooldowns
    private int skill1CurrentCooldown;
    private int skill2CurrentCooldown;

    public HexCell CurrentTile { get; set; }
    public int MovementPoints 
    { 
        get => currentMovementPoints;
        set => currentMovementPoints = value;
    }

    // Add these public properties to access current stats
    public int CurrentHealth { get => currentHealth; }
    public int CurrentPhysicalArmor { get => currentPhysicalArmor; }
    public int CurrentMagicArmor { get => currentMagicArmor; }

    private const int MOVEMENT_POINTS_PER_TURN = 2; // Now characters can only move 2 hexes per turn

    private void Start()
    {
        InitializeCharacter();
    }

    private void InitializeCharacter()
    {
        currentHealth = data.maxHealth;
        currentPhysicalArmor = data.maxPhysicalArmor;
        currentMagicArmor = data.maxMagicArmor;
        currentMovementPoints = data.movementPoints;
        currentActionPoints = 4; // Standard AP per turn
        
        skill1CurrentCooldown = 0;
        skill2CurrentCooldown = 0;
    }

    public void UseSkill(int skillNumber, Character target)
    {
        if (skillNumber == 0) // Basic attack
        {
            ApplyDamage(target, data.attackDamage, data.attackType);
        }
        else if (skillNumber == 1 && skill1CurrentCooldown <= 0)
        {
            ApplySkill1(target);
            skill1CurrentCooldown = data.skill1Cooldown;
        }
        else if (skillNumber == 2 && skill2CurrentCooldown <= 0)
        {
            ApplySkill2(target);
            skill2CurrentCooldown = data.skill2Cooldown;
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
                if (target.currentPhysicalArmor > 0)
                {
                    target.currentPhysicalArmor -= amount;
                    if (target.currentPhysicalArmor < 0)
                    {
                        target.currentHealth += target.currentPhysicalArmor;
                        target.currentPhysicalArmor = 0;
                    }
                }
                else
                {
                    target.currentHealth -= amount;
                }
                break;

            case SkillType.MagicalAttack:
                if (target.currentMagicArmor > 0)
                {
                    target.currentMagicArmor -= amount;
                    if (target.currentMagicArmor < 0)
                    {
                        target.currentHealth += target.currentMagicArmor;
                        target.currentMagicArmor = 0;
                    }
                }
                else
                {
                    target.currentHealth -= amount;
                }
                break;

            case SkillType.Heal:
                target.currentHealth = Mathf.Min(target.currentHealth + amount, target.data.maxHealth);
                break;

            case SkillType.Debuff:
                // For debuff, we'll reduce physical armor
                target.currentPhysicalArmor = Mathf.Max(0, target.currentPhysicalArmor - amount);
                break;
        }

        // Check for death
        if (target.currentHealth <= 0)
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
        currentMovementPoints = MOVEMENT_POINTS_PER_TURN;
        // Reset other stats as needed
        
        // Reduce cooldowns
        if (skill1CurrentCooldown > 0) skill1CurrentCooldown--;
        if (skill2CurrentCooldown > 0) skill2CurrentCooldown--;
    }

    public bool CanUseSkill(int skillNumber)
    {
        if (skillNumber == 0) return currentActionPoints >= 2; // Basic attack cost
        if (skillNumber == 1) return currentActionPoints >= data.skill1ActionPointCost && skill1CurrentCooldown <= 0;
        if (skillNumber == 2) return currentActionPoints >= data.skill2ActionPointCost && skill2CurrentCooldown <= 0;
        return false;
    }
} 