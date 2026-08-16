using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnstableExperiment.Core;
using UnstableExperiment.Data;
using UnstableExperiment.World;

namespace UnstableExperiment.Combat
{
    public class CombatManager : MonoBehaviour
    {
        public bool IsActive { get; private set; }
        public string LastLog { get; private set; } = "";

        private RoomManager _rooms;
        private EnemyController _enemy;
        private readonly System.Random _rng = new();

        private List<string> _roll = new();
        private List<string> _discard = new();
        private List<HandTicket> _hand = new();

        private int _playerHp;
        private int _playerBlock;
        private int _playerPoison;
        private int _nextTurnBlock;
        private int _nextTurnEnergyMod;

        private int _enemyHp;
        private int _enemyBlock;
        private int _enemyBleed;

        private int _energy;
        private int _turn;
        private string _sectorId;
        private int? _forgedHandIndex;
        private bool _combatEnded;

        private struct HandTicket
        {
            public string RealId;
            public string VisualId;
            public bool Forged;
        }

        public void Initialize(RoomManager rooms) => _rooms = rooms;

        public void StartCombat(EnemyController enemy)
        {
            if (IsActive) return;
            IsActive = true;
            _combatEnded = false;
            _enemy = enemy;
            _sectorId = GameState.Instance.CurrentSectorId;

            var cfg = GameDatabase.Tickets.combat;
            _playerHp = GameState.Instance.PlayerHp;
            _playerBlock = 0;
            _playerPoison = 0;
            _nextTurnBlock = 0;
            _nextTurnEnergyMod = 0;
            _enemyHp = enemy.Hp;
            _enemyBlock = 0;
            _enemyBleed = 0;
            _energy = cfg.energyPerTurn;
            _turn = 1;

            _roll = new List<string>(GameDatabase.Tickets.starterRoll);
            _discard.Clear();
            _hand.Clear();
            _forgedHandIndex = null;

            ApplyCombatStartRules();
            DrawHand(cfg.handSize);
            ApplyForgedTicketRule();
            LastLog = $"Бой: {enemy.DisplayName} (HP {_enemyHp})";
        }

        private void ApplyCombatStartRules()
        {
            var unstable = GameDatabase.GetUnstable(_sectorId);
            if (unstable?.worldRule?.combatStartEffects == null) return;
            foreach (var fx in unstable.worldRule.combatStartEffects)
                ApplyEffect(fx, targetEnemy: false);
        }

        private void ApplyForgedTicketRule()
        {
            var unstable = GameDatabase.GetUnstable(_sectorId);
            var rule = unstable?.ticketRules?.FirstOrDefault(r => r.type == "ForgedVisual");
            if (rule == null || _hand.Count == 0) return;

            int idx = _rng.Next(_hand.Count);
            var ht = _hand[idx];
            ht.Forged = true;
            ht.VisualId = rule.visualPool[_rng.Next(rule.visualPool.Length)];
            _hand[idx] = ht;
            _forgedHandIndex = idx;
        }

        private void DrawHand(int count)
        {
            while (_hand.Count < count)
            {
                if (_roll.Count == 0)
                {
                    if (_discard.Count == 0)
                    {
                        var fallback = GameDatabase.Tickets.exhaustedRollFallback;
                        _roll.Add(fallback);
                    }
                    else
                    {
                        _roll.AddRange(_discard);
                        _discard.Clear();
                    }
                }

                int pick = _rng.Next(_roll.Count);
                string id = _roll[pick];
                _roll.RemoveAt(pick);
                _hand.Add(new HandTicket { RealId = id, VisualId = id, Forged = false });
            }
        }

        public void RedeemTicket(int handIndex)
        {
            if (!IsActive || _combatEnded || handIndex < 0 || handIndex >= _hand.Count) return;

            var ht = _hand[handIndex];
            var def = GameDatabase.GetTicket(ht.RealId);
            if (_energy < def.cost)
            {
                LastLog = "Не хватает Energy";
                return;
            }

            _energy -= def.cost;
            if (ht.Forged)
                LastLog = "SPECIMEN MISMATCH — roll по реальному типу";

            var outcome = RollOutcome(def);
            ApplyOutcome(outcome, def.id);
            MaybeToxicScratch();

            _hand.RemoveAt(handIndex);
            _discard.Add(ht.RealId);

            if (_enemyHp <= 0) EndCombat(true);
            else if (_playerHp <= 0) EndCombat(false);
        }

        public void EndPlayerTurn()
        {
            if (!IsActive || _combatEnded) return;

            _hand.Clear();
            var cfg = GameDatabase.Tickets.combat;
            DrawHand(cfg.handSize);
            ApplyForgedTicketRule();

            _playerBlock = 0;
            _energy = cfg.energyPerTurn + _nextTurnEnergyMod;
            _nextTurnEnergyMod = 0;
            _playerBlock += _nextTurnBlock;
            _nextTurnBlock = 0;

            if (_playerPoison > 0)
            {
                _playerHp -= _playerPoison;
                LastLog = $"Яд {_playerPoison}";
            }

            if (_enemyBleed > 0)
            {
                _enemyHp -= _enemyBleed;
                LastLog += $" · Bleed {_enemyBleed} врагу";
            }

            int enemyDmg = _sectorId == "A" ? 4 : 5;
            DealDamageToPlayer(enemyDmg);
            _turn++;

            if (_enemyHp <= 0) EndCombat(true);
            else if (_playerHp <= 0) EndCombat(false);
        }

