using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Hub
{
    public class ReadmeBehaviour : MonoBehaviour, IReadme
    {
        [SerializeField] private ReadmeHeader m_Header = new ReadmeHeader();
        [SerializeField] private ReadmeSection[] m_Sections = new ReadmeSection[0];

        public ReadmeHeader header
        {
            get { return m_Header; }
        }

        public ReadmeSection[] sections
        {
            get { return m_Sections; }
        }
    }
}