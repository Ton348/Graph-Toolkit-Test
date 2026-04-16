using Dreamteck.Splines.Editor;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
	public class RectangleEditor : PrimitiveEditor
	{
		public override string GetName()
		{
			return "Rectangle";
		}

		public override void Open(DreamteckSplinesEditor editor)
		{
			base.Open(editor);
			m_primitive = new Rectangle();
			m_primitive.offset = origin;
		}

		protected override void OnGui()
		{
			base.OnGui();
			var rect = (Rectangle)m_primitive;
			rect.size = EditorGUILayout.Vector2Field("Size", rect.size);
		}
	}
}