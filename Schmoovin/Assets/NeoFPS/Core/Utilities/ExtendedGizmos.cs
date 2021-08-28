using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
	/// <summary>
	/// A simple static class for drawing debug geo using Unity Gizmos;
	/// Currently supports:
	/// - Sphere
	/// - 2D Arrow (x,z plane)
	/// - 3D Arrow
	/// </summary>
	public static class ExtendedGizmos
	{
		private static Vector3[] m_Arrow2DPoints = null;
        private static Vector3[] m_Arrow3DPoints = null;
        private static Vector3[] m_Corners = null;

        #region INITIALISATION

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

		#endregion

		#region DRAW FUNCTIONS

		public static void DrawArrowMarker2D (Vector3 position, float angle, float size, Color colour)
		{
			// Initialise corner points
			InitialiseArrow2DPoints ();

			// Set colour
			Color prevColour = Gizmos.color;
			Gizmos.color = colour;

			// Get rotation quaternion
			Quaternion rotation = Quaternion.Euler (0f, angle, 0f);

			// Create new array of transformed verts to reduce calculations
			Vector3[] transformed = new Vector3[m_Arrow2DPoints.Length];
			transformed[0] = position;
			for (int i = 1; i < m_Arrow2DPoints.Length; ++i)
				transformed[i] = position + (rotation * m_Arrow2DPoints[i] * size);
			
			// Draw lines
			Gizmos.DrawLine (transformed[0], transformed[1]);
			Gizmos.DrawLine (transformed[1], transformed[2]);
			Gizmos.DrawLine (transformed[2], transformed[3]);
			Gizmos.DrawLine (transformed[3], transformed[4]);
			Gizmos.DrawLine (transformed[4], transformed[5]);
			Gizmos.DrawLine (transformed[5], transformed[6]);
			Gizmos.DrawLine (transformed[6], transformed[7]);
			Gizmos.DrawLine (transformed[7], transformed[0]);

			// Reset colour
			Gizmos.color = prevColour;
        }

        public static void DrawArrowMarkerFlat(Vector3 position, Quaternion rotation, float angle, float size, Color colour)
        {
            // Initialise corner points
            InitialiseArrow2DPoints();

            // Set colour
            Color prevColour = Gizmos.color;
            Gizmos.color = colour;

            // Get rotation quaternion
            rotation = rotation * Quaternion.Euler(0f, angle, 0f);

            // Create new array of transformed verts to reduce calculations
            Vector3[] transformed = new Vector3[m_Arrow2DPoints.Length];
            transformed[0] = position;
            for (int i = 1; i < m_Arrow2DPoints.Length; ++i)
                transformed[i] = position + (rotation * m_Arrow2DPoints[i] * size);

            // Draw lines
            Gizmos.DrawLine(transformed[0], transformed[1]);
            Gizmos.DrawLine(transformed[1], transformed[2]);
            Gizmos.DrawLine(transformed[2], transformed[3]);
            Gizmos.DrawLine(transformed[3], transformed[4]);
            Gizmos.DrawLine(transformed[4], transformed[5]);
            Gizmos.DrawLine(transformed[5], transformed[6]);
            Gizmos.DrawLine(transformed[6], transformed[7]);
            Gizmos.DrawLine(transformed[7], transformed[0]);

            // Reset colour
            Gizmos.color = prevColour;
        }

        public static void DrawArrowMarker3D (Vector3 position, Quaternion rotation, float size, Color colour)
		{
			// Initialise corner points
			InitialiseArrow3DPoints ();

			// Set colour
			Color prevColour = Gizmos.color;
			Gizmos.color = colour;

			// Create new array of transformed verts to reduce calculations
			Vector3[] transformed = new Vector3[m_Arrow3DPoints.Length];
			for (int i = 0; i < m_Arrow3DPoints.Length; ++i)
				transformed[i] = position + (rotation * m_Arrow3DPoints[i] * size);

			// Draw start pyramid
			Gizmos.DrawLine (position, transformed[0]);
			Gizmos.DrawLine (position, transformed[1]);
			Gizmos.DrawLine (position, transformed[2]);
			Gizmos.DrawLine (position, transformed[3]);

			// Draw start square
			Gizmos.DrawLine (transformed[0], transformed[1]);
			Gizmos.DrawLine (transformed[1], transformed[2]);
			Gizmos.DrawLine (transformed[2], transformed[3]);
			Gizmos.DrawLine (transformed[3], transformed[0]);

			// Draw connection square
			Gizmos.DrawLine (transformed[4], transformed[5]);
			Gizmos.DrawLine (transformed[5], transformed[6]);
			Gizmos.DrawLine (transformed[6], transformed[7]);
			Gizmos.DrawLine (transformed[7], transformed[4]);

			// Draw struts
			Gizmos.DrawLine (transformed[0], transformed[4]);
			Gizmos.DrawLine (transformed[1], transformed[5]);
			Gizmos.DrawLine (transformed[2], transformed[6]);
			Gizmos.DrawLine (transformed[3], transformed[7]);

			// Draw connection spurs
			Gizmos.DrawLine (transformed[4], transformed[8]);
			Gizmos.DrawLine (transformed[5], transformed[9]);
			Gizmos.DrawLine (transformed[6], transformed[10]);
			Gizmos.DrawLine (transformed[7], transformed[11]);

			// Draw arrow square
			Gizmos.DrawLine (transformed[8], transformed[9]);
			Gizmos.DrawLine (transformed[9], transformed[10]);
			Gizmos.DrawLine (transformed[10], transformed[11]);
			Gizmos.DrawLine (transformed[11], transformed[8]);

			// Draw arrow pyramid
			Gizmos.DrawLine (transformed[8], transformed[12]);
			Gizmos.DrawLine (transformed[9], transformed[12]);
			Gizmos.DrawLine (transformed[10], transformed[12]);
			Gizmos.DrawLine (transformed[11], transformed[12]);

			// Reset colour
			Gizmos.color = prevColour;
		}

        public static void DrawCircleMarker2D(Vector3 position, float radius, Color colour)
        {
            // Set colour
            Color prevColour = Gizmos.color;
            Gizmos.color = colour;

            int segments = 32;
            float increment = 2f * Mathf.PI / segments;
            for (int i = 0; i < segments; ++i)
            {
                int next = i + 1;
                if (next == segments)
                    next = 0;

                Vector3 p1 = new Vector3(Mathf.Sin(increment * i), 0f, Mathf.Cos(increment * i));
                Vector3 p2 = new Vector3(Mathf.Sin(increment * next), 0f, Mathf.Cos(increment * next));

                Gizmos.DrawLine(position + p1 * radius, position + p2 * radius);
            }

            // Reset colour
            Gizmos.color = prevColour;
        }

        public static void DrawSphereMarker (Vector3 position, float radius, Color colour)
		{
			// Set colour
			Color prevColour = Gizmos.color;
			Gizmos.color = colour;

			// Draw the sphere
			Gizmos.DrawWireSphere (position, radius);

			// Reset colour
			Gizmos.color = prevColour;
		}

		public static void DrawCapsuleMarker (float radius, float height, Vector3 center, Color colour)
		{
			// Set colour
			Color prevColour = Gizmos.color;
			Gizmos.color = colour;

			// Calculate positions
			Vector3 topSphere = center + new Vector3 (0f, (height * 0.5f) - radius, 0f);
			Vector3 bottomSphere = center - new Vector3 (0f, (height * 0.5f) - radius, 0f);

            if (m_Corners == null)
                m_Corners = new Vector3[8];
            m_Corners[0] = topSphere + new Vector3(radius, 0f, 0f);
            m_Corners[1] = bottomSphere + new Vector3(radius, 0f, 0f);
            m_Corners[2] = topSphere + new Vector3(0f, radius, 0f);
            m_Corners[3] = bottomSphere + new Vector3(0f, radius, 0f);
            m_Corners[4] = topSphere + new Vector3(-radius, 0f, 0f);
            m_Corners[5] = bottomSphere + new Vector3(-radius, 0f, 0f);
            m_Corners[6] = topSphere + new Vector3(0f, -radius, 0f);
            m_Corners[7] = bottomSphere + new Vector3(0f, -radius, 0f);

			// Draw the spheres
			Gizmos.DrawWireSphere (topSphere, radius);
			Gizmos.DrawWireSphere (bottomSphere, radius);

			// Draw the struts
			Gizmos.DrawLine (m_Corners[0], m_Corners[1]);
			Gizmos.DrawLine (m_Corners[2], m_Corners[3]);
			Gizmos.DrawLine (m_Corners[4], m_Corners[5]);
			Gizmos.DrawLine (m_Corners[6], m_Corners[7]);

			// Reset colour
			Gizmos.color = prevColour;
		}

        public static void DrawCuboidMarker (Vector3 position, float width, float height, Quaternion rotation, Color colour)
        {
            // Set colour
            Color prevColour = Gizmos.color;
            Gizmos.color = colour;

            float halfWidth = width * 0.5f;
            if (m_Corners == null)
                m_Corners = new Vector3[8];
            m_Corners[0] = position + rotation * new Vector3(halfWidth, 0f, halfWidth);
            m_Corners[1] = position + rotation * new Vector3(halfWidth, 0f, -halfWidth);
            m_Corners[2] = position + rotation * new Vector3(-halfWidth, 0f, -halfWidth);
            m_Corners[3] = position + rotation * new Vector3(-halfWidth, 0f, halfWidth);
            m_Corners[4] = position + rotation * new Vector3(halfWidth, height, halfWidth);
            m_Corners[5] = position + rotation * new Vector3(halfWidth, height, -halfWidth);
            m_Corners[6] = position + rotation * new Vector3(-halfWidth, height, -halfWidth);
            m_Corners[7] = position + rotation * new Vector3(-halfWidth, height, halfWidth);

            Gizmos.DrawLine(m_Corners[0], m_Corners[1]);
            Gizmos.DrawLine(m_Corners[1], m_Corners[2]);
            Gizmos.DrawLine(m_Corners[2], m_Corners[3]);
            Gizmos.DrawLine(m_Corners[3], m_Corners[0]);
            Gizmos.DrawLine(m_Corners[4], m_Corners[5]);
            Gizmos.DrawLine(m_Corners[5], m_Corners[6]);
            Gizmos.DrawLine(m_Corners[6], m_Corners[7]);
            Gizmos.DrawLine(m_Corners[7], m_Corners[4]);
            Gizmos.DrawLine(m_Corners[0], m_Corners[4]);
            Gizmos.DrawLine(m_Corners[1], m_Corners[5]);
            Gizmos.DrawLine(m_Corners[2], m_Corners[6]);
            Gizmos.DrawLine(m_Corners[3], m_Corners[7]);

            // Reset colour
            Gizmos.color = prevColour;
        }


        public static void DrawBoxMarker(Vector3 position, Quaternion rotation, Vector3 size, Color colour)
        {
            // Set colour
            Color prevColour = Gizmos.color;
            Gizmos.color = colour;

            Vector3 halfSize = size * 0.5f;
            if (m_Corners == null)
                m_Corners = new Vector3[8];
            m_Corners[0] = position + rotation * new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            m_Corners[1] = position + rotation * new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            m_Corners[2] = position + rotation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            m_Corners[3] = position + rotation * new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            m_Corners[4] = position + rotation * new Vector3(halfSize.x, halfSize.y, halfSize.z);
            m_Corners[5] = position + rotation * new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            m_Corners[6] = position + rotation * new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            m_Corners[7] = position + rotation * new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            Gizmos.DrawLine(m_Corners[0], m_Corners[1]);
            Gizmos.DrawLine(m_Corners[1], m_Corners[2]);
            Gizmos.DrawLine(m_Corners[2], m_Corners[3]);
            Gizmos.DrawLine(m_Corners[3], m_Corners[0]);
            Gizmos.DrawLine(m_Corners[4], m_Corners[5]);
            Gizmos.DrawLine(m_Corners[5], m_Corners[6]);
            Gizmos.DrawLine(m_Corners[6], m_Corners[7]);
            Gizmos.DrawLine(m_Corners[7], m_Corners[4]);
            Gizmos.DrawLine(m_Corners[0], m_Corners[4]);
            Gizmos.DrawLine(m_Corners[1], m_Corners[5]);
            Gizmos.DrawLine(m_Corners[2], m_Corners[6]);
            Gizmos.DrawLine(m_Corners[3], m_Corners[7]);

            // Reset colour
            Gizmos.color = prevColour;
        }

        public static void DrawBoxMarker2D(Vector3 position, Quaternion rotation, Vector2 size, Color colour)
        {
            // Set colour
            Color prevColour = Gizmos.color;
            Gizmos.color = colour;

            Vector2 halfSize = size * 0.5f;
            if (m_Corners == null)
                m_Corners = new Vector3[4];
            m_Corners[0] = position + rotation * new Vector3(halfSize.x, 0f, halfSize.y);
            m_Corners[1] = position + rotation * new Vector3(halfSize.x, 0f, -halfSize.y);
            m_Corners[2] = position + rotation * new Vector3(-halfSize.x, 0f, -halfSize.y);
            m_Corners[3] = position + rotation * new Vector3(-halfSize.x, 0f, halfSize.y);

            Gizmos.DrawLine(m_Corners[0], m_Corners[1]);
            Gizmos.DrawLine(m_Corners[1], m_Corners[2]);
            Gizmos.DrawLine(m_Corners[2], m_Corners[3]);
            Gizmos.DrawLine(m_Corners[3], m_Corners[0]);

            // Reset colour
            Gizmos.color = prevColour;
        }

        public static void DrawRay(Vector3 position, Vector3 direction, float length, Color colour)
        {
            // Set colour
            Color prevColour = Gizmos.color;
            Gizmos.color = colour;

            Gizmos.DrawLine(position, position + direction.normalized * length);

            // Reset colour
            Gizmos.color = prevColour;
        }

        #endregion

        #region ALTERNATIVES

        // Alternate spheres
        public static void DrawSphereMarker (Vector3 position, Color colour) {
			DrawSphereMarker (position, 1f, colour);
		}

		// Alternate 2D arrow
		public static void DrawArrowMarker2D (Vector3 position, Vector3 direction, float size, Color colour) {
            float angle = Quaternion.FromToRotation (Vector3.forward, direction.normalized).eulerAngles.y;
			DrawArrowMarker2D (position, angle, size, colour);
		}
		public static void DrawArrowMarker2D (Vector3 position, Vector2 direction, float size, Color colour) {
			float angle = Quaternion.FromToRotation (Vector3.forward, new Vector3 (direction.x, 0f, direction.y)).eulerAngles.y;
			DrawArrowMarker2D (position, angle, size, colour);
        }

        // Alternate 3D arrow
        public static void DrawArrowMarker3D (Vector3 position, float rx, float ry, float rz, float size, Color colour) {
			DrawArrowMarker3D (position, Quaternion.Euler (rx, ry, rz), size, colour);
		}
		public static void DrawArrowMarker3D (Vector3 position, Vector3 direction, float size, Color colour) {
            DrawArrowMarker3D (position, Quaternion.FromToRotation  (Vector3.forward, direction.normalized), size, colour);
		}

		#endregion
	}
}