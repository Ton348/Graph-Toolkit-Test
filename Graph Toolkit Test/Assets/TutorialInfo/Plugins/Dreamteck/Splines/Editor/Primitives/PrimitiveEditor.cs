using System;
using Dreamteck.Splines.Editor;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
	[Serializable]
	public class PrimitiveEditor
	{
		[NonSerialized]
		protected DreamteckSplinesEditor m_editor;

		protected SplinePrimitive m_primitive = new();

		[NonSerialized]
		public Vector3 origin = Vector3.zero;

		public virtual string GetName()
		{
			return "Primitive";
		}

		public virtual void Open(DreamteckSplinesEditor editor)
		{
			m_editor = editor;
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
			else
			{
				m_primitive.rotation = EditorGUILayout.Vector3Field("Rotation", m_primitive.rotation);
			}
		}
	}
}