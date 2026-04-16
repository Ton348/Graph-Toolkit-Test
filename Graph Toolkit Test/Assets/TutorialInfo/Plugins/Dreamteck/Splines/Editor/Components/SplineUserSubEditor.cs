using UnityEditor;

namespace Dreamteck.Splines.Editor
{
	public class SplineUserSubEditor
	{
		public bool alwaysOpen = false;
		protected SplineUserEditor m_editor;

		private bool m_foldout;
		protected string m_title = "";
		protected SplineUser m_user;

		public SplineUserSubEditor(SplineUser user, SplineUserEditor editor)
		{
			m_editor = editor;
			m_user = user;
		}

		public bool isOpen => m_foldout || alwaysOpen;

		public virtual void DrawInspector()
		{
			if (!alwaysOpen)
			{
				m_foldout = EditorGUILayout.Foldout(m_foldout, m_title);
			}
		}

		public virtual void DrawScene()
		{
		}
	}
}