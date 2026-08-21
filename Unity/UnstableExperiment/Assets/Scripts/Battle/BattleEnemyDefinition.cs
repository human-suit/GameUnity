using UnityEngine;

[CreateAssetMenu(
    fileName = "BattleEnemy",
    menuName = "Unstable Experiment/Battle Enemy")]
public class BattleEnemyDefinition : ScriptableObject
{
    public string enemyId = "enemy";
    public string displayName = "Враг";

    [Header("Body sprites")]
    public Sprite head;
    public Sprite torso;
    public Sprite leftArm;
    public Sprite rightArm;
    public Sprite leftLeg;
    public Sprite rightLeg;

    [Header("Battle scene")]
    public Sprite defaultBackground;
}
