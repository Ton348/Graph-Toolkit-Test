using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Dreamteck
{
	public static class DreamteckEditorGui
	{
		private static Texture2D s_blankImage;

		public static readonly Color backgroundColor = new(0.95f, 0.95f, 0.95f);
		public static Color iconColor = Color.black;

		public static readonly Color highlightColor = new(0f, 0.564f, 1f, 1f);
		public static readonly Color highlightContentColor = new(1f, 1f, 1f, 0.95f);


		public static readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f, 0.5f);
		public static readonly Color activeColor = new(1f, 1f, 1f, 1f);

		public static readonly Color baseColor = Color.white;
		public static readonly Color lightColor = Color.white;
		public static readonly Color lightDarkColor = Color.white;
		public static readonly Color darkColor = Color.white;
		public static readonly Color borderColor = Color.white;

		private static readonly List<int> s_layerNumbers = new();

		public static readonly GUIStyle labelText;
		private static float s_scale = -1f;

		static DreamteckEditorGui()
		{
			baseColor = EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255);
			lightColor = EditorGUIUtility.isProSkin ? new Color32(84, 84, 84, 255) : new Color32(222, 222, 222, 255);
			lightDarkColor = EditorGUIUtility.isProSkin
				? new Color32(30, 30, 30, 255)
				: new Color32(180, 180, 180, 255);
			darkColor = EditorGUIUtility.isProSkin ? new Color32(15, 15, 15, 255) : new Color32(152, 152, 152, 255);
			borderColor = EditorGUIUtility.isProSkin ? new Color32(5, 5, 5, 255) : new Color32(100, 100, 100, 255);
			backgroundColor = baseColor;
			backgroundColor -= new Color(0.1f, 0.1f, 0.1f, 0f);
			iconColor = GUI.skin.label.normal.textColor;

			labelText = new GUIStyle(GUI.skin.GetStyle("label"));
			labelText.fontStyle = FontStyle.Bold;
			labelText.alignment = TextAnchor.MiddleRight;
			labelText.normal.textColor = Color.white;
			SetScale(1f);
		}

		public static Texture2D blankImage
		{
			get
			{
				if (s_blankImage == null)
				{
					s_blankImage = new Texture2D(1, 1);
					s_blankImage.SetPixel(0, 0, Color.white);
					s_blankImage.Apply();
				}

				return s_blankImage;
			}
		}

		public static void SetScale(float newScale)
		{
			if (s_scale == newScale)
			{
				return;
			}

			s_scale = newScale;
			labelText.fontSize = Mathf.RoundToInt(12f * s_scale);
		}

		public static void Label(Rect position, string text, bool active = true, GUIStyle style = null)
		{
			if (style == null)
			{
				style = labelText;
			}

			if (!active)
			{
				GUI.color = inactiveColor;
			}
			else
			{
				GUI.color = activeColor;
			}

			GUI.color = new Color(0f, 0f, 0f, GUI.color.a * 0.5f);
			GUI.Label(new Rect(position.x - 1, position.y + 1, position.width, position.height), text, style);
			if (!active)
			{
				GUI.color = inactiveColor;
			}
			else
			{
				GUI.color = activeColor;
			}

			GUI.Label(position, text, style);
		}

		public static LayerMask LayermaskField(string label, LayerMask layerMask)
		{
			string[] layers = InternalEditorUtility.layers;

			s_layerNumbers.Clear();

			for (var i = 0; i < layers.Length; i++)
			{
				s_layerNumbers.Add(LayerMask.NameToLayer(layers[i]));
			}

			var maskWithoutEmpty = 0;
			for (var i = 0; i < s_layerNumbers.Count; i++)
			{
				if (((1 << s_layerNumbers[i]) & layerMask.value) > 0)
				{
					maskWithoutEmpty |= 1 << i;
				}
			}

			maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

			var mask = 0;
			for (var i = 0; i < s_layerNumbers.Count; i++)
			{
				if ((maskWithoutEmpty & (1 << i)) > 0)
				{
					mask |= 1 << s_layerNumbers[i];
				}
			}

			layerMask.value = mask;

			return layerMask;
		}

		public static bool DropArea<T>(Rect rect, out T[] content, bool acceptProjectAssets = false)
		{
			content = new T[0];
			switch (Event.current.type)
			{
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!rect.Contains(Event.current.mousePosition))
					{
						return false;
					}

					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

					if (Event.current.type == EventType.DragPerform)
					{
						DragAndDrop.AcceptDrag();
						var contentList = new List<T>();
						foreach (object dragged_object in DragAndDrop.objectReferences)
						{
							if (dragged_object is GameObject)
							{
								var gameObject = (GameObject)dragged_object;
								if (acceptProjectAssets || !AssetDatabase.Contains(gameObject))
								{
									if (gameObject.GetComponent<T>() != null)
									{
										contentList.Add(gameObject.GetComponent<T>());
									}
								}
							}
						}

						content = contentList.ToArray();
						return true;
					}

					return false;
			}

			return false;
		}


		public static Gradient GradientField(string label, Gradient gradient, params GUILayoutOption[] options)
		{
			return EditorGUILayout.GradientField(label, gradient, options);
		}

		public static void DrawSeparator()
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			Rect rect = GUILayoutUtility.GetRect(Screen.width / 2f, 2f);
			EditorGUI.DrawRect(rect, darkColor);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
	}
}