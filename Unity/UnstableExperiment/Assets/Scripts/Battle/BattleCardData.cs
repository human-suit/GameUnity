using UnityEngine;

public enum BattleCardType
{
    Attack,
    Defense
}

[CreateAssetMenu(
    fileName = "BattleCard",
    menuName = "Unstable Experiment/Battle Card")]
public class BattleCardData : ScriptableObject
{
    public string cardId = "card";
    public string displayName = "Карта";

    [TextArea(2, 5)]
    public string description;

    public BattleCardType type;
    [Min(0)] public int energyCost = 1;
    [Min(0)] public int damage;
    [Min(0)] public int block;
    public Sprite artwork;
}
