using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private CinemachineCamera virtualCamera;

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
    }

    private void Start()
    {
        // Subscribe to character spawn event
        WorldSettingManager.Instance.onCharactersSpawned.AddListener(OnCharactersSpawned);
        
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
} 