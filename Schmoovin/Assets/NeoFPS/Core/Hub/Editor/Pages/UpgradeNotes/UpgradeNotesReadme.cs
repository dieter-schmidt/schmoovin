using NeoFPS.Hub;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor.Hub.Pages
{
#if NEOFPS_INTERNAL
    [CreateAssetMenu(fileName = "UpgradeNotes", menuName = "NeoFPS Internal/Upgrade Notes")]
#endif
    public class UpgradeNotesReadme : ScriptableObject
    {
        public ReadmeHeader header = new ReadmeHeader();
        public UpdateReadme[] updates = new UpdateReadme[0];

        public int latest
        {
            get
            {
                if (updates.Length == 0)
                    return 0;
                else
                    return updates[updates.Length - 1].version;
            }
        }

        public bool CheckIsUpToDate()
        {
            int version = NeoFpsEditorPrefs.currentNeoFPSVersion;
            if (version == latest)
                return true;
            else
                return false;
        }
    }
}