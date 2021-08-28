using NeoFPS;
using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class ManagerPage_Input : EmbeddedEditorPage<NeoFpsInputManager>
    {
        public ManagerPage_Input(NeoFpsInputManager targetObject) : base(targetObject)
        { }

        public override MessageType notification
        {
            get
            {
                if (target.showError)
                    return MessageType.Error;
                if (target.showWarning)
                    return MessageType.Warning;
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
            get { return "Input Manager"; }
        }

        public override string editorNotFoundErrorMessage
        {
            get { return "InputManager asset not found"; }
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();
            base.OnGUI();
        }
    }
}
