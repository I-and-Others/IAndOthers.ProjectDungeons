using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    public string characterName;
    public CharacterType characterType;
    public Sprite characterIcon;
    public GameObject characterPrefab;

    [Header("Base Stats")]
    public int maxHealth = 100;
    public int maxPhysicalArmor = 50;
    public int maxMagicArmor = 50;
    public int movementPoints = 4;
    public int initiative = 5;


    [Header("Basic Attack")]
    public string attackName = "Attack";
    public int attackDamage = 10;
    public SkillType attackType = SkillType.PhysicalAttack;
    public Sprite attackIcon;

    [Header("Skill 1")]
    public string skill1Name;
    public int skill1Damage;
    public SkillType skill1Type;
    public Sprite skill1Icon;
    public int skill1ActionPointCost = 2;
    public int skill1Cooldown = 2;

    [Header("Skill 2")]
    public string skill2Name;
    public int skill2Damage;
    public SkillType skill2Type;
    public Sprite skill2Icon;
    public int skill2ActionPointCost = 2;
    public int skill2Cooldown = 3;
} 