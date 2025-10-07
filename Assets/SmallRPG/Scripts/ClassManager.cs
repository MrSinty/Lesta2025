using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SmallRPG;
using System;

public class ClassManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ClassDatabase classDatabase;
    [SerializeField] private readonly int winsNeeded = 5;
    [SerializeField] private Image enemyImage;
    [SerializeField] private List<Sprite> enemySprites;

    private CharacterModel player;
    private ProgressionService progression;
    private System.Random rng;
    private int currentWinStreak;
    private bool awaitingLevelChoice;
    private bool awaitingStartChoice;
    private RPG.Fighter currentPlayer;
    private RPG.Fighter currentEnemy;
    private bool playersTurn;
    private int currentTurn;

    void Start()
    {
        // Create a demo player
        player = new CharacterModel("Hero");
        progression = new ProgressionService(classDatabase, player);
        rng = new System.Random();

        EnsureRuntimeDatabase();

        awaitingStartChoice = true;
        UpdateAllDropdownUI();
        TextManager.GetInstance().CreateAndAddToScrollView("Choose your starting class using the dropdown, then press NEXT FIGHT.");
        UIManager.GetInstance();
        UIManager.GetInstance().FightEnded();
    }

    // // Example: call this after a fight ends.
    // public void OnFightFinished_GrantLevelAndChoose(string classIdToLevelOrAdd, bool addNew)
    // {
    //     if (addNew)
    //     {
    //         progression.AddNewClassAtLevelOne(classIdToLevelOrAdd);
    //     }
    //     else
    //     {
    //         progression.AwardLevelToExistingClass(classIdToLevelOrAdd);
    //     }
    //     PrintState("After level up");
    // }

    public void PickStartingClass(string classId)
    {
        progression.AddNewClassAtLevelOne(classId);
    }

    // UI hook: choose starting class before the first fight
    public void ChooseStartingClass(string classId)
    {
        if (!awaitingStartChoice)
        {
            // MyLogger.Log("Starting class already chosen.");
            TextManager.GetInstance().CreateAndAddToScrollView("Starting class already chosen.");
            return;
        }
        if (string.IsNullOrEmpty(classId))
        {
            // MyLogger.Log("Invalid starting class id.");
            TextManager.GetInstance().CreateAndAddToScrollView("Invalid starting class id.");
            return;
        }
        if (player.HasClass(classId))
        {
            // MyLogger.Log("You already have this class.");
            TextManager.GetInstance().CreateAndAddToScrollView("You already have this class.");
            return;
        }
        PickStartingClass(classId);
        ApplyStartingLoadout(classId);

        var dropdownUIs = FindObjectsOfType<SmallRPG.ClassDropdownUI>(true);
        for (int i = 0; i < dropdownUIs.Length; i++)
        {
            dropdownUIs[i].AddingMode = SmallRPG.ClassDropdownUI.UIMode.AddNew;
        }

        awaitingStartChoice = false;
        PrintState($"Starting as {classId}");
    }

    private void ApplyStartingLoadout(string classId)
    {
        var def = classDatabase.GetById(classId);
        if (def == null) return;

        Weapon startingWeapon = null;
        if (def.Id == "bandit") startingWeapon = classDatabase.GetWeaponById("dagger");
        else if (def.Id == "warrior") startingWeapon = classDatabase.GetWeaponById("sword");
        else if (def.Id == "barbarian") startingWeapon = classDatabase.GetWeaponById("club");

        int baseHpPerLevel = HpPerLevel(def.Id);
        int startingHp = baseHpPerLevel; // level 1
        player.SetStartingLoadout(startingHp, startingWeapon);
    }

    private int HpPerLevel(string classId)
    {
        return classId switch
        {
            "bandit" => 4,
            "warrior" => 5,
            "barbarian" => 6,
            _ => 5,
        };
    }

    private void EnsureRuntimeDatabase()
    {
        // Minimal weapons
        if (classDatabase.Weapons.Count == 0)
        {
            typeof(ClassDatabase).GetField("weapons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(classDatabase, new List<Weapon>
                {
                    new ("dagger", "Dagger", 2, DamageType.Piercing),
                    new ("sword", "Sword", 3, DamageType.Slashing),
                    new ("club", "Club", 4, DamageType.Crushing),
                    new ("axe", "Axe", 4, DamageType.Slashing),
                    new ("spear", "Spear", 5, DamageType.Piercing),
                    new ("legendary_sword", "Legendary Sword", 10, DamageType.Slashing)
                });
        }

        if (classDatabase.Classes.Count == 0)
        {
            typeof(ClassDatabase).GetField("classes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(classDatabase, new List<ClassDefinition>
                {
                    new ("bandit", "Bandit", new List<Ability>{ new ("bandit_lvl1_agility_edge", "Bandit edge", "+1 dmg if AGI > enemy") }),
                    new ("warrior", "Warrior", new List<Ability>{ new ("warrior_lvl1_opening_strike", "Opening strike", "Double damage on turn 1") }),
                    new ("barbarian", "Barbarian", new List<Ability>{ new ("barbarian_lvl1_rage_flow", "Rage flow", "+2 dmg first 3 turns, then -1") })
                });
        }
    }

    public void SimulateRun()
    {
        RunOneFightPublic();
    }

    public void RunOneFightPublic()
    {
        if (awaitingStartChoice)
        {
            TextManager.GetInstance().CreateAndAddToScrollView("Choose a starting class first.");

            return;
        }
        if (awaitingLevelChoice)
        {
            TextManager.GetInstance().CreateAndAddToScrollView("Awaiting level choice. Choose to level current class or add a new class.");
            return;
        }

        UIManager.GetInstance().FightStarted();
        var result = RunOneFight();

        UIManager.GetInstance().FightEnded();
        if (result)
        {
            currentWinStreak++;
            if (currentWinStreak >= winsNeeded)
            {
                TextManager.GetInstance().CreateAndAddToScrollView("You win the run!");
            }
        }
        else
        {
            TextManager.GetInstance().CreateAndAddToScrollView("Defeat. Restart run.");
            currentWinStreak = 0;
            player.HealToFull();
        }
    }

    // Hook these to UI for manual, per-hit combat
    public void StartNewFight()
    {
        if (awaitingStartChoice)
        {
            TextManager.GetInstance().CreateAndAddToScrollView("Choose a starting class first.");
            return;
        }
        if (awaitingLevelChoice)
        {
            TextManager.GetInstance().CreateAndAddToScrollView("Finish your level/add choice first.");
            return;
        }
        var stats = progression.GetAggregatedStats();
        currentPlayer = new RPG.Fighter
        {
            Name = player.CharacterName,
            MaxHp = player.MaxHp,
            CurrentHp = player.CurrentHp,
            Stats = stats,
            Weapon = player.Weapon,
            Abilities = progression.GetUnlockedAbilities()
        };
        currentEnemy = BuildRandomEnemy();
        playersTurn = currentPlayer.Stats.Agility >= currentEnemy.Stats.Agility;
        currentTurn = 1;
        TextManager.GetInstance().CreateAndAddToScrollView($"Encounter! You face {currentEnemy.Name} (HP {currentEnemy.CurrentHp}/{currentEnemy.MaxHp}).");
        UIManager.GetInstance().FightStarted();
    }

    public void StepFightPublic()
    {
        if (currentPlayer == null || currentEnemy == null || currentPlayer.MaxHp == 0 || currentEnemy.MaxHp == 0)
        {
            TextManager.GetInstance().CreateAndAddToScrollView("No active fight. Press StartNewFight().");
            return;
        }
        if (currentPlayer.CurrentHp <= 0 || currentEnemy.CurrentHp <= 0)
        {
            TextManager.GetInstance().CreateAndAddToScrollView("Fight already ended. Start a new fight.");
            return;
        }

        if (playersTurn)
        {
            RPG.StepAttack(currentPlayer, currentEnemy, rng, true, currentTurn);
            TextManager.GetInstance().CreateAndAddToScrollView($"You act (Turn {currentTurn}). Enemy HP: {currentEnemy.CurrentHp}/{currentEnemy.MaxHp}");
            if (currentEnemy.CurrentHp <= 0)
            {
                OnManualFightEnded(playerWon: true, dropped: currentEnemy.Award);
                return;
            }
            RPG.StepAttack(currentEnemy, currentPlayer, rng, false, currentTurn);
            TextManager.GetInstance().CreateAndAddToScrollView($"Enemy acts (Turn {currentTurn}). Your HP: {currentPlayer.CurrentHp}/{currentPlayer.MaxHp}");
        }
        else
        {
            RPG.StepAttack(currentEnemy, currentPlayer, rng, false, currentTurn);
            TextManager.GetInstance().CreateAndAddToScrollView($"Enemy acts (Turn {currentTurn}). Your HP: {currentPlayer.CurrentHp}/{currentPlayer.MaxHp}");
            
            if (currentPlayer.CurrentHp <= 0)
            {
                OnManualFightEnded(playerWon: false, dropped: currentEnemy.Award);
                return;
            }
            RPG.StepAttack(currentPlayer, currentEnemy, rng, true, currentTurn);
            TextManager.GetInstance().CreateAndAddToScrollView($"You act (Turn {currentTurn}). Enemy HP: {currentEnemy.CurrentHp}/{currentEnemy.MaxHp}");
        }

        if (currentEnemy.CurrentHp <= 0)
        {
            OnManualFightEnded(playerWon: true, dropped: currentEnemy.Award);
            return;
        }
        if (currentPlayer.CurrentHp <= 0)
        {
            OnManualFightEnded(playerWon: false, dropped: currentEnemy.Award);
            return;
        }
        currentTurn++;
    }

    public void WeaponChangeConfirmed()
    {
        player.EquipWeapon(currentEnemy.Award);
        UIManager.GetInstance().HideDroppedWeaponPanel();
        currentPlayer = null;
        currentEnemy = null;
    }

    public void WeaponChangeDeclined()
    {
        UIManager.GetInstance().HideDroppedWeaponPanel();
        currentPlayer = null;
        currentEnemy = null;
    }

    private void OnManualFightEnded(bool playerWon, Weapon dropped)
    {
        if (playerWon)
        {
            currentWinStreak++;

            TextManager.GetInstance().CreateAndAddToScrollView($"You defeated {currentEnemy.Name}! Win {currentWinStreak}/{winsNeeded}");
            enemyImage.enabled = false;

            if (currentWinStreak >= winsNeeded)
            {
                UIManager.GetInstance().ShowWinWindow();
                Restart();
                return;
            }
                

            if (dropped != null)
            {
                UIManager.GetInstance().ShowDroppedWeaponPanel(dropped);
            }
                

            player.HealToFull();
            awaitingLevelChoice = true;

            ChangeAllDropdownUIMode(SmallRPG.ClassDropdownUI.UIMode.AddNew);
            UpdateAllDropdownUI();

            PrintState("Post-fight (won) - awaiting choice");
            UIManager.GetInstance().ShowLvlUpPanel(this);
        }
        else
        {
            TextManager.GetInstance().CreateAndAddToScrollView($"You were defeated by {currentEnemy.Name}. Restart run by choosing starting class and pressing NEXT FIGHT");
            Restart();
        }
        UIManager.GetInstance().FightEnded();
    }

    private void Restart()
    {
            currentWinStreak = 0;
            ChangeAllDropdownUIMode(SmallRPG.ClassDropdownUI.UIMode.Starting);
            UpdateAllDropdownUI();
            player.HealToFull();
            player.RestartPlayer("Hero");
            awaitingLevelChoice = false;
            awaitingStartChoice = true;


    }

    private bool RunOneFight()
    {
        // Build fighters
        var stats = progression.GetAggregatedStats();
        var playerFighter = new RPG.Fighter
        {
            Name = player.CharacterName,
            MaxHp = player.MaxHp,
            CurrentHp = player.CurrentHp,
            Stats = stats,
            Weapon = player.Weapon,
            Abilities = progression.GetUnlockedAbilities()
        };

        var enemy = BuildRandomEnemy();
        var fight = RPG.RunTurnBasedFight(playerFighter, enemy, rng);

        if (fight.PlayerWon)
        {
            // Equip drop
            if (fight.DroppedWeapon != null)
            {
                player.EquipWeapon(fight.DroppedWeapon);
            }

            // Heal after fight regardless
            player.HealToFull();
            // Now wait for player to choose: level current class or add a new class
            awaitingLevelChoice = true;

            TextManager.GetInstance().CreateAndAddToScrollView("Choose to level current class or add a new class.");
            UpdateAllDropdownUI();
            PrintState("Post-fight (won) - awaiting choice");
            UIManager.GetInstance().ShowLvlUpPanel(this);
            return true;
        }
        else
        {
            PrintState("Post-fight (lost)");
            return false;
        }
    }

    private RPG.Fighter BuildRandomEnemy()
    {
        // Choose among templates
        int pick = rng.Next(0, 6);
        enemyImage.enabled = true;
        switch (pick)
        {
            case 0: // Goblin
            default:
            {
                var aw = classDatabase.GetWeaponById("dagger");
                var w = classDatabase.GetWeaponById("goblin_weapon");
                enemyImage.sprite = enemySprites[0];
                return new RPG.Fighter{ 
                    Name = "Goblin", 
                    MaxHp = 5, 
                    CurrentHp = 5, 
                    Stats = new Stats(1,1,1), 
                    Weapon = w, 
                    Abilities = new List<Ability>(),
                    Award = aw
                };
            }
            case 1: // Skeleton
            {
                var aw = classDatabase.GetWeaponById("sword");
                var w = classDatabase.GetWeaponById("skeleton_weapon");
                enemyImage.sprite = enemySprites[1];
                return new RPG.Fighter{ 
                    Name = "Skeleton", 
                    MaxHp = 10, 
                    CurrentHp = 10, 
                    Stats = new Stats(2,2,1), 
                    Weapon = w, 
                    Abilities = new List<Ability>{ new ("enemy_vuln_crushing_x2", "Brittle Bones", "Takes x2 from Crushing") },
                    Award = aw
                };
            }
            case 2: // Slime
            {
                var aw = classDatabase.GetWeaponById("spear");
                var w = classDatabase.GetWeaponById("slime_weapon");
                enemyImage.sprite = enemySprites[2];
                return new RPG.Fighter{ 
                    Name = "Slime", 
                    MaxHp = 8, 
                    CurrentHp = 8, 
                    Stats = new Stats(3,1,2), 
                    Weapon = w, 
                    Abilities = new List<Ability>{ new ("enemy_slash_immune_weapon", "Gelatinous", "No weapon slashing dmg") },
                    Award = aw
                };
            }
            case 3: // Ghost
            {
                var aw = classDatabase.GetWeaponById("sword");
                var w = classDatabase.GetWeaponById("ghost_weapon");
                enemyImage.sprite = enemySprites[3];
                return new RPG.Fighter{ 
                    Name = "Ghost", 
                    MaxHp = 6, 
                    CurrentHp = 6, 
                    Stats = new Stats(1,3,1), 
                    Weapon = w, 
                    Abilities = new List<Ability>{ new ("bandit_lvl1_agility_edge", "Sneak Attack", "+1 dmg if AGI > enemy") },
                    Award = aw
                };
            }
            case 4: // Golem
            {
                var aw = classDatabase.GetWeaponById("axe");
                var w = classDatabase.GetWeaponById("golem_weapon");
                enemyImage.sprite = enemySprites[4];
                return new RPG.Fighter{ 
                    Name = "Golem", 
                    MaxHp = 10, 
                    CurrentHp = 10, 
                    Stats = new Stats(3,1,3), 
                    Weapon = w, 
                    Abilities = new List<Ability>{ new ("enemy_stone_skin", "Stone Skin", "Reduce incoming by END") },
                    Award = aw
                };
            }
            case 5: // Dragon
            {
                var aw = classDatabase.GetWeaponById("legendary_sword");
                var w = classDatabase.GetWeaponById("dragon_weapon");
                enemyImage.sprite = enemySprites[5];
                return new RPG.Fighter{ 
                    Name = "Dragon", 
                    MaxHp = 20, 
                    CurrentHp = 20, 
                    Stats = new Stats(3,3,3), 
                    Weapon = w, 
                    Abilities = new List<Ability>{ new ("enemy_fire_breath", "Fire Breath", "+3 dmg every 3rd turn") },
                    Award = aw
                };
            }
        }
    }

    // UI hook: Level up current class (first class) if below 3
    public void ChooseLevelUpCurrent()
    {
        if (!awaitingLevelChoice) 
        { 
            TextManager.GetInstance().CreateAndAddToScrollView("No pending choice.");
            UIManager.GetInstance().HideLvlUpPanel();
            return; 
        }
        if (player.ClassLevels.Count == 0)
        { 
            TextManager.GetInstance().CreateAndAddToScrollView("No class to level.");
            UIManager.GetInstance().HideLvlUpPanel();
            awaitingLevelChoice = false; 
            return; 
        }


        // var cId = player.ClassLevels[0].classId;
        var cId = UIManager.GetInstance().GetSelectedClassID();
        int lvl = player.GetClassLevel(cId);
        if (lvl < 3)
        {
            progression.AwardLevelToExistingClass(cId);
            progression.ApplyLevelUpHp(cId);
            TextManager.GetInstance().CreateAndAddToScrollView($"Leveled {cId} to {lvl + 1}");
        }
        else
        {
            TextManager.GetInstance().CreateAndAddToScrollView($"{cId} already at max level.");
        }
        awaitingLevelChoice = false;
        UIManager.GetInstance().HideLvlUpPanel();
        PrintState("After choice: leveled current class");
    }

    // UI hook: Add a new class by id (e.g., "warrior", "bandit", "barbarian")
    public void ChooseAddNewClass(string newClassId)
    {
        if (!awaitingLevelChoice)
        { 
            // MyLogger.Log("No pending choice.");
            TextManager.GetInstance().CreateAndAddToScrollView("No pending choice.");
            return;
        }

        if (string.IsNullOrEmpty(newClassId)) 
        { 
            // MyLogger.Log("Invalid class id.");
            TextManager.GetInstance().CreateAndAddToScrollView("Invalid class id.");
            return; 
        }
        
        if (player.HasClass(newClassId)) 
        { 
            // MyLogger.Log("Already has this class.");
            TextManager.GetInstance().CreateAndAddToScrollView("Already has this class.");
            return; 
        }

        UIManager.GetInstance().HideLvlUpPanel();
        progression.AddNewClassAtLevelOne(newClassId);
        // Gain HP for new class level as well
        progression.ApplyLevelUpHp(newClassId);
        awaitingLevelChoice = false;
        PrintState($"After choice: added new class {newClassId}");
    }

    // Helper: returns ids of classes not yet owned
    public List<string> GetAvailableNewClassIds()
    {
        List<string> ids = new();
        foreach (var c in classDatabase.Classes)
        {
            if (!player.HasClass(c.Id)) ids.Add(c.Id);
        }
        return ids;
    }

    public List<string> GetOwnedClassIds()
    {
        List<string> ids = new();
        foreach (var c in classDatabase.Classes)
        {
            if (player.HasClass(c.Id)) ids.Add(c.Id);
        }
        return ids;
    }

    // Helper: returns all class ids (used for starting selection)
    public List<string> GetAllClassIds()
    {
        List<string> ids = new();
        foreach (var c in classDatabase.Classes)
        {
            ids.Add(c.Id);
        }
        return ids;
    }
    
    private void PrintState(string title)
    {
        Stats stats = progression.GetAggregatedStats();
        List<Ability> abilities = progression.GetUnlockedAbilities();

        TextManager.GetInstance().CreateAndAddToScrollView($"==== {title} ====");
        TextManager.GetInstance().CreateAndAddToScrollView($"Classes: {FormatClasses()}");
        TextManager.GetInstance().CreateAndAddToScrollView($"Stats -> STR:{stats.Strength} AGI:{stats.Agility} ENDUR:{stats.Endurance}");
        TextManager.GetInstance().CreateAndAddToScrollView($"Abilities: {string.Join(", ", abilities.ConvertAll(a => a.DisplayName))}");
        TextManager.GetInstance().CreateAndAddToScrollView($"Weapon: {player.Weapon?.DisplayName ?? "None"}, HP: {player.CurrentHp}/{player.MaxHp}");
    }

    private string FormatClasses()
    {
        List<string> parts = new();
        foreach (var cl in player.ClassLevels)
        {
            ClassDefinition def = classDatabase.GetById(cl.classId);
            string name = def != null ? def.DisplayName : cl.classId;
            parts.Add($"{name} {cl.level}");
        }
        return string.Join(", ", parts);
    }

    private void UpdateAllDropdownUI()
    {
        // Refresh any class dropdowns so they show up-to-date available classes
        var dropdownUIs = FindObjectsOfType<SmallRPG.ClassDropdownUI>(true);
        for (int i = 0; i < dropdownUIs.Length; i++)
        {
            dropdownUIs[i].RefreshOptions();
        }
    }

    private void ChangeAllDropdownUIMode(SmallRPG.ClassDropdownUI.UIMode newMode)
    {
        var dropdownUIs = FindObjectsOfType<SmallRPG.ClassDropdownUI>(true);
        for (int i = 0; i < dropdownUIs.Length; i++)
        {
            dropdownUIs[i].AddingMode = newMode;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
