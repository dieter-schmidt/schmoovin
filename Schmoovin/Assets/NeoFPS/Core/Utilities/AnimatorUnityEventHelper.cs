using UnityEngine;

namespace NeoFPS
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorUnityEventHelper : MonoBehaviour
    {
        [SerializeField]
        private string m_BoolKey = string.Empty;

        private Animator m_Animator = null;
        private int m_BoolKeyHash = -1;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_BoolKeyHash = Animator.StringToHash(m_BoolKey);
        }

        public void SetBoolTrue()
        {
            m_Animator.SetBool(m_BoolKeyHash, true);
        }

        public void SetBoolFalse()
        {
            m_Animator.SetBool(m_BoolKeyHash, false);
        }
    }
}
