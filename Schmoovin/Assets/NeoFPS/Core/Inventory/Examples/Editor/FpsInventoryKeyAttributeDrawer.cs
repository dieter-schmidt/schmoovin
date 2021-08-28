using UnityEngine;
using UnityEditor;
using NeoFPS;
using UnityEditor.Animations;

namespace NeoFPSEditor
{
    [CustomPropertyDrawer(typeof(FpsInventoryKeyAttribute))]
    public class FpsInventoryKeyAttributeDrawer : PropertyDrawer
    {
        private SerializedProperty m_PickerProperty = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            int id = property.intValue;
            bool databaseValid = NeoFpsInventoryDatabase.CheckInstance();
            FpsInventoryDatabaseEntry entry = databaseValid && id != 0 ? NeoFpsInventoryDatabase.GetEntry(id) : null;

            var castAttribute = attribute as FpsInventoryKeyAttribute;
            if (castAttribute.showLabel)
            {
                var r1 = position;
                r1.width = EditorGUIUtility.labelWidth;

                if (castAttribute.required)
                    NeoFpsEditorGUI.DrawRequiredPrefixLabel(r1, property, entry != null);
                else
                    EditorGUI.PrefixLabel(r1, new GUIContent(property.displayName, property.tooltip));


                position.width -= EditorGUIUtility.labelWidth;
                position.x += EditorGUIUtility.labelWidth;
            }

            if (!databaseValid)
            {
                EditorGUI.HelpBox(position, "Project database not set up", MessageType.Error);
            }
            else
            {
                string buttonLabel;
                if (id == 0)
                    buttonLabel = "<Not Set>";
                else
                {
                    if (entry == null)
                        buttonLabel = string.Format("<Missing: {0}>", id);
                    else
                        buttonLabel = entry.displayName;
                }

                if (GUI.Button(position, buttonLabel))
                {
                    m_PickerProperty = property;
                    FpsInventoryKeyPopup.PickKey(OnPicked, null);
                }
            }

            EditorGUI.EndProperty();
        }

        void OnPicked(int id)
        {
            m_PickerProperty.intValue = id;
            m_PickerProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
