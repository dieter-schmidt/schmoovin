using UnityEngine;
using System;

namespace NeoFPS
{
	public interface IHealthManager
	{
        event HealthDelegates.OnIsAliveChanged onIsAliveChanged;
        event HealthDelegates.OnHealthChanged onHealthChanged;
        event HealthDelegates.OnHealthMaxChanged onHealthMaxChanged;

        bool isAlive { get; }
		float health { get; set; }
		float healthMax { get; set; }
		float normalisedHealth { get; set; }

		void AddDamage (float damage);
		void AddDamage (float damage, bool critical);
		void AddDamage (float damage, IDamageSource source);
		void AddDamage(float damage, bool critical, RaycastHit hit);
		void AddDamage (float damage, bool critical, IDamageSource source);
		void AddDamage(float damage, bool critical, IDamageSource source, RaycastHit hit);
		void AddHealth (float h);
		void AddHealth (float h, IDamageSource source);
	}

    public static class HealthDelegates
    {
        public delegate void OnIsAliveChanged(bool alive);
        public delegate void OnHealthChanged(float from, float to, bool critical, IDamageSource source);
        public delegate void OnHealthMaxChanged(float from, float to);
    }
}