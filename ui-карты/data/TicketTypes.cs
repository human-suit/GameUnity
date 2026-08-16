using System;
using System.Collections.Generic;

namespace UnstableExperiment.Combat
{
    /// <summary>Импорт из ui-карты/data/tickets.json — эталон для Unity.</summary>
    public enum EffectType
    {
        Damage,
        DamageSelf,
        Block,
        BlockNextTurn,
        ClearPoison,
        PoisonSelf,
        Bleed,
        Heal,
        DrawTicket,
        Energy,
        EnergyNextTurn,
        Whiff
    }

    [Serializable]
    public struct EffectPayload
    {
        public EffectType Type;
        public int Value;
    }

    [Serializable]
    public class TicketOutcome
    {
        public string Tier; // A, B, C
        public int Weight;
        public List<EffectPayload> Effects = new();
    }

    [Serializable]
    public class TicketDefinition
    {
        public string Id;
        public string Name;
        public string NameRu;
        public string Color;
        public int Cost;
        public List<TicketOutcome> Outcomes = new();
        public List<string> WildPools = new();
    }

    [Serializable]
    public class TicketInstance
    {
        public TicketDefinition Definition;
        public string ForgedVisualId; // Sector C: null = honest
        public bool Used;
    }

    public static class TicketRoll
    {
        /// <summary>Weighted roll. unstableExtra — PoisonSelf from Sector B etc.</summary>
        public static TicketOutcome RollOutcome(
            TicketDefinition def,
            Random rng,
            IReadOnlyList<EffectPayload> unstableExtra = null)
        {
            // pick tier by weight, apply effects + unstableExtra
            throw new NotImplementedException("Load from JSON ScriptableObject");
        }
    }
}
