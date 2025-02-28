using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum SelectionMode
{
    Move,
    TargetSelect
}

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    [SerializeField] private Color selectedCharacterColor = Color.green;
    [SerializeField] private Color unselectedCharacterColor = Color.yellow;
    [SerializeField] private Color validMoveColor = Color.green;
    [SerializeField] private Color invalidMoveColor = Color.red;

    [SerializeField] private LayerMask characterLayer; // Set this to the layer your characters are on
    [SerializeField] private LayerMask hexLayer;       // Set this to the layer your hex tiles are on

    private Character selectedCharacter;
    private List<HexCell> highlightedCells = new List<HexCell>();
    private Camera mainCamera;

    private SelectionMode currentMode = SelectionMode.Move;
    private int currentRange;
    [SerializeField] private SkillBarUI skillBarUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        mainCamera = Camera.main;
    }

    private void Start()
    {
        // Initialize all character outlines
        foreach (var character in FindObjectsOfType<Character>())
        {
            Outline outline = character.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
                outline.OutlineColor = unselectedCharacterColor;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the pointer is over a UI element
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                HandleSelection();
            }
        }
    }

    private void HandleSelection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // First try to hit characters
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, characterLayer))
        {
            Character hitCharacter = hit.collider.GetComponent<Character>();
            if (hitCharacter != null)
            {
                SelectCharacter(hitCharacter);
            }
        }
        // Then try to hit hex cells
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, hexLayer))
        {
            HexCell hitCell = hit.collider.GetComponent<HexCell>();
            if (hitCell != null && selectedCharacter != null)
            {
                OnHexClicked(hitCell);
            }
        }
    }

    public void SelectCharacter(Character character)
    {
        // Only allow selection of the active character
        if (!GameManager.Instance.IsCharacterTurn(character))
        {
            return;
        }

        // Deselect previous character
        if (selectedCharacter != null)
        {
            UpdateCharacterOutline(selectedCharacter, false);
            ClearHexHighlights();
        }

        selectedCharacter = character;
        UpdateCharacterOutline(selectedCharacter, true);
        HighlightReachableCells();
    }

    private void UpdateCharacterOutline(Character character, bool isSelected)
    {
        Outline outline = character.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineWidth = 5f;
            outline.OutlineColor = isSelected ? selectedCharacterColor : unselectedCharacterColor;
        }
    }

    private void HighlightReachableCells()
    {
        ClearHexHighlights();

        if (selectedCharacter == null) return;

        CharacterMovement movement = selectedCharacter.GetComponent<CharacterMovement>();
        var reachableCells = currentMode == SelectionMode.Move 
            ? movement.FindReachableCells() 
            : movement.FindCellsInRange(currentRange);

        foreach (var cell in reachableCells)
        {
            Outline outline = cell.GetComponent<Outline>();
            if (outline == null)
                outline = cell.gameObject.AddComponent<Outline>();

            outline.enabled = true;
            outline.OutlineColor = currentMode == SelectionMode.Move ? validMoveColor : selectedCharacterColor;
            highlightedCells.Add(cell);
        }
    }

    private void ClearHexHighlights()
    {
        foreach (var cell in highlightedCells)
        {
            if (cell != null)
            {
                var outline = cell.GetComponent<Outline>();
                if (outline != null)
                    outline.enabled = false;
            }
        }
        highlightedCells.Clear();
    }

    private void TryMoveCharacter(HexCell targetCell)
    {
        if (selectedCharacter == null) return;

        CharacterMovement movement = selectedCharacter.GetComponent<CharacterMovement>();
        if (movement != null)
        {
            // Debug.Log($"Attempting to move to hex at: {targetCell.q}, {targetCell.r}");
            movement.MoveTo(targetCell);
        }
    }

    public Character GetSelectedCharacter()
    {
        return selectedCharacter;
    }

    // Add this method to update highlights after movement
    public void UpdateHighlights()
    {
        if (selectedCharacter != null)
        {
            HighlightReachableCells();
        }
    }

    public void SetSelectionMode(SelectionMode mode, int range = 0)
    {
        currentMode = mode;
        currentRange = range;
        // Update highlights based on new mode
        UpdateHighlights();
    }

    private void OnHexClicked(HexCell cell)
    {
        if (currentMode == SelectionMode.Move)
        {
            TryMoveCharacter(cell);
        }
        else if (currentMode == SelectionMode.TargetSelect)
        {
            skillBarUI.TryUseSkillOnCell(cell);
        }
    }
} 