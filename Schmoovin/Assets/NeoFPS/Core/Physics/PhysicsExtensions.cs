using System;
using UnityEngine;

namespace NeoFPS
{
	public static class PhysicsExtensions
	{
		private static RaycastHit[] s_Hits = new RaycastHit[64];

        public static bool RaycastFiltered(Ray ray, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null)
        {
            int hitCount = Physics.RaycastNonAlloc(ray, s_Hits, maxDistance, layerMask);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // Check if closer
                    if (closest == -1 || s_Hits[i].distance < s_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // Check if transform or parents match ignore root
                            Transform t = s_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // Check if all ignored
                if (closest == -1)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public static bool RaycastNonAllocSingle (Ray ray, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int hitCount = Physics.RaycastNonAlloc (ray, s_Hits, maxDistance, layerMask, queryTriggerInteraction);
			if (hitCount > 0)
			{
				// Get the closest (not ignored)
				int closest = -1;
				for (int i = 0; i < hitCount; ++i)
				{
					// Check if closer
                    if (closest == -1 || s_Hits[i].distance < s_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // Check if transform or parents match ignore root
                            Transform t = s_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
				// Check if all ignored
                if (closest == -1)
                {
                    hit = new RaycastHit();
                    return false;
                }
				// Return the relevant hit
                hit = s_Hits [closest];
				return true;
			}
			else
			{
				hit = new RaycastHit ();
				return false;
			}
		}

        public static bool SphereCastFiltered(Ray ray, float radius, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null)
        {
            int hitCount = Physics.SphereCastNonAlloc(ray, radius, s_Hits, maxDistance, layerMask);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // Check if closer
                    if (closest == -1 || s_Hits[i].distance < s_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // Check if transform or parents match ignore root
                            Transform t = s_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // Check if all ignored
                if (closest == -1)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public static bool SphereCastNonAllocSingle (Ray ray, float radius, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int hitCount = Physics.SphereCastNonAlloc (ray, radius, s_Hits, maxDistance, layerMask, queryTriggerInteraction);
			if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // Check if closer
                    if (closest == -1 || s_Hits[i].distance < s_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // Check if transform or parents match ignore root
                            Transform t = s_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // Check if all ignored
                if (closest == -1)
                {
                    hit = new RaycastHit();
                    return false;
                }
                // Return the relevant hit
                hit = s_Hits[closest];
                return true;
			}
			else
			{
				hit = new RaycastHit ();
				return false;
			}
		}

        public static bool CapsuleCastFiltered(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null)
        {
            int hitCount = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, s_Hits, maxDistance, layerMask);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // Check if closer
                    if (closest == -1 || s_Hits[i].distance < s_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // Check if transform or parents match ignore root
                            Transform t = s_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // Check if all ignored
                if (closest == -1)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public static bool CapsuleCastNonAllocSingle (Vector3 point1, Vector3 point2, float radius, Vector3 direction, out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int hitCount = Physics.CapsuleCastNonAlloc (point1, point2, radius, direction, s_Hits, maxDistance, layerMask, queryTriggerInteraction);
			if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // Check if closer
                    if (closest == -1 || s_Hits[i].distance < s_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // Check if transform or parents match ignore root
                            Transform t = s_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // Check if all ignored
                if (closest == -1)
                {
                    hit = new RaycastHit();
                    return false;
                }
                // Return the relevant hit
                hit = s_Hits[closest];
                return true;
			}
			else
			{
				hit = new RaycastHit ();
				return false;
			}
		}

        public static bool BoxCastFiltered(Vector3 center, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null)
        {
            int hitCount = Physics.BoxCastNonAlloc(center, halfExtents, direction, s_Hits, orientation, maxDistance, layerMask);
            if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // Check if closer
                    if (closest == -1 || s_Hits[i].distance < s_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // Check if transform or parents match ignore root
                            Transform t = s_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // Check if all ignored
                if (closest == -1)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public static bool BoxCastNonAllocSingle (Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hit, Quaternion orientation, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoreRoot = null, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int hitCount = Physics.BoxCastNonAlloc (center, halfExtents, direction, s_Hits, orientation, maxDistance, layerMask, queryTriggerInteraction);
			if (hitCount > 0)
            {
                // Get the closest (not ignored)
                int closest = -1;
                for (int i = 0; i < hitCount; ++i)
                {
                    // Check if closer
                    if (closest == -1 || s_Hits[i].distance < s_Hits[closest].distance)
                    {
                        if (ignoreRoot != null)
                        {
                            // Check if transform or parents match ignore root
                            Transform t = s_Hits[i].transform;
                            bool ignore = false;
                            while (t != null)
                            {
                                if (t == ignoreRoot)
                                {
                                    ignore = true;
                                    break;
                                }
                                t = t.parent;
                            }
                            // Not ignored. This is closest
                            if (!ignore)
                                closest = i;
                        }
                        else
                            closest = i;
                    }
                }
                // Check if all ignored
                if (closest == -1)
                {
                    hit = new RaycastHit();
                    return false;
                }
                // Return the relevant hit
                hit = s_Hits[closest];
                return true;
			}
			else
			{
				hit = new RaycastHit ();
				return false;
			}
		}
	}
}

