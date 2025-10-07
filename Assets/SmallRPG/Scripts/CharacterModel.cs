using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmallRPG
{
    [Serializable]
    public class ClassDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private List<Ability> abilitiesByLevel = new List<Ability>();

        public string Id => id;
        public string DisplayName => displayName;
        public IReadOnlyList<Ability> AbilitiesByLevel => abilitiesByLevel;

        public ClassDefinition(string id, string displayName, List<Ability> abilities)
        {
            this.id = id;
            this.displayName = displayName;
            abilitiesByLevel = abilities ?? new List<Ability>();
        }
    }


    [Serializable]
    public class CharacterModel
    {
        [SerializeField] private string characterName;
        [SerializeField] private List<ClassLevel> classLevels = new List<ClassLevel>();
        [SerializeField] private Stats baseStats;
        [SerializeField] private int maxHp;
        [SerializeField] private int currentHp;
        [SerializeField] private Weapon weapon;

        [Serializable]
        public class ClassLevel
        {
            public string classId;
            public int level;
        }

        public string CharacterName => characterName;
        public IReadOnlyList<ClassLevel> ClassLevels => classLevels;
        public Stats BaseStats => baseStats;
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public Weapon Weapon => weapon;

        public CharacterModel(string name)
        {
            characterName = name;
            // Randomize base stats between 1 and 3 inclusive
            baseStats = new Stats(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4));
        }

        public void SetStartingLoadout(int startingHp, Weapon startingWeapon)
        {
            maxHp = Mathf.Max(1, startingHp);
            currentHp = maxHp;
            weapon = startingWeapon;
        }

        public void HealToFull()
        {
            currentHp = maxHp;
        }

        public void TakeDamage(int amount)
        {
            currentHp = Mathf.Max(0, currentHp - Mathf.Max(0, amount));
        }

        public void GainMaxHp(int amount)
        {
            maxHp += Mathf.Max(0, amount);
            currentHp = Mathf.Min(currentHp, maxHp);
        }

        public void EquipWeapon(Weapon newWeapon)
        {
            if (newWeapon != null) weapon = newWeapon;
        }

        public bool HasClass(string classId)
        {
            for (int i = 0; i < classLevels.Count; i++)
            {
                if (classLevels[i].classId == classId) return true;
            }
            return false;
        }

        public int GetClassLevel(string classId)
        {
            for (int i = 0; i < classLevels.Count; i++)
            {
                if (classLevels[i].classId == classId) return classLevels[i].level;
            }
            return 0;
        }

        public void AddNewClass(string classId)
        {
            if (HasClass(classId)) return;
            classLevels.Add(new ClassLevel { classId = classId, level = 1 });
        }

        public void IncreaseClassLevel(string classId)
        {
            for (int i = 0; i < classLevels.Count; i++)
            {
                if (classLevels[i].classId == classId)
                {
                    classLevels[i].level = Mathf.Clamp(classLevels[i].level + 1, 1, 3);
                    return;
                }
            }
        }

        public void RestartPlayer(string newName)
        {
            characterName = newName;
            classLevels.Clear();
            baseStats = new Stats(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(1, 4));
        }

        public Stats GetBaseStats()
        {
            return baseStats;
        }

        public List<Ability> GetUnlockedAbilities(Func<string, ClassDefinition> getClassById)
        {
            List<Ability> result = new List<Ability>();
            for (int i = 0; i < classLevels.Count; i++)
            {
                ClassDefinition def = getClassById(classLevels[i].classId);
                if (def == null) continue;
                int lvl = Mathf.Clamp(classLevels[i].level, 0, 3);
                for (int a = 0; a < lvl && a < def.AbilitiesByLevel.Count; a++)
                {
                    result.Add(def.AbilitiesByLevel[a]);
                }
            }
            return result;
        }
    }
}