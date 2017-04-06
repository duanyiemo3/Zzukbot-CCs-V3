using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

/**
 * TBD: More thorough usage of cool spells (like death wish?)
 * Possible stance dancing (def stance for multiple enemies?)
 * 
 * To test:
 * Bandaging, resting
 * 
 * To figure out:
 * proper ranged pulling,
 * MeleePullSafety implementation
 * 
 * 
 */
[Export(typeof(CustomClass))]
public class Bulwark : CustomClass
{
    public override void Dispose() { }
    public override bool Load() { return true; }

    //Credits for the logger to baka
    public static void DebugMsg(string String)
    {
        ZzukBot.ExtensionMethods.StringExtensions.Log("Debug: " + String, "RogueLog.txt", true);
        Lua.Instance.Execute("DEFAULT_CHAT_FRAME:AddMessage(\"DEBUG: " + String + "\");");
    }

    public override bool OnBuff()
    {
        return true;
    }

    private bool UseBleed()
    {
        if (Target.CreatureType == Enums.CreatureType.Elemental || Target.CreatureType == Enums.CreatureType.Mechanical)
            return false;
        else return true;
        //To be confirmed.
        //According to this list Aaron provided (props to him for this) it should work.
        // trinitycore.atlassian.net/wiki/display/tc/creature_template
    }

    private bool MeleePullSafety()
    {
        return true;
    }

    public bool HaveBandage()
    {
        if (Inventory.Instance.GetLastItem(firstAidBandages) != "")
            return true;
        else return false;
    }

    public string[] firstAidBandages =
       {
            "Linen Bandage", "Heavy Linen Bandage", "Wool Bandage", "Heavy Wool Bandage",
            "Silk Bandage", "Heavy Silk Bandage", "Mageweave Bandage",
            "Heavy Mageweave Bandage", "Runecloth Bandage", "Heavy Runecloth Bandage"
        };

    public void SelectBandage()
    { //TODO: implement this in OnRest
        string bandageToUse = Inventory.Instance.GetLastItem(firstAidBandages);
        WoWItem Bandage = Inventory.Instance.GetItem(bandageToUse);
        Bandage.Use();
    }

    public string[] foodNames =
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

    public void SelectFood()
    {
        string foodToUse = Inventory.Instance.GetLastItem(foodNames);
        Local.Eat(foodToUse);
    }


    public string[] potNames = { "Minor Healing Potion", "Lesser Healing Potion", "Discolored Healing Potion", "Healing Potion", "Greater Healing Potion", "Superior Healing Potion", "Major Healing Potion" };


    public void SelectHPotion()
    {
        string potToUse = Inventory.Instance.GetLastItem(potNames);
        if (potToUse != "")
        {
            WoWItem Potion = Inventory.Instance.GetItem(potToUse);
            if (Local.HealthPercent <= 20)
                Potion.Use();
        }
    }

    public override void OnFight()
    {//Fight
        CombatDistance = 5;
        SelectHPotion();
        Spell.Instance.Attack();
        KickEnemy();
        HandleMultipleEnemies();
        GeneralCombat();
    }

    private void GeneralCombat()
    {
        CombatDistance = 4;
        int rage = Local.Rage;
        
        Spell.Attack();
        if (Target.IsFleeing && !Target.GotDebuff("Hamstring") && Spell.Instance.GetSpellRank("Hamstring") != 0 && Local.Rage > 15)
        {
            Spell.Instance.Cast("Hamstring");
            return;
        }

        if (Local.CanOverpower && Spell.GetSpellRank("Overpower") != 0 && Spell.IsSpellReady("Overpower") && Local.Rage >= 5)
            Spell.Cast("Overpower");

        if (!Local.GotAura("Battle Shout") && Spell.GetSpellRank("Battle Shout") != 0 && Local.Rage >= 10)
            Spell.Cast("Battle Shout");

        if (Local.Rage < 25 && Local.HealthPercent >= 75 && Spell.Instance.GetSpellRank("Bloodrage") != 0 && Spell.IsSpellReady("Bloodrage"))
            Spell.Cast("Bloodrage");

        if (Target.HealthPercent <= 20 && Spell.GetSpellRank("Execute") != 0 && Local.Rage >= 15)
            Spell.Cast("Execute");

        if (UseBleed() && Spell.GetSpellRank("Rend") != 0 && !Target.GotDebuff("Rend") && Local.Rage >= 10)
            Spell.Cast("Rend");

        if ((Spell.GetSpellRank("Mortal Strike") != 0 || Spell.GetSpellRank("Bloodthirst") != 0) && Local.Rage >= 30)
        {   if(Spell.GetSpellRank("Mortal Strike") != 0 && Spell.IsSpellReady("Mortal Strike"))
                Spell.Cast("Mortal Strike");
            if (Spell.GetSpellRank("Bloodthirst") != 0 && Spell.IsSpellReady("Bloodthirst"))
                Spell.Cast("Bloodthirst");
        }
        else if (Spell.GetSpellRank("Mortal Strike") == 0 && Spell.GetSpellRank("Bloodthirst") == 0 && Local.Rage >= 15)
            Spell.Cast("Heroic Strike");
    }

    private void UseCombatRacial()
    {
        if (Local.Race == "Tauren" || Local.Race == "Orc" || Local.Race == "Troll")
        {
            if (Local.Race == "Orc" && Spell.IsSpellReady("Blood Fury"))
                Spell.Instance.Cast("Blood Fury");
            if (Local.Race == "Troll" && Spell.IsSpellReady("Berserking"))
                Spell.Instance.Cast("Berserking");
            if (Local.Race == "Tauren" && Spell.IsSpellReady("War Stomp"))
                Spell.Instance.Cast("War Stomp");
        }
    }

    private void HandleMultipleEnemies()
    {
        if (Attackers.Count > 1)
        {
            if (Attackers.Count > 1 && Spell.Instance.GetSpellRank("Demoralizing Shout") != 0 && Local.Rage >= 10 && !Target.GotDebuff("Demoralizing Shout"))
                Spell.Cast("Demoralizing Shout");


            if (Attackers.Count > 2 && Spell.GetSpellRank("Retaliation") != 0 && Spell.IsSpellReady("Retaliation"))
                Spell.Cast("Retaliation");

            if (Spell.GetSpellRank("Bloodrage") != 0 && Spell.IsSpellReady("Blood Rage"))
                Spell.Cast("Blood Rage");

            UseCombatRacial();

            if (Spell.GetSpellRank("Sweeping Strikes") != 0 && Spell.IsSpellReady("Sweeping Strikes"))
                Spell.Cast("Sweeping Strikes");


            if (Spell.GetSpellRank("Cleave") != 0 && Local.Rage > 30 && Attackers.Count > 2)
                Spell.Cast("Cleave");
        }

    }

    private void KickEnemy()
    {
        if (Spell.GetSpellRank("Pummel") != 0 && Spell.IsSpellReady("Pummel"))
        {
            int properTargetH = Attackers.Min(Target => Target.HealthPercent);
            var properTarget = Attackers.FirstOrDefault(Target => Target.HealthPercent == properTargetH);
            var castingUnit = Attackers.FirstOrDefault(Caster => Caster.Casting != 0 || Caster.Channeling != 0);
            if (castingUnit != null && Spell.IsSpellReady("Pummel"))
            {
                Spell.Instance.StopCasting();
                if (castingUnit.Guid != Target.Guid)
                {
                    Local.SetTarget(castingUnit);
                }
                if (Target.Casting != 0 || Target.Channeling != 0)
                {
                    Spell.Instance.Cast("Pummel");
                }
                Local.SetTarget(properTarget);
                return;
            }
        }
    
        
    }
    
    private bool CanCharge()
    {
        return Spell.GetSpellRank("Charge") != 0 && Spell.IsSpellReady("Charge") && Target.DistanceToPlayer > 8;
    }

    public override void OnPull() => CanCharge() > 8 ? Spell.Cast("Charge") : Spell.Instance.Attack();
    
    public override void OnRest()
    {
        //Bandage
        if (HaveBandage() && !Local.GotDebuff("Recently Bandaged") && Local.Channeling == 0)
            SelectBandage();
        //Food
        if (Local.Channeling == 0 && !Local.IsEating)
        {
            if (Local.Race == "Night Elf" && Spell.Instance.IsSpellReady("Shadowmeld"))
            {
                SelectFood();
            }
            return;
        }

    }


    public override void ShowGui() { }
    public override void Unload() { }

    public WoWUnit Target { get { return ObjectManager.Instance.Target; } }
    public LocalPlayer Local { get { return ObjectManager.Instance.Player; } }
    public List<WoWUnit> Attackers { get { return UnitInfo.Instance.NpcAttackers; } }
    public Spell Spell { get { return Spell.Instance; } }

    public override string Author { get { return "sensgates"; } }
    public override string Name { get { return "Bulwark, WarriorCC"; } }
    public override int Version { get { return 1; } }
    public override Enums.ClassId Class { get { return Enums.ClassId.Warrior; } }
    public override bool SuppressBotMovement { get { return false; } }
    public override float CombatDistance => Spell.GetSpellRank("Charge") != 0 && Spell.IsSpellReady("Charge") ? 9 : 4;

}

