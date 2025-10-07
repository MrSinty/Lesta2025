using System.Collections.Generic;
using UnityEngine;

namespace SmallRPG
{
    public static class RPG
    {
        public static int ComputeWeaponDamage(Weapon weapon, Stats stats)
        {
            int baseDmg = weapon != null ? weapon.BaseDamage : 0;
            return baseDmg + Mathf.Max(0, stats.Strength);
        }

        public static bool RollHit(int attackerAgility, int defenderAgility, System.Random rng)
        {
            // New rule: roll 1..(attackerAgi + defenderAgi); if roll <= defenderAgi, miss
            int sum = Mathf.Max(1, attackerAgility + defenderAgility);
            int roll = rng.Next(1, sum + 1);
            return roll > defenderAgility;
        }

        public class Fighter
        {
            public string Name;
            public int CurrentHp;
            public int MaxHp;
            public Stats Stats;
            public Weapon Weapon;
            public List<Ability> Abilities = new List<Ability>();
            public Weapon Award;
        }

        public class FightResult
        {
            public bool PlayerWon;
            public Weapon DroppedWeapon;
        }

        public static FightResult RunTurnBasedFight(Fighter player, Fighter enemy, System.Random rng, int maxTurns = 100)
        {
            // Determine first attacker by agility; tie: player first
            bool playersTurn = player.Stats.Agility >= enemy.Stats.Agility;
            int turn = 1;
            while (player.CurrentHp > 0 && enemy.CurrentHp > 0 && turn <= maxTurns)
            {
                if (playersTurn)
                {
                    StepAttack(player, enemy, rng, isPlayersTurn: true, turn: turn);
                    if (enemy.CurrentHp <= 0) break;
                    StepAttack(enemy, player, rng, isPlayersTurn: false, turn: turn);
                }
                else
                {
                    StepAttack(enemy, player, rng, isPlayersTurn: false, turn: turn);
                    if (player.CurrentHp <= 0) break;
                    StepAttack(player, enemy, rng, isPlayersTurn: true, turn: turn);
                }

                turn++;
            }

            return new FightResult
            {
                PlayerWon = enemy.CurrentHp <= 0 && player.CurrentHp > 0,
                DroppedWeapon = enemy.Weapon
            };
        }

        public static bool TryHit(Fighter attacker, Fighter defender, System.Random rng)
        {
            int sum = Mathf.Max(1, attacker.Stats.Agility + defender.Stats.Agility);
            int roll = rng.Next(1, sum + 1);
            bool hit = roll > defender.Stats.Agility;
            
            TextManager.GetInstance().CreateAndAddToScrollView($"HIT CHECK | {attacker.Name} AGI:{attacker.Stats.Agility} vs {defender.Name} AGI:{defender.Stats.Agility} | roll 1..{sum} = {roll} => {(hit ? "HIT" : "MISS")}");
            return hit;
        }

        public static void StepAttack(Fighter attacker, Fighter defender, System.Random rng, bool isPlayersTurn, int turn)
        {
            TextManager.GetInstance().CreateAndAddToScrollView($"TURN {turn} | {(isPlayersTurn ? "Player" : "Enemy")} ATTACK | {attacker.Name} -> {defender.Name}");
            if (attacker.Weapon != null)
            {
                TextManager.GetInstance().CreateAndAddToScrollView($"WEAPON | {attacker.Name} uses {attacker.Weapon.DisplayName} [{attacker.Weapon.Type}] base:{attacker.Weapon.BaseDamage}");
            }
            else
            {
                TextManager.GetInstance().CreateAndAddToScrollView($"WEAPON | {attacker.Name} is unarmed (base:0)");
            }

            if (!TryHit(attacker, defender, rng))
            {
                TextManager.GetInstance().CreateAndAddToScrollView($"RESULT | MISS | {defender.Name} HP: {defender.CurrentHp}/{defender.MaxHp}");
                return;
            }

            int dmg = ComputeWeaponDamage(attacker.Weapon, attacker.Stats);
            int startDmg = dmg;
            TextManager.GetInstance().CreateAndAddToScrollView($"BASE DMG | weapon:{(attacker.Weapon != null ? attacker.Weapon.BaseDamage : 0)} + STR:{attacker.Stats.Strength} = {startDmg}");

            // Offensive hooks
            for (int i = 0; i < attacker.Abilities.Count; i++)
            {
                var a = attacker.Abilities[i];
                switch (a.Id)
                {
                    case "bandit_lvl1_agility_edge":
                        if (attacker.Stats.Agility > defender.Stats.Agility) { dmg += 1; TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Bandit Edge +1 (AGI advantage)"); }
                        break;
                    case "bandit_lvl2_agility_extra":
                        attacker.Stats.Agility += 1;
                        TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Bandit L2 AGI +1 (now {attacker.Stats.Agility})");
                        break;
                    case "bandit_lvl3_poison":
                        dmg += turn - 1; TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Poison +{turn - 1}");
                        break;
                    case "warrior_lvl1_opening_strike":
                        if (turn == 1) { dmg *= 2; TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Opening Strike x2"); }
                        break;
                    case "warrior_lvl3_extra_str":
                        attacker.Stats.Strength += 1; TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Warrior L3 STR +1 (now {attacker.Stats.Strength})");
                        break;
                    case "barbarian_lvl1_rage_flow":
                        if (turn <= 3) { dmg += 2; TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Rage Flow +2 (first 3 turns)"); } else { int before = dmg; dmg = Mathf.Max(0, dmg - 1); TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Rage Flow fatigue {before}->" + dmg); }
                        break;
                    case "barbarian_lvl3_extra_endur":
                        attacker.Stats.Endurance += 1; TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Barbarian L3 ENDUR +1 (now {attacker.Stats.Endurance})");
                        break;
                    case "enemy_fire_breath":
                        if (turn % 3 == 0) { dmg += 3; TextManager.GetInstance().CreateAndAddToScrollView($"OFF ABILITY | {attacker.Name} Fire Breath +3 (every 3rd turn)"); }
                        break;
                    // Enemy offensive abilities could go here
                }
            }

            // Defensive hooks
            for (int i = 0; i < defender.Abilities.Count; i++)
            {
                var d = defender.Abilities[i];
                switch (d.Id)
                {
                    case "warrior_lvl2_shield":
                        if (defender.Stats.Strength > attacker.Stats.Strength) { int before = dmg; dmg -= 3; if (dmg < 0) dmg = 0; TextManager.GetInstance().CreateAndAddToScrollView($"DEF ABILITY | {defender.Name} Shield -3 ({before}->{dmg})"); }
                        break;
                    case "barbarian_lvl2_stone_skin":
                        {
                            int reduce = Mathf.Max(0, attacker.Stats.Endurance);
                            int before = dmg;
                            dmg = Mathf.Max(0, dmg - reduce);
                            TextManager.GetInstance().CreateAndAddToScrollView($"DEF ABILITY | {defender.Name} Stone Skin -{reduce} ({before}->{dmg})");
                        }
                        break;
                    case "enemy_vuln_crushing_x2":
                        if (attacker.Weapon != null && attacker.Weapon.Type == DamageType.Crushing) { int before = dmg; dmg *= 2; TextManager.GetInstance().CreateAndAddToScrollView($"DEF TRAIT | {defender.Name} Brittle Bones x2 ({before}->{dmg})"); }
                        break;
                    case "enemy_slash_immune_weapon":
                        if (attacker.Weapon != null && attacker.Weapon.Type == DamageType.Slashing)
                        {
                            int before = dmg;
                            dmg = Mathf.Max(0, dmg - attacker.Weapon.BaseDamage);
                            TextManager.GetInstance().CreateAndAddToScrollView($"DEF TRAIT | {defender.Name} Slashing weapon damage ignored -{attacker.Weapon.BaseDamage} ({before}->{dmg})");
                        }
                        break;
                    case "enemy_stone_skin":
                        // Reduce incoming by defender Endurance
                        {
                            int reduce = Mathf.Max(0, defender.Stats.Endurance);
                            int before = dmg;
                            dmg = Mathf.Max(0, dmg - reduce);
                            TextManager.GetInstance().CreateAndAddToScrollView($"DEF TRAIT | {defender.Name} Stone Skin -{reduce} ({before}->{dmg})");
                        }
                        break;
                }
            }

            if (dmg > 0)
            {
                int beforeHp = defender.CurrentHp;
                defender.CurrentHp = Mathf.Max(0, defender.CurrentHp - dmg);
                TextManager.GetInstance().CreateAndAddToScrollView($"RESULT | HIT for {dmg} | {defender.Name} HP {beforeHp}->{defender.CurrentHp}/{defender.MaxHp}");
            }
            else
            {
                TextManager.GetInstance().CreateAndAddToScrollView($"RESULT | NO DAMAGE | {defender.Name} HP: {defender.CurrentHp}/{defender.MaxHp}");
            }
        }
    }
}


