using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	public interface IBulletCasing
	{
		void Eject (Vector3 velocity, Vector3 angular, bool player);
	}
}