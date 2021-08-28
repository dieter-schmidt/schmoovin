using NeoFPS.Hub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPSEditor.Hub
{
#if NEOFPS_INTERNAL
    [CreateAssetMenu(fileName = "HubSectionReadme", menuName = "NeoFPS Internal/Hub-Section Readme")]
#endif
    public class HubSectionReadme : ReadmeAsset
    {
        public void ShowPage(string pageID)
        {
            NeoFpsHubEditor.ShowPage(pageID);
        }
    }
}