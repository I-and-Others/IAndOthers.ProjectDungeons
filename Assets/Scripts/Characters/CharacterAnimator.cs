using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    private Transform characterModel;
    
    // Animation parameter hashes
    private readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
    private readonly int AttackHash = Animator.StringToHash("Attack");
    private readonly int DeathHash = Animator.StringToHash("Death");

    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private float speedMultiplier = 2f; // Adjusted to match animation thresholds

    private Vector3 lastPosition;
    private bool isDead;
    private Vector3 targetDirection;
    private float currentMovementSpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterModel = transform.GetChild(0);
        lastPosition = transform.position;
        targetDirection = characterModel.forward;
    }

    private void Update()
    {
        if (!isDead)
        {
            UpdateRotation();
        }
    }

    private void UpdateRotation()
    {
        if (currentMovementSpeed > movementThreshold)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            characterModel.rotation = Quaternion.Slerp(
                characterModel.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    public void UpdateMovementSpeed(float normalizedSpeed) // 0-1 range
    {
        currentMovementSpeed = normalizedSpeed;
        // Map the normalized speed (0-1) to animation blend values (0 = idle, 2 = walk, 6 = run)
        float animationSpeed = normalizedSpeed * speedMultiplier * 6f; // Map to maximum blend value
        animator.SetFloat(MovementSpeedHash, animationSpeed);
    }

    public void UpdateMoveDirection(Vector3 direction)
    {
        if (direction.magnitude > movementThreshold)
        {
            targetDirection = direction;
        }
    }

    public void PlayAttack()
    {
        if (!isDead)
        {
            animator.SetTrigger(AttackHash);
        }
    }

    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            animator.SetTrigger(DeathHash);
        }
    }
} 