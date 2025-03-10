using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Scripts.Entities.Class;

public class Enemy : Character
{
    private Character targetCharacter;
    private CharacterMovement movement;
    private EnemyData enemyData => (EnemyData)data;

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
    }

    public void StartTurn()
    {
        base.StartTurn();
        ExecuteTurn();
    }

    public void ExecuteTurn()
    {
        // Find closest hero
        targetCharacter = FindClosestHero();
        
        if (targetCharacter != null)
        {
            // Calculate distance to target
            int distanceToTarget = HexDistance(CurrentTile, targetCharacter.CurrentTile);
            
            // If target is within attack range, attack
            if (distanceToTarget <= enemyData.preferredRange)
            {
                PerformAttack();
            }
            // Otherwise, move towards target if not in attack range
            else
            {
                MoveTowardsTarget();
            }
        }
        
        // End turn after actions
        GameManager.Instance.EndTurn();
    }

    private void PerformAttack()
    {
        // Choose a skill to use based on cooldowns and target
        int skillToUse = ChooseSkill();
        
        if (skillToUse >= 0)
        {
            // Create event args with attack information
            var attackArgs = new OnAIAttackEventArgs
            {
                Attacker = this,
                Target = targetCharacter,
                SkillIndex = skillToUse,
                Damage = GetSkillDamage(skillToUse),
                SkillType = GetSkillType(skillToUse)
            };

            // Trigger the AI attack event
            EventManager.Instance.Trigger(GameEvents.ON_AI_ATTACK, this, attackArgs);
            
            // Apply the skill effect
            UseSkill(skillToUse, targetCharacter);
        }
    }

    private int ChooseSkill()
    {
        // Try to use skills in priority order (skill2 > skill1 > basic attack)
        if (CanUseSkill(2)) return 2;
        if (CanUseSkill(1)) return 1;
        if (CanUseSkill(0)) return 0;
        return -1;
    }

    private int GetSkillDamage(int skillIndex)
    {
        switch (skillIndex)
        {
            case 0: return data.attackDamage;
            case 1: return data.skill1Damage;
            case 2: return data.skill2Damage;
            default: return 0;
        }
    }

    private SkillType GetSkillType(int skillIndex)
    {
        switch (skillIndex)
        {
            case 0: return data.attackType;
            case 1: return data.skill1Type;
            case 2: return data.skill2Type;
            default: return SkillType.PhysicalAttack;
        }
    }

    private Character FindClosestHero()
    {
        var heroes = FindObjectsOfType<Character>()
            .Where(c => !(c is Enemy) && c.gameObject.activeInHierarchy)
            .ToList();

        if (heroes.Count == 0) return null;

        return heroes.OrderBy(h => HexDistance(CurrentTile, h.CurrentTile))
            .FirstOrDefault();
    }

    private void MoveTowardsTarget()
    {
        if (targetCharacter == null || movement == null) return;

        // Get the target's hex cell
        HexCell targetCell = targetCharacter.CurrentTile;
        if (targetCell == null) return;

        // Get current position
        HexCell currentCell = CurrentTile;
        if (currentCell == null) return;

        // Get all reachable cells with our current movement points
        var reachableCells = movement.FindReachableCells();
        if (reachableCells.Count == 0) return;

        // Find the cell that gets us closest to the preferred range
        HexCell bestCell = reachableCells
            .OrderBy(cell => Mathf.Abs(HexDistance(cell, targetCell) - enemyData.preferredRange))
            .FirstOrDefault();

        // If we found a valid cell to move to
        if (bestCell != null)
        {
            movement.MoveTo(bestCell);
        }
    }

    private int HexDistance(HexCell a, HexCell b)
    {
        return (Mathf.Abs(a.q - b.q) 
                + Mathf.Abs(a.q + a.r - b.q - b.r)
                + Mathf.Abs(a.r - b.r)) / 2;
    }
} 