using UnityEngine;
using System.Collections;
using UnityEditor;
using Dreamteck.Splines.Editor;

namespace Dreamteck.Splines.Primitives
{
    public class RoundedRectangleEditor : PrimitiveEditor
    {
        public override string GetName()
        {
            return "Rounded Rect";
        }

        public override void Open(DreamteckSplinesEditor editor)
        {
            base.Open(editor);
            m_primitive = new RoundedRectangle();
            m_primitive.offset = origin;
        }

        protected override void OnGui()
        {
            base.OnGui();
            RoundedRectangle rect = (RoundedRectangle)m_primitive;
            rect.size = EditorGUILayout.Vector2Field("Size", rect.size);
            rect.xRadius = EditorGUILayout.FloatField("X Radius", rect.xRadius);
            rect.yRadius = EditorGUILayout.FloatField("Y Radius", rect.yRadius);
        }
    }
}