        private OutcomeDef RollOutcome(TicketDef def)
        {
            if (def.id == "wild" && def.wildPools != null && def.wildPools.Length > 0)
            {
                string poolId = def.wildPools[_rng.Next(def.wildPools.Length)];
                def = GameDatabase.GetTicket(poolId);
            }

            int total = 0;
            foreach (var o in def.outcomes) total += o.weight;
            int roll = _rng.Next(total);
            foreach (var o in def.outcomes)
            {
                roll -= o.weight;
                if (roll < 0) return o;
            }

            return def.outcomes[0];
        }

        private void ApplyOutcome(OutcomeDef outcome, string ticketId)
        {
            var sb = new StringBuilder();
            sb.Append($"[{ticketId} {outcome.tier}] ");
            if (outcome.effects != null)
            {
                foreach (var fx in outcome.effects)
                {
                    ApplyEffect(fx, targetEnemy: IsEnemyTarget(fx.type));
                    sb.Append($"{fx.type} {fx.value} ");
                }
            }
            LastLog = sb.ToString().Trim();
        }

        private static bool IsEnemyTarget(string type) =>
            type is "Damage" or "Bleed";

        private void ApplyEffect(EffectDef fx, bool targetEnemy)
        {
            switch (fx.type)
            {
                case "Damage":
                    if (targetEnemy) DealDamageToEnemy(fx.value);
                    break;
                case "DamageSelf":
                    DealDamageToPlayer(fx.value);
                    break;
                case "Block":
                    _playerBlock += fx.value;
                    break;
                case "BlockNextTurn":
                    _nextTurnBlock += fx.value;
                    break;
                case "ClearPoison":
                    _playerPoison = 0;
                    break;
                case "PoisonSelf":
                    _playerPoison += fx.value;
                    break;
                case "Bleed":
                    _enemyBleed += fx.value;
                    break;
                case "Heal":
                    _playerHp = Mathf.Min(GameState.Instance.PlayerMaxHp, _playerHp + fx.value);
                    break;
                case "DrawTicket":
                    if (_roll.Count > 0)
                    {
                        int i = _rng.Next(_roll.Count);
                        string id = _roll[i];
                        _roll.RemoveAt(i);
                        _hand.Add(new HandTicket { RealId = id, VisualId = id });
                    }
                    break;
                case "Energy":
                    _energy += fx.value;
                    break;
                case "EnergyNextTurn":
                    _nextTurnEnergyMod -= fx.value;
                    break;
                case "Whiff":
                    break;
            }
        }

        private void MaybeToxicScratch()
        {
            var unstable = GameDatabase.GetUnstable(_sectorId);
            var rule = unstable?.ticketRules?.FirstOrDefault(r => r.type == "OnScratchRoll");
            if (rule == null) return;
            if (_rng.NextDouble() < rule.chance && rule.extraEffects != null)
                foreach (var fx in rule.extraEffects)
                    ApplyEffect(fx, false);
        }

        private void DealDamageToEnemy(int amount)
        {
            int dmg = Mathf.Max(0, amount - _enemyBlock);
            _enemyBlock = Mathf.Max(0, _enemyBlock - amount);
            _enemyHp -= dmg;
        }

        private void DealDamageToPlayer(int amount)
        {
            int dmg = Mathf.Max(0, amount - _playerBlock);
            _playerBlock = Mathf.Max(0, _playerBlock - amount);
            _playerHp -= dmg;
        }

        private void EndCombat(bool playerWon)
        {
            _combatEnded = true;
            IsActive = false;
            GameState.Instance.PlayerHp = Mathf.Max(0, _playerHp);

            if (playerWon)
            {
                GameState.Instance.CombatsWon++;
                if (_sectorId == "A" && GameState.Instance.CombatsWon >= 1)
                    GameState.Instance.RevealOutcomes = true;
                _enemy?.OnCombatEnded(true);
                LastLog = "Победа!";
            }
            else
            {
                _enemy?.OnCombatEnded(false);
                LastLog = "Поражение — отступите и попробуйте снова";
                GameState.Instance.PlayerHp = GameDatabase.Tickets.combat.playerHp;
            }
        }

        public int GetHandCount() => _hand.Count;

        public string GetHandLabel(int index)
        {
            if (index >= _hand.Count) return "";
            var ht = _hand[index];
            var def = GameDatabase.GetTicket(ht.VisualId);
            return $"{def.nameRu} (cost {def.cost})";
        }

        public int PlayerHp => _playerHp;
        public int PlayerBlock => _playerBlock;
        public int PlayerPoison => _playerPoison;
        public int EnemyHp => _enemyHp;
        public int Energy => _energy;
        public int Turn => _turn;
        public int RollRemaining => _roll.Count;
        public string EnemyName => _enemy?.DisplayName ?? "";
    }
}
