using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.CharacterMotion
{
    public class MotionGraphPropertyDrawer
    {
        public SerializedObject serializedObject { get; private set; }

        public MotionGraphEditor editor { get; private set; }

        public void Initialise(Object o, MotionGraphEditor ed)
        { 
            serializedObject = new SerializedObject(o);
            editor = ed;
        }

        public void Draw(Rect r)
        {
            serializedObject.Update();

            Rect temp = r;
            temp.width *= 0.5f;
            r.width -= temp.width;
            r.x += temp.width;
            temp.width -= 4f;

            var prop = serializedObject.FindProperty("m_Name");
            string newName = EditorGUI.TextField(temp, "", prop.stringValue);
            if (newName != prop.stringValue && IsValidName(newName))
                prop.stringValue = newName;

            DrawValue(r);

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual bool IsValidName (string n)
        {
            return true;
        }

        protected virtual void DrawValue (Rect r)
        {

        }
    }
}