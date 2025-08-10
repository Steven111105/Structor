using UnityEngine;

public class GridObject : MonoBehaviour, IBeamReceiver, IGridItem
{
    [Header("Grid Object Settings")]
    public CardData cardData;
    public Vector2Int gridPosition;
    public Direction inputDirection = Direction.South;
    
    [Header("Runtime Booster Effects")]
    public float damageBoostMultiplier = 1f; // Applied on top of cardData.damageMultiplier
    public bool isBoosted = false;
    
    private GridManager gridManager;
    
    // IGridItem implementation
    public Vector2Int GridPosition 
    { 
        get => gridPosition; 
        set => gridPosition = value; 
    }
    
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
        if (cardData == null) 
        {
            Debug.Log($"[{name}] REJECTED beam {damage} from {incomingDirection} - no cardData");
            return;
        }
        
        Debug.Log($"[{name}] RECEIVED beam {damage} from {incomingDirection}, type: {cardData.cardType}");
        
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
        if (outputDirection != (Direction)(-1))
        {
            Debug.Log($"[{name}] ACCEPTED straight wire: {incomingDirection} → {outputDirection}");
            // Apply booster effects if this wire is boosted
            float finalDamage = isBoosted ? damage * damageBoostMultiplier : damage;
            PassBeamToNeighbor(finalDamage, outputDirection);
        }
        else
        {
            Debug.Log($"[{name}] REJECTED straight wire: {incomingDirection} (wrong direction for current orientation)");
        }
    }
    
    void ProcessLeftBendWire(float damage, Direction incomingDirection)
    {
        Direction outputDirection = GetLeftBendWireOutput(incomingDirection);
        if (outputDirection != (Direction)(-1))
        {
            Debug.Log($"[{name}] ACCEPTED left bend: {incomingDirection} → {outputDirection}");
            // Apply booster effects if this wire is boosted
            float finalDamage = isBoosted ? damage * damageBoostMultiplier : damage;
            PassBeamToNeighbor(finalDamage, outputDirection);
        }
        else
        {
            Debug.Log($"[{name}] REJECTED left bend: {incomingDirection} (wrong direction for current orientation)");
        }
    }
    
    void ProcessRightBendWire(float damage, Direction incomingDirection)
    {
        Direction outputDirection = GetRightBendWireOutput(incomingDirection);
        if (outputDirection != (Direction)(-1))
        {
            Debug.Log($"[{name}] ACCEPTED right bend: {incomingDirection} → {outputDirection}");
            // Apply booster effects if this wire is boosted
            float finalDamage = isBoosted ? damage * damageBoostMultiplier : damage;
            PassBeamToNeighbor(finalDamage, outputDirection);
        }
        else
        {
            Debug.Log($"[{name}] REJECTED right bend: {incomingDirection} (wrong direction for current orientation)");
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
        
        // Apply both the CardData multiplier AND the runtime booster multiplier
        float totalMultiplier = cardData.damageMultiplier * damageBoostMultiplier;
        float boostedDamage = damage * totalMultiplier;
        
        gridManager.OnBeamProcessed?.Invoke(boostedDamage);
        
        Direction outputDirection = (Direction)(((int)inputDirection + 2) % 4);
        PassBeamToNeighbor(boostedDamage, outputDirection);
    }
    
    void ProcessSensor(float damage, Direction incomingDirection)
    {
        Debug.Log($"[{name}] SENSOR HIT! Received {damage} damage from {incomingDirection}");
        
        int contribution = Mathf.RoundToInt(damage * cardData.sensorValue);
        Debug.Log($"[{name}] Sensor contribution: {damage} × {cardData.sensorValue} = {contribution}");
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
