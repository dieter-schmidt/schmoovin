using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public interface IWaterZone
    {
        Vector3 FlowAtPosition(Vector3 position);
        WaterSurfaceInfo SurfaceInfoAtPosition(Vector3 position);
    }

    public struct WaterSurfaceInfo
    {
        public Vector3 normal;
        public float height;

        public WaterSurfaceInfo(Vector3 n, float h)
        {
            normal = n;
            height = h;
        }
    }
}