using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;
    public CardType cardType;
    public Vector2Int size = Vector2Int.one;
    
    [Header("Visuals")]
    public Sprite cardSprite;
    public GameObject prefab;
    
    [Header("Gameplay")]
    public float damageMultiplier = 1f; // For boosters
    public int sensorValue = 10; // For sensors
    public bool canRotate = true; // Some items might be fixed orientation
}
