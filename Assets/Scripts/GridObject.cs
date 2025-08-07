using UnityEngine;

public class GridObject : MonoBehaviour, IBeamReceiver, IGridItem
{
    [Header("Grid Object Settings")]
    public CardData cardData;
    public Vector2Int gridPosition;
    public Direction inputDirection = Direction.South; // Which direction this wire accepts input from
    
    private GridManager gridManager;
    
    // IGridItem implementation
    public Vector2Int GridPosition 
    { 
        get => gridPosition; 
        set => gridPosition = value; 
    }
    
    public Vector2Int Size => cardData?.size ?? Vector2Int.one;
    public CardData CardData => cardData;
    public bool CanRotate => cardData?.canRotate ?? true;
    
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        SetupVisualComponents();
        UpdateVisualRotation();
    }
    
    void SetupVisualComponents()
    {
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider2D = gameObject.AddComponent<BoxCollider2D>();
            collider2D.size = Vector2.one;
        }
    }
    
    public void UpdateVisualRotation()
    {
        float rotationAngle = 0f;
        
        switch (inputDirection)
        {
            case Direction.North:
                rotationAngle = 180f;
                break;
            case Direction.East:
                rotationAngle = 90f;
                break;
            case Direction.South:
                rotationAngle = 0f;
                break;
            case Direction.West:
                rotationAngle = 270f;
                break;
        }
        
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }
    
    public virtual void BeamComingFromNorth(float damage)
    {
        ProcessBeam(damage, Direction.North);
    }
    
    public virtual void BeamComingFromSouth(float damage)
    {
        ProcessBeam(damage, Direction.South);
    }
    
    public virtual void BeamComingFromEast(float damage)
    {
        ProcessBeam(damage, Direction.East);
    }
    
    public virtual void BeamComingFromWest(float damage)
    {
        ProcessBeam(damage, Direction.West);
    }
    
    protected virtual void ProcessBeam(float damage, Direction incomingDirection)
    {
        if (cardData == null) return;
        
        switch (cardData.cardType)
        {
            case CardType.StraightWire:
                ProcessStraightWire(damage, incomingDirection);
                break;
            case CardType.LeftBendWire:
                ProcessLeftBendWire(damage, incomingDirection);
                break;
            case CardType.RightBendWire:
                ProcessRightBendWire(damage, incomingDirection);
                break;
            case CardType.TSplitter:
                ProcessTSplitter(damage, incomingDirection);
                break;
            case CardType.Booster:
                ProcessBooster(damage, incomingDirection);
                break;
            case CardType.Sensor:
                ProcessSensor(damage, incomingDirection);
                break;
        }
    }
    
    void ProcessStraightWire(float damage, Direction incomingDirection)
    {
        Direction outputDirection = GetStraightWireOutput(incomingDirection);
        if (outputDirection != (Direction)(-1)) // Check for invalid direction
        {
            PassBeamToNeighbor(damage, outputDirection);
        }
    }
    
    void ProcessLeftBendWire(float damage, Direction incomingDirection)
    {
        Direction outputDirection = GetLeftBendWireOutput(incomingDirection);
        if (outputDirection != (Direction)(-1)) // Check for invalid direction
        {
            PassBeamToNeighbor(damage, outputDirection);
        }
    }
    
    void ProcessRightBendWire(float damage, Direction incomingDirection)
    {
        Direction outputDirection = GetRightBendWireOutput(incomingDirection);
        if (outputDirection != (Direction)(-1)) // Check for invalid direction
        {
            PassBeamToNeighbor(damage, outputDirection);
        }
    }
    
    void ProcessTSplitter(float damage, Direction incomingDirection)
    {
        if (incomingDirection != inputDirection) return;
        
        Direction output1 = (Direction)(((int)inputDirection + 1) % 4);
        Direction output2 = (Direction)(((int)inputDirection - 1 + 4) % 4);
        
        float splitDamage = damage * 0.5f;
        PassBeamToNeighbor(splitDamage, output1);
        PassBeamToNeighbor(splitDamage, output2);
    }
    
    void ProcessBooster(float damage, Direction incomingDirection)
    {
        if (incomingDirection != inputDirection) return;
        
        float boostedDamage = damage * cardData.damageMultiplier;
        gridManager.OnBeamProcessed?.Invoke(boostedDamage);
        
        Direction outputDirection = (Direction)(((int)inputDirection + 2) % 4);
        PassBeamToNeighbor(boostedDamage, outputDirection);
    }
    
    void ProcessSensor(float damage, Direction incomingDirection)
    {
        int contribution = Mathf.RoundToInt(damage * cardData.sensorValue);
        gridManager.OnSensorHit?.Invoke(contribution, gridPosition);
    }
    
    Direction GetStraightWireOutput(Direction incomingDirection)
    {
        if (incomingDirection != inputDirection) return (Direction)(-1);
        return (Direction)(((int)inputDirection + 2) % 4);
    }
    
    Direction GetLeftBendWireOutput(Direction incomingDirection)
    {
        if (incomingDirection != inputDirection) return (Direction)(-1);
        return (Direction)(((int)inputDirection + 1) % 4);
    }
    
    Direction GetRightBendWireOutput(Direction incomingDirection)
    {
        if (incomingDirection != inputDirection) return (Direction)(-1);
        return (Direction)(((int)inputDirection - 1 + 4) % 4);
    }
    
    void PassBeamToNeighbor(float damage, Direction direction)
    {
        var neighbor = gridManager.GetNeighbor(gridPosition, direction);
        if (neighbor != null)
        {
            switch (direction)
            {
                case Direction.North:
                    neighbor.BeamComingFromSouth(damage);
                    break;
                case Direction.South:
                    neighbor.BeamComingFromNorth(damage);
                    break;
                case Direction.East:
                    neighbor.BeamComingFromWest(damage);
                    break;
                case Direction.West:
                    neighbor.BeamComingFromEast(damage);
                    break;
            }
        }
    }
    
    public virtual void Rotate()
    {
        if (!CanRotate) return;
        
        inputDirection = (Direction)(((int)inputDirection + 1) % 4);
        UpdateVisualRotation();
    }
    
    void OnMouseDown()
    {
        if (CanRotate)
        {
            Rotate();
        }
    }
}
