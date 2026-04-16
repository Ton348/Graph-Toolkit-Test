using UnityEngine;
using System.Collections;
using UnityEditor;
using Dreamteck.Splines.Editor;

namespace Dreamteck.Splines.Primitives
{
    public class EllipseEditor : PrimitiveEditor
    {
        

        public override string GetName()
        {
            return "Ellipse";
        }

        public override void Open(DreamteckSplinesEditor editor)
        {
            base.Open(editor);
            m_primitive = new Ellipse();
            m_primitive.offset = origin;
        }

        protected override void OnGui()
        {
            base.OnGui();
            Ellipse ellipse = (Ellipse)m_primitive;
            ellipse.xRadius = EditorGUILayout.FloatField("X Radius", ellipse.xRadius);
            ellipse.yRadius = EditorGUILayout.FloatField("Y Radius", ellipse.yRadius);
        }
    }
}
