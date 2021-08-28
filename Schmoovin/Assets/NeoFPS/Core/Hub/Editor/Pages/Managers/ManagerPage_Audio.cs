using NeoFPS;
using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class ManagerPage_Audio : EmbeddedEditorPage<NeoFpsAudioManager>
    {
        public ManagerPage_Audio(NeoFpsAudioManager targetObject) : base(targetObject)
        { }

        public override MessageType notification
        {
            get
            {
                if (!target.IsValid())
                    return MessageType.Error;
                return base.notification;
            }
        }

        private ReadmeHeader m_Heading = null;
        public ReadmeHeader heading
        {
            get
            {
                if (m_Heading == null)
                    m_Heading = new ReadmeHeader(LoadIcon("EditorImage_NeoFpsIconRound", "EditorImage_NeoFpsCrosshair"), pageHeader);
                return m_Heading;
            }
        }

        public override string pageHeader
        {
            get { return "Audio Manager"; }
        }

        public override string editorNotFoundErrorMessage
        {
            get { return "NeoFpsAudioManager asset not found"; }
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();
            base.OnGUI();
        }
    }
}
