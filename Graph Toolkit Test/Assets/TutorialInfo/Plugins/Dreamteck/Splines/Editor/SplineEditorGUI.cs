using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
	public static class SplineEditorGui
	{
		public static readonly GUIStyle leftButtonStyle, midButtonStyle, rightButtonStyle, boxStyle;

		private static Color s_previousContentColor,
			s_previousBackgroundColor,
			s_highLightBgcolor,
			s_highlightContentColor;


		public static readonly GUIStyle defaultButton;
		public static readonly GUIStyle defaultEditorButton = null;
		public static readonly GUIStyle defaultEditorButtonSelected = null;
		public static readonly GUIStyle dropdownItem;
		public static readonly GUIStyle bigButton;
		public static readonly GUIStyle bigButtonSelected;
		public static readonly GUIStyle labelText;
		private static readonly GUIStyle s_whiteBox;
		private static readonly GUIStyle s_defaultField;
		private static readonly GUIStyle s_smallField;
		public static readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f, 0.3f);
		public static readonly Color textColor = new(0.2f, 0.2f, 0.2f, 1f);
		public static readonly Color activeColor = new(1f, 1f, 1f, 1f);
		public static readonly Color blackColor = new(0, 0, 0, 0.7f);
		public static readonly Color buttonContentColor = Color.black;
		private static bool[] s_controlStates = new bool[0];
		private static string[] s_floatFieldContents = new string[0];
		private static int s_controlIndex;

		public static float scale = 1f;
		private static Texture2D s_white;

		static SplineEditorGui()
		{
			midButtonStyle = new GUIStyle(GUI.skin.button);
			midButtonStyle.margin = new RectOffset(0, 0, midButtonStyle.margin.top, midButtonStyle.margin.bottom);
			midButtonStyle.padding = new RectOffset(3, 3, midButtonStyle.padding.top, midButtonStyle.padding.bottom);

			leftButtonStyle = new GUIStyle(midButtonStyle);
			leftButtonStyle.contentOffset = new Vector2(-leftButtonStyle.border.left * 0.5f, 0f);
			rightButtonStyle = new GUIStyle(midButtonStyle);
			rightButtonStyle.contentOffset = new Vector2(rightButtonStyle.border.right * 0.5f, 0f);

			boxStyle = new GUIStyle(GUI.skin.GetStyle("box"));
			boxStyle.normal.background = DreamteckEditorGui.blankImage;
			boxStyle.margin = new RectOffset(0, 0, 0, 2);

			defaultButton = new GUIStyle(GUI.skin.GetStyle("button"));
			s_whiteBox = new GUIStyle(GUI.skin.GetStyle("box"));
			s_whiteBox.normal.background = white;
			s_defaultField = new GUIStyle(GUI.skin.GetStyle("textfield"));
			s_defaultField.normal.background = white;
			s_defaultField.normal.textColor = Color.white;
			defaultField.alignment = TextAnchor.MiddleLeft;
			s_smallField = new GUIStyle(GUI.skin.GetStyle("textfield"));
			s_smallField.normal.background = white;
			s_smallField.normal.textColor = Color.white;
			s_smallField.alignment = TextAnchor.MiddleLeft;
			s_smallField.clipping = TextClipping.Clip;
			labelText = new GUIStyle(GUI.skin.GetStyle("label"));
			labelText.fontStyle = FontStyle.Bold;
			labelText.alignment = TextAnchor.MiddleRight;
			labelText.normal.textColor = Color.white;
			dropdownItem = new GUIStyle(GUI.skin.GetStyle("button"));
			dropdownItem.normal.background = white;
			dropdownItem.normal.textColor = Color.white;
			dropdownItem.alignment = TextAnchor.MiddleLeft;
			bigButton = new GUIStyle(GUI.skin.GetStyle("button"));
			bigButton.fontStyle = FontStyle.Bold;
			bigButton.normal.textColor = buttonContentColor;
			bigButtonSelected = new GUIStyle(GUI.skin.GetStyle("button"));
			bigButtonSelected.fontStyle = FontStyle.Bold;
			buttonContentColor = defaultButton.normal.textColor;
			//If the button text color is too dark, generate a brightened version
			float avg = (buttonContentColor.r + buttonContentColor.g + buttonContentColor.b) / 3f;
			if (avg <= 0.2f)
			{
				buttonContentColor = new Color(0.2f, 0.2f, 0.2f);
			}

			Rescale();
		}

		public static GUIStyle whiteBox
		{
			get
			{
				if (s_whiteBox.normal.background == null)
				{
					s_whiteBox.normal.background = white;
				}

				return s_whiteBox;
			}
		}

		public static GUIStyle defaultField
		{
			get
			{
				if (s_defaultField.normal.background == null)
				{
					s_defaultField.normal.background = white;
				}

				return s_defaultField;
			}
		}

		public static GUIStyle smallField
		{
			get
			{
				if (s_smallField.normal.background == null)
				{
					s_smallField.normal.background = white;
				}

				return s_smallField;
			}
		}

		public static Texture2D white
		{
			get
			{
				if (s_white == null)
				{
					s_white = new Texture2D(1, 1);
					s_white.SetPixel(0, 0, Color.white);
					s_white.Apply();
				}

				return s_white;
			}
		}

		public static void Update()
		{
			s_controlStates = new bool[0];
			s_floatFieldContents = new string[0];
		}

		public static void Reset()
		{
			s_controlIndex = 0;
		}

		public static void SetHighlightColors(Color background, Color content)
		{
			s_highLightBgcolor = background;
			s_highlightContentColor = content;
		}

		public static bool LeftButton(GUIContent content, bool selected)
		{
			var clicked = false;
			Rect rect = ButtonBegin(selected, leftButtonStyle);
			if (GUI.Button(new Rect(0, 0, rect.width + leftButtonStyle.border.right, rect.height), content,
				    leftButtonStyle))
			{
				clicked = true;
			}

			ButtonEnd();
			return clicked;
		}

		public static bool MidButton(GUIContent content, bool selected)
		{
			var clicked = false;
			Rect rect = ButtonBegin(selected, midButtonStyle);
			if (GUI.Button(
				    new Rect(-midButtonStyle.border.left, 0,
					    rect.width + midButtonStyle.border.left + midButtonStyle.border.right, rect.height), content,
				    midButtonStyle))
			{
				clicked = true;
			}

			ButtonEnd();
			return clicked;
		}

		public static bool RightButton(GUIContent content, bool selected)
		{
			var clicked = false;
			Rect rect = ButtonBegin(selected, rightButtonStyle);
			if (GUI.Button(
				    new Rect(-rightButtonStyle.border.left, 0, rect.width + rightButtonStyle.border.left, rect.height),
				    content, rightButtonStyle))
			{
				clicked = true;
			}

			ButtonEnd();
			return clicked;
		}

		private static Rect ButtonBegin(bool selected, GUIStyle style)
		{
			s_previousContentColor = GUI.contentColor;
			s_previousBackgroundColor = GUI.backgroundColor;
			GUI.contentColor = style.normal.textColor;
			if (selected)
			{
				GUI.backgroundColor = s_highLightBgcolor;
				GUI.contentColor = s_highlightContentColor;
			}

			Rect rect = GUILayoutUtility.GetRect(30f, 22f);
			GUI.BeginGroup(rect);
			return rect;
		}

		public static void ButtonEnd()
		{
			GUI.EndGroup();
			GUI.contentColor = s_previousContentColor;
			GUI.backgroundColor = s_previousBackgroundColor;
		}

		public static int ButtonRibbon(GUIContent[] contents, float buttonWidth, int highLighted = -1)
		{
			int selected = -1;
			float width = contents.Length * buttonWidth;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			for (var i = 0; i < contents.Length; i++)
			{
				if (i == 0)
				{
					if (LeftButton(contents[i], highLighted == i))
					{
						selected = i;
					}
				}
				else if (i == contents.Length - 1)
				{
					if (RightButton(contents[i], highLighted == i))
					{
						selected = i;
					}
				}
				else
				{
					if (MidButton(contents[i], highLighted == i))
					{
						selected = i;
					}
				}
			}

			EditorGUILayout.EndHorizontal();
			return selected;
		}


		public static void BeginContainerBox(ref bool open, string name)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUI.indentLevel++;
			GUI.color = new Color(1f, 1f, 1f, 0.7f);
			open = Foldout(open, name, true);
			GUI.color = Color.white;
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}

		public static void EndContainerBox()
		{
			EditorGUILayout.EndVertical();
		}

		public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick)
		{
			Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.foldout);
			return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, EditorStyles.foldout);
		}

		public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick)
		{
			return Foldout(foldout, new GUIContent(content), toggleOnLabelClick);
		}

		private static void Rescale()
		{
			defaultField.padding = new RectOffset(Mathf.RoundToInt(5 * scale), Mathf.RoundToInt(5 * scale),
				Mathf.RoundToInt(5 * scale), Mathf.RoundToInt(5 * scale));
			smallField.padding = new RectOffset(Mathf.RoundToInt(2 * scale), Mathf.RoundToInt(2 * scale),
				Mathf.RoundToInt(2 * scale), Mathf.RoundToInt(2 * scale));
			dropdownItem.padding = new RectOffset(Mathf.RoundToInt(10 * scale), 0, 0, 0);
			bigButton.padding = new RectOffset(Mathf.RoundToInt(3 * scale), Mathf.RoundToInt(3 * scale),
				Mathf.RoundToInt(3 * scale), Mathf.RoundToInt(3 * scale));
			bigButtonSelected.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
			bigButton.padding = new RectOffset(Mathf.RoundToInt(4 * scale), Mathf.RoundToInt(4 * scale),
				Mathf.RoundToInt(4 * scale), Mathf.RoundToInt(4 * scale));
			bigButton.fontSize = Mathf.RoundToInt(30 * scale);
			bigButtonSelected.fontSize = Mathf.RoundToInt(30 * scale);
			defaultButton.fontSize = Mathf.RoundToInt(14 * scale);
			dropdownItem.fontSize = Mathf.RoundToInt(12 * scale);
			labelText.fontSize = Mathf.RoundToInt(12 * scale);
			defaultField.fontSize = Mathf.RoundToInt(14 * scale);
			smallField.fontSize = Mathf.RoundToInt(11 * scale);
		}

		public static void SetScale(float s)
		{
			if (s != scale)
			{
				scale = s;
				Rescale();
			}

			scale = s;
		}

		public static bool EditorLayoutSelectableButton(
			GUIContent content,
			bool active = true,
			bool selected = false,
			params GUILayoutOption[] options)
		{
			Color prevColor = GUI.color;
			Color prevContentColor = GUI.contentColor;
			Color prevBackgroundColor = GUI.backgroundColor;
			GUIStyle selectedStyle = GUI.skin.button;
			if (!active)
			{
				GUI.color = inactiveColor;
			}
			else
			{
				GUI.color = activeColor;
				if (selected)
				{
					GUI.backgroundColor = s_highLightBgcolor;
					GUI.contentColor = s_highlightContentColor;
					selectedStyle = new GUIStyle(selectedStyle);
					selectedStyle.normal.textColor = Color.white;
					selectedStyle.hover.textColor = Color.white;
					selectedStyle.active.textColor = Color.white;
				}
				else
				{
					GUI.contentColor = buttonContentColor;
				}
			}

			bool clicked = GUILayout.Button(content, selectedStyle, options);
			GUI.color = prevColor;
			GUI.contentColor = prevContentColor;
			GUI.backgroundColor = prevBackgroundColor;
			return clicked && active;
		}

		private static string CleanStringForFloat(string input)
		{
			if (Regex.Match(input, @"^-?[0-9]*(?:\.[0-9]*)?$").Success)
			{
				return input;
			}

			return "0";
		}

		private static void HandleControlsCount()
		{
			if (s_controlIndex >= s_controlStates.Length)
			{
				var newStates = new bool[s_controlStates.Length + 1];
				s_controlStates.CopyTo(newStates, 0);
				s_controlStates = newStates;

				var newContents = new string[s_controlStates.Length + 1];
				s_floatFieldContents.CopyTo(newContents, 0);
				s_floatFieldContents = newContents;
			}
		}

#if DREAMTECK_SPLINES
		public static double ScreenPointToSplinePercent(SplineComputer computer, Vector2 screenPoint)
		{
			SplinePoint[] points = computer.GetPoints();
			float closestDistance = (screenPoint - HandleUtility.WorldToGUIPoint(points[0].position)).sqrMagnitude;
			var closestPercent = 0.0;
			double add = computer.moveStep;
			if (computer.type == Spline.Type.Linear)
			{
				add /= 2f;
			}

			var count = 0;
			for (double i = add; i < 1.0; i += add)
			{
				SplineSample result = computer.Evaluate(i);
				Vector2 point = HandleUtility.WorldToGUIPoint(result.position);
				float dist = (point - screenPoint).sqrMagnitude;
				if (dist < closestDistance)
				{
					closestDistance = dist;
					closestPercent = i;
				}

				count++;
			}

			return closestPercent;
		}
#endif
	}
}