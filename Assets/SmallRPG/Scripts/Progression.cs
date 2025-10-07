using System.Collections.Generic;
using UnityEngine;

namespace SmallRPG
{
    public class ProgressionService
    {
        private readonly ClassDatabase database;
        private readonly CharacterModel character;
        private readonly System.Random rng = new ();

        public ProgressionService(ClassDatabase database, CharacterModel character)
        {
            this.database = database;
            this.character = character;
        }

        public List<ClassDefinition> GetAvailableClasses()
        {
            return new List<ClassDefinition>(database.Classes);
        }

        // Starting stats are now randomized inside CharacterModel

        public bool CanLevelClass(string classId)
        {
            int current = character.GetClassLevel(classId);
            return current > 0 && current < 3;
        }

        public bool CanAddNewClass(string classId)
        {
            if (character.HasClass(classId)) return false;
            return true;
        }

        public void AwardLevelToExistingClass(string classId)
        {
            if (!CanLevelClass(classId)) return;
            character.IncreaseClassLevel(classId);
        }

        public void AddNewClassAtLevelOne(string classId)
        {
            if (!CanAddNewClass(classId)) return;
            character.AddNewClass(classId);
        }

        public void ApplyLevelUpHp(string classId)
        {
            // HP gain per level now uses the character's Endurance
            int hpGain = Mathf.Max(1, character.BaseStats.Endurance);
            character.GainMaxHp(hpGain);
            character.HealToFull();
        }

        public Stats GetAggregatedStats()
        {
            return character.GetBaseStats();
        }

        public List<Ability> GetUnlockedAbilities()
        {
            return character.GetUnlockedAbilities(database.GetById);
        }
    }
}











