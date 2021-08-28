//======================================================================================================
// WARNING: This file is auto-generated. Any manual changes might be lost.
//======================================================================================================

using System;
using UnityEngine;

namespace NeoFPS
{
	[Serializable]
	public struct FpsInputButton
	{
		public const int None = 0;
		public const int Menu = 1;
		public const int Back = 2;
		public const int Cancel = 3;
		public const int Forward = 4;
		public const int Backward = 5;
		public const int Left = 6;
		public const int Right = 7;
		public const int Jump = 8;
		public const int Sprint = 9;
		public const int SprintToggle = 10;
		public const int Crouch = 11;
		public const int CrouchToggle = 12;
		public const int LeanLeft = 13;
		public const int LeanRight = 14;
		public const int Use = 15;
		public const int PickUp = 16;
		public const int PrimaryFire = 17;
		public const int SecondaryFire = 18;
		public const int SwitchWeaponModes = 19;
		public const int Reload = 20;
		public const int Aim = 21;
		public const int AimToggle = 22;
		public const int Quickslot1 = 23;
		public const int Quickslot2 = 24;
		public const int Quickslot3 = 25;
		public const int Quickslot4 = 26;
		public const int Quickslot5 = 27;
		public const int Quickslot6 = 28;
		public const int Quickslot7 = 29;
		public const int Quickslot8 = 30;
		public const int Quickslot9 = 31;
		public const int Quickslot10 = 32;
		public const int PrevWeapon = 33;
		public const int NextWeapon = 34;
		public const int SwitchWeapon = 35;
		public const int DropWeapon = 36;
		public const int Inspect = 37;
		public const int Holster = 38;
		public const int QuickMenu = 39;
		public const int Stats = 40;
		public const int Inventory = 41;
		public const int Character = 42;
		public const int Crafting = 43;
		public const int Journal = 44;
		public const int Map = 45;
		public const int QuickSave = 46;
		public const int QuickLoad = 47;
		public const int Ability = 48;
		public const int Flashlight = 49;
		public const int OpticsLightPlus = 50;
		public const int OpticsLightMinus = 51;

		public const int count = 52;

		public static readonly string[] names = new string[]
		{
			"None",
			"Menu",
			"Back",
			"Cancel",
			"Forward",
			"Backward",
			"Left",
			"Right",
			"Jump",
			"Sprint",
			"SprintToggle",
			"Crouch",
			"CrouchToggle",
			"LeanLeft",
			"LeanRight",
			"Use",
			"PickUp",
			"PrimaryFire",
			"SecondaryFire",
			"SwitchWeaponModes",
			"Reload",
			"Aim",
			"AimToggle",
			"Quickslot1",
			"Quickslot2",
			"Quickslot3",
			"Quickslot4",
			"Quickslot5",
			"Quickslot6",
			"Quickslot7",
			"Quickslot8",
			"Quickslot9",
			"Quickslot10",
			"PrevWeapon",
			"NextWeapon",
			"SwitchWeapon",
			"DropWeapon",
			"Inspect",
			"Holster",
			"QuickMenu",
			"Stats",
			"Inventory",
			"Character",
			"Crafting",
			"Journal",
			"Map",
			"QuickSave",
			"QuickLoad",
			"Ability",
			"Flashlight",
			"OpticsLightPlus",
			"OpticsLightMinus"

		};

		[SerializeField] 
		private int m_Value;
		public int value
		{
			get { return m_Value; }
			set
			{
				int max = (int)(count - 1);
				if (value < 0)
					value = 0;
				if (value > max)
					value = 0; // Reset to default
				m_Value = value;
			}
		}

		private FpsInputButton (int v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (int v)
		{
			int cast = (int)v;
			return (cast >= 0) && (cast < count);
		}

		// Checks
		public static bool operator ==(FpsInputButton x, FpsInputButton y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsInputButton x, int y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsInputButton x, FpsInputButton y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsInputButton x, int y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsInputButton)
				return value == ((FpsInputButton)obj).value;
			if (obj is int)
				return value == (int)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsInputButton (int v)
		{
			int max = count - 1;
			if (v < 0)
				v = 0;
			if (v > max)
				v = 0; // Reset to default
			return new FpsInputButton (v);
		}

		public static implicit operator int (FpsInputButton dam)
		{
			return dam.value;
		}

		public override string ToString ()
		{
			return names [value];
		}

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
	}
}