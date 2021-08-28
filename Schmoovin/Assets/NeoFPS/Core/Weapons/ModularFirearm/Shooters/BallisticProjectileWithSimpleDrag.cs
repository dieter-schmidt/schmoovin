using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-ballisticprojectilewithsimpledrag.html")]
	public class BallisticProjectileWithSimpleDrag : BallisticProjectile
	{
        [SerializeField, Range(0f, 1f), Tooltip("The strength of the drag on the projectile (uses a simplified multiplier)")]
        private float m_DragEffect = 0.25f;

        [SerializeField, Tooltip("If true, then drag will only be applied to the vertical velocity while the projectile is climbing")]
        private bool m_DragIgnoresGravity = true;
        
        protected override Vector3 ApplyForces(Vector3 v)
        {
            // Apply drag effect
            if (m_DragEffect > 0.0001f)
            {
                float dragMultiplier = Mathf.Lerp(1f, 1f - m_DragEffect, Time.deltaTime * 2.5f);
                if (m_DragIgnoresGravity && v.y < 0f)
                {
                    float fall = v.y;
                    v.y = 0f;
                    v *= dragMultiplier;
                    v.y = fall;
                }
                else
                    v *= dragMultiplier;
            }

            // Get the base velocity
            v = base.ApplyForces(v);

            return v;
        }
    }
}