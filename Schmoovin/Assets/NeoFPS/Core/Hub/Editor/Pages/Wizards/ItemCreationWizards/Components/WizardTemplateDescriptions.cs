using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public static class WizardTemplateDescriptions
    {
        public static readonly TemplateDescription[] playerCharacterTemplates = new TemplateDescription[]
        {
            new TemplateDescription(
                "Basic",
                "This character features all of the basics without all the bells and whistles. It uses the standard health system, a quick-switch inventory, and does not use stamina."),
            new TemplateDescription(
                "Sci-Fi",
                "This character uses jetpack based movement and has energy shields on top of the standard health system. It uses a swappable inventory."),
            new TemplateDescription(
                "Soldier",
                "The soldier uses the demo-facility movement setup along with a full stamina system. For health, it uses inventory based armour to protect against wounds.")
        };

        public static readonly TemplateDescription[] firearmTemplates = new TemplateDescription[]
        {
            new TemplateDescription(
                "Basic Assault Rifle",
                "The basic assault rifle is set up for the quick-switch inventory system and uses procedural sprint animations, though it does not have any fatigue. The gun fires bursts of hitscan shots with simple damage. The muzzle flash is a basic game-object."),
            new TemplateDescription(
                "Advanced Assault Rifle",
                "The advanced assault rifle is also set up for the quick-switch inventory. It has a procedural sprinting system set up, along with aim fatigue. It uses overheat mechanics, including glow and heat haze. The shooter is projectile based and its projectiles are set to penetrate thin surfaces, with damage falling off based on speed. The muzzle flash uses realistic particle systems."),
            new TemplateDescription(
                "Anti-Material Rifle",
                "The anti-material rifle is a sniper weapon intended to destroy heavy targets. It uses a projectile that can penetrate surfaces up to 0.4m, and delivers an explosive payload after that. The muzzle effect is a realistic particle system, and the weapon uses an ammo pool attached to the weapon, instead of taking ammo from the wielder's inventory.")
        };

        public static readonly TemplateDescription[] meleeTemplates = new TemplateDescription[]
        {
            new TemplateDescription(
                "Baton",
                "The baton melee weapon template reproduces the sample baton setup, with procedural sprint animations and using an existing animator controller.")
        };

        public static readonly TemplateDescription[] thrownTemplates = new TemplateDescription[]
        {
            new TemplateDescription(
                "Grenade",
                "The grenade thrown weapon template reproduces the sample frag grenades. It uses proceduraly sprint animations, and using an existing animator controller.")
        };

        public static readonly TemplateDescription[] pickupTemplates = new TemplateDescription[]
        {
            new TemplateDescription(
                "Ammo Crate",
                "The ammo crate is a static multi-pickup object containing a range of ammo items. It must be interacted with to collect the items."),
            new TemplateDescription(
                "Armour (Contact)",
                "This pickup template is a contact based (trigger zone) pickup for the body armour inventory object. The pickup is static and will be destroyed when picked up."),
            new TemplateDescription(
                "Armour (Interactive)",
                "This pickup template is an interactive pickup for the body armour inventory object. The pickup has physics and can be knocked over and pushed around."),
            new TemplateDescription(
                "Firearm Pistol",
                "This is a pickup / firearm drop object for the sample quick-switch pistol. It must be interacted with to pick up, but it also has a contact based ammo pickup. You can set this as the drop object for a weapon, so that when the player character drops their weapon, this is the pickup that is spawned."),
            new TemplateDescription(
                "Health Pack (Contact)",
                "This pickup template is a contact based pickup that heals up to 25 points of health. The pickup is static and will be destroyed when picked up."),
            new TemplateDescription(
                "Health Pack (Interactive)",
                "This pickup template is an interactive pickup that heals up to 25 points of health. The pickup has physics and can be knocked over and pushed around. Using the item will consume it even if the character only needed a small amount of health."),
            new TemplateDescription(
                "Shield Booster (Contact)",
                "This pickup template is a contact based pickup that restores one shield bar to the character's shields. The pickup is static and will be destroyed when picked up."),
            new TemplateDescription(
                "Shield Booster (Interactive)",
                "This pickup template is an interactive pickup that restores one shield bar to the character's shields. The pickup has physics and can be knocked over and pushed around."),
            new TemplateDescription(
                "Wieldable Baton",
                "This is an item pickup for the demo baton melee weapon. It must be interacted with to pick up.")
        };

        public static readonly TemplateDescription[] interactiveTemplates = null;
    }
}
