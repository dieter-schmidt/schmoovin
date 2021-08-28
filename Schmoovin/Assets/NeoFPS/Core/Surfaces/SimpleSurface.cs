using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/surfacesref-mb-simplesurface.html")]
    public class SimpleSurface : BaseSurface
	{
		[SerializeField, Tooltip("The surface material ID.")]
		private FpsSurfaceMaterial m_Surface = FpsSurfaceMaterial.Default;

        public override FpsSurfaceMaterial GetSurface ()
		{
			return m_Surface;
		}
		public override FpsSurfaceMaterial GetSurface (RaycastHit hit)
		{
			return m_Surface;
        }
        public override FpsSurfaceMaterial GetSurface (ControllerColliderHit hit)
        {
            return m_Surface;
        }
	}
}