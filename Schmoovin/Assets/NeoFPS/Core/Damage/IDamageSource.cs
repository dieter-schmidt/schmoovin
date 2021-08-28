using UnityEngine;

namespace NeoFPS
{
	public interface IDamageSource
	{
		DamageFilter outDamageFilter 
		{
			get;
			set;
		}

		IController controller
		{
			get;
		}

		Transform damageSourceTransform
		{
			get;
		}

		string description
		{
			get;
		}
	}
}