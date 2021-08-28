using NeoFPS;
using UnityEngine;
using UnityEditor;
using NeoFPS.Hub;

namespace NeoFPSEditor.Hub.Pages
{
    public class SettingsPage : EmbeddedEditorPage<SettingsContextBase>
    {
        public SettingsPage(SettingsContextBase targetObject) : base(targetObject)
        { }

        public override string pageHeader
        {
            get { return target.displayTitle; }
        }

        public override string editorNotFoundErrorMessage
        {
            get { return pageHeader + " asset not found"; }
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

        public override void OnGUI()
        {
            ReadmeEditorUtility.DrawReadmeHeader(heading, true);

            base.OnGUI();
        }
    }
}
