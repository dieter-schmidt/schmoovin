using System;
using UnityEngine;

namespace NeoFPS
{
	[Flags]
	public enum DamageType : byte
	{
		None = 0,
		Default = 1,
		Fall = 2,
        Explosion = 4,
        Drowning = 8,
		All = 255
	}

	[Flags]
	public enum DamageTeamFilter : byte
	{
		None = 0,		// 00000000

		Team1 = 1,		// 00000001
		Team2 = 2,		// 00000010
		Team3 = 4,		// 00000100
		Team4 = 8,		// 00001000
		Team5 = 16,		// 00010000
		Team6 = 32,		// 00100000
		Team7 = 64,		// 01000000
		Team8 = 128,	// 10000000

		All = 255,		// 11111111
	}

    public static class DamageTeamFilterMasks
    {
        public const byte NotTeam1 = 254; // 11111110
        public const byte NotTeam2 = 253; // 11111101
        public const byte NotTeam3 = 251; // 11111011
        public const byte NotTeam4 = 247; // 11110111
        public const byte NotTeam5 = 239; // 11101111
        public const byte NotTeam6 = 223; // 11011111
        public const byte NotTeam7 = 191; // 10111111
        public const byte NotTeam8 = 127; // 01111111
    }

    [Serializable]
	public struct DamageFilter
	{
		public static readonly DamageFilter AllDamageAllTeams = new DamageFilter (DamageType.All, DamageTeamFilter.All);
		public static readonly DamageFilter DefaultAllTeams = new DamageFilter (DamageType.Default, DamageTeamFilter.All);

		public static readonly DamageFilter AllTeam1 = new DamageFilter (DamageType.All, DamageTeamFilter.Team1);
		public static readonly DamageFilter AllTeam2 = new DamageFilter (DamageType.All, DamageTeamFilter.Team2);
		public static readonly DamageFilter AllTeam3 = new DamageFilter (DamageType.All, DamageTeamFilter.Team3);
		public static readonly DamageFilter AllTeam4 = new DamageFilter (DamageType.All, DamageTeamFilter.Team4);
		public static readonly DamageFilter AllTeam5 = new DamageFilter (DamageType.All, DamageTeamFilter.Team5);
		public static readonly DamageFilter AllTeam6 = new DamageFilter (DamageType.All, DamageTeamFilter.Team6);
		public static readonly DamageFilter AllTeam7 = new DamageFilter (DamageType.All, DamageTeamFilter.Team7);
		public static readonly DamageFilter AllTeam8 = new DamageFilter (DamageType.All, DamageTeamFilter.Team8);
		public static readonly DamageFilter AllNotTeam1 = new DamageFilter (DamageType.All, 254);
		public static readonly DamageFilter AllNotTeam2 = new DamageFilter (DamageType.All, 253);
		public static readonly DamageFilter AllNotTeam3 = new DamageFilter (DamageType.All, 251);
		public static readonly DamageFilter AllNotTeam4 = new DamageFilter (DamageType.All, 247);
		public static readonly DamageFilter AllNotTeam5 = new DamageFilter (DamageType.All, 239);
		public static readonly DamageFilter AllNotTeam6 = new DamageFilter (DamageType.All, 223);
		public static readonly DamageFilter AllNotTeam7 = new DamageFilter (DamageType.All, 191);
		public static readonly DamageFilter AllNotTeam8 = new DamageFilter (DamageType.All, 127);

		public static readonly DamageFilter DefaultTeam1 = new DamageFilter (DamageType.Default, DamageTeamFilter.Team1);
		public static readonly DamageFilter DefaultTeam2 = new DamageFilter (DamageType.Default, DamageTeamFilter.Team2);
		public static readonly DamageFilter DefaultTeam3 = new DamageFilter (DamageType.Default, DamageTeamFilter.Team3);
		public static readonly DamageFilter DefaultTeam4 = new DamageFilter (DamageType.Default, DamageTeamFilter.Team4);
		public static readonly DamageFilter DefaultTeam5 = new DamageFilter (DamageType.Default, DamageTeamFilter.Team5);
		public static readonly DamageFilter DefaultTeam6 = new DamageFilter (DamageType.Default, DamageTeamFilter.Team6);
		public static readonly DamageFilter DefaultTeam7 = new DamageFilter (DamageType.Default, DamageTeamFilter.Team7);
		public static readonly DamageFilter DefaultTeam8 = new DamageFilter (DamageType.Default, DamageTeamFilter.Team8);
		public static readonly DamageFilter DefaultNotTeam1 = new DamageFilter (DamageType.Default, 254);
		public static readonly DamageFilter DefaultNotTeam2 = new DamageFilter (DamageType.Default, 253);
		public static readonly DamageFilter DefaultNotTeam3 = new DamageFilter (DamageType.Default, 251);
		public static readonly DamageFilter DefaultNotTeam4 = new DamageFilter (DamageType.Default, 239);
		public static readonly DamageFilter DefaultNotTeam5 = new DamageFilter (DamageType.Default, 239);
		public static readonly DamageFilter DefaultNotTeam6 = new DamageFilter (DamageType.Default, 223);
		public static readonly DamageFilter DefaultNotTeam7 = new DamageFilter (DamageType.Default, 191);
		public static readonly DamageFilter DefaultNotTeam8 = new DamageFilter (DamageType.Default, 127);

