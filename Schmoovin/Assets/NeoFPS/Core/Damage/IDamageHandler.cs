using UnityEngine;

namespace NeoFPS
{
	public interface IDamageHandler
	{
		DamageFilter inDamageFilter 
		{
			get;
			set;
		}

		DamageResult AddDamage(float damage);
        DamageResult AddDamage(float damage, RaycastHit hit);
        DamageResult AddDamage(float damage, IDamageSource source);
        DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source);
    }

	public enum DamageResult
	{
		Standard,
		Critical,
		Ignored,
        Blocked
	}
}