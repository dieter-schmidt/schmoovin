//======================================================================================================
// WARNING: This file is auto-generated.
// Any manual changes will be lost.
// Use the constant generator system instead
//======================================================================================================

using System;
using UnityEngine;

namespace NeoFPS
{
	[Serializable]
	public struct FpsInputAxis
	{
		public const int MouseX = 0;
		public const int MouseY = 1;
		public const int MouseScroll = 2;
		public const int MoveX = 3;
		public const int MoveY = 4;
		public const int LookX = 5;
		public const int LookY = 6;
		public const int GyroX = 7;
		public const int GyroY = 8;

		public const int count = 9;

		public static readonly string[] names = new string[]
		{
			"MouseX",
			"MouseY",
			"MouseScroll",
			"MoveX",
			"MoveY",
			"LookX",
			"LookY",
			"GyroX",
			"GyroY"
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

		private FpsInputAxis (int v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (int v)
		{
			int cast = (int)v;
			return (cast >= 0) && (cast < count);
		}

		// Checks
		public static bool operator ==(FpsInputAxis x, FpsInputAxis y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsInputAxis x, int y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsInputAxis x, FpsInputAxis y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsInputAxis x, int y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsInputAxis)
				return value == ((FpsInputAxis)obj).value;
			if (obj is int)
				return value == (int)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsInputAxis (int v)
		{
			int max = count - 1;
			if (v < 0)
				v = 0;
			if (v > max)
				v = 0; // Reset to default
			return new FpsInputAxis (v);
		}

		public static implicit operator int (FpsInputAxis dam)
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