using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraAngle = 45f;
    [SerializeField] private float smoothTime = 0.5f;

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

        // Configure virtual camera if needed
        if (virtualCamera != null)
        {
            ConfigureVirtualCamera();
        }
    }

    private void ConfigureVirtualCamera()
    {
        // Set initial camera position and rotation
        var transposer = virtualCamera.GetComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_FollowOffset = new Vector3(0, cameraDistance, -cameraDistance);
            transposer.m_XDamping = smoothTime;
            transposer.m_YDamping = smoothTime;
            transposer.m_ZDamping = smoothTime;
        }

        // Set camera rotation
        virtualCamera.transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
    }

    private void Start()
    {
        // Subscribe to character spawn event
        if (WorldSettingManager.Instance != null)
        {
            WorldSettingManager.Instance.onCharactersSpawned.AddListener(OnCharactersSpawned);
        }
        
        // Subscribe to turn changes
        GameManager.Instance.onTurnStart.AddListener(OnTurnStart);
    }

    private void OnCharactersSpawned(List<Character> characters)
    {
        if (characters.Count > 0)
        {
            SetCameraTarget(characters[0].transform);
        }
    }

    private void OnTurnStart()
    {
        Character activeCharacter = GameManager.Instance.GetActiveCharacter();
        if (activeCharacter != null)
        {
            SetCameraTarget(activeCharacter.transform);
        }
    }

    public void SetCameraTarget(Transform target)
    {
        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
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