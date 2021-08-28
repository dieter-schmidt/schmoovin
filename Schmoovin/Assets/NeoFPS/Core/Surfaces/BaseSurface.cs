using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
	public abstract class BaseSurface : MonoBehaviour
	{
		public abstract FpsSurfaceMaterial GetSurface ();
        public abstract FpsSurfaceMaterial GetSurface (RaycastHit hit);
        public abstract FpsSurfaceMaterial GetSurface (ControllerColliderHit hit);
	}
}