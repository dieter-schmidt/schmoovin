using UnityEngine;

namespace NeoFPS
{
	public interface IImpactHandler
	{
		void HandlePointImpact (Vector3 position, Vector3 force);
	}
}