        [SerializeField]
        private ushort m_Value;

		public ushort value
		{
			get { return m_Value; }
		}

		public DamageFilter (ushort v)
		{
			m_Value = v;
		}

		public DamageFilter (DamageType type, byte teamFilter)
		{
			m_Value = (ushort)((uint)type + ((uint)teamFilter << 8));
		}

		public DamageFilter (DamageType type, DamageTeamFilter teamFilter)
		{
			m_Value = (ushort)((uint)type + ((uint)teamFilter << 8));
		}

		// Checks
		public static bool operator ==(DamageFilter x, DamageFilter y)
		{
			return (x.m_Value == y.m_Value);
		}

		public static bool operator !=(DamageFilter x, DamageFilter y)
		{
			return (x.m_Value != y.m_Value);
		}

		public override bool Equals (object obj)
		{
			if (obj is DamageFilter)
				return m_Value == ((DamageFilter)obj).m_Value;
			if (obj is ushort)
				return m_Value == (ushort)obj;
			return false;
		}

		public override int GetHashCode ()
		{
			return m_Value.GetHashCode ();
		}

		public bool CollidesWith (DamageFilter other, bool friendlyFire)
		{
			// Compare damage types
			uint x = (uint)m_Value & 0xFF;
			uint y = (uint)other.m_Value & 0xFF;
			if ((x & y) == 0)
				return false;

			// Ignore team filters with friendly fire
			if (friendlyFire)
				return true;
			
			// Compare team filters
			x = (uint)m_Value & 0xFF00;
			y = (uint)other.m_Value & 0xFF00;
			return (x & y) != 0;
		}

		// Implicit conversions
		public static implicit operator DamageFilter (ushort value)
		{
			return new DamageFilter (value);
		}

		public static implicit operator ushort (DamageFilter dam)
		{
			return dam.value;
		}

        // Getters
        public DamageTeamFilter GetTeamFilter ()
        {
            uint teamFilter = (uint)m_Value & 0xFF00;
            return (DamageTeamFilter)(teamFilter >> 8);
        }

        public DamageType GetDamageType ()
        {
            uint damageType = (uint)m_Value & 0x00FF;
            return (DamageType)damageType;
        }

        public bool IsDamageType (DamageType dt)
        {
            uint damageType = (uint)m_Value & 0x00FF;
            return damageType == (uint)dt;
        }

        public bool HasDamageType (DamageType dt)
        {
            uint damageType = (uint)m_Value & 0x00FF;
            return (damageType & (uint)dt) != 0;
        }

        public bool IsTeam(DamageTeamFilter team)
        {
            uint damageType = ((uint)m_Value & 0xFF00) >> 8;
            return damageType == (uint)team;
        }

        public bool HasTeam(DamageTeamFilter team)
        {
            uint damageType = ((uint)m_Value & 0xFF00) >> 8;
            return (damageType & (uint)team) != 0;
        }

        // Modifiers
        public void SetDamageType (DamageType type)
		{
			uint teamFilter = (uint)m_Value & 0xFF00;
			m_Value = (ushort)((uint)type + teamFilter);
		}

		public void SetTeamFilter (DamageTeamFilter teamFilter)
		{
			uint damageType = (uint)m_Value & 0xFF;
			m_Value = (ushort)(damageType + ((uint)teamFilter << 8));
		}

		public void AddTeam (byte team)
		{
			if (team > 8)
			{
				Debug.LogError ("DamageFilter cannot use a team value greater than 8");
				return;
			}

			if (team == 0)
			{
				Debug.LogError ("Attempting to add team 0 (must be 1-8)");
				return;
			}

			uint teamFilter = 1U << (7 + team);
			m_Value = (ushort)(m_Value | teamFilter);
		}

		public void RemoveTeam (byte team)
		{
			if (team > 8)
			{
				Debug.LogError ("DamageFilter cannot use a team value greater than 8");
				return;
			}

			if (team == 0)
			{
				Debug.LogError ("Attempting to remove team 0 (must be 1-8)");
				return;
			}

			uint teamFilter = ~(1U << (7 + team));
			m_Value = (ushort)(m_Value & teamFilter);
		}

		public static DamageFilter FromTypeAndTeam (DamageType type, byte team)
		{
			if (team > 8)
			{
				Debug.LogError ("DamageFilter cannot use a team value greater than 8");
				return new DamageFilter (type, DamageTeamFilter.None);
			}

			if (team == 0)
				return new DamageFilter (type, DamageTeamFilter.All);

			ushort result = (ushort)((uint)type + (1U << (7 + team)));
			return new DamageFilter (result);
		}

		public static DamageFilter FromTypeAndExcludedTeam (DamageType type, byte team)
		{
			if (team > 8)
			{
				Debug.LogError ("DamageFilter cannot use a team value greater than 8");
				return new DamageFilter (type, DamageTeamFilter.All);
			}

			if (team == 0)
				return new DamageFilter (type, DamageTeamFilter.All);

			uint result = (uint)type | 0x00FF;
			result |= ~(1U << (7 + team));
			return new DamageFilter ((ushort)result);
		}
	}
}