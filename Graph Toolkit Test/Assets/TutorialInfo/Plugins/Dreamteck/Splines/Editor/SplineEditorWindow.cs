using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class SplineEditorWindow : EditorWindow
	{
		protected UnityEditor.Editor m_editor;
		protected SplineComputerEditor m_splineEditor;

		public void Init(UnityEditor.Editor e, string inputTitle, Vector2 min, Vector2 max)
		{
			minSize = min;
			maxSize = max;
			Init(e, inputTitle);
		}

		public void Init(UnityEditor.Editor e, Vector2 min, Vector2 max)
		{
			minSize = min;
			maxSize = max;
			Init(e);
		}

		public void Init(UnityEditor.Editor e, Vector2 size)
		{
			minSize = maxSize = size;
			Init(e);
		}

		public void Init(UnityEditor.Editor e, string inputTitle)
		{
			Init(e);
			Title(inputTitle);
		}

		public void Init(UnityEditor.Editor e)
		{
			m_editor = e;
			if (m_editor is SplineComputerEditor)
			{
				m_splineEditor = (SplineComputerEditor)m_editor;
			}
			else
			{
				m_splineEditor = null;
			}

			Title(GetTitle());
			OnInitialize();
		}

		protected virtual void OnInitialize()
		{
		}

		protected virtual string GetTitle()
		{
			return "Spline Editor Window";
		}

		private void Title(string inputTitle)
		{
			titleContent = new GUIContent(inputTitle);
		}
	}
}