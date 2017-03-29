using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;
using System;

[Export(typeof(CustomClass))]
public class AffLock : CustomClass
{
    
    private void SelectHPotion()
    {
        string potToUse = Inventory.Instance.GetLastItem(potNames);
        if (potToUse != "")
        {
            WoWItem Potion = Inventory.Instance.GetItem(potToUse);
            if (Local.HealthPercent <= 20 && Potion.CanUse())
                Potion.Use();
        }
    }

    private string[] potNames = 
    {
        "Minor Healing Potion", "Lesser Healing Potion", "Discolored Healing Potion", "Healing Potion", "Greater Healing Potion", "Superior Healing Potion", "Major Healing Potion"
    };

    private void SelectMPotion()
    {// to be added

    }

    private string[] potNamesMana =
    { //to be added

    };

    private void SelectHealthstone()
    {
        string healthstoneToUse = Inventory.Instance.GetLastItem(healthstoneList);
        if (healthstoneToUse != "")
        {
            WoWItem Healthstone = Inventory.Instance.GetItem(healthstoneToUse);
            if (Healthstone.CanUse())
                Healthstone.Use();
        }
    }

    private string[] healthstoneList =
    {
        "Minor Healthstone", "Lesser Healthstone", "Healthstone", "Greater Healthstone", "Major Healthstone"
    };

    private void SelectFood()
    {
        string foodToUse = Inventory.Instance.GetLastItem(foodNames);
        Local.Eat(foodToUse);
    }

    private string[] foodNames =
    {
            "Tough Hunk of Bread", "Darnassian Bleu", "Slitherskin Mackeral",
            "Shiny Red Apple", "Forest Mushroom Cap", "Tough Jerky",
            "Freshly Baked Bread", "Dalaran Sharp", "Longjaw Mud Snapper",
            "Tel'Abim Banana", "Red-speckled Mushroom", "Haunch of Meat",
            "Moist Cornbread", "Dwarven Mild", "Bristle Whisker Catfish",
            "Snapvine Watermelon", "Spongy Morel", "Mutton Chop",
            "Mulgore Spice Bread", "Stormwind Brie", "Rockscale Cod",
            "Goldenbark Apple", "Delicious Cave Mold", "Wild Hog Shank",
            "Soft Banana Bread", "Fine Aged Cheddar", "Spotted Yellowtail",
            "Striped Yellowtail", "Moon Harvest Pumpkin", "Raw Black Truffle",
            "Cured Ham Steak", "Homemade Cherry Pie", "Alterac Swiss",
            "Spinefin Halibut", "Deep Fried Plantains", "Dried King Bolete",
            "Roasted Quail", "Conjured Sweet Roll", "Conjured Cinnamon Roll"
    };

    private void SelectDrink()
    {
        string drinkToUse = Inventory.Instance.GetLastItem(drinkNames);
        Local.Drink(drinkToUse);
    }

    private string[] drinkNames =
    {

    };

    private void GeneralCombat()
    {
        bool canWand = Local.IsWandEquipped();
        if (Local.GotAura("Shadow Trance"))
            Spell.CastWait("Shadow Bolt", 1500);
        if (Spell.GetSpellRank("Shadowburn") != 0 && Spell.IsSpellReady("Shadowburn") && Target.HealthPercent <= 15 && Local.ManaPercent >= 15)
            Spell.CastWait("Shadowburn", 500);
        if (Spell.GetSpellRank("Life Tap") != 0 && Attackers.Count < 2 && Local.HealthPercent >= 50 && Local.HasPet && Pet.IsAlive() && Local.ManaPercent <= 40 && Local.Channeling == 0 && Local.Casting == 0 && Target.HealthPercent <= 15)
            Spell.Cast("Life Tap");
        if (Spell.GetSpellRank("Immolate") == 0 && Local.Casting == 0 && Local.Channeling == 0)
            Spell.Cast("Shadow Bolt");
        if (Spell.GetSpellRank("Corruption") != 0 && !Target.GotDebuff("Corruption") && Local.Casting == 0 && Local.Channeling == 0 && Local.ManaPercent >= 10 && Target.HealthPercent >= 10)
            Spell.CastWait("Corruption", 500);
        if (Spell.GetSpellRank("Curse of Agony") != 0 && !Target.GotDebuff("Curse of Agony") && Local.Casting == 0 && Local.Channeling == 0 && Local.ManaPercent >= 10 && Target.HealthPercent >= 40)
            Spell.CastWait("Curse of Agony", 500);
        if (Spell.GetSpellRank("Siphon Life") != 0 && !Target.GotDebuff("Siphon Life") && Local.Casting == 0 && Local.Channeling == 0 && Local.ManaPercent >= 10 && Target.HealthPercent >= 15)
            Spell.CastWait("Siphon Life", 500);
        if (Spell.GetSpellRank("Drain Life") != 0 && Local.HealthPercent <= 50 && Local.ManaPercent >= 25 && Local.Casting == 0 && Local.Channeling == 0)
            Spell.CastWait("Drain Life", 1000);
        if (Spell.GetSpellRank("Immolate") != 0 && !Target.GotDebuff("Immolate") && Local.Casting == 0 && Local.Channeling == 0 && Local.ManaPercent >= 15 && Target.HealthPercent >= 10)
            Spell.CastWait("Immolate", 500);

        if (Local.Casting == 0 && Local.Channeling == 0 && canWand)
        {
            if (!Local.HasPet || (Spell.IsSpellReady("Immolate") && Local.ManaPercent <= 20) || Spell.IsSpellReady("Immolate") && Local.HealthPercent >= 60 && Local.ManaPercent > 30 && Target.GotDebuff("Immolate") && Target.GotDebuff("Corruption") && Target.GotDebuff("Curse of Agony") || Spell.IsSpellReady("Immolate") && Inventory.Instance.GetItemCount("Soul Shard") == 3 && Target.HealthPercent <= 15 && Attackers.Count < 2 || Spell.GetSpellRank("Curse of Agony") == 0)
            {
                Spell.StartWand();
            }
            else Spell.Attack();
        }

    }
    
    private bool MultiDot()
    {
        if (Attackers.Count >= 2 && Local.ManaPercent >= 40)
        {
            int properTargetH = Attackers.Min(Target => Target.HealthPercent);
            var properTarget = Attackers.FirstOrDefault(Target => Target.HealthPercent == properTargetH);
            int newAddH = Attackers.Max(Enemy => Enemy.HealthPercent);
            var newAdd = Attackers.FirstOrDefault(Enemy => Enemy.HealthPercent == newAddH);
            if (newAdd != null && newAdd.Guid != Target.Guid && (!newAdd.GotDebuff("Curse of Agony") && Spell.GetSpellRank("Curse of Agony") != 0) && (!newAdd.GotDebuff("Corruption") && Spell.GetSpellRank("Corruption") != 0) && (!newAdd.GotDebuff("Siphon Life") && Spell.GetSpellRank("Siphon Life") != 0))
            {

                    if (Spell.IsSpellReady("Curse of Agony"))
                    {
                        Local.SetTarget(newAdd);
                        Spell.Cast("Curse of Agony");
                        Spell.Cast("Corruption");
                        Spell.Cast("Siphon Life");
                    }
                
            }
            Local.SetTarget(properTarget);
        }
        return true;
    }
    
    private void HandleMultipleEnemies()
    {   //Credits to dgcfus for this snippet.
        if (Local.HasPet)
        {
            if(Attackers.Count >= 2 && Local.HasPet && Pet.IsAlive())
            {
                var attackUnit = Attackers.FirstOrDefault(Mob => Mob.TargetGuid == Local.Guid);
                if (attackUnit != null)
                {
                    Local.SetTarget(attackUnit);
                    if (!Pet.IsOnMyTarget())
                    {
                        Pet.Attack();
                        Pet.Cast("Suffering");
                    }
                }
                else
                {
                    int LowerHP = Attackers.Min(Mob => Mob.HealthPercent);
                    var LowerHPUnit = Attackers.SingleOrDefault(Mob => Mob.HealthPercent == LowerHP);
                    if (LowerHPUnit != null && LowerHPUnit.Guid != Target.Guid)
                        Local.SetTarget(LowerHPUnit);
                }
            }
        }

    }

    private void GetSoulshard()
    {
        if (Inventory.Instance.GetItemCount("Soul Shard") < 3 && Spell.GetSpellRank("Drain Soul") != 0 && Target.HealthPercent < 9)
            Spell.Cast("Drain Soul");

    }

    private void PullPriority()
    {
        if (Spell.GetSpellRank("Curse of Agony") != 0 && !Target.GotDebuff("Curse of Agony"))
        {
            Spell.CastWait("Curse of Agony", 500);
        }
        if (Local.HasPet)
            Pet.Attack();
        else if (Spell.GetSpellRank("Corruption") != 0 && !Target.GotDebuff("Corruption"))
            Spell.CastWait("Corruption", 500);
        else if (Spell.GetSpellRank("Immolate") != 0 && !Target.GotDebuff("Immolate"))
            Spell.CastWait("Immolate", 500);
        else if (Local.ManaPercent >= 35)
            Spell.Cast("Shadow Bolt");
        else Spell.Attack();
    }


    private void GetHealthstone()
    {
        if (Inventory.Instance.GetItemCount("Soul Shard") >= 1)
        {
            if (Spell.GetSpellRank("Create Healthstone (Major)") != 0 && Inventory.Instance.GetItemCount("Major Healthstone") == 0)
                Spell.CastWait("Create Healthstone(Major)", 3500);
            else if (Spell.GetSpellRank("Create Healthstone (Greater)") != 0 && Inventory.Instance.GetItemCount("Greater Healthstone") == 0)
                Spell.CastWait("Create Healthstone(Greater)", 3500);
            else if (Spell.GetSpellRank("Create Healthstone") != 0 && Inventory.Instance.GetItemCount("Healthstone") == 0)
                Spell.CastWait("Create Healthstone", 3500);
            else if (Spell.GetSpellRank("Create Healthstone(Lesser)") != 0 && Inventory.Instance.GetItemCount("Lesser Healthstone") == 0)
                Spell.CastWait("Create Healthstone(Lesser)", 3500);
            else if (Spell.GetSpellRank("Create Healthstone(Minor)") != 0 && Inventory.Instance.GetItemCount("Minor Healthstone") == 0)
                Spell.CastWait("Create Healthstone(Minor)", 3500);
            else return;             
        }
    }

    public string CheckPets()
    {
        if (Spell.GetSpellRank("Summon Voidwalker") == 0 && Spell.GetSpellRank("Summon Imp") == 0)
        {
            string petToUse = null;
            return petToUse;
        }
        else if (Spell.GetSpellRank("Summon Voidwalker ") == 0 && Spell.GetSpellRank("Summon Imp") != 0)
        {
            string petToUse = "Imp";
            return petToUse;
        }
        else if (Spell.GetSpellRank("Summon Voidwalker") != 0)
        {
            string petToUse = "Voidwalker";
            return petToUse;
        }
        else return null;
    }
    public override bool OnBuff()
    {
        string petToUse;
        if(!Local.HasPet)
        {
            petToUse = CheckPets();
            if (petToUse == null)
                return true;
            else if (petToUse == "Imp")
                Spell.Cast("Summon Imp");
            else if (petToUse == "Voidwalker" && Inventory.Instance.GetItemCount("Soul Shard") != 0)
                Spell.Cast("Summon Voidwalker");
        }
        if (!Local.GotAura("Demon Skin") || !Local.GotAura("Demon Armor") && (Spell.GetSpellRank("Demon Skin") != 0 || Spell.GetSpellRank("Demon Armor") != 0))
        {
            if (Spell.GetSpellRank("Demon Armor") == 0 && !Local.GotAura("Demon Skin"))
                Spell.Cast("Demon Skin");
            if (Spell.GetSpellRank("Demon Armor") != 0 && !Local.GotAura("Demon Armor"))
                Spell.Cast("Demon Armor");
        }

        GetHealthstone();

        return true;
    }

    public override void OnPull()
    {
        PullPriority();    
    }
    

    public override void OnFight()
    {
        if (Local.HasPet && Pet.HealthPercent > 0)
            Pet.Attack();
        if ((Pet.CanUse("Sacrifice") && Pet.HealthPercent < 10 && Pet.IsAlive()) || Pet.CanUse("Sacrifice") && Local.HealthPercent < 10 && Pet.IsAlive())
            Pet.Cast("Sacrifice");

        HandleMultipleEnemies();
        SelectHealthstone();
        SelectMPotion();
        SelectHPotion();
        MultiDot();
        GeneralCombat();
        
    }
    public override void OnRest()
    {
        if (Spell.GetSpellRank("Life Tap") != 0 && Local.ManaPercent <= 80 && Local.HealthPercent >= 70)
            Spell.Cast("Life Tap");
        SelectDrink();
        SelectFood();
        if (!Local.IsEating && !Local.IsDrinking)
            Lua.Instance.Execute("DoEmote('Sit')");
    }

    

    public override void ShowGui() { }
    public override void Unload() { }

    public WoWUnit Target { get { return ObjectManager.Instance.Target; } }
    public LocalPlayer Local { get { return ObjectManager.Instance.Player; } }
    public LocalPet Pet { get { return ObjectManager.Instance.Pet; } }
    public List<WoWUnit> Attackers { get { return UnitInfo.Instance.NpcAttackers; } }
    public Spell Spell { get { return Spell.Instance; } }

    public override string Author { get { return "sensgates"; } }
    public override string Name { get { return "AffLock, original author Fedelis"; } }
    public override int Version { get { return 1; } }
    public override Enums.ClassId Class { get { return Enums.ClassId.Warlock; } }
    public override bool SuppressBotMovement { get { return false; } }
    public override float CombatDistance { get { return 25.0f; } }

    
}