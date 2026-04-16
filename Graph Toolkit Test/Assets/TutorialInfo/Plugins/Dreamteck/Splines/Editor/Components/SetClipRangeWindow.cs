using System;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public class ClipRangeWindow : EditorWindow
	{
		private float m_from;
		private float m_length;
		private Action<float, float> m_rcv;
		private float m_to;

		public void Init(Action<float, float> receiver, float fromDistance, float toDistance, float totalLength)
		{
			m_rcv = receiver;
			m_length = totalLength;
			m_from = fromDistance;
			m_to = toDistance;
			titleContent = new GUIContent("Set Clip Range Distances");
			minSize = maxSize = new Vector2(240, 120);
		}

		private void OnGui()
		{
			if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter ||
			                                                Event.current.keyCode == KeyCode.Return))
			{
				m_rcv(m_from, m_to);
				Close();
			}

			m_from = EditorGUILayout.FloatField("From ", m_from);
			if (m_from < 0f)
			{
				m_from = 0f;
			}
			else if (m_from > m_length)
			{
				m_from = m_length;
			}

			m_to = EditorGUILayout.FloatField("To ", m_to);
			if (m_to < 0f)
			{
				m_to = 0f;
			}
			else if (m_to > m_length)
			{
				m_to = m_length;
			}

			EditorGUILayout.HelpBox("Enter the distance and press Enter. Current spline length: " + m_length,
				MessageType.Info);
		}
	}
}