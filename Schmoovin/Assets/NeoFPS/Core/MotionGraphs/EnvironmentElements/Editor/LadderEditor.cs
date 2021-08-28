using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    public class LadderEditor : Editor
    {
        protected void OnSceneGUI()
        {
            ILadder ladder = target as ILadder;
            MonoBehaviour mb = target as MonoBehaviour;
            if (ladder != null && mb != null)
                DrawLadderHandles(ladder, mb.transform);
        }

        protected virtual void DrawLadderHandles (ILadder ladder, Transform transform)
        {
            Color c = Handles.color;

            float radius = 0.5f;

            Quaternion rotation = transform.rotation;
            //Vector3 ladderTop = transform.position + (rotation * ladder.top);
            Vector3 forward = transform.forward;
            Vector3 up = transform.up;
            float halfWidth = ladder.width * 0.5f;

            Vector3 p = transform.position;

            Vector3[] corners = new Vector3[4];
            corners[0] = new Vector3(-halfWidth, 0f, 0f);
            corners[1] = new Vector3(halfWidth, 0f, 0f);
            corners[2] = new Vector3(halfWidth, -ladder.length, 0f);
            corners[3] = new Vector3(-halfWidth, -ladder.length, 0f);

            for (int i = 0; i < 4; ++i)
                corners[i] = p + rotation * corners[i];//(ladderTop + corners[i]);
            Handles.DrawSolidRectangleWithOutline(corners, new Color (0f, 0f, 1f, 0.15f), Color.cyan);

            corners[0] = new Vector3(-halfWidth, 0f, radius);
            corners[1] = new Vector3(halfWidth, 0f, radius);
            corners[2] = new Vector3(halfWidth, -ladder.length, radius);
            corners[3] = new Vector3(-halfWidth, -ladder.length, radius);

            for (int i = 0; i < 4; ++i)
                corners[i] = p + rotation * corners[i];//(ladderTop + corners[i]);
            Handles.DrawSolidRectangleWithOutline(corners, new Color(1f, 0f, 0f, 0.15f), Color.magenta);

            float angle = 0f;
            float increment = 15f * Mathf.Deg2Rad;
            for (int i = 0; i < 6; ++i)
            {
                Vector3 angle1 = new Vector3(0f, Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius);
                Vector3 angle2 = new Vector3(0f, Mathf.Sin(angle + increment) * radius, Mathf.Cos(angle + increment) * radius);

                corners[0] = new Vector3(-halfWidth, 0f, 0f) + angle1;
                corners[1] = new Vector3(halfWidth, 0f, 0f) + angle1;
                corners[2] = new Vector3(halfWidth, 0f, 0f) + angle2;
                corners[3] = new Vector3(-halfWidth, 0f, 0f) + angle2;

                for (int j = 0; j < 4; ++j)
                    corners[j] = p + rotation * corners[j];//(ladderTop + corners[j]);
                Handles.DrawSolidRectangleWithOutline(corners, new Color(1f, 0f, 0f, 0.15f), Color.magenta);

                angle += increment;
            }

            Handles.color = c;
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
        }
    }
}