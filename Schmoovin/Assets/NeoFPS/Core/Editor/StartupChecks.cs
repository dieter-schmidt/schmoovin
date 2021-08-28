using NeoFPSEditor.Hub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NeoFPSEditor
{
    class StartupChecks
    {
        const int k_FrameDelay = 50;

        static int s_FrameDelay = 0;
        static bool s_Processing = false;
        static int s_Index = 0;
        static Action[] s_Callbacks = new Action[]
        {
            CheckPackages,
            ShowHub,
            Completed
        };

        [InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            // Skip if playing
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            else
            {
                s_Index = 0;
                s_FrameDelay = 0;
                s_Processing = false;
                EditorApplication.update += WaitForEditor;
            }
        }

        static void WaitForEditor()
        {
            if (!s_Processing && !EditorApplication.isUpdating && !EditorApplication.isCompiling)
            {
                if (++s_FrameDelay > k_FrameDelay)
                {
                    s_Processing = true;
                    s_Callbacks[s_Index]();
                    s_FrameDelay = 0;
                }
            }
            else
                s_FrameDelay = 0;
        }

        static void GetNextCallback()
        {
            ++s_Index;
            s_Processing = false;
            s_FrameDelay = 0;
        }

        static void CheckPackages()
        {
            PackageDependencyChecker.CheckPackages(GetNextCallback);
        }

        static void ShowHub()
        {
            NeoFpsHubEditor.ShowOnStartup(GetNextCallback);
        }

        static void Completed()
        {
            s_Index = 0;
            s_FrameDelay = 0;
            s_Processing = false;
            NeoFpsEditorPrefs.firstRun = false;
            EditorApplication.update -= WaitForEditor;
        }
    }
}
