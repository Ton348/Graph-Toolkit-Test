using UnityEngine;
using System.Collections;
using UnityEditor;
using Dreamteck.Splines.Editor;

namespace Dreamteck.Splines.Primitives
{
    public class LineEditor : PrimitiveEditor
    {
        public override string GetName()
        {
            return "Line";
        }

        public override void Open(DreamteckSplinesEditor editor)
        {
            base.Open(editor);
            m_primitive = new Line();
        }

        protected override void OnGui()
        {
            base.OnGui();
            Line line = (Line)m_primitive;
            line.length = EditorGUILayout.FloatField("Length", line.length);
            line.mirror = EditorGUILayout.Toggle("Mirror", line.mirror);
            line.rotation = EditorGUILayout.Vector3Field("Rotation", line.rotation);
            line.segments = EditorGUILayout.IntField("Segments", line.segments);
        }
    }
}
