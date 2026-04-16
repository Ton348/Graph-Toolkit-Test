using UnityEngine;
using System.Collections;
using UnityEditor;
using Dreamteck.Splines.Editor;

namespace Dreamteck.Splines.Primitives
{
    public class StarEditor : PrimitiveEditor
    {
        public override string GetName()
        {
            return "Star";
        }

        public override void Open(DreamteckSplinesEditor editor)
        {
            base.Open(editor);
            m_primitive = new Star();
        }

        protected override void OnGui()
        {
            base.OnGui();
            Star star = (Star)m_primitive;
            star.radius = EditorGUILayout.FloatField("Radius", star.radius);
            star.depth = EditorGUILayout.FloatField("Depth", star.depth);
            star.sides = EditorGUILayout.IntField("Sides", star.sides);
            if (star.sides < 3) star.sides = 3;
        }
    }
}
