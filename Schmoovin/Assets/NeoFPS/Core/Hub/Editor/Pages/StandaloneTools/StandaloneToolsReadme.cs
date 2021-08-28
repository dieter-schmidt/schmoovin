using NeoFPS.Hub;
using UnityEngine;

namespace NeoFPSEditor.Hub
{
    //[CreateAssetMenu(fileName = "StandaloneToolsReadme", menuName = "NeoFPS Internal/Standalone Tools Readme")]
    public class StandaloneToolsReadme : ReadmeAsset
    {
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