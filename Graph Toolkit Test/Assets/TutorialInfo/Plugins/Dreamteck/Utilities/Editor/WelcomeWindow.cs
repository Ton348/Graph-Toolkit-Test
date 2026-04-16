using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Dreamteck
{
	public class WelcomeWindow : EditorWindow
	{
		public delegate void EmptyHandler();

		protected static GUIStyle s_wrapText;
		protected static GUIStyle s_buttonTitleText;
		protected static GUIStyle s_warningText;
		protected static GUIStyle s_titleText;
		private static bool s_init = true;
		protected Data m_bannerData;
		protected bool m_hasSentImageRequest;
		protected Texture2D m_header;
		protected string m_headerTitle = "";
		protected WindowPanel[] m_panels = new WindowPanel[0];
		protected List<UnityWebRequest> m_textureWebRequests;
		protected virtual Vector2 windowSize => new(450, 500);

		protected void OnEnable()
		{
			s_init = true;
		}

		public virtual void Load()
		{
			minSize = maxSize = windowSize;
			s_buttonTitleText = new GUIStyle(GUI.skin.GetStyle("label"));
			s_buttonTitleText.fontStyle = FontStyle.Bold;
			s_titleText = new GUIStyle(GUI.skin.GetStyle("label"));
			s_titleText.fontSize = 25;
			s_titleText.fontStyle = FontStyle.Bold;
			s_titleText.alignment = TextAnchor.MiddleLeft;
			s_titleText.normal.textColor = Color.white;
			s_warningText = new GUIStyle(GUI.skin.GetStyle("label"));
			s_warningText.fontSize = 18;
			s_warningText.fontStyle = FontStyle.Bold;
			s_warningText.normal.textColor = Color.red;
			s_warningText.alignment = TextAnchor.MiddleCenter;
			s_wrapText = new GUIStyle(GUI.skin.GetStyle("label"));
			s_wrapText.wordWrap = true;
			s_init = false;
		}

		protected virtual void SetTitle(string titleBar, string header)
		{
			titleContent = new GUIContent(titleBar);
			m_headerTitle = header;
		}

		protected virtual void GetHeader()
		{
			m_header = null;
		}

		protected void OnGui()
		{
			if (s_init)
			{
				Load();
			}

			if (m_header == null)
			{
				GetHeader();
			}

			GUI.DrawTexture(new Rect(0, 0, maxSize.x, 82), m_header, ScaleMode.StretchToFill);
			GUI.Label(new Rect(90, 15, Screen.width - 95, 50), m_headerTitle, s_titleText);
			for (var i = 0; i < m_panels.Length; i++)
			{
				m_panels[i].Draw();
			}

			Repaint();
		}

		protected Data LoadBannersData(string url, string savePrefKey)
		{
			var data = default(Data);

			using (UnityWebRequest mainDataReq = UnityWebRequest.Get(url))
			{
				mainDataReq.SendWebRequest();

				while (!mainDataReq.isDone || mainDataReq.result == UnityWebRequest.Result.InProgress)
				{
				}

				if (mainDataReq.result == UnityWebRequest.Result.ProtocolError ||
				    mainDataReq.result == UnityWebRequest.Result.DataProcessingError ||
				    mainDataReq.result == UnityWebRequest.Result.ConnectionError)
				{
					Debug.LogError("An error occured while fetching the banners data.");
				}
				else
				{
					var jObj = JsonUtility.FromJson<Data>(mainDataReq.downloadHandler.text);

					data = new Data();
					data.version = jObj.version;
					data.banners = jObj.banners;

					int currentVersion = EditorPrefs.GetInt(savePrefKey, -1);

					if (currentVersion < 0 || currentVersion < data.version)
					{
						EditorPrefs.SetInt(savePrefKey, data.version);
					}
				}
			}

			return data;
		}

		protected void OnEditorUpdate()
		{
			if (!m_hasSentImageRequest)
			{
				m_hasSentImageRequest = false;
				EditorApplication.update -= OnEditorUpdate;
				return;
			}

			for (var i = 0; i < m_textureWebRequests.Count; i++)
			{
				UnityWebRequest request = m_textureWebRequests[i];

				if (!request.isDone || request.result == UnityWebRequest.Result.InProgress)
				{
					if (request.result == UnityWebRequest.Result.ConnectionError ||
					    request.result == UnityWebRequest.Result.ProtocolError ||
					    request.result == UnityWebRequest.Result.DataProcessingError)
					{
						m_textureWebRequests.RemoveAt(i);
						i--;
						Debug.LogError("A banner request failed for the spline welcome screen! Please investigate!");
					}

					return;
				}
			}

			for (var i = 0; i < m_textureWebRequests.Count; i++)
			{
				UnityWebRequest request = m_textureWebRequests[i];

				if (request.result == UnityWebRequest.Result.Success)
				{
					Texture2D texture = DownloadHandlerTexture.GetContent(request);
					BannerData data = m_bannerData.banners[i];
					var banner = new WindowPanel.Banner(texture, data.title, data.description, 400f, 70f,
						new ActionLink(data.forwardUrl));

					m_panels[0].elements.Add(new WindowPanel.Space(400, 10));
					m_panels[0].elements.Add(banner);
					request.Dispose();
				}
			}

			DrawFooter();
			m_hasSentImageRequest = false;
			m_textureWebRequests.Clear();
			m_textureWebRequests = null;
			EditorApplication.update -= OnEditorUpdate;
		}

		protected virtual void DrawFooter()
		{
		}

		public class WindowPanel
		{
			public enum SlideDiretion
			{
				Left,
				Right,
				Up,
				Down
			}

			public WindowPanel back;
			public SlideDiretion closeDirection = SlideDiretion.Right;
			public List<Element> elements = new();
			private bool m_goingBack;
			private bool m_open;
			private Vector2 m_origin = Vector2.zero;
			public SlideDiretion openDirection = SlideDiretion.Left;
			public float slideDuration = 1f;
			public float slideStart;

			public WindowPanel(string title, bool o, float slideDur = 1f)
			{
				slideDuration = slideDur;
				SetState(o, false);
			}

			public WindowPanel(string title, bool o, WindowPanel backPanel, float slideDur = 1f)
			{
				slideDuration = slideDur;
				SetState(o, false);
				back = backPanel;
			}

			public bool isActive => m_open || Time.realtimeSinceStartup - slideStart <= slideDuration;

			public void Back()
			{
				Close(true, true);
				back.Open(true, true);
			}

			public void Close(bool useTransition, bool goBack = false)
			{
				SetState(false, useTransition, goBack);
			}

			public void Open(bool useTransition, bool goBack = false)
			{
				m_goingBack = false;
				SetState(true, useTransition, goBack);
			}

			private Vector2 GetSize()
			{
				return new Vector2(Screen.width, Screen.height - 82);
			}

			private void HandleOrigin()
			{
				float percent = Mathf.Clamp01((Time.realtimeSinceStartup - slideStart) / slideDuration);
				Vector2 size = GetSize();
				SlideDiretion dir = openDirection;
				if (m_goingBack)
				{
					dir = closeDirection;
				}

				if (m_open)
				{
					switch (dir)
					{
						case SlideDiretion.Left:
							m_origin.x = Mathf.SmoothStep(size.x, 0f, percent);
							m_origin.y = 0f;
							break;

						case SlideDiretion.Right:
							m_origin.x = Mathf.SmoothStep(-size.x, 0f, percent);
							m_origin.y = 0f;
							break;

						case SlideDiretion.Up:
							m_origin.x = 0f;
							m_origin.y = Mathf.SmoothStep(size.y, 0f, percent);
							break;

						case SlideDiretion.Down:
							m_origin.x = 0f;
							m_origin.y = Mathf.SmoothStep(-size.y, 0f, percent);
							break;
					}
				}
				else
				{
					switch (dir)
					{
						case SlideDiretion.Left:
							m_origin.x = Mathf.SmoothStep(0f, -size.x, percent);
							m_origin.y = 0f;
							break;

						case SlideDiretion.Right:
							m_origin.x = Mathf.SmoothStep(0f, size.x, percent);
							m_origin.y = 0f;
							break;

						case SlideDiretion.Up:
							m_origin.x = 0f;
							m_origin.y = Mathf.SmoothStep(0f, -size.y, percent);
							break;

						case SlideDiretion.Down:
							m_origin.x = 0f;
							m_origin.y = Mathf.SmoothStep(0f, -size.y, percent);
							break;
					}
				}
			}

			private void SetState(bool state, bool useTransition, bool goBack = false)
			{
				if (m_open == state)
				{
					return;
				}

				m_open = state;
				if (useTransition)
				{
					slideStart = Time.realtimeSinceStartup;
				}
				else
				{
					slideStart = Time.realtimeSinceStartup + slideDuration;
				}

				m_goingBack = goBack;
			}

			public void Draw()
			{
				if (!isActive)
				{
					return;
				}

				HandleOrigin();
				Vector2 size = GetSize();
				GUILayout.BeginArea(new Rect(m_origin.x + 25, m_origin.y + 85, size.x - 25, size.y));
				//Back button
				if (back != null)
				{
					if (GUILayout.Button("◄", GUILayout.Width(45), GUILayout.Height(25)))
					{
						Back();
					}
				}

				for (var i = 0; i < elements.Count; i++)
				{
					elements[i].Draw();
				}

				GUILayout.EndArea();
			}

			public class Element
			{
				public ActionLink action;
				protected Vector2 m_size = Vector2.zero;

				public Element(float x, float y, ActionLink a = null)
				{
					m_size = new Vector2(x, y);
					action = a;
				}

				internal virtual void Draw()
				{
				}
			}

			public class Space : Element
			{
				public Space(float x, float y) : base(x, y)
				{
				}

				internal override void Draw()
				{
					GUILayoutUtility.GetRect(m_size.x, m_size.y);
				}
			}

			public class Button : Element
			{
				private readonly string m_text = "";

				public Button(float x, float y, string t, ActionLink a) : base(x, y, a)
				{
					m_text = t;
				}

				internal override void Draw()
				{
					base.Draw();
					if (GUILayout.Button(m_text, GUILayout.Width(m_size.x), GUILayout.Height(m_size.y)))
					{
						if (action != null)
						{
							action.Do();
						}
					}
				}
			}

			public class Banner : Element
			{
				private readonly string m_description;
				private readonly Texture m_image;
				private readonly string m_title;

				public Banner(float x, float y, ActionLink a = null) : base(x, y, a)
				{
				}

				public Banner(
					Texture image,
					string title,
					string description,
					float x,
					float y,
					ActionLink a = null) : this(x, y, a)
				{
					m_title = title;
					m_description = description;
					m_image = image;
					m_size = new Vector2(image.width, image.height);
				}

				internal override void Draw()
				{
					Rect rect = GUILayoutUtility.GetRect(m_size.x, m_size.y);

					EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

					GUI.BeginGroup(rect);
					if (GUI.Button(new Rect(0, 0, m_size.x, m_size.y), ""))
					{
						action.Do();
					}

					GUI.DrawTexture(new Rect(Vector2.one, m_size), m_image, ScaleMode.StretchToFill);

					var hoverRect = new Rect(0, 0, m_size.x, m_size.y);
					if (hoverRect.Contains(Event.current.mousePosition))
					{
						EditorGUI.DrawRect(hoverRect, new Color(1, 1, 1, 0.5f));
					}

					var titleStyle = new GUIStyle();
					titleStyle.fontSize = 19;
					titleStyle.fontStyle = FontStyle.Bold;
					titleStyle.alignment = TextAnchor.MiddleLeft;
					titleStyle.normal.textColor = Color.white;
					EditorGUI.DropShadowLabel(new Rect(6, 5, 370 - 65, 18), m_title, titleStyle);

					var descriptionStyle = new GUIStyle();
					descriptionStyle.fontSize = 11;
					descriptionStyle.wordWrap = true;
					descriptionStyle.fontStyle = FontStyle.Bold;
					descriptionStyle.alignment = TextAnchor.MiddleLeft;
					descriptionStyle.normal.textColor = Color.white;

					EditorGUI.DropShadowLabel(new Rect(6, 20, 380, 40), m_description, descriptionStyle);

					GUI.EndGroup();
					GUILayout.Space(5);
				}
			}

			public class Thumbnail : Element
			{
				private readonly Texture2D m_thumbnail;
				private readonly string m_thumbnailName = "";
				private readonly string m_thumbnailPath = "";
				public string description = "";
				public string title = "";

				public Thumbnail(
					string path,
					string fileName,
					string t,
					string d,
					ActionLink a,
					float x = 400,
					float y = 60) : base(x, y, a)
				{
					title = t;
					description = d;
					m_thumbnailPath = path;
					m_thumbnailName = fileName;

					m_thumbnail = ResourceUtility.EditorLoadTexture(m_thumbnailPath, m_thumbnailName);
				}

				internal override void Draw()
				{
					Rect rect = GUILayoutUtility.GetRect(m_size.x, m_size.y);
					Color buttonColor = Color.clear;
					if (rect.Contains(Event.current.mousePosition))
					{
						buttonColor = Color.white;
					}

					GUI.BeginGroup(rect);
					GUI.color = buttonColor;
					if (GUI.Button(new Rect(0, 0, m_size.x, m_size.y), ""))
					{
						action.Do();
					}

					GUI.color = Color.white;
					if (m_thumbnail != null)
					{
						var offset = new Vector2(5, (m_size.y - 50) / 2);
						GUI.DrawTexture(new Rect(offset, Vector2.one * 50), m_thumbnail, ScaleMode.StretchToFill);
					}

					GUI.Label(new Rect(60, 5, 370 - 65, 16), title, s_buttonTitleText);
					GUI.Label(new Rect(60, 20, 370 - 65, 40), description, s_wrapText);
					GUI.EndGroup();
					GUILayout.Space(5);
				}
			}

			public class ScrollText : Element
			{
				private readonly string m_text = "";
				private Vector2 m_scroll = Vector2.zero;

				public ScrollText(float x, float y, string t) : base(x, y)
				{
					m_text = t;
				}

				internal override void Draw()
				{
					base.Draw();
					m_scroll = GUILayout.BeginScrollView(m_scroll, GUILayout.Width(m_size.x),
						GUILayout.MaxHeight(m_size.y));
					EditorGUILayout.LabelField(m_text, s_wrapText, GUILayout.Width(m_size.x - 30));
					GUILayout.EndScrollView();
				}
			}

			public class Label : Element
			{
				private readonly Color m_color;
				private readonly GUIStyle m_style;
				private readonly string m_text = "";

				public Label(string t, GUIStyle s, Color col) : base(400, 30)
				{
					m_color = col;
					m_text = t;
					m_style = s;
				}

				public Label(string t, GUIStyle s, Color col, float x, float y) : base(x, y)
				{
					m_color = col;
					m_text = t;
					m_style = s;
				}

				internal override void Draw()
				{
					base.Draw();
					Color prev = GUI.color;
					GUI.color = m_color;
					if (m_style == null)
					{
						EditorGUILayout.LabelField(m_text, GUILayout.Width(m_size.x), GUILayout.Height(m_size.y));
					}
					else
					{
						EditorGUILayout.LabelField(m_text, m_style, GUILayout.Width(m_size.x),
							GUILayout.Height(m_size.y));
					}

					GUI.color = prev;
				}
			}
		}

		public class ActionLink
		{
			private readonly WindowPanel m_currentPanel;
			private readonly EmptyHandler m_customHandler;
			private readonly WindowPanel m_targetPanel;
			private readonly string m_uRl = "";

			public ActionLink(string u)
			{
				m_uRl = u;
			}

			public ActionLink(EmptyHandler handler)
			{
				m_customHandler = handler;
			}

			public ActionLink(WindowPanel target, WindowPanel current)
			{
				m_currentPanel = current;
				m_targetPanel = target;
			}

			public void Do()
			{
				if (m_customHandler != null)
				{
					m_customHandler();
				}
				else if (m_uRl != "")
				{
					Application.OpenURL(m_uRl);
				}
				else if (m_targetPanel != null && m_currentPanel != null)
				{
					m_currentPanel.Close(true);
					m_targetPanel.Open(true);
				}
			}
		}

		[Serializable]
		public class Data
		{
			public BannerData[] banners;
			public int version;
		}

		[Serializable]
		public class BannerData
		{
			public string title;
			public string description;
			public string bannerUrl;
			public string forwardUrl;
			public int height;
		}
	}
}