using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraAngle = 45f;
    [SerializeField] private float smoothTime = 0.5f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float followSpeed = 5f;

    private Vector2 moveInput;
    private Transform cameraTarget;
    private Vector3 targetPosition;
    private bool isFollowingCharacter = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (virtualCamera != null)
        {
            ConfigureVirtualCamera();
        }
    }

    private void ConfigureVirtualCamera()
    {
        var transposer = virtualCamera.GetComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_FollowOffset = new Vector3(0, cameraDistance, -cameraDistance);
            transposer.m_XDamping = smoothTime;
            transposer.m_YDamping = smoothTime;
            transposer.m_ZDamping = smoothTime;
        }

        virtualCamera.transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
        
        if (cameraTarget == null)
        {
            var targetObj = new GameObject("CameraTarget");
            cameraTarget = targetObj.transform;
            targetPosition = Vector3.zero;
            virtualCamera.Follow = cameraTarget;
        }
    }

    private void Update()
    {
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (isFollowingCharacter && GameManager.Instance.GetActiveCharacter() != null)
        {
            targetPosition = GameManager.Instance.GetActiveCharacter().transform.position;
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, targetPosition, Time.deltaTime * followSpeed);
        }
        else
        {
            Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);
            targetPosition += moveDir * (moveSpeed * Time.deltaTime);
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, targetPosition, Time.deltaTime * 10f);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput != Vector2.zero)
        {
            isFollowingCharacter = false;
            targetPosition = cameraTarget.position;
        }
    }

    // Called when character moves
    public void OnCharacterMoved(Character character)
    {
        if (character == GameManager.Instance.GetActiveCharacter() && moveInput == Vector2.zero)
        {
            isFollowingCharacter = true;
            targetPosition = character.transform.position;
        }
    }

    private void OnCharactersSpawned(List<Character> characters)
    {
        if (characters.Count > 0)
        {
            FocusOnCharacter(characters[0].transform);
        }
    }

    private void OnTurnStart()
    {
        Character activeCharacter = GameManager.Instance.GetActiveCharacter();
        if (activeCharacter != null)
        {
            FocusOnCharacter(activeCharacter.transform);
        }
    }

    public void FocusOnCharacter(Transform character)
    {
        isFollowingCharacter = true;
        targetPosition = character.position;
        cameraTarget.position = targetPosition;
    }

    private void Start()
    {
        if (WorldSettingManager.Instance != null)
        {
            WorldSettingManager.Instance.onCharactersSpawned.AddListener(OnCharactersSpawned);
        }
        
        GameManager.Instance.onTurnStart.AddListener(OnTurnStart);
    }

    private void OnDestroy()
    {
        if (WorldSettingManager.Instance != null)
        {
            WorldSettingManager.Instance.onCharactersSpawned.RemoveListener(OnCharactersSpawned);
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onTurnStart.RemoveListener(OnTurnStart);
        }
    }
} 