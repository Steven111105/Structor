using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum Direction
{
    // North = 0, East = 1, South = 2, West = 3
    South = 0,
    East = 1,
    North = 2,
    West = 3,
}

public enum CardType
{
    StraightWire, BendWire, TSplitter, Booster, Sensor, CPU, SuperCPU, SolarPanel, OverclockModule, QuantumProcessor
}

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    [Header("Grid Settings")]
    public int gridWidth = 15;
    public int gridHeight = 11;
    public float cellSize = 1f;

    // Grid storage
    private GridObject[,] grid;
    private Vector2Int cpuPosition;
    public List<Vector2Int> cpuPositions = new List<Vector2Int>(); // Track all CPU positions
    public List<BeamPathTracker> successfulBeamPaths = new List<BeamPathTracker>();

    public GameObject gridObjectPrefab;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void InitializeGrid()
    {
        grid = new GridObject[gridWidth, gridHeight];
        cpuPosition = new Vector2Int(gridWidth / 2, gridHeight / 2); // Center position
        cpuPositions.Clear(); // Clear all CPUs when loading a new battle
        // make middle CPU
        CreateGridObject(SelectedDeckData.instance.selectedDeck.middleShooter, cpuPosition);
    }


    public void FireBeams()
    {
        Debug.Log("=== FIRING BEAMS FROM ALL CPUs ===");
        // Fire beams in all 4 directions from every CPU
        // get the card data from the grid object to get the base cpu damage
        float baseDamage = 0f;
        foreach (var cpuPos in cpuPositions)
        {
            Debug.Log($"[CPU] Firing beams from CPU at {cpuPos}");
            GridObject gridObj = GetGridObject(cpuPos); // Assume first CPU for damage reference
            if (gridObj != null)
            {
                baseDamage = gridObj.cardData.baseDamage;
                FireBeamInDirection(Direction.North, cpuPos, baseDamage);
                FireBeamInDirection(Direction.South, cpuPos, baseDamage);
                FireBeamInDirection(Direction.East, cpuPos, baseDamage);
                FireBeamInDirection(Direction.West, cpuPos, baseDamage);
            }
            else
            {
                Debug.LogError($"No GridObject found at CPU position {cpuPos}");
            }
        }
    }

    void FireBeamInDirection(Direction direction, Vector2Int cpuPos, float baseDamage)
    {
        Vector2Int targetPos = GetPositionInDirection(cpuPos, direction);
        // Debug.Log($"[CPU] Firing beam {direction} from {cpuPos} to {targetPos}");
        if (IsValidPosition(targetPos))
        {
            var target = grid[targetPos.x, targetPos.y];
            if (target != null)
            {
                var tracker = new BeamPathTracker(); // Create a new tracker for this beam
                switch (direction)
                {
                    case Direction.North:
                        target.BeamComingFromSouth(baseDamage, tracker);
                        break;
                    case Direction.South:
                        target.BeamComingFromNorth(baseDamage, tracker);
                        break;
                    case Direction.East:
                        target.BeamComingFromWest(baseDamage, tracker);
                        break;
                    case Direction.West:
                        target.BeamComingFromEast(baseDamage, tracker);
                        break;
                }
            }
        }
        else
        {
            Debug.Log($"[CPU] Invalid position {targetPos}");
        }
    }

    public GridObject GetNeighbor(Vector2Int position, Direction direction)
    {
        Vector2Int neighborPos = GetPositionInDirection(position, direction);

        if (IsValidPosition(neighborPos))
        {
            return grid[neighborPos.x, neighborPos.y];
        }

        return null;
    }

    public GridObject GetGridObject(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            return grid[position.x, position.y];
        }
        Debug.Log("Position out of bounds: " + position);
        return null;
    }

    Vector2Int GetPositionInDirection(Vector2Int position, Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector2Int(position.x, position.y + 1);
            case Direction.South:
                return new Vector2Int(position.x, position.y - 1);
            case Direction.East:
                return new Vector2Int(position.x + 1, position.y);
            case Direction.West:
                return new Vector2Int(position.x - 1, position.y);
            default:
                return position;
        }
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth &&
               position.y >= 0 && position.y < gridHeight;
    }

    public bool PlaceObject(Vector2Int position, GridObject obj)
    {
        // Check if position is within grid bounds
        if (!IsValidPosition(position))
        {
            return false;
        }

        // Check if position is already occupied
        if (grid[position.x, position.y] != null)
        {
            return false;
        }

        // Place the object in the single cell
        grid[position.x, position.y] = obj;

        // Assign sprite to the GridObject if it's a GridObject
        if (obj is GridObject gridObject)
        {
            AssignSpriteToGridObject(gridObject);
            // Update visual rotation after sprite assignment to ensure proper orientation
            gridObject.UpdateVisualRotation();
        }

        return true;
    }

    public void AssignSpriteToGridObject(GridObject gridObject)
    {
        if (gridObject.cardData == null) return;

        // Get or create SpriteRenderer on main object
        SpriteRenderer spriteRenderer = gridObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gridObject.gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 1;
        }

        // Assign the correct sprite based on card type
        switch (gridObject.cardData.cardType)
        {
            case CardType.StraightWire:
            case CardType.BendWire:
            case CardType.TSplitter:
            case CardType.Sensor:
                spriteRenderer.sprite = gridObject.cardData.gridObjectSprite;
                break;
        }
        spriteRenderer.color = Color.white;
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        // Center the grid around the GridManager's position
        // For a 15x11 grid, center is at (7,5), so we offset by that amount
        float offsetX = cpuPosition.x * cellSize;
        float offsetY = cpuPosition.y * cellSize;

        Vector3 worldPos = new Vector3(
            (gridPos.x * cellSize) - offsetX,
            (gridPos.y * cellSize) - offsetY,
            0
        );

        return transform.position + worldPos;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        // Convert world position back to grid coordinates
        Vector3 localPos = worldPos - transform.position;

        float offsetX = cpuPosition.x * cellSize;
        float offsetY = cpuPosition.y * cellSize;

        return new Vector2Int(
            Mathf.RoundToInt((localPos.x + offsetX) / cellSize),
            Mathf.RoundToInt((localPos.y + offsetY) / cellSize)
        );
    }

    public Vector2Int ScreenToGridPosition(Vector3 screenPos)
    {
        // Convert screen position (like mouse position) to grid coordinates
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0; // Ensure we're working in 2D
        return WorldToGridPosition(worldPos);
    }

    void Update()
    {
        // Debug: Show grid position under mouse (remove this after testing)
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Vector2Int gridPos = ScreenToGridPosition(Input.mousePosition);
        //     Vector3 worldPos = GridToWorldPosition(gridPos);
        //     Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     mouseWorldPos.z = 0;

        //     Debug.Log($"Mouse clicked at:");
        //     Debug.Log($"  Screen: {Input.mousePosition}");
        //     Debug.Log($"  Mouse World: {mouseWorldPos}");
        //     Debug.Log($"  Grid Position: {gridPos}");
        //     Debug.Log($"  Grid World Position: {worldPos}");
        //     Debug.Log($"  GridManager Position: {transform.position}");

        //     // Also show what's at that position
        //     var gridObj = GetGridObject(gridPos);
        //     if (gridObj != null)
        //     {
        //         Debug.Log($"  Found object: {gridObj.CardData.cardName} at {gridObj.transform.position}");
        //     }
        //     else
        //     {
        //         Debug.Log("  No object at this position");
        //     }
        // }

        // // Test beam firing with spacebar
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     FireBeams();
        // }
    }

    public IEnumerator ShowSuccessfulBeamPaths()
    {
        float duration = 0.25f; // Duration to show each damage text
        foreach (BeamPathTracker tracker in successfulBeamPaths)
        {
            Vector2Int[] path = tracker.path.ToArray();
            Debug.Log($"Processiong beam path: {string.Join(" -> ", path)}");
            int index = 0;
            foreach (Vector2Int pos in path)
            {
                // Create a text object at each position in the path
                Debug.Log($"Created damage text at {pos} for {tracker.damages[index]} damage");
                yield return StartCoroutine(CreateDamageText(pos, tracker.damages[index], duration));
                index++;
            }
        }
        successfulBeamPaths.Clear(); // Clear after displaying
        yield return new WaitForSeconds(1f); // Wait a moment to let player see the texts
        yield return null;
    }

    IEnumerator CreateDamageText(Vector2Int gridPos, int damage, float duration)
    {
        // Create a new GameObject for the text
        GameObject textObj = new GameObject("DamageText");
        textObj.transform.SetParent(transform);

        // Position it at the center of the grid cell
        textObj.transform.position = GridToWorldPosition(gridPos) + new Vector3(0, 0, -1); // Slightly in front
        textObj.transform.localScale = Vector3.one * 0.001f; // Scale down for world space

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        Debug.Log($"Added TMP_Text component at {textObj.transform.position}");
        text.text = damage.ToString();
        text.enableAutoSizing = true;
        text.fontSizeMin = 24;
        text.fontSizeMax = 50;
        text.fontSize = 64;
        text.color = Color.red;
        text.alignment = TextAlignmentOptions.Center;
        textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);

        yield return StartCoroutine(TextBounce(text,duration));
    }

    IEnumerator TextBounce(TMP_Text text, float duration)
    {
        float elapsed = 0f;
        Vector3 originalScale = text.transform.localScale;
        Vector3 targetScale = Vector3.one * 0.015f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Bounce effect using sine wave
            float scale = Mathf.Sin(t * Mathf.PI);
            text.transform.localScale = Vector3.Lerp(originalScale, targetScale, scale);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure it ends at original scale
        text.transform.localScale = originalScale;

        // Destroy the text object after animation
        Destroy(text.gameObject);
    }
    // Visual debugging - draw grid in scene view
    void OnDrawGizmos()
    {
        // Draw grid outline
        Gizmos.color = Color.white;
        Vector3 gridSize = new Vector3(gridWidth * cellSize, gridHeight * cellSize, 0);
        Gizmos.DrawWireCube(transform.position, gridSize);

        // Draw CPU position
        Gizmos.color = Color.yellow;
        Vector3 cpuWorldPos = GridToWorldPosition(cpuPosition);
        Gizmos.DrawWireCube(cpuWorldPos, Vector3.one * cellSize * 0.8f);

        // Draw grid cells
        Gizmos.color = Color.gray;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 cellWorldPos = GridToWorldPosition(new Vector2Int(x, y));
                Gizmos.DrawWireCube(cellWorldPos, Vector3.one * cellSize * 0.9f);
            }
        }
    }

    // Create Grid Objects
    public GridObject CreateGridObject(CardData cardData, Vector2Int position)
    {
        if(!IsValidPosition(position)) return null;
        GameObject obj = Instantiate(gridObjectPrefab);
        obj.name = $"{cardData.cardName}_{position.x}_{position.y}";

        // Parent the object to the GridManager for organization
        obj.transform.SetParent(transform);

        GridObject gridObj = obj.GetComponent<GridObject>();

        // Create minimal CardData
        CardData testCard = cardData.Clone();

        gridObj.cardData = testCard;
        gridObj.gridPosition = position;
        gridObj.transform.localRotation = Quaternion.identity;

        // Explicitly set input direction to South BEFORE any rotation/visual setup
        gridObj.InitializeGridObject();

        // Position in world - this already accounts for GridManager's position
        obj.transform.position = GridToWorldPosition(position);

        // Scale the object to match the cell size
        obj.transform.localScale = Vector3.one * cellSize;

        // If this is a CPU card, add to cpuPositions and log
        if (cardData.cardType == CardType.CPU || cardData.cardType == CardType.SuperCPU)
        {
            cpuPositions.Add(position);
            Debug.Log($"[CPU] Added extra CPU at {position} by card");
        }

        // Add to grid - this will automatically handle sprite assignment
        bool placed = PlaceObject(position, gridObj);
        if (!placed)
        {
            // Debug.LogError($"Failed to place {baseName} at {position}!");
            DestroyImmediate(obj);
            return null;
        }
        if(position != cpuPosition)
        {
            SFXManager.instance.PlaySFX("Deploy");
        }
        // Debug.Log($"Created {baseName} at grid {position}, world {obj.transform.position}");
        return gridObj;
    }

    public void ClearGridObjects()
    {
        foreach (Transform child in transform)
        {
            GridObject gridObj = child.GetComponent<GridObject>();
            if (gridObj != null)
            {
                Destroy(child.gameObject);
            }
        }
        grid = new GridObject[gridWidth, gridHeight];
        cpuPositions.Clear();
    }
    
    public void DestroySelf()
    {
        instance = null;
        Destroy(gameObject);
    }
    
}
