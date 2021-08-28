using UnityEngine;

namespace NeoFPS
{
	public static class MathExtensions
	{
		public static Vector2 TopDown (Vector3 v)
		{
			return new Vector2 (v.x, v.z);
		}

		public static Vector2 TopDownDirection (Vector3 v)
		{
			Vector2 result = new Vector2 (v.x, v.z);
			result.Normalize ();
			return result;
		}

		public static Quaternion ScaleRotation (Quaternion rot, float scale)
		{
			// Get an angle axis rotation for the quaternion
			Vector3 axis;
			float angle;
			rot.ToAngleAxis (out angle, out axis);

			// Scale the angle
			angle *= scale;

			// Return a new quaternion
			return Quaternion.AngleAxis (angle, axis);
		}
    }
}