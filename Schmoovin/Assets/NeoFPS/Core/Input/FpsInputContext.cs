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
	public struct FpsInputContext
	{
		public const int None = 0;
		public const int Character = 1;
		public const int Menu = 2;
		public const int Cutscene = 3;

		public const int count = 4;

		public static readonly string[] names = new string[]
		{
			"None",
			"Character",
			"Menu",
			"Cutscene"
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

		private FpsInputContext (int v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (int v)
		{
			int cast = (int)v;
			return (cast >= 0) && (cast < count);
		}

		// Checks
		public static bool operator ==(FpsInputContext x, FpsInputContext y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsInputContext x, int y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsInputContext x, FpsInputContext y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsInputContext x, int y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsInputContext)
				return value == ((FpsInputContext)obj).value;
			if (obj is int)
				return value == (int)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsInputContext (int v)
		{
			int max = count - 1;
			if (v < 0)
				v = 0;
			if (v > max)
				v = 0; // Reset to default
			return new FpsInputContext (v);
		}

		public static implicit operator int (FpsInputContext dam)
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