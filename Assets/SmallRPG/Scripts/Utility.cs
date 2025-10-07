using System;
using UnityEngine;

namespace SmallRPG
{
    [Serializable]
    public struct Stats
    {
        public int Strength;
        public int Agility;
        public int Endurance;

        public Stats(int strength, int agility, int endurance)
        {
            Strength = strength;
            Agility = agility;
            Endurance = endurance;
        }

        public static Stats operator +(Stats a, Stats b)
        {
            return new Stats(a.Strength + b.Strength, a.Agility + b.Agility, a.Endurance + b.Endurance);
        }
    }

    [Serializable]
    public class Ability
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] [TextArea] private string description;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;

        public Ability(string id, string displayName, string description)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
        }

        public override string ToString()
        {
            return $"{displayName}: {description}";
        }
    }

    public enum DamageType
    {
        Slashing,
        Crushing,
        Piercing
    }

    [Serializable]
    public class Weapon
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private int baseDamage;
        [SerializeField] private DamageType damageType;

        public string Id => id;
        public string DisplayName => displayName;
        public int BaseDamage => baseDamage;
        public DamageType Type => damageType;

        public Weapon(string id, string displayName, int baseDamage, DamageType damageType)
        {
            this.id = id;
            this.displayName = displayName;
            this.baseDamage = baseDamage;
            this.damageType = damageType;
        }

        public override string ToString()
        {
            return $"{displayName} (+{baseDamage} {damageType})";
        }
    }
}
