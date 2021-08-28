using NeoFPS;
using NeoFPS.Hub;
using UnityEngine;

namespace NeoFPSEditor.Hub
{
    [CreateAssetMenu(fileName = "Quickstart_01_NewQuickstart", menuName = "NeoFPS/Readmes/Quickstart Readme", order = NeoFpsMenuPriorities.readme_quickstart)]
    public class QuickstartReadme : ReadmeAsset
    {
        public string subFolder = string.Empty;
        public string pageName = "quickstart_qs";

        public void ShowPage(string pageID)
        {
            NeoFpsHubEditor.ShowPage(pageID);
        }

        public void ShowMotionGraphEditor()
        {
            CharacterMotion.MotionGraphEditor.CreateWindow();
        }

        public void ShowMotionDebugger()
        {
            CharacterMotion.Debugger.MotionControllerDebugger.CreateWindow();
        }

        public void ShowNeoSaveFileInspector()
        {
            NeoSaveGames.SaveGameInspector.ShowWindow();
        }
    }
}