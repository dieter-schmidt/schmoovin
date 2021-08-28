using UnityEngine;

namespace NeoCC
{
	public interface IAimController
	{
        float pitch { get; }
        Quaternion yawLocalRotation { get; }
        Quaternion pitchLocalRotation { get; }

        //Quaternion rotation { get; set; }
        Vector3 heading { get; }
        Vector3 forward { get; }
        Vector3 yawUp { get; }

        // Used for slowing turn when zooming, etc
        float turnRateMultiplier { get; set; }

        // Steering
        float steeringRate { get; set; }
        float aimYawDiff { get; }

        void AddYaw (float rotation);
        void ResetYawLocal ();
        
		void AddPitch (float rotation);
        void ResetPitchLocal ();

        void AddRotation (float y, float p);
        void AddRotationInput(Vector2 input, Transform relativeTo);

        void SetYawConstraints(Vector3 center, float range);
        void SetPitchConstraints(float min, float max);
        void ResetYawConstraints();
        void ResetPitchConstraints();

        Transform transform { get; }
    }
}