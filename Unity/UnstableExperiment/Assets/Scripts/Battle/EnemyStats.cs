using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private int maxHealth = 90;
    public int attackDamage = 10;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    private readonly List<EnemyBodyPart> _bodyParts =
        new List<EnemyBodyPart>();

    public IReadOnlyList<EnemyBodyPart> BodyParts => _bodyParts;
    public bool IsDefeated =>
        CurrentHealth <= 0 ||
        IsPartDestroyed(EnemyBodyPartType.Head) ||
        IsPartDestroyed(EnemyBodyPartType.Torso);

    private void Awake()
    {
        CurrentHealth = maxHealth;
        _bodyParts.Clear();
        _bodyParts.Add(new EnemyBodyPart(
            EnemyBodyPartType.Head, "Голова", 10, 45));
        _bodyParts.Add(new EnemyBodyPart(
            EnemyBodyPartType.Torso, "Туловище", 30, 95));
        _bodyParts.Add(new EnemyBodyPart(
            EnemyBodyPartType.LeftArm, "Левая рука", 15, 80));
        _bodyParts.Add(new EnemyBodyPart(
            EnemyBodyPartType.RightArm, "Правая рука", 15, 80));
        _bodyParts.Add(new EnemyBodyPart(
            EnemyBodyPartType.LeftLeg, "Левая нога", 10, 75));
        _bodyParts.Add(new EnemyBodyPart(
            EnemyBodyPartType.RightLeg, "Правая нога", 10, 75));
    }

    public EnemyBodyPart GetBodyPart(EnemyBodyPartType type)
    {
        foreach (EnemyBodyPart part in _bodyParts)
        {
            if (part.Type == type)
                return part;
        }

        return null;
    }

    public int GetHitChance(EnemyBodyPartType type)
    {
        EnemyBodyPart part = GetBodyPart(type);
        if (part == null)
            return 0;

        int chance = part.BaseHitChance;
        EnemyBodyPart leftLeg = GetBodyPart(EnemyBodyPartType.LeftLeg);
        EnemyBodyPart rightLeg = GetBodyPart(EnemyBodyPartType.RightLeg);

        if (type != EnemyBodyPartType.LeftLeg &&
            leftLeg != null &&
            leftLeg.IsDestroyed)
            chance += 10;

        if (type != EnemyBodyPartType.RightLeg &&
            rightLeg != null &&
            rightLeg.IsDestroyed)
            chance += 10;

        return Mathf.Clamp(chance, 0, 100);
    }

    private bool IsPartDestroyed(EnemyBodyPartType type)
    {
        EnemyBodyPart part = GetBodyPart(type);
        return part != null && part.IsDestroyed;
    }

    public bool TryAttackBodyPart(
        EnemyBodyPartType type,
        int damage,
        out string message)
    {
        EnemyBodyPart part = GetBodyPart(type);
        if (part == null)
        {
            message = "Эта часть тела не найдена.";
            return false;
        }

        if (part.IsDestroyed)
        {
            message = $"{part.DisplayName} уже уничтожена.";
            return false;
        }

        int hitChance = GetHitChance(type);
        int roll = Random.Range(1, 101);

        if (roll > hitChance)
        {
            message =
                $"Промах по цели «{part.DisplayName}» " +
                $"({roll} > {hitChance}).";
            Debug.Log(message);
            return true;
        }

        int appliedDamage = part.ApplyDamage(damage);
        CurrentHealth = Mathf.Max(0, CurrentHealth - appliedDamage);
        message =
            $"{part.DisplayName}: нанесено {appliedDamage} урона. " +
            $"HP {part.CurrentHealth}/{part.MaxHealth}.";

        Debug.Log(message);

        if (!part.IsDestroyed)
            return true;

        message += $" {part.DisplayName} уничтожена.";

        if (type == EnemyBodyPartType.Head ||
            type == EnemyBodyPartType.Torso)
        {
            CurrentHealth = 0;
        }

        return true;
    }

    // Совместимость со старым вызовом: обычный урон идёт в туловище.
    public void TakeDamage(int damage)
    {
        TryAttackBodyPart(
            EnemyBodyPartType.Torso,
            damage,
            out _);
    }

    public int GetCurrentAttackDamage()
    {
        int intactArms = 0;

        EnemyBodyPart leftArm = GetBodyPart(EnemyBodyPartType.LeftArm);
        EnemyBodyPart rightArm = GetBodyPart(EnemyBodyPartType.RightArm);

        if (leftArm != null && !leftArm.IsDestroyed)
            intactArms++;
        if (rightArm != null && !rightArm.IsDestroyed)
            intactArms++;

        if (intactArms == 0)
            return 0;
        if (intactArms == 1)
            return Mathf.CeilToInt(attackDamage * 0.5f);

        return attackDamage;
    }

    public void Attack()
    {
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();

        if (battleManager == null)
        {
            Debug.LogError("EnemyStats: BattleManager not found.", this);
            return;
        }

        int currentDamage = GetCurrentAttackDamage();
        if (currentDamage <= 0)
        {
            battleManager.SetBattleMessage(
                "Обе руки врага уничтожены — он не может атаковать.");
            Debug.Log("Enemy cannot attack: both arms are destroyed.");
            return;
        }

        battleManager.SetBattleMessage(
            $"Враг атакует и наносит {currentDamage} урона.");
        Debug.Log("Enemy attacks for " + currentDamage + " damage!");
        battleManager.TakePlayerDamage(currentDamage);
    }
}
