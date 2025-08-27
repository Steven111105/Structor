using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;
    public CardType cardType;

    [Header("Visuals")]
    public Sprite cardSprite;

    [Header("Gameplay")]
    public bool canRotate = true; // Some items might be fixed orientation

    [Header("Damage Settings")]
    public int baseDamage = 20; // Damage from passing wire
    public int sensorValue = 10; // For sensors only

    [Header("Booster Settings")]
    public float damageMultiplier = 0f; // For boosters only: 1.5f = +50%, 2.0f = x2 damage
    public float damageAddition = 0f; // For boosters only: 1.5f = +50%, 2.0f = x2 damage
    public bool isMult = false;
}
