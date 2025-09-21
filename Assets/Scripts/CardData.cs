using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string cardName;
    public CardType cardType;

    [Header("Visuals")]
    public Sprite cardSprite;
    public Sprite gridObjectSprite; // Sprite used when placed on the grid

    [Header("Gameplay")]
    public bool canRotate = true; // Some items might be fixed orientation

    [Header("Damage Settings")]
    public int baseDamage = 20; // Damage from passing wire

    [Header("Booster Settings")]
    public float damageMultiplier = 0f; // For boosters only: 1.5f = +50%, 2.0f = x2 damage
    public float damageAddition = 0f; // For boosters only: 1.5f = +50%, 2.0f = x2 damage
    public bool isMult = false;

    public CardData Clone()
    {
        CardData clone = ScriptableObject.CreateInstance<CardData>();
        clone.cardName = cardName;
        clone.cardType = cardType;
        clone.cardSprite = cardSprite;
        clone.gridObjectSprite = gridObjectSprite;
        clone.canRotate = canRotate;
        clone.baseDamage = baseDamage;
        clone.damageMultiplier = damageMultiplier;
        clone.damageAddition = damageAddition;
        clone.isMult = isMult;
        return clone;
    }
}
