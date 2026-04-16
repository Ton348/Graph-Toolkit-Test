namespace Dreamteck.Splines.Primitives
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using Dreamteck.Splines.Editor;

    [System.Serializable]
    public class PrimitiveEditor
    {
        [System.NonSerialized]
        protected DreamteckSplinesEditor m_editor;
        [System.NonSerialized]
        public Vector3 origin = Vector3.zero;

        protected SplinePrimitive m_primitive = new SplinePrimitive();

        public virtual string GetName()
        {
            return "Primitive";
        }

        public virtual void Open(DreamteckSplinesEditor editor)
        {
            this.m_editor = editor;
            m_primitive.is2D = editor.is2D;
            m_primitive.Calculate();
        }

        public void Draw()
        {
            EditorGUI.BeginChangeCheck();
            OnGui();
            if (EditorGUI.EndChangeCheck())
            {
                Update();
            }
        }

        public void Update()
        {
            m_primitive.is2D = m_editor.is2D;
            m_primitive.Calculate();
            m_editor.SetPointsArray(m_primitive.GetPoints());
            m_editor.SetSplineType(m_primitive.GetSplineType());
            m_editor.SetSplineClosed(m_primitive.GetIsClosed());
            m_editor.ApplyModifiedProperties(true);
        }

        protected virtual void OnGui()
        {
            m_primitive.is2D = m_editor.is2D;
            m_primitive.offset = EditorGUILayout.Vector3Field("Offset", m_primitive.offset);
            if (m_editor.is2D)
            {
                float rot = m_primitive.rotation.z;
                rot = EditorGUILayout.FloatField("Rotation", rot);
                m_primitive.rotation = new Vector3(0f, 0f, rot);
            }
             else m_primitive.rotation = EditorGUILayout.Vector3Field("Rotation", m_primitive.rotation);
        }
    }
}
