using UnityEngine;

[CreateAssetMenu(fileName = "Test Card Setup", menuName = "Game/Test Card Setup")]
public class TestCardSetup : ScriptableObject
{
    [Header("Test Cards")]
    public CardData straightWire;
    public CardData leftBendWire;
    public CardData rightBendWire;
    public CardData booster;
    public CardData sensor2x2;
    public CardData sensor3x3;
    
    [Header("Test Sprites")]
    public Sprite straightWireSprite;
    public Sprite leftBendSprite;
    public Sprite rightBendSprite;
    public Sprite boosterSprite;
    public Sprite sensorSprite;
    
    [ContextMenu("Create Test Cards")]
    public void CreateTestCards()
    {
        // This will help you quickly create test cards
        Debug.Log("Use this to create test CardData assets");
    }
}
