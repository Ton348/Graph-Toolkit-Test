using Dreamteck.Splines.Editor;
using UnityEditor;

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
			var ellipse = (Ellipse)m_primitive;
			ellipse.xRadius = EditorGUILayout.FloatField("X Radius", ellipse.xRadius);
			ellipse.yRadius = EditorGUILayout.FloatField("Y Radius", ellipse.yRadius);
		}
	}
}