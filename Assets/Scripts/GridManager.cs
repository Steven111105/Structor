using System.Collections;
using System.Collections.Generic;
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

    [Header("Sprites")]
    public Sprite cpuSprite;
    public Sprite straightWireSprite;
    public Sprite bendWireSprite;
    public Sprite tSplitterSprite;
    public Sprite boosterSprite;
    public Sprite sensorSprite;

    // Grid storage
    private IBeamReceiver[,] grid;
    private Vector2Int cpuPosition;
    public List<Vector2Int> cpuPositions = new List<Vector2Int>(); // Track all CPU positions

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
        grid = new IBeamReceiver[gridWidth, gridHeight];
        cpuPosition = new Vector2Int(gridWidth / 2, gridHeight / 2); // Center position
        cpuPositions.Clear(); // Clear all CPUs when loading a new battle
        SetupCPU(); // Always ensure CPU is set up
    }


    void SetupCPU()
    {
        CreateCPUVisual();
        cpuPositions.Add(cpuPosition); // Track all CPU positions
    }

    void CreateCPUVisual()
    {
        // Spawn CPU using prefab
        GameObject cpuObj = Instantiate(gridObjectPrefab);
        cpuObj.name = "CPU";
        cpuObj.transform.position = GridToWorldPosition(cpuPosition);
        cpuObj.transform.SetParent(transform); // Make it a child of GridManager
        cpuObj.transform.localScale = Vector3.one * cellSize;

        CardData cpuCardData = null;
        // Get the middle shooter card type from the deck
        switch (SelectedDeckData.instance.selectedDeck.middleShooter.cardType)
        {
            case CardType.CPU:
            case CardType.SuperCPU:
                cpuCardData = SelectedDeckData.instance.selectedDeck.middleShooter;
                break;
            default:
                Debug.LogWarning("Middle shooter is not a CPU type! Defaulting to CPU.");
                break;
        }

        var gridObj = cpuObj.GetComponent<GridObject>();
        gridObj.cardData = cpuCardData;
        Debug.Log($"CPU Card Type: {cpuCardData.cardType}");
        gridObj.gridPosition = cpuPosition;
        gridObj.transform.localRotation = Quaternion.identity;
        gridObj.InitializeGridObject(this);

        cpuObj.GetComponent<SpriteRenderer>().sortingOrder = 1; // Higher than other objects
        cpuObj.tag = "CPU";
    }

    public void FireBeams()
    {
        Debug.Log("=== FIRING BEAMS FROM ALL CPUs ===");
        // Fire beams in all 4 directions from every CPU
        // get the card data from the grid object to get the base cpu damage
        float baseDamage = 0f;
        foreach (var cpuPos in cpuPositions)
        {
            GridObject gridObj = GetGridObject(cpuPos); // Assume first CPU for damage reference
            if (gridObj != null && gridObj.cardData != null)
            {
                baseDamage = gridObj.cardData.baseDamage;
                baseDamage = gridObj.cardData.baseDamage;
            }
        }
        foreach (var cpuPos in cpuPositions)
        {
            FireBeamInDirection(Direction.North, cpuPos, baseDamage);
            FireBeamInDirection(Direction.South, cpuPos, baseDamage);
            FireBeamInDirection(Direction.East, cpuPos, baseDamage);
            FireBeamInDirection(Direction.West, cpuPos, baseDamage);
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

    public IBeamReceiver GetNeighbor(Vector2Int position, Direction direction)
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
            return grid[position.x, position.y] as GridObject;
        }
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

    public bool PlaceObject(Vector2Int position, IBeamReceiver obj)
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

        // Check if trying to place at CPU position (blocked for player objects)
        if (position == cpuPosition)
        {
            Debug.Log($"Cannot place object at CPU position {cpuPosition}");
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

        // Check if there's already a visual child (created by QuickTestSetup)
        Transform existingVisual = null;
        foreach (Transform child in gridObject.transform)
        {
            if (child.name.Contains("Visual"))
            {
                existingVisual = child;
                break;
            }
        }

        // If there's an existing visual child, don't add another SpriteRenderer
        if (existingVisual != null)
        {
            return; // Keep the existing visual setup
        }

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
                spriteRenderer.sprite = straightWireSprite;
                break;
            case CardType.BendWire:
                spriteRenderer.sprite = bendWireSprite;
                break;
            case CardType.TSplitter:
                spriteRenderer.sprite = tSplitterSprite;
                break;
            case CardType.Sensor:
                spriteRenderer.sprite = sensorSprite;
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

    // Object Creation Methods
    public GridObject CreateGridObject(CardData cardData, Vector2Int position)
    {
        GameObject obj = Instantiate(gridObjectPrefab);
        obj.name = $"{cardData.cardName}_{position.x}_{position.y}";

        // Parent the object to the GridManager for organization
        obj.transform.SetParent(transform);

        GridObject gridObj = obj.GetComponent<GridObject>();

        // Create minimal CardData
        CardData testCard = ScriptableObject.CreateInstance<CardData>();
        testCard.cardName = cardData.cardName;
        testCard.cardType = cardData.cardType;
        testCard.canRotate = cardData.canRotate;
        testCard.baseDamage = cardData.baseDamage;

        gridObj.cardData = testCard;
        gridObj.gridPosition = position;
        gridObj.transform.localRotation = Quaternion.identity;

        // Explicitly set input direction to South BEFORE any rotation/visual setup
        gridObj.InitializeGridObject(this);

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
        SFXManager.instance.PlaySFX("Deploy");
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
        grid = new IBeamReceiver[gridWidth, gridHeight];
        cpuPositions.Clear();
    }
    
    public void DestroySelf()
    {
        instance = null;
        Destroy(gameObject);
    }
    
}
