using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Direction
{
    North = 0, East = 1, South = 2, West = 3
}

public enum CardType
{
    StraightWire, LeftBendWire, RightBendWire, TSplitter, Booster, Sensor
}

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 15;
    public int gridHeight = 11;
    public float cellSize = 1f;
    
    [Header("CPU Settings")]
    public float baseDamage = 1f;
    
    [Header("Sprites")]
    public Sprite straightWireSprite;
    public Sprite leftBendWireSprite;
    public Sprite rightBendWireSprite;
    public Sprite tSplitterSprite;
    public Sprite boosterSprite;
    public Sprite sensorSprite;
    
    // Grid storage
    private IBeamReceiver[,] grid;
    private Vector2Int cpuPosition;
    
    // Events
    [Header("Events")]
    public UnityEvent<float, Vector2Int, Direction> OnBeamFired = new UnityEvent<float, Vector2Int, Direction>();
    public UnityEvent<float> OnBeamProcessed = new UnityEvent<float>(); // For boosters to listen to
    public UnityEvent<int, Vector2Int> OnSensorHit = new UnityEvent<int, Vector2Int>();
    
    void Start()
    {
        InitializeGrid();
        SetupCPU();
    }
    
    void InitializeGrid()
    {
        grid = new IBeamReceiver[gridWidth, gridHeight];
        cpuPosition = new Vector2Int(gridWidth / 2, gridHeight / 2); // Center position
    }
    
    void SetupCPU()
    {
        // CPU is at center, will fire beams in 4 directions
        Debug.Log($"CPU positioned at: {cpuPosition}");
        
        // Create visual CPU GameObject
        CreateCPUVisual();
    }
    
    void CreateCPUVisual()
    {
        // Remove existing CPU visual if it exists
        GameObject existingCPU = GameObject.Find("CPU");
        if (existingCPU != null)
        {
            DestroyImmediate(existingCPU);
        }
        
        // Create CPU GameObject
        GameObject cpuObj = new GameObject("CPU");
        cpuObj.transform.position = GridToWorldPosition(cpuPosition);
        cpuObj.transform.SetParent(transform); // Make it a child of GridManager
        
        // Scale CPU to match cell size like everything else
        cpuObj.transform.localScale = Vector3.one * cellSize;
        
        // Add 2D visual representation
        SpriteRenderer spriteRenderer = cpuObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCPUSprite();
        spriteRenderer.color = Color.yellow;
        spriteRenderer.sortingOrder = 3; // Higher than other objects
        
        // Add a tag for identification
        cpuObj.tag = "CPU";
        
        Debug.Log($"CPU visual created at world position: {cpuObj.transform.position}");
    }
    
    // Helper method to create a CPU sprite (diamond/cross shape)
    Sprite CreateCPUSprite()
    {
        // Create a CPU-like texture with cross pattern
        Texture2D texture = new Texture2D(32, 32, TextureFormat.ARGB32, false);
        Color[] pixels = new Color[32 * 32];
        
        // Create a cross/plus pattern for CPU
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                // Horizontal bar (center)
                if (y >= 12 && y <= 19)
                {
                    pixels[y * 32 + x] = Color.white;
                }
                // Vertical bar (center)
                else if (x >= 12 && x <= 19)
                {
                    pixels[y * 32 + x] = Color.white;
                }
                // Corner accents
                else if ((x <= 3 || x >= 28) && (y <= 3 || y >= 28))
                {
                    pixels[y * 32 + x] = Color.white;
                }
                else
                {
                    pixels[y * 32 + x] = Color.clear;
                }
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }
    
    public void FireBeams()
    {
        // Fire beams in all 4 directions from CPU
        FireBeamInDirection(Direction.North);
        FireBeamInDirection(Direction.South);
        FireBeamInDirection(Direction.East);
        FireBeamInDirection(Direction.West);
    }
    
    void FireBeamInDirection(Direction direction)
    {
        Vector2Int targetPos = GetPositionInDirection(cpuPosition, direction);
        Debug.Log($"[CPU] Firing beam {direction} from {cpuPosition} to {targetPos}");
        
        if (IsValidPosition(targetPos))
        {
            var target = grid[targetPos.x, targetPos.y];
            if (target != null)
            {
                Debug.Log($"[CPU] Found target at {targetPos}: {((MonoBehaviour)target).name}");
                // Call the appropriate beam method based on direction
                switch (direction)
                {
                    case Direction.North:
                        target.BeamComingFromSouth(baseDamage);
                        break;
                    case Direction.South:
                        target.BeamComingFromNorth(baseDamage);
                        break;
                    case Direction.East:
                        target.BeamComingFromWest(baseDamage);
                        break;
                    case Direction.West:
                        target.BeamComingFromEast(baseDamage);
                        break;
                }
            }
            else
            {
                // Debug.Log($"[CPU] No target found at {targetPos}");
            }
        }
        else
        {
            Debug.Log($"[CPU] Invalid position {targetPos}");
        }
        
        // Fire event for any systems that want to know about beam firing
        OnBeamFired?.Invoke(baseDamage, targetPos, direction);
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
    
    bool IsValidPosition(Vector2Int position)
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
                spriteRenderer.color = Color.blue;
                break;
            case CardType.LeftBendWire:
                spriteRenderer.sprite = leftBendWireSprite;
                spriteRenderer.color = Color.green;
                break;
            case CardType.RightBendWire:
                spriteRenderer.sprite = rightBendWireSprite;
                spriteRenderer.color = Color.magenta;
                break;
            case CardType.TSplitter:
                spriteRenderer.sprite = tSplitterSprite;
                spriteRenderer.color = Color.yellow;
                break;
            case CardType.Booster:
                spriteRenderer.sprite = boosterSprite;
                spriteRenderer.color = Color.cyan;
                break;
            case CardType.Sensor:
                spriteRenderer.sprite = sensorSprite;
                spriteRenderer.color = Color.red;
                break;
            default:
                spriteRenderer.color = Color.white;
                break;
        }
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
    public GridObject CreateGridObject(string baseName, CardType cardType, Vector2Int position)
    {
        GameObject obj = new GameObject($"{baseName}_{position.x}_{position.y}");
        
        // Parent the object to the GridManager for organization
        obj.transform.SetParent(transform);
        
        GridObject gridObj = obj.AddComponent<GridObject>();
        
        // Create minimal CardData
        CardData testCard = ScriptableObject.CreateInstance<CardData>();
        testCard.cardName = baseName;
        testCard.cardType = cardType;
        testCard.canRotate = cardType != CardType.Sensor; // Sensors typically don't rotate
        
        // Set sensor-specific properties
        if (cardType == CardType.Sensor)
        {
            testCard.sensorValue = 25;
        }
        
        gridObj.cardData = testCard;
        gridObj.gridPosition = position;
        
        // Explicitly set input direction to South BEFORE any rotation/visual setup
        gridObj.inputDirection = Direction.South;
        
        // Position in world - this already accounts for GridManager's position
        obj.transform.position = GridToWorldPosition(position);
        
        // Scale the object to match the cell size
        obj.transform.localScale = Vector3.one * cellSize;
        
        // Add to grid - this will automatically handle sprite assignment
        bool placed = PlaceObject(position, gridObj);
        if (!placed)
        {
            // Debug.LogError($"Failed to place {baseName} at {position}!");
            DestroyImmediate(obj);
            return null;
        }
        
        Debug.Log($"Created {baseName} at grid {position}, world {obj.transform.position}");
        return gridObj;
    }
    
    // Testing and Debug Methods
    [ContextMenu("Create Test Wires")]
    public void CreateTestWires()
    {
        // Create test setup - CPU is at (7,5)
        // Path: CPU -West-> Straight -West-> RightBend -North-> Straight -North-> LeftBend -West-> Sensor
        CreateGridObject("TestStraightWire", CardType.StraightWire, new Vector2Int(6, 5)); // West of CPU: East input → West output
        CreateGridObject("TestRightBendWire", CardType.RightBendWire, new Vector2Int(5, 5)); // Right turn: East input → North output  
        CreateGridObject("TestStraightWire", CardType.StraightWire, new Vector2Int(5, 6)); // Vertical: South input → North output
        CreateGridObject("TestLeftBendWire", CardType.LeftBendWire, new Vector2Int(5, 7)); // Left turn: South input → West output
        CreateGridObject("TestSensor", CardType.Sensor, new Vector2Int(4, 7)); // Target for beam
        
        Debug.Log("Test setup created! Path: CPU(7,5) -West-> Straight(6,5) -West-> RightBend(5,5) -North-> Straight(5,6) -North-> LeftBend(5,7) -West-> Sensor(4,7)");
        Debug.Log("Press SPACE to fire beams");
    }
    
    [ContextMenu("Create Test T-Splitter")]
    public void CreateTestTSplitterOnly()
    {
        // Create T-splitter test setup - CPU is at (7,5)
        // Path: CPU -West-> T-Splitter -North-> Sensor1
        //                             -South-> Sensor2
        
        // Place T-splitter directly west of CPU
        CreateGridObject("TestTSplitter", CardType.TSplitter, new Vector2Int(6, 5)); // T-splitter with default direction
        
        // Place sensors at both T-splitter outputs
        CreateGridObject("TestSensor", CardType.Sensor, new Vector2Int(6, 6)); // North output sensor
        CreateGridObject("TestSensor", CardType.Sensor, new Vector2Int(6, 4)); // South output sensor
        
        Debug.Log("Created T-splitter test setup:");
        Debug.Log("CPU(7,5) -West-> T-Splitter(6,5) splits to:");
        Debug.Log("  - North output -> Sensor(6,6)");
        Debug.Log("  - South output -> Sensor(6,4)");
        Debug.Log("T-splitter has default direction. Press SPACE to fire beams!");
    }
    
    [ContextMenu("Test Fire Beams")]
    public void TestFireBeams()
    {
        Debug.Log("=== FIRING TEST BEAMS ===");
        FireBeams();
    }
    
    [ContextMenu("Clear All Test Objects")]
    public void ClearAllTestObjects()
    {
        // Find and destroy all test objects
        GameObject[] testObjects = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in testObjects)
        {
            if (obj.name.StartsWith("Test"))
            {
                DestroyImmediate(obj);
            }
        }
        Debug.Log("Cleared all test objects");
    }
    
    [ContextMenu("Debug Wire States")]
    public void DebugWireStates()
    {
        // Find all test wires and show their current input directions
        GridObject[] allGridObjects = FindObjectsOfType<GridObject>();
        Debug.Log("=== WIRE STATES ===");
        foreach (GridObject gridObj in allGridObjects)
        {
            if (gridObj.name.StartsWith("Test"))
            {
                Vector3 arrowPos = Vector3.zero;
                Transform arrow = gridObj.transform.Find("OutputArrow");
                if (arrow != null)
                {
                    arrowPos = arrow.localPosition;
                }
                Debug.Log($"{gridObj.name} at {gridObj.gridPosition}: inputDirection={gridObj.inputDirection}, arrow at {arrowPos}");
            }
        }
    }
    
    [ContextMenu("Debug Grid State")]
    public void DebugGridState()
    {
        Debug.Log($"=== GRID DEBUG ===");
        Debug.Log($"Grid size: {gridWidth}x{gridHeight}");
        Debug.Log($"Cell size: {cellSize}");
        Debug.Log($"GridManager position: {transform.position}");
        
        // Check CPU position in world space
        Vector2Int cpuGridPos = new Vector2Int(7, 5);
        Vector3 cpuWorldPos = GridToWorldPosition(cpuGridPos);
        Debug.Log($"CPU grid pos: {cpuGridPos}, world pos: {cpuWorldPos}");
        
        // Check test positions
        Vector2Int wirePos = new Vector2Int(6, 5);
        Vector3 wireWorldPos = GridToWorldPosition(wirePos);
        Debug.Log($"Wire grid pos: {wirePos}, world pos: {wireWorldPos}");
        
        Vector2Int sensorPos = new Vector2Int(4, 5);
        Vector3 sensorWorldPos = GridToWorldPosition(sensorPos);
        Debug.Log($"Sensor grid pos: {sensorPos}, world pos: {sensorWorldPos}");
    }
}
