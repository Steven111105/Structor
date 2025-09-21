using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class BeamPathTracker
{
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<int> damages = new List<int>();
    private HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

    public BeamPathTracker()
    {
        // Start with an empty path and visited set
        path.Clear();
        visited.Clear();
        damages.Clear();
    }

    // Copy constructor for cloning
    public BeamPathTracker(BeamPathTracker other)
    {
        path = new List<Vector2Int>(other.path);
        visited = new HashSet<Vector2Int>(other.visited);
        damages = new List<int>(other.damages);
    }

    public bool HasVisited(Vector2Int pos)
    {
        return visited.Contains(pos);
    }

    public void MarkVisited(Vector2Int pos, int damage = 0)
    {
        visited.Add(pos);
        path.Add(pos);
        damages.Add(damage);
    }

    public string GetPathString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("Beam Path: ");
        for (int i = 0; i < path.Count; i++)
        {
            sb.Append(path[i]);
            sb.Append(damages[i].ToString());
            if (i < path.Count - 1)
                sb.Append(" -> ");
        }
        return sb.ToString();
    }
}

public class GridObject : MonoBehaviour
{
    [Header("Grid Object Settings")]
    public CardData cardData;
    public Vector2Int gridPosition;

    // Bidirectional wire support
    public List<Direction> connectedSides = new List<Direction>();

    [Header("Runtime Booster Effects")]
    public float damageMultiplier = 1f;
    public float damageAddition = 0f;
    public bool isBoosted = false;

    // IGridItem implementation
    public Vector2Int GridPosition
    {
        get => gridPosition;
        set => gridPosition = value;
    }

    public CardData CardData => cardData;
    public bool CanRotate => cardData?.canRotate ?? true;
    public TMP_Text boosterText;

    public void InitializeGridObject()
    {
        Debug.Log($"Initializing GridObject at {gridPosition} with CardType: {cardData.cardType}");
        boosterText = transform.GetChild(0).GetComponent<TMP_Text>();
        SetAnimation();
        SetupVisualComponents();
        SetupConnectedSides();
        UpdateVisualRotation();
    }

    void SetAnimation()
    {
        switch (cardData.cardType)
        {
            case CardType.CPU:
                // Set animator to CPU animation
                Debug.Log("Setting cpu anim");
                GetComponent<Animator>().Play("CPU");
                break;
            case CardType.SuperCPU:
                GetComponent<Animator>().Play("SuperCPU");
                break;
            default:
                // deactivate animator
                GetComponent<Animator>().enabled = false;
                break;
        }
    }

    void SetupVisualComponents()
    {
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider2D = gameObject.AddComponent<BoxCollider2D>();
            collider2D.size = Vector2.one;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = cardData.gridObjectSprite;
        Debug.Log($"Set sprite for {cardData.cardName} at {gridPosition}");
    }

    // Set up connected sides based on card type
    void SetupConnectedSides()
    {
        connectedSides.Clear();
        switch (cardData.cardType)
        {
            case CardType.StraightWire:
                connectedSides.Add(Direction.South);
                connectedSides.Add(Direction.North);
                break;
            case CardType.BendWire: // Now just "Bend"
                connectedSides.Add(Direction.South);
                connectedSides.Add(Direction.West);
                break;
            case CardType.TSplitter:
                connectedSides.Add(Direction.South);
                connectedSides.Add(Direction.West);
                connectedSides.Add(Direction.East);
                break;
            case CardType.Booster:
            case CardType.Sensor:
                // No wire sides needed
                break;
        }
    }

    public void UpdateVisualRotation()
    {
        // For simplicity, rotate by 90 degrees per rotation
        // You may want to update this for your visuals
        float rotationAngle = 0f;
        if (connectedSides.Count > 0)
        {
            // Use the first side as reference for rotation
            rotationAngle = 90f * (int)connectedSides[0];
        }
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }

    public virtual void BeamComingFromNorth(float damage, BeamPathTracker tracker)
    {
        if (tracker.HasVisited(gridPosition))
        {
            Debug.Log($"[TRACKER REJECTED] Already visited {gridPosition}");
            return;
        }
        // tracker.MarkVisited(gridPosition);
        ProcessBeam(damage, Direction.North, tracker);
    }

    public virtual void BeamComingFromSouth(float damage, BeamPathTracker tracker)
    {
        if (tracker.HasVisited(gridPosition))
        {
            Debug.Log($"[TRACKER REJECTED] Already visited {gridPosition}");
            return;
        }
        // tracker.MarkVisited(gridPosition);
        ProcessBeam(damage, Direction.South, tracker);
    }

    public virtual void BeamComingFromEast(float damage, BeamPathTracker tracker)
    {
        if (tracker.HasVisited(gridPosition))
        {
            Debug.Log($"[TRACKER REJECTED] Already visited {gridPosition}");
            return;
        }
        // tracker.MarkVisited(gridPosition);
        ProcessBeam(damage, Direction.East, tracker);
    }

    public virtual void BeamComingFromWest(float damage, BeamPathTracker tracker)
    {
        if (tracker.HasVisited(gridPosition))
        {
            Debug.Log($"[TRACKER REJECTED] Already visited {gridPosition}");
            return;
        }
        ProcessBeam(damage, Direction.West, tracker);
    }

    protected virtual void ProcessBeam(float damage, Direction incomingDirection, BeamPathTracker tracker)
    {
        if (cardData == null)
        {
            Debug.Log($"[{name}] REJECTED beam {damage} from {incomingDirection} - no cardData");
            return;
        }
        damage += cardData.baseDamage;
        tracker.MarkVisited(gridPosition, Mathf.RoundToInt(damage));

        // Debug.Log($"[{name}] RECEIVED beam {damage} from {incomingDirection}, type: {cardData.cardType.ToString()}");

        switch (cardData.cardType)
        {
            case CardType.StraightWire:
            case CardType.BendWire:
            case CardType.TSplitter:
                ProcessNextWire(damage, incomingDirection, tracker);
                break;
            case CardType.Sensor:
                ProcessSensor(damage, tracker, incomingDirection);
                break;
        }
    }

    void ProcessNextWire(float damage, Direction incomingDirection, BeamPathTracker tracker)
    {
        if (!connectedSides.Contains(incomingDirection))
        {
            // Debug.Log($"[{name}] REJECTED wire: {incomingDirection} not a connected side");
            return;
        }
        // Apply booster effects if this wire is boosted
        float finalDamage = isBoosted ? (damage + damageAddition) * damageMultiplier : damage;

        foreach (var side in connectedSides)
        {
            if (side == incomingDirection) continue;
            BeamPathTracker branchTracker = new BeamPathTracker(tracker); // Clone the tracker for each branch
            // Debug.Log($"[{name}] ACCEPTED, Sending {incomingDirection} → {side} with {finalDamage} damage");
            PassBeamToNeighbor(finalDamage, side, branchTracker);
        }
    }

    void ProcessSensor(float damage, BeamPathTracker tracker, Direction incomingDirection)
    {
        Debug.Log($"[{name}] SENSOR HIT! Received {damage} damage from {incomingDirection}");
        Debug.Log(tracker.GetPathString());
        int contribution = Mathf.RoundToInt(damage);
        if (GameManager.instance.isSignalInterference)
        {
            contribution = Mathf.RoundToInt(contribution * 0.8f); // Example: reduce contribution by 20%
        }
        GameManager.instance.OnSensorHit(contribution);
        GridManager.instance.successfulBeamPaths.Add(tracker);
    }

    void PassBeamToNeighbor(float damage, Direction direction, BeamPathTracker tracker)
    {
        var neighbor = GridManager.instance.GetNeighbor(gridPosition, direction);
        if (neighbor != null)
        {
            switch (direction)
            {
                case Direction.North:
                    neighbor.BeamComingFromSouth(damage, tracker);
                    break;
                case Direction.South:
                    neighbor.BeamComingFromNorth(damage, tracker);
                    break;
                case Direction.East:
                    neighbor.BeamComingFromWest(damage, tracker);
                    break;
                case Direction.West:
                    neighbor.BeamComingFromEast(damage, tracker);
                    break;
            }
        }
    }

    public virtual void Rotate()
    {
        if (!CanRotate) return;
        // Rotate all connected sides counter-clockwise
        for (int i = 0; i < connectedSides.Count; i++)
        {
            connectedSides[i] = (Direction)(((int)connectedSides[i] + 3) % 4); // -1 mod 4
        }

        StartCoroutine(LerpRotation());
    }

    private IEnumerator LerpRotation()
    {
        float duration = 0.12f;
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        float targetAngle = 0f;
        if (connectedSides.Count > 0)
        {
            targetAngle = 90f * (int)connectedSides[0];
        }
        Quaternion endRot = Quaternion.Euler(0, 0, targetAngle);
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / duration);
            boosterText.transform.rotation = Quaternion.identity; // Keep text upright
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;
    }

    public void DisplayBoostedText(bool isMult, float amount)
    {
        string text = isMult ? $"x{amount}" : $"+{amount}";
        boosterText.text = text;
    }

    void OnMouseDown()
    {
        // Debug.Log($"Rotating {GridPosition.x} {GridPosition.y}");
        if (CanRotate)
        {
            Rotate();
        }
    }
}
