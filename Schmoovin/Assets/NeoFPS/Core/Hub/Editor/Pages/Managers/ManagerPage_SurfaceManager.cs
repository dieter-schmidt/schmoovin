using NeoFPS;
using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class ManagerPage_SurfaceManager : EmbeddedEditorPage<SurfaceManager>
    {
        public ManagerPage_SurfaceManager(SurfaceManager targetObject) : base (targetObject)
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

        public override string pageHeader
        {
            get { return "Surface Manager"; }
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

        public override string editorNotFoundErrorMessage
        {
            get { return "SurfaceManager asset not found"; }
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();
            base.OnGUI();
        }
    }
}
