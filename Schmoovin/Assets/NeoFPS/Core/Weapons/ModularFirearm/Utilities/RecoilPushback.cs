using NeoCC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-recoilpushback.html")]
    public class RecoilPushback : MonoBehaviour
    {
        [SerializeField, Tooltip("The force to push the character back with.")]
        private float m_Force = 20f;
        [SerializeField, Tooltip("How character aim pitch affects the pushback. ScaleForce will push the character back horizontally based on pitch, while Full3D will push the character directly away from their aim direction.")]
        private PitchMode m_PitchMode = PitchMode.ScaleForce;
        [SerializeField, Tooltip("Should the recoil only push back when the character is not grounded.")]
        private bool m_AirborneOnly = false;

        private IModularFirearm m_ModularFirearm = null;
        private IShooter m_Shooter = null;
        private IAimController m_AimController = null;
        private INeoCharacterController m_CharacterController = null;

        public enum PitchMode
        {
            Ignore,
            ScaleForce,
            Full3D
        }

        private void Start()
        {
            m_ModularFirearm = GetComponent<IModularFirearm>();
            if (m_ModularFirearm != null)
            {
                m_ModularFirearm.onWielderChanged += OnWielderChanged;
                OnWielderChanged(m_ModularFirearm.wielder);
                m_ModularFirearm.onShooterChange += OnShooterChange;
                OnShooterChange(m_ModularFirearm, m_ModularFirearm.shooter);
            }
            else
                Debug.LogError("RecoilPushback component must be attached to an object with a component that implements IModularFirearm", gameObject);
        }

        private void OnDestroy()
        {
            if (m_ModularFirearm != null)
            {
                OnWielderChanged(null);
                OnShooterChange(m_ModularFirearm, null);
                m_ModularFirearm.onShooterChange -= OnShooterChange;
                m_ModularFirearm.onWielderChanged -= OnWielderChanged;
            }
        }

        private void OnWielderChanged(ICharacter character)
        {
            if (character != null)
            {
                m_CharacterController = character.motionController.characterController;
                m_AimController = character.aimController;
            }
            else
            {
                m_CharacterController = null;
                m_AimController = null;
            }
        }

        private void OnShooterChange(IModularFirearm firearm, IShooter shooter)
        {
            if (m_Shooter != null)
                m_Shooter.onShoot -= OnShoot;
            m_Shooter = shooter;
            if (m_Shooter != null)
                m_Shooter.onShoot += OnShoot;
        }

        private void OnShoot(IModularFirearm firearm)
        {
            if (m_CharacterController != null && m_AimController != null && !(m_AirborneOnly && m_CharacterController.isGrounded))
            {
                switch (m_PitchMode)
                {
                    case PitchMode.Ignore:
                        m_CharacterController.AddForce(m_AimController.heading * -m_Force, ForceMode.Impulse, false);
                        break;
                    case PitchMode.Full3D:
                        m_CharacterController.AddForce(m_AimController.forward * -m_Force, ForceMode.Impulse, true);
                        break;
                    case PitchMode.ScaleForce:
                        {
                            float multiplier = Mathf.Cos(m_AimController.pitch * Mathf.Deg2Rad);
                            m_CharacterController.AddForce(m_AimController.heading * (-m_Force * multiplier), ForceMode.Impulse, false);
                        }
                        break;
                }
            }
        }
    }
}