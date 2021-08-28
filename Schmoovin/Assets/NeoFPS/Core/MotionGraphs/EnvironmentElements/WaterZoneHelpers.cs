using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public static class WaterZoneHelpers
    {
        public static Vector3 GetHighestSphereCenter(IMotionController controller)
        {
            var characterController = controller.characterController;
            if (characterController.characterGravity != null)
            {
                if (characterController.up.y >= 0f)
                    return controller.localTransform.position + characterController.up * (characterController.height - characterController.radius);
                else
                    return controller.localTransform.position + characterController.up * characterController.radius;
            }
            else
                return controller.localTransform.position + new Vector3(0f, characterController.height - characterController.radius, 0f);
        }

        public static Vector3 GetLowestSphereCenter(IMotionController controller)
        {
            var characterController = controller.characterController;
            if (characterController.characterGravity != null)
            {
                if (characterController.up.y >= 0f)
                    return controller.localTransform.position + characterController.up * characterController.radius;
                else
                    return controller.localTransform.position + characterController.up * (characterController.height - characterController.radius);
            }
            else
                return controller.localTransform.position + new Vector3(0f, characterController.radius, 0f);
        }

        public static float GetHighestPoint(IMotionController controller)
        {
            if (controller.characterController.characterGravity != null)
            {
                var top = GetHighestSphereCenter(controller);
                return top.y + controller.characterController.radius;
            }
            else
                return controller.localTransform.position.y + controller.characterController.height;
        }

        public static float GetLowestPoint(IMotionController controller)
        {
            if (controller.characterController.characterGravity != null)
            {
                var bottom = GetLowestSphereCenter(controller);
                return bottom.y - controller.characterController.radius;
            }
            else
                return controller.localTransform.position.y;
        }

        public static float CompareHighestToSurface(IMotionController controller, IWaterZone waterZone)
        {
            var characterController = controller.characterController;
            if (characterController.characterGravity != null)
            {
                var top = GetHighestSphereCenter(controller);
                var surface = waterZone.SurfaceInfoAtPosition(top);
                return top.y + controller.characterController.radius - surface.height;
            }
            else
            {
                Vector3 position = controller.localTransform.position;
                var surface = waterZone.SurfaceInfoAtPosition(position);
                return position.y + characterController.height - surface.height;
            }
        }

        public static float CompareLowestToSurface(IMotionController controller, IWaterZone waterZone)
        {
            var characterController = controller.characterController;
            if (characterController.characterGravity != null)
            {
                var bottom = GetLowestSphereCenter(controller);
                var surface = waterZone.SurfaceInfoAtPosition(bottom);
                return bottom.y - controller.characterController.radius - surface.height;
            }
            else
            {
                Vector3 position = controller.localTransform.position;
                var surface = waterZone.SurfaceInfoAtPosition(position);
                return position.y - surface.height;
            }
        }
    }
}
