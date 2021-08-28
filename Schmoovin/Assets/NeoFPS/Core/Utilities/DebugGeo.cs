using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
	/// <summary>
	/// A simple static class for drawing debug geo using Unity Debug.DrawLine ();
	/// Currently supports:
	/// - 3D Cross (very simple)
	/// - Cube
	/// - Sphere
	/// - 2D Arrow (x,z plane)
	/// - 3D Arrow
	/// </summary>
	public static class DebugGeo
	{
		private static Vector3[] m_MarkerPoints = null;
        private static Vector3[] m_Arrow2DPoints = null;
        private static Vector3[] m_Arrow3DPoints = null;
        private static Vector3[] m_SpherePoints = null;

        #region INITIALISATION

        private static void InitialiseMarkerPoints ()
		{
			if (m_MarkerPoints == null) {
				m_MarkerPoints = new Vector3[]
				{
					new Vector3 (-1f, -1f, 1f),
					new Vector3 (1f, -1f, 1f),
					new Vector3 (1f, -1f, -1f),
					new Vector3 (-1f, -1f, -1f),
					new Vector3 (-1f, 1f, 1f),
					new Vector3 (1f, 1f, 1f),
					new Vector3 (1f, 1f, -1f),
					new Vector3 (-1f, 1f, -1f)
				};
			}
		}

		private static void InitialiseArrow2DPoints ()
		{
			if (m_Arrow2DPoints == null) {
				m_Arrow2DPoints = new Vector3[]
				{
					Vector3.zero,
					new Vector3 (-0.1f, 0f, 0.1f),
					new Vector3 (-0.1f, 0f, 0.6f),
					new Vector3 (-0.4f, 0f, 0.6f),
					new Vector3 (0f, 0f, 1f),
					new Vector3 (0.4f, 0f, 0.6f),
					new Vector3 (0.1f, 0f, 0.6f),
					new Vector3 (0.1f, 0f, 0.1f)
				};
			}
		}

		private static void InitialiseArrow3DPoints ()
		{
			if (m_Arrow3DPoints == null) {
				m_Arrow3DPoints = new Vector3[]
				{
					new Vector3 (-0.1f, -0.1f, 0.1f),
					new Vector3 (-0.1f, 0.1f, 0.1f),
					new Vector3 (0.1f, 0.1f, 0.1f),
					new Vector3 (0.1f, -0.1f, 0.1f),

					new Vector3 (-0.1f, -0.1f, 0.6f),
					new Vector3 (-0.1f, 0.1f, 0.6f),
					new Vector3 (0.1f, 0.1f, 0.6f),
					new Vector3 (0.1f, -0.1f, 0.6f),

					new Vector3 (-0.3f, -0.3f, 0.6f),
					new Vector3 (-0.3f, 0.3f, 0.6f),
					new Vector3 (0.3f, 0.3f, 0.6f),
					new Vector3 (0.3f, -0.3f, 0.6f),

					new Vector3 (0f, 0f, 1f)
				};
			}
		}

		private static void InitialiseSpherePoints ()
		{
			if (m_SpherePoints == null) {
				// Based on 2 step geo-sphere (octa) in 3DS Max
				m_SpherePoints = new Vector3[]
				{
					new Vector3 (0f, 0f, -0.5f), 

					new Vector3 (0f, 0.354f, -0.354f),
					new Vector3 (-0.354f, 0f, -0.354f),
					new Vector3 (0f, -0.354f, -0.354f),
					new Vector3 (0.354f, 0f, -0.354f),

					new Vector3 (0f, 0.5f, 0f),
					new Vector3 (-0.354f, 0.354f, 0f),
					new Vector3 (-0.5f, 0f, 0f),
					new Vector3 (-0.354f, -0.354f, 0f),
					new Vector3 (0f, -0.5f, 0f),
					new Vector3 (0.354f, -0.354f, 0f),
					new Vector3 (0.5f, 0f, 0f),
					new Vector3 (0.354f, 0.354f, 0f),

					new Vector3 (0f, 0.354f, 0.354f),
					new Vector3 (-0.354f, 0f, 0.354f),
					new Vector3 (0f, -0.354f, 0.354f),
					new Vector3 (0.354f, -0f, 0.354f),

					new Vector3 (0f, 0f, 0.5f)
				};
			}
		}

		#endregion

		#region DRAW FUNCTIONS

		public static void DrawCrossMarker (Vector3 position, float size, Color colour)
		{
			// Initialise corner points
			InitialiseMarkerPoints ();

			// Get vector multiplier
			float multiplier = size * 0.5f;

			// Draw lines
			Debug.DrawLine (position + (m_MarkerPoints[0] * multiplier), position + (m_MarkerPoints[6] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[1] * multiplier), position + (m_MarkerPoints[7] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[2] * multiplier), position + (m_MarkerPoints[4] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[3] * multiplier), position + (m_MarkerPoints[5] * multiplier), colour);
		}

		public static void DrawCubeMarker (Vector3 position, float size, Color colour)
		{
			// Initialise corner points
			InitialiseMarkerPoints ();

			// Get vector muiltiplier
			float multiplier = size * 0.5f;

			// Draw bottom square
			Debug.DrawLine (position + (m_MarkerPoints[0] * multiplier), position + (m_MarkerPoints[1] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[1] * multiplier), position + (m_MarkerPoints[2] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[2] * multiplier), position + (m_MarkerPoints[3] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[3] * multiplier), position + (m_MarkerPoints[0] * multiplier), colour);

			// Draw top square
			Debug.DrawLine (position + (m_MarkerPoints[4] * multiplier), position + (m_MarkerPoints[5] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[5] * multiplier), position + (m_MarkerPoints[6] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[6] * multiplier), position + (m_MarkerPoints[7] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[7] * multiplier), position + (m_MarkerPoints[4] * multiplier), colour);

			// Draw vertical struts
			Debug.DrawLine (position + (m_MarkerPoints[0] * multiplier), position + (m_MarkerPoints[4] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[1] * multiplier), position + (m_MarkerPoints[5] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[2] * multiplier), position + (m_MarkerPoints[6] * multiplier), colour);
			Debug.DrawLine (position + (m_MarkerPoints[3] * multiplier), position + (m_MarkerPoints[7] * multiplier), colour);
		}

		public static void DrawArrowMarker2D (Vector3 position, float angle, float size, Color colour)
		{
			// Initialise corner points
			InitialiseArrow2DPoints ();

			// Get rotation quaternion
			Quaternion rotation = Quaternion.Euler (0f, angle, 0f);

			// Create new array of transformed verts to reduce calculations
			Vector3[] transformed = new Vector3[m_Arrow2DPoints.Length];
			transformed[0] = position;
			for (int i = 1; i < m_Arrow2DPoints.Length; ++i)
				transformed[i] = position + (rotation * m_Arrow2DPoints[i] * size);
			
			// Draw lines
			Debug.DrawLine (transformed[0], transformed[1], colour);
			Debug.DrawLine (transformed[1], transformed[2], colour);
			Debug.DrawLine (transformed[2], transformed[3], colour);
			Debug.DrawLine (transformed[3], transformed[4], colour);
			Debug.DrawLine (transformed[4], transformed[5], colour);
			Debug.DrawLine (transformed[5], transformed[6], colour);
			Debug.DrawLine (transformed[6], transformed[7], colour);
			Debug.DrawLine (transformed[7], transformed[0], colour);
		}

		public static void DrawArrowMarker3D (Vector3 position, Quaternion rotation, float size, Color colour)
		{
			// Initialise corner points
			InitialiseArrow3DPoints ();

			// Create new array of transformed verts to reduce calculations
			Vector3[] transformed = new Vector3[m_Arrow3DPoints.Length];
			for (int i = 0; i < m_Arrow3DPoints.Length; ++i)
				transformed[i] = position + (rotation * m_Arrow3DPoints[i] * size);

			// Draw start pyramid
			Debug.DrawLine (position, transformed[0], colour);
			Debug.DrawLine (position, transformed[1], colour);
			Debug.DrawLine (position, transformed[2], colour);
			Debug.DrawLine (position, transformed[3], colour);

			// Draw start square
			Debug.DrawLine (transformed[0], transformed[1], colour);
			Debug.DrawLine (transformed[1], transformed[2], colour);
			Debug.DrawLine (transformed[2], transformed[3], colour);
			Debug.DrawLine (transformed[3], transformed[0], colour);

			// Draw connection square
			Debug.DrawLine (transformed[4], transformed[5], colour);
			Debug.DrawLine (transformed[5], transformed[6], colour);
			Debug.DrawLine (transformed[6], transformed[7], colour);
			Debug.DrawLine (transformed[7], transformed[4], colour);

			// Draw struts
			Debug.DrawLine (transformed[0], transformed[4], colour);
			Debug.DrawLine (transformed[1], transformed[5], colour);
			Debug.DrawLine (transformed[2], transformed[6], colour);
			Debug.DrawLine (transformed[3], transformed[7], colour);

			// Draw connection spurs
			Debug.DrawLine (transformed[4], transformed[8], colour);
			Debug.DrawLine (transformed[5], transformed[9], colour);
			Debug.DrawLine (transformed[6], transformed[10], colour);
			Debug.DrawLine (transformed[7], transformed[11], colour);

			// Draw arrow square
			Debug.DrawLine (transformed[8], transformed[9], colour);
			Debug.DrawLine (transformed[9], transformed[10], colour);
			Debug.DrawLine (transformed[10], transformed[11], colour);
			Debug.DrawLine (transformed[11], transformed[8], colour);

			// Draw arrow pyramid
			Debug.DrawLine (transformed[8], transformed[12], colour);
			Debug.DrawLine (transformed[9], transformed[12], colour);
			Debug.DrawLine (transformed[10], transformed[12], colour);
			Debug.DrawLine (transformed[11], transformed[12], colour);
		}

		public static void DrawSphereMarker (Vector3 position, float radius, Color colour)
		{
			// Initialise corner points
			InitialiseSpherePoints ();

			// Create new array of transformed verts to reduce calculations
			Vector3[] transformed = new Vector3[m_SpherePoints.Length];
			for (int i = 0; i < m_SpherePoints.Length; ++i)
				transformed[i] = position + (m_SpherePoints[i] * 2 * radius);

			// Draw bottom ring
			Debug.DrawLine (transformed[1], transformed[2], colour);
			Debug.DrawLine (transformed[2], transformed[3], colour);
			Debug.DrawLine (transformed[3], transformed[4], colour);
			Debug.DrawLine (transformed[4], transformed[1], colour);

			// Draw center ring
			Debug.DrawLine (transformed[5], transformed[6], colour);
			Debug.DrawLine (transformed[6], transformed[7], colour);
			Debug.DrawLine (transformed[7], transformed[8], colour);
			Debug.DrawLine (transformed[8], transformed[9], colour);
			Debug.DrawLine (transformed[9], transformed[10], colour);
			Debug.DrawLine (transformed[10], transformed[11], colour);
			Debug.DrawLine (transformed[11], transformed[12], colour);
			Debug.DrawLine (transformed[12], transformed[5], colour);

			// Draw top ring
			Debug.DrawLine (transformed[13], transformed[14], colour);
			Debug.DrawLine (transformed[14], transformed[15], colour);
			Debug.DrawLine (transformed[15], transformed[16], colour);
			Debug.DrawLine (transformed[16], transformed[13], colour);

			// Draw bottom pyramid
			Debug.DrawLine (transformed[0], transformed[1], colour);
			Debug.DrawLine (transformed[0], transformed[2], colour);
			Debug.DrawLine (transformed[0], transformed[3], colour);
			Debug.DrawLine (transformed[0], transformed[4], colour);

			// Draw top pyramid
			Debug.DrawLine (transformed[17], transformed[13], colour);
			Debug.DrawLine (transformed[17], transformed[14], colour);
			Debug.DrawLine (transformed[17], transformed[15], colour);
			Debug.DrawLine (transformed[17], transformed[16], colour);

			// Connect bottom ring to center
			Debug.DrawLine (transformed[1], transformed[5], colour);
			Debug.DrawLine (transformed[1], transformed[6], colour);
			Debug.DrawLine (transformed[1], transformed[12], colour);

			Debug.DrawLine (transformed[2], transformed[6], colour);
			Debug.DrawLine (transformed[2], transformed[7], colour);
			Debug.DrawLine (transformed[2], transformed[8], colour);

			Debug.DrawLine (transformed[3], transformed[8], colour);
			Debug.DrawLine (transformed[3], transformed[9], colour);
			Debug.DrawLine (transformed[3], transformed[10], colour);

			Debug.DrawLine (transformed[4], transformed[10], colour);
			Debug.DrawLine (transformed[4], transformed[11], colour);
			Debug.DrawLine (transformed[4], transformed[12], colour);

			// Connect center ring to top
			Debug.DrawLine (transformed[13], transformed[5], colour);
			Debug.DrawLine (transformed[13], transformed[6], colour);
			Debug.DrawLine (transformed[13], transformed[12], colour);

			Debug.DrawLine (transformed[14], transformed[6], colour);
			Debug.DrawLine (transformed[14], transformed[7], colour);
			Debug.DrawLine (transformed[14], transformed[8], colour);

			Debug.DrawLine (transformed[15], transformed[8], colour);
			Debug.DrawLine (transformed[15], transformed[9], colour);
			Debug.DrawLine (transformed[15], transformed[10], colour);

			Debug.DrawLine (transformed[16], transformed[10], colour);
			Debug.DrawLine (transformed[16], transformed[11], colour);
			Debug.DrawLine (transformed[16], transformed[12], colour);
		}

		#endregion

		#region ALTERNATIVES

		// Alternate crosses
		public static void DrawCrossMarker (Vector3 position, Color colour) {
			DrawCrossMarker (position, 1f, colour);
		}
		public static void DrawCrossMarker (Vector3 position, float size) {
			DrawCrossMarker (position, size, Color.white);
		}
		public static void DrawCrossMarker (Vector3 position) {
			DrawCrossMarker (position, 1f, Color.white);
		}

		// Alternate cubes
		public static void DrawCubeMarker (Vector3 position, Color colour) {
			DrawCubeMarker (position, 1f, colour);
		}
		public static void DrawCubeMarker (Vector3 position, float size) {
			DrawCubeMarker (position, size, Color.white);
		}
		public static void DrawCubeMarker (Vector3 position) {
			DrawCubeMarker (position, 1f, Color.white);
		}

		// Alternate spheres
		public static void DrawSphereMarker (Vector3 position, Color colour) {
			DrawSphereMarker (position, 1f, colour);
		}
		public static void DrawSphereMarker (Vector3 position, float size) {
			DrawSphereMarker (position, size, Color.white);
		}
		public static void DrawSphereMarker (Vector3 position) {
			DrawSphereMarker (position, 1f, Color.white);
		}

		// Alternate 2D arrow
		public static void DrawArrowMarker2D (Vector3 position, float angle, float size) {
			DrawArrowMarker2D (position, angle, size, Color.white);
		}
		public static void DrawArrowMarker2D (Vector3 position, Vector3 direction, float size, Color colour) {
			float angle = Quaternion.FromToRotation (Vector3.forward, direction).eulerAngles.y;
			DrawArrowMarker2D (position, angle, size, colour);
		}
		public static void DrawArrowMarker2D (Vector3 position, Vector3 direction, float size) {
			float angle = Quaternion.FromToRotation (Vector3.forward, direction).eulerAngles.y;
			DrawArrowMarker2D (position, angle, size, Color.white);
		}
		public static void DrawArrowMarker2D (Vector3 position, Vector2 direction, float size, Color colour) {
			float angle = Quaternion.FromToRotation (Vector3.forward, new Vector3 (direction.x, 0f, direction.y)).eulerAngles.y;
			DrawArrowMarker2D (position, angle, size, colour);
		}
		public static void DrawArrowMarker2D (Vector3 position, Vector2 direction, float size) {
			float angle = Quaternion.FromToRotation (Vector3.forward, new Vector3 (direction.x, 0f, direction.y)).eulerAngles.y;
			DrawArrowMarker2D (position, angle, size, Color.white);
		}

		// Alternate 3D arrow
		public static void DrawArrowMarker3D (Vector3 position, Quaternion rotation, float size) {
			DrawArrowMarker3D (position, rotation, size, Color.white);
		}
		public static void DrawArrowMarker3D (Vector3 position, float rx, float ry, float rz, float size, Color colour) {
			DrawArrowMarker3D (position, Quaternion.Euler (rx, ry, rz), size, colour);
		}
		public static void DrawArrowMarker3D (Vector3 position, float rx, float ry, float rz, float size) {
			DrawArrowMarker3D (position, Quaternion.Euler (rx, ry, rz), size, Color.white);
		}
		public static void DrawArrowMarker3D (Vector3 position, Vector3 direction, float size, Color colour) {
			DrawArrowMarker3D (position, Quaternion.FromToRotation  (Vector3.forward, direction), size, colour);
		}
		public static void DrawArrowMarker3D (Vector3 position, Vector3 direction, float size) {
			DrawArrowMarker3D (position, Quaternion.FromToRotation  (Vector3.forward, direction), size, Color.white);
		}

		#endregion
	}
}