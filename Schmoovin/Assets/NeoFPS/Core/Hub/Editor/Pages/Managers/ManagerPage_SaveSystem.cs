using NeoFPS;
using NeoSaveGames;
using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class ManagerPage_SaveSystem : EmbeddedEditorPage<SaveGameManager>
    {
        public ManagerPage_SaveSystem(SaveGameManager targetObject) : base (targetObject)
        { }

        //public override MessageType notification
        //{
        //    get
        //    {
        //        if (!target.IsValid())
        //            return MessageType.Error;
        //        return base.notification;
        //    }
        //}

        public override string pageHeader
        {
            get { return "Save-Game Manager"; }
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
            get { return "SaveGameManager asset not found"; }
        }

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);
            EditorGUILayout.Space();
            base.OnGUI();
        }
    }
}
