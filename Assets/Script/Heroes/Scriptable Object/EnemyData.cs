using UnityEngine;

[CreateAssetMenu(menuName = "TD Merge/Enemy Data", fileName = "EnemyData_")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string id;               // e.g. "Grunt", "Brute"
    public Enemy prefab;

    [Header("Visuals")]
    [PreviewSprite(72f)] public Sprite icon;    // portrait shown in inspector + UI

    [Header("Stats")]
    [StatBar(100)]  public int hp           = 3;
    [StatBar(20)]  public int attackDamage = 1;
    public float speed      = 1f;
    public float attackCooldown = 1f;

    [Header("Reward")]
    public int goldReward   = 5;
}
