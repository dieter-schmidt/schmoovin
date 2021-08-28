using NeoFPS;
using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class ManagerPage_Pooling : EmbeddedEditorPage<PoolManager>
    {
        public ManagerPage_Pooling(PoolManager targetObject) : base(targetObject)
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
            get { return "Pooling System Manager"; }
        }

        public override string editorNotFoundErrorMessage
        {
            get { return "PoolManager asset not found"; }
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();
            base.OnGUI();
        }
    }
}
