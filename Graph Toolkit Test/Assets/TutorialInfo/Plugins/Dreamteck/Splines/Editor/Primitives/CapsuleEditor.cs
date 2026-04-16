using Dreamteck.Splines.Editor;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
	public class CapsuleEditor : PrimitiveEditor
	{
		public override string GetName()
		{
			return "Capsule";
		}

		public override void Open(DreamteckSplinesEditor editor)
		{
			base.Open(editor);
			m_primitive = new Capsule();
		}

		protected override void OnGui()
		{
			base.OnGui();
			var capsule = (Capsule)m_primitive;
			capsule.radius = EditorGUILayout.FloatField("Radius", capsule.radius);
			capsule.height = EditorGUILayout.FloatField("Height", capsule.height);
		}
	}
}