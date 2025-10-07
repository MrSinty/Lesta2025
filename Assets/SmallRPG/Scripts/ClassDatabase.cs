using System.Collections.Generic;
using UnityEngine;

namespace SmallRPG
{
    [CreateAssetMenu(menuName = "SmallRPG/Class Database", fileName = "ClassDatabase")]
    public class ClassDatabase : ScriptableObject
    {
        [SerializeField] private List<ClassDefinition> classes = new List<ClassDefinition>();
        [SerializeField] private List<Weapon> weapons = new List<Weapon>();

        public IReadOnlyList<ClassDefinition> Classes => classes;
        public IReadOnlyList<Weapon> Weapons => weapons;

        public ClassDefinition GetById(string id)
        {
            for (int i = 0; i < classes.Count; i++)
            {
                if (classes[i].Id == id) return classes[i];
            }
            return null;
        }

        public ClassDefinition GetByName(string name)
        {
            for (int i = 0; i < classes.Count; i++)
            {
                if (classes[i].DisplayName == name) return classes[i];
            }
            return null;
        }

    public Weapon GetWeaponById(string id)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].Id == id) return weapons[i];
        }
        return null;
    }
    }
}