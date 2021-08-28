using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Conditions;

namespace NeoFPSEditor.CharacterMotion.Conditions
{
    [MotionGraphConditionDrawer(typeof(ConditionGroupCondition))]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgc-conditiongroupcondition.html")]
    public class ConditionGroupConditionDrawer : MotionGraphConditionDrawer
    {
        protected override string displayName
        {
            get { return "Condition Group"; }
        }

        protected override void Inspect(Rect line1)
        {
            Rect r1 = line1;
            Rect r2 = r1;
            r1.width *= 0.5f;
            r2.width *= 0.5f;
            r2.x += r1.width;
            EditorGUI.LabelField(r1, "Condition Group");

            var cast = condition as ConditionGroupCondition;

            // Get the connection
            var prop = serializedObject.FindProperty("m_Connection");
            var connection = prop.objectReferenceValue as MotionGraphConnection;
            if (connection != null)
            {
                string dropdownLabel = "<Not Set>";
                int index = -1;
                
                // Get the condition group
                int id = serializedObject.FindProperty("m_ID").intValue;
                if (id != 0)
                {
                    for (int i = 0; i < connection.conditionGroups.Count; ++i)
                    {
                        if (connection.conditionGroups[i].id == id)
                        {
                            index = i;
                            break;
                        }
                    }
                }

                // Get dropdown label
                if (index != -1)
                    dropdownLabel = connection.conditionGroups[index].name;
                
                // Draw dropdown
                if (EditorGUI.DropdownButton(r2, new GUIContent(dropdownLabel), FocusType.Passive))
                {
                    var menu = new GenericMenu();

                    if (connection.conditionGroups.Count > 0)
                    {
                        for (int i = 0; i < connection.conditionGroups.Count; ++i)
                        {
                            if (connection.conditionGroups[i].id != cast.id)
                                menu.AddItem(new GUIContent(connection.conditionGroups[i].name), false, OnConditionGroupSelect, i);
                        }
                        menu.AddSeparator(string.Empty);
                    }
                    menu.AddItem(new GUIContent("Add New Group"), false, OnAddNewCondition);
                    menu.ShowAsContext();
                }
            }
            else
                EditorGUI.LabelField(r2, "<Link Broken>");
        }

        void OnAddNewCondition()
        {
            var prop = serializedObject.FindProperty("m_Connection");
            var connection = prop.objectReferenceValue as MotionGraphConnection;
            if (connection != null)
            {
                serializedObject.FindProperty("m_ID").intValue = MotionGraphEditorFactory.CreateConditionGroup(connection);
                serializedObject.ApplyModifiedProperties();
            }
        }

        void OnConditionGroupSelect(object o)
        {
            int index = (int)o;
            var prop = serializedObject.FindProperty("m_Connection");
            var connection = prop.objectReferenceValue as MotionGraphConnection;
            if (connection != null)
            {
                serializedObject.FindProperty("m_ID").intValue = connection.conditionGroups[index].id;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
