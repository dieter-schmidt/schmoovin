using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(ModularFirearm))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-firearmtransformmatchsetter.html")]
    public class FirearmTransformMatchSetter : MonoBehaviour
    {
        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The target transform to match.")]
        private Transform m_Target = null;

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The transform to use when calculating the offset of the target transform.")]
        private Transform m_RelativeTo = null;

        [SerializeField, Range(0f, 1f), Tooltip("The strength of the effect. 1 matches the movement absolutely, while 0 is no movement.")]
        private float m_Weight = 1f;

        [SerializeField, Tooltip("")]
        private MatchSetterLocation m_MatchSetterLocation = MatchSetterLocation.Head;

        private ModularFirearm m_Firearm;
        private TransformMatcher m_TransformMatcher;

#if UNITY_EDITOR
        private bool m_FlaggedError = false;
#endif

        public enum MatchSetterLocation
        {
            Head,
            UpperBody,
            AimTransform
        }

        private void Awake()
        {
            m_Firearm = GetComponent<ModularFirearm>();
        }

        private void OnEnable()
        {
            if (m_Firearm.wielder == null)
                return;

            switch (m_MatchSetterLocation)
            {
                case MatchSetterLocation.Head:
                    if (m_Firearm.wielder.headTransformHandler != null)
                        m_TransformMatcher = m_Firearm.wielder.headTransformHandler.GetComponent<TransformMatcher>();
                    break;
                case MatchSetterLocation.UpperBody:
                    if (m_Firearm.wielder.bodyTransformHandler != null)
                        m_TransformMatcher = m_Firearm.wielder.bodyTransformHandler.GetComponent<TransformMatcher>();
                    break;
                case MatchSetterLocation.AimTransform:
                    if (m_Firearm.wielder.fpCamera != null)
                        m_TransformMatcher = m_Firearm.wielder.fpCamera.aimTransform.GetComponent<TransformMatcher>();
                    break;
            }
        
            if (m_TransformMatcher != null)
                m_TransformMatcher.SetTargetTransforms(m_Target, m_RelativeTo, m_Weight);
#if UNITY_EDITOR
            else
            {
                if (!m_FlaggedError)
                {
                    m_FlaggedError = true;
                    Debug.Log("Couldn't find transform matcher: " + m_MatchSetterLocation);
                }
            }
#endif
        }

        private void OnDisable()
        {
            if (m_TransformMatcher != null)
                m_TransformMatcher.ClearTargetTransforms();
            m_TransformMatcher = null;
        }
    }
}
