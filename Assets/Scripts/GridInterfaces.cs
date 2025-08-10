using UnityEngine;

public interface IBeamReceiver
{
    void BeamComingFromNorth(float damage);
    void BeamComingFromSouth(float damage);
    void BeamComingFromEast(float damage);
    void BeamComingFromWest(float damage);
    void Rotate();
}

public interface IGridItem
{
    Vector2Int GridPosition { get; set; }
    CardData CardData { get; }
    bool CanRotate { get; }
}
