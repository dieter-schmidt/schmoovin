using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
	public abstract class BaseHitFxBehaviour : MonoBehaviour
	{
        public abstract bool forceInitialise { get; }

        public abstract void OnActiveSceneChange();

		public abstract void Hit (GameObject hitObject, Vector3 position, Vector3 normal);
		public abstract void Hit (GameObject hitObject, Vector3 position, Vector3 normal, float size);
		public abstract void Hit (GameObject hitObject, Vector3 position, Vector3 normal, Vector3 ray, float size);
		public abstract void Hit (GameObject hitObject, Vector3 position, Vector3 normal, Vector3 ray, float size, bool decal);
	}
}