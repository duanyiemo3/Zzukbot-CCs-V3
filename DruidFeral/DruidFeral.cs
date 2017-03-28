using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

/**
 *To work out:
 * FB Usage
 * Proper healing (shifting back afterwards, to be more precise)
 * CreatureTypes to finish CanBleed
 * Claw isnt being used bruv...
 * 
 * To test:
 * Combat distances
 * Scrolls
 * Drinking/Eating
 * 
 * Working:
 * Shifting / conditional shifting
 * Healing (but not shifting back)
 * Buffs
 * 
 * 
 */

[Export(typeof(CustomClass))]
public class DruidFeral : CustomClass
{
    public override void Dispose() { }
    public override bool Load() { return true; }

    //Credits for the logger to baka
    public static void DebugMsg(string String)
    {
        ZzukBot.ExtensionMethods.StringExtensions.Log("Debug: " + String, "DruidLog.txt", true);
        Lua.Instance.Execute("DEFAULT_CHAT_FRAME:AddMessage(\"DEBUG: " + String + "\");");
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

    private string[] drinkNames = {
        "Refreshing Spring Water", "Ice Cold Milk", "Melon Juice", "Moonberry Juice",
        "Sweet Nectar", "Morning Glory Dew", "Conjured Purified Water", "Conjured Spring Water", "Conjured Mineral Water", "Conjured Sparkling Water", "Conjured Crystal Water"};

    private void SelectFood()
    {
        string foodToUse = Inventory.Instance.GetLastItem(foodNames);
        Local.Eat(foodToUse);
    }

    private void SelectDrink()
    {
        string drinkToUse = Inventory.Instance.GetLastItem(drinkNames);
        Local.Drink(drinkToUse);
    }


    private string[] potNamesHeal = { "Minor Healing Potion", "Lesser Healing Potion", "Discolored Healing Potion", "Healing Potion", "Greater Healing Potion", "Superior Healing Potion", "Major Healing Potion" };

    private string[] potNamesMana = { "Minor Mana Potion", "Lesser Mana Potion", "Mana Potion", "Greater Mana Potion", "Superior Mana Potion", "Major Mana Potion" };

    private void SelectHPotion()
    {
        if (Local.HealthPercent <= 35)
        {
            DebugMsg("Checking Healing Potions..");
            string potToUseH = Inventory.Instance.GetLastItem(potNamesHeal);
            if (potToUseH != "")
            {
                WoWItem HPotion = Inventory.Instance.GetItem(potToUseH);
                if (HPotion.CanUse())
                {
                    if (Spell.IsShapeShifted)
                        Spell.Instance.CancelShapeshift();
                    HPotion.Use();   
                }
                
            }
        }
    }

    private void SelectMPotion()
    {
        DebugMsg("Checking Mana Potions..");
        string potToUseM = Inventory.Instance.GetLastItem(potNamesMana);
        if (potToUseM != "" && Local.ManaPercent <= 35)
        {
            WoWItem MPotion = Inventory.Instance.GetItem(potToUseM);
            if (MPotion.CanUse())
            {
                if (Spell.IsShapeShifted)
                    Spell.Instance.CancelShapeshift();
                MPotion.Use();
            }
        }
       

        
    }

    private void GoAnimal()
    {
        if (CanBeAnimal())
        {
            if (Spell.Instance.GetSpellRank("Cat Form") != 0 && Attackers.Count < 2 && !Local.GotAura("Cat Form"))
                Spell.Instance.Cast("Cast Form");
            if (Spell.Instance.GetSpellRank("Dire Bear Form") != 0 && Attackers.Count >= 2 && !Local.GotAura("Dire Bear Form"))
                Spell.Instance.Cast("Dire Bear Form");
            if (Spell.Instance.GetSpellRank("Bear Form") != 0 && !Local.GotAura("Bear Form") && !Local.GotAura("Dire Bear Form"))
                Spell.Instance.Cast("Bear Form");
        }
    }

    private bool CanBeAnimal()
    {
        if (Spell.Instance.GetSpellRank("Bear Form") != 0 || Spell.Instance.GetSpellRank("Cat Form") != 0 || Spell.Instance.GetSpellRank("Dire Bear Form") != 0)
        {
            DebugMsg("We can shapeshift! (CanBeAnimal evaluated to true.)");
            return true;
        }
        else return false;
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

    private void UrgentSituation()
    {
        if (Local.HealthPercent < 40)
        {
            if (Spell.IsShapeShifted && Local.Casting == 0 && Local.Channeling == 0)
                Spell.Instance.CancelShapeshift();
            if (Spell.Instance.GetSpellRank("Barkskin") != 0 && Spell.Instance.IsSpellReady("Barkskin"))
                Spell.Instance.Cast("Barkskin");
            if (Local.ManaPercent <= 75 && Spell.Instance.GetSpellRank("Innverate") != 0 && Spell.Instance.IsSpellReady("Innervate"))
                Spell.Instance.Cast("Innervate");
            if (!Local.GotAura("Innervate") && Local.ManaPercent <= 50)
                SelectMPotion();
            else SelectHPotion();
            if (Spell.Instance.GetSpellRank("Healing Touch") != 0 && Local.HealthPercent <= 40)
            {
                if (Local.Race == "Tauren" && Spell.Instance.IsSpellReady("War Stomp"))
                    Spell.Instance.Cast("War Stomp");
                Spell.Instance.Cast("Healing Touch");
            }
            if (Spell.Instance.GetSpellRank("Rejuvenation") != 0)
                Spell.Instance.Cast("Rejuvenation");
            if (Local.ManaPercent < 40)
            {
                SelectMPotion();
                if (Spell.Instance.GetSpellRank("Innervate") != 0 && Spell.Instance.IsSpellReady("Innervate") && Local.ManaPercent <= 40)
                    Spell.Instance.Cast("Innervate");
            }
        }
        return;
    }

    public override bool OnBuff()
    {
        if ((!Local.GotAura("Omen of Clarity") && Spell.Instance.GetSpellRank("Omen of Clarity") != 0) || (!Local.GotAura("Thorns") && Spell.Instance.GetSpellRank("Thorns") != 0) || (!Local.GotAura("Mark of the Wild") && Spell.Instance.GetSpellRank("Mark of the Wild") != 0))
        {
            if (Spell.IsShapeShifted)
            {
                Spell.Instance.CancelShapeshift();
                return false;
            }
            if (!Local.GotAura("Omen of Clarity") && Spell.Instance.GetSpellRank("Omen of Clarity") != 0)
            {
                Spell.Instance.Cast("Omen of Clarity");
                return false;
            }
            if (!Local.GotAura("Thorns") && Spell.Instance.GetSpellRank("Thorns") != 0)
            {
                Spell.Instance.Cast("Thorns");
                return false;
            }
            if (!Local.GotAura("Mark of the Wild") && Spell.Instance.GetSpellRank("Mark of the Wild") != 0)
            {
                Spell.Instance.Cast("Mark of the Wild");
                return false;
            }
        }
        if (!Spell.IsShapeShifted && CanBeAnimal())
        {
            DebugMsg("Not shifted during buff, trying to go animal..");
            if (Spell.GetSpellRank("Cat Form") != 0)
            {
                Spell.Cast("Cat Form");
                return false;
            }
            else
            {
                Spell.Cast("Bear Form");
                return false;
            }    
        }
        return true;
    }

    public override void OnFight()
    {//Fight
        CombatDistance = 5.0f;
        int energy = Local.Energy;
        int comboPoint = Local.ComboPoints;
        UrgentSituation();
        if (Attackers.Count < 2 && Spell.GetSpellRank("Cat Form") != 0 && !Local.GotAura("Cat Form"))
        {
            Spell.CancelShapeshift();
            Spell.Cast("Cat Form");
        }
        if (!Spell.IsShapeShifted)
        {
            DebugMsg("Not shifted in fight, looking to go into animal form..");
            if (Spell.GetSpellRank("Cat Form") != 0 && Attackers.Count < 2)
                Spell.Cast("Cat Form");
            else if (Spell.Instance.GetSpellRank("Dire Bear Form") != 0 && Attackers.Count >= 2)
                Spell.Cast("Dire Bear Form");
            else Spell.Cast("Dire Bear Form");
        }
        Spell.Instance.Attack();
        HandleMultipleEnemies();
        GeneralCombat();
    }

    private void HandleMultipleEnemies()
    {
        if (Attackers.Count >= 2)
        {
            if (Spell.IsShapeShifted && !Local.GotAura("Bear Form") && !Local.GotAura("Dire Bear Form"))
                Spell.Instance.CancelShapeshift();
            GoAnimal();
            //if (Spell.Instance.GetSpellRank("Dire Bear Form") != 0)
            //    Spell.Instance.Cast("Dire Bear Form");
            //else Spell.Instance.Cast("Bear Form");
        }

    }

    private void GeneralCombat()
    {
        int combopoints = Local.ComboPoints;
        int energy = Local.Energy;
        int rage = Local.Rage;
        DebugMsg("We are in GeneralCombat, current stats: CPs: " + combopoints + ", energy: " + energy + ", rage: " + rage);
        DebugMsg("Current CombatDistance: " + CombatDistance);
        if (!CanBeAnimal())
        {//Spell.Instance.GetSpellRank("Bear Form") == 0 && Spell.Instance.GetSpellRank("Cat Form") == 0
            DebugMsg("No possible shifting, going melee / casting..");
            if (Local.ManaPercent > 35)
            {
                CombatDistance = 25;
                if (Spell.Instance.GetSpellRank("Moonfire") != 0 && !Target.GotDebuff("Moonfire"))
                    Spell.Instance.Cast("Moonfire");
                Spell.Instance.Cast("Wrath");
            }
            CombatDistance = 5;
            Spell.Instance.Attack();
        }
        else if ((CanBeAnimal() && Spell.Instance.GetSpellRank("Cat Form") == 0) || Attackers.Count >= 2)
        {
            DebugMsg("multiple enemies or no cat form yet, going bearmode..");
            if (!Spell.IsShapeShifted && Local.Casting == 0 && Local.Channeling == 0)
            {
                GoAnimal();
                //if (Spell.GetSpellRank("Dire Bear Form") != 0 && !Local.GotAura("Dire Bear Form"))
                //    Spell.Cast("Dire Bear Form");
                //else if (Spell.GetSpellRank("Bear Form") != 0 && !Local.GotAura("Bear Form") && !Local.GotAura("Dire Bear Form"))
                //    Spell.Cast("Bear Form");
            }
            CombatDistance = 5;
            if (Local.Rage < 10 && Spell.Instance.IsSpellReady("Enrage"))
                Spell.Instance.Cast("Enrage");
            if (Spell.Instance.GetSpellRank("Demoralizing Roar") != 0 && !Target.GotDebuff("Demoralizing Roar") && Spell.Instance.IsSpellReady("Demoralizing Roar"))
                Spell.Instance.Cast("Demoralizing Roar");
            if (Spell.Instance.GetSpellRank("Bash") != 0 && Spell.Instance.IsSpellReady("Bash"))
                Spell.Instance.Cast("Bash");
            if (Spell.Instance.GetSpellRank("Frenzied Regeneration") != 0 && Spell.Instance.IsSpellReady("Frenzied Regeneration") && Local.HealthPercent <= 60)
                Spell.Instance.Cast("Frenzied Regeneration");
            if (Attackers.Count >= 2 && rage >= 40 && Spell.Instance.GetSpellRank("Swipe") != 0)
                Spell.Instance.Cast("Swipe");
            if (rage >= 40)
                Spell.Instance.CastWait("Maul", 3000);
        }
        else if (Spell.Instance.GetSpellRank("Cat Form") != 0 && Attackers.Count < 2)
        {

            if (!Spell.IsShapeShifted && Local.Casting == 0 && Local.Channeling == 0)
            {
                GoAnimal();
            }
                //Spell.Cast("Cat Form");
            DebugMsg("We are a Cat! going for DPS.");
            CombatDistance = 5;
            DebugMsg("Current CombatDistance: " + CombatDistance);
            if (!Target.GotDebuff("Faerie Fire (Feral)") && Spell.Instance.GetSpellRank("Faerie Fire (Feral)") != 0 && UseBleed())
                Lua.Instance.Execute("CastSpellByName('Faerie Fire (Feral)()');");
            Spell.Instance.Attack();
            if ((FBiteEvaluation() && energy >= 35) || (combopoints == 5 && energy >= 35))
                Spell.Instance.Cast("Ferocious Bite");
            if (energy > 30 && !Local.GotAura("Tiger's Fury") && Spell.Instance.GetSpellRank("Tiger's Fury") != 0)
                Spell.Instance.Cast("Tiger's Fury");
            //if (combopoints < 5 && energy > 30 && !Target.GotDebuff("Rake") && UseBleed() && Spell.Instance.GetSpellRank("Rake") != 0 && !FBiteEvaluation())
            //    Spell.Instance.Cast("Rake");
            if (combopoints < 5 && energy > 40)
                Spell.Instance.Cast("Claw");

        }
    }

    

    private int[][] feroBiteDamage =
    { //first value is always some kind of approximate AP.
      //Example: checked on a level 57 druid with standard leveling gear: 801 AP -> .1526 * 801 = 122.
      // Might use Lua.ExecuteWithResult to get actual AP Values in the future.
      // Thanks and credits to Emu or krycess for the general idea of this.
        new int[] {0, 0, 0, 0, 0},                            //rank 0
        new int[] {25, 66, 102, 138, 174, 210},               //rank 1
        new int[] {50, 103, 162, 221, 280, 339},
        new int[] {75, 162, 254, 346, 438, 530},
        new int[] {115, 223, 351, 479, 607, 735},
        new int[] {150, 259, 406, 553, 700, 847}

    };

    private bool FBiteEvaluation()
    {
        /**
         * FB Damage Calc:
         * MaxDamage =
         *  (( AP * 0,1526 + [energy - 35] * 2.5 + MaxDamageRank ) 
         *  * 1.15 [5/5 Feral Aggression] )
         *  * 1.1 [5/5Natural Talents]
         *  MinDamage: change MaxDamageRank with MinDamageRank respectively.
         *  Source: forum.nostalrius.org/viewtopic.php?f=41&t=27786
         *  To be more liberal with FB usage, only going to consider max damage rolls and no armor reduction.
         */
        int fbRank = Spell.GetSpellRank("Ferocious Bite");
        int cp = Local.ComboPoints;
        int energy = Local.Energy;
        DebugMsg("Calculation values for FB Damage - Rank: " + fbRank + ", energy: " + energy);
        double fbDamage = (feroBiteDamage[fbRank][cp] + feroBiteDamage[fbRank][0]);
        DebugMsg("Calculated FB Damage: " + fbDamage + " , current target health: " + Target.Health);
        if (Target.Health < fbDamage)
            return true;
        else return false;
    }

    public override void OnPull()
    {   //TBD: figure out ranged pull.
        //Just to log the different creature types for later use in UseBleed().
        DebugMsg("Creature Type:" + Target.CreatureType + " Creature Name:" + Target.Name);
        if (Spell.IsShapeShifted)
        {
            CombatDistance = 15;
            DebugMsg("shapeshifted pull, Current CombatDistance: " + CombatDistance);
            Lua.Instance.Execute("CastSpellByName('Faerie Fire (Feral)()');");
            CombatDistance = 5;
            Spell.Instance.Attack();
        }

        else if (Local.ManaPercent > 35 && !Spell.IsShapeShifted)
        {
            CombatDistance = 25;
            DebugMsg("We are not shifted, pulling with Moonfire and increasing Current CombatDistance to: " + CombatDistance);
            if (Spell.GetSpellRank("Moonfire") != 0)
                Spell.Instance.Cast("Moonfire");
            else Spell.Cast("Wrath");
        }
        else
        {

            DebugMsg("Melee pull, Current CombatDistance: " + CombatDistance);
            Spell.Instance.Attack();
        }

    }
    public override void OnRest()
    {
        DebugMsg("We are in Rest now. checking what to do..");
        if (Spell.IsShapeShifted)
            Spell.CancelShapeshift();
        OocHeal();
        if (Local.HealthPercent < 50 || Local.ManaPercent < 50)
        {
            if (Local.Channeling == 0 && !Local.IsEating && !Local.IsDrinking)
            {
                DebugMsg("We will now look for food and eat / drink..");
                if (Local.Race == "Night Elf" && Spell.Instance.IsSpellReady("Shadowmeld"))
                    Spell.Instance.Cast("Shadowmeld");
                SelectDrink();
                SelectFood();
            }
        }
    }

    private void OocHeal()
    {
        DebugMsg("Rest Healing was called..");
        if (Local.HealthPercent <= 35)
        {
            Spell.Instance.Cast("Healing Touch");
            if (Spell.Instance.GetSpellRank("Rejuvenation") != 0)
                Spell.Instance.Cast("Rejuvenation");
        }
        if (Local.HealthPercent <= 50)
            Spell.Instance.Cast("Healing Touch");

        if (Local.ManaPercent <= 50 && Spell.Instance.GetSpellRank("Innervate") != 0 && Spell.Instance.IsSpellReady("Innervate"))
            Spell.Instance.Cast("Innervate");

    }
    public override void ShowGui() { }
    public override void Unload() { }


    public WoWUnit Target { get { return ObjectManager.Instance.Target; } }
    public LocalPlayer Local { get { return ObjectManager.Instance.Player; } }
    public List<WoWUnit> Attackers { get { return UnitInfo.Instance.NpcAttackers; } }
    public Spell Spell { get { return Spell.Instance; } }



    public override string Author { get { return "sensgates"; } }
    public override string Name { get { return "Druid Feral"; } }
    public override int Version { get { return 1; } }
    public override Enums.ClassId Class { get { return Enums.ClassId.Druid; } }
    public override bool SuppressBotMovement { get { return false; } }
    //public override float CombatDistance { get { return 5.0f; } }
}
