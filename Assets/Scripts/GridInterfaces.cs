using UnityEngine;

public interface IBeamReceiver
{
    void BeamComingFromNorth(float damage, BeamPathTracker tracker);
    void BeamComingFromSouth(float damage, BeamPathTracker tracker);
    void BeamComingFromEast(float damage, BeamPathTracker tracker);
    void BeamComingFromWest(float damage, BeamPathTracker tracker);
    void Rotate();
}

public interface IGridItem
{
    Vector2Int GridPosition { get; set; }
    CardData CardData { get; }
    bool CanRotate { get; }
}
