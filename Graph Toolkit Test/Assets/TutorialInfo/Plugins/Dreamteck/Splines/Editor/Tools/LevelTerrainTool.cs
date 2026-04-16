using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines
{
	public class LevelTerrainTool : SplineTool
	{
		public float clipFrom;
		public float clipTo = 1f;
		public int feather = 1;
		private Texture2D m_basePreview;
		private Texture2D m_brushPreview;
		private Texture2D m_drawPreview;
		private float[,] m_heights;


		private bool m_init;

		private float m_maxDrawHeight;

		private Terrain m_terrain;
		public float offset;

		public float size = 1f;

		public override string GetName()
		{
			return "Level Terrain";
		}

		protected override string GetPrefix()
		{
			return "LevelTerrainTool";
		}


		private void GetSplinesAndTerrain()
		{
			if (m_splines.Count == 0)
			{
				GetSplines();
			}

			for (var i = 0; i < Selection.gameObjects.Length; i++)
			{
				if (m_terrain == null)
				{
					m_terrain = Selection.gameObjects[i].GetComponent<Terrain>();
				}
			}

			Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
			if (terrains.Length == 1)
			{
				//if there is only one terrain in the scene, automatically select it
				m_terrain = terrains[0];
			}
		}

		private void OnGui()
		{
			// Draw();
		}

		public override void Open(EditorWindow window)
		{
			base.Open(window);
			GetSplinesAndTerrain();
		}

		public override void Close()
		{
			base.Close();
			if (m_promptSave)
			{
				if (EditorUtility.DisplayDialog("Apply changes?",
					    "Changes to the terrain have been made. Do you want to keep them?", "Yes", "No"))
				{
					SaveChanges();
				}
				else
				{
					RevertToBase();
				}
			}
		}

		public override void Draw(Rect windowRect)
		{
			base.Draw(windowRect);

			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			m_terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", m_terrain, typeof(Terrain), true);
			if (EditorGUI.EndChangeCheck())
			{
				m_heights = null;
			}

			if (m_splines.Count == 0)
			{
				EditorGUILayout.HelpBox("No spline selected! Select an object with a SplineComputer component.",
					MessageType.Warning);
			}

			if (m_terrain == null)
			{
				EditorGUILayout.HelpBox("No terrain selected! You need to select a terrain.", MessageType.Warning);
			}

			if (m_splines.Count == 0 || m_terrain == null)
			{
				return;
			}

			if (!m_init)
			{
				m_init = true;
				m_brushPreview = GenerateBrushThumbnail();
			}

			if (m_heights == null)
			{
				GetBase();
			}

			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			float lastSize = size;
			size = EditorGUILayout.FloatField("Brush radius", size);
			if (size < 0f)
			{
				size = 0f;
			}

			if (lastSize != size)
			{
				m_brushPreview = GenerateBrushThumbnail();
			}

			int lastBlur = feather;
			int maxFeatherCount = Mathf.Max(m_heights.GetLength(0) / 64, 2);
			feather = EditorGUILayout.IntSlider("Feather", feather, 0, maxFeatherCount);
			if (lastBlur != feather)
			{
				m_brushPreview = GenerateBrushThumbnail();
			}

			GUILayout.EndVertical();
			GUILayout.Box("", GUILayout.Width(64), GUILayout.Height(64));
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.DrawTexture(rect, m_brushPreview);
			GUILayout.EndHorizontal();
			offset = EditorGUILayout.FloatField("Height offset", offset);
			EditorGUILayout.MinMaxSlider(new GUIContent("Spline range"), ref clipFrom, ref clipTo, 0f, 1f);
			if (GUILayout.Button("Level"))
			{
				CarveTerrain();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("Terrain heightmap:");
			GUILayout.Label("Path heightmap:");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Box("", GUILayout.Width((windowRect.width - 10) / 2),
				GUILayout.Height((windowRect.width - 10) / 2));
			rect = GUILayoutUtility.GetLastRect();
			GUI.DrawTexture(rect, m_basePreview);
			GUILayout.Box("", GUILayout.Width((windowRect.width - 10) / 2),
				GUILayout.Height((windowRect.width - 10) / 2));
			rect = GUILayoutUtility.GetLastRect();
			GUI.DrawTexture(rect, m_drawPreview);
			GUILayout.EndHorizontal();

			if (m_promptSave)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Revert"))
				{
					RevertToBase();
				}

				if (GUILayout.Button("Apply"))
				{
					SaveChanges();
				}

				GUILayout.EndHorizontal();
			}
		}

		private void OnFocus()
		{
			GetSplinesAndTerrain();
			if (m_promptSave)
			{
				var isChanged = false;
				float[,] newHeights = m_terrain.terrainData.GetHeights(0, 0, m_terrain.terrainData.heightmapResolution,
					m_terrain.terrainData.heightmapResolution);
				if (newHeights.GetLength(0) != m_heights.GetLength(0) ||
				    newHeights.GetLength(1) != m_heights.GetLength(1))
				{
					isChanged = true;
				}
				else
				{
					for (var x = 0; x < m_heights.GetLength(0); x++)
					{
						for (var y = 0; y < m_heights.GetLength(1); y++)
						{
							if (m_heights[x, y] != newHeights[x, y])
							{
								isChanged = true;
								break;
							}
						}
					}
				}

				if (isChanged)
				{
					if (EditorUtility.DisplayDialog("Preserve terrain ?",
						    "The terrain has been edited from outside. Do you want to load the new height data? \r\n WARNING: Doing so will apply your leveled data to the terrain.",
						    "Yes", "No"))
					{
						GetBase();
					}
				}
			}
		}

		private void OnLostFocus()
		{
			// RevertToBase();
		}

		private void CarveTerrain()
		{
			var drawLayer = new float[m_terrain.terrainData.heightmapResolution,
				m_terrain.terrainData.heightmapResolution];
			var alphaLayer = new float[m_terrain.terrainData.heightmapResolution,
				m_terrain.terrainData.heightmapResolution];
			Undo.RecordObject(m_terrain, "Carve");
			for (var i = 0; i < m_splines.Count; i++)
			{
				PaintHeightMap(m_terrain, m_splines[i], ref drawLayer, ref alphaLayer);
			}

			var blurLayer = new float[drawLayer.GetLength(0), drawLayer.GetLength(1)];
			GaussBlur(ref drawLayer, ref blurLayer, feather);
			var blurAlphaLayer = new float[drawLayer.GetLength(0), drawLayer.GetLength(1)];
			GaussBlur(ref alphaLayer, ref blurAlphaLayer, feather);
			var finalLayer = new float[drawLayer.GetLength(0), drawLayer.GetLength(1)];


			Color[] pixels = m_drawPreview.GetPixels();


			m_drawPreview = new Texture2D(drawLayer.GetLength(0), drawLayer.GetLength(1));
			for (var x = 0; x < drawLayer.GetLength(0); x++)
			{
				for (var y = 0; y < drawLayer.GetLength(1); y++)
				{
					finalLayer[x, y] = Mathf.Lerp(m_heights[x, y], blurLayer[x, y], blurAlphaLayer[x, y]);
					pixels[x * m_drawPreview.width + y] = Color.Lerp(Color.black, Color.white,
						blurLayer[x, y] / m_maxDrawHeight * blurAlphaLayer[x, y]);
				}
			}

			m_terrain.terrainData.SetHeights(0, 0, finalLayer);
			m_drawPreview.SetPixels(pixels);
			m_drawPreview.Apply();
		}

		private Texture2D GenerateBrushThumbnail()
		{
			var tex = new Texture2D(65, 65, TextureFormat.RGB24, false);
			Color[] colors = tex.GetPixels();
			for (var i = 0; i < colors.Length; i++)
			{
				colors[i] = Color.white;
			}

			//Get the brush size, compared to the blur amount
			int hmSize = ToHeightmapSize(size);
			var percent = 1f;
			if (hmSize > 0)
			{
				percent = Mathf.Clamp01((float)(feather * feather) / hmSize);
			}

			int r = Mathf.RoundToInt(30 * (1f - percent));
			var center = 32;
			for (int x = center - 30; x <= center; x++)
			{
				for (int y = center - 30; y <= center; y++)
				{
					float value = (x - center) * (x - center) + (y - center) * (y - center);
					int xSym = center - (x - center);
					int ySym = center - (y - center);

					if (value <= r * r)
					{
						colors[x * tex.width + y] = Color.black;
						colors[xSym * tex.width + y] = Color.black;
						colors[x * tex.width + ySym] = Color.black;
						colors[xSym * tex.width + ySym] = Color.black;
					}
					else if (value <= 30 * 30 && value > r * r)
					{
						float rr = r * r;
						float val = value - rr;
						float div = 30 * 30 - rr;
						float alpha = Mathf.Clamp01(val / div);
						//Debug.Log(val + "/" + div + " = " + alpha);
						Color col = Color.Lerp(Color.black, Color.white, alpha);
						colors[x * tex.width + y] = col;
						colors[xSym * tex.width + y] = col;
						colors[x * tex.width + ySym] = col;
						colors[xSym * tex.width + ySym] = col;
					}

					if (value <= 30 * 30 && value >= 29 * 29)
					{
						Color col = Color.Lerp(Color.gray, Color.white, 1f - percent);
						colors[x * tex.width + y] = col;
						colors[xSym * tex.width + y] = col;
						colors[x * tex.width + ySym] = col;
						colors[xSym * tex.width + ySym] = col;
					}
				}
			}


			tex.SetPixels(colors);
			tex.Apply();
			return tex;
		}

		private void GetBase()
		{
			GetSplinesAndTerrain();
			if (m_terrain == null)
			{
				return;
			}

			m_heights = m_terrain.terrainData.GetHeights(0, 0, m_terrain.terrainData.heightmapResolution,
				m_terrain.terrainData.heightmapResolution);
			m_basePreview = new Texture2D(m_heights.GetLength(0), m_heights.GetLength(1));
			m_drawPreview = new Texture2D(m_heights.GetLength(0), m_heights.GetLength(1));
			var pixels = new Color[m_basePreview.width * m_basePreview.height];
			var blackPixels = new Color[m_basePreview.width * m_basePreview.height];
			var maxHeight = 0f;
			for (var x = 0; x < m_basePreview.width; x++)
			{
				for (var y = 0; y < m_basePreview.height; y++)
				{
					if (m_heights[x, y] > maxHeight)
					{
						maxHeight = m_heights[x, y];
					}

					pixels[x * m_basePreview.width + y] = Color.Lerp(Color.black, Color.white, m_heights[x, y]);
					blackPixels[x * m_basePreview.width + y] = Color.black;
				}
			}

			if (maxHeight > 0f)
			{
				for (var x = 0; x < m_basePreview.width; x++)
				{
					for (var y = 0; y < m_basePreview.height; y++)
					{
						pixels[x * m_basePreview.width + y] /= maxHeight;
					}
				}
			}

			m_basePreview.SetPixels(pixels);
			m_basePreview.Apply();
			m_drawPreview.SetPixels(blackPixels);
			m_drawPreview.Apply();
			m_promptSave = false;
		}

		private void SaveChanges()
		{
			GetBase();
		}

		private void RevertToBase()
		{
			if (m_terrain == null)
			{
				return;
			}

			m_terrain.terrainData.SetHeights(0, 0, m_heights);
			m_heights = null;
		}

		private void PaintHeightMap(
			Terrain terrain,
			SplineComputer computer,
			ref float[,] drawLayer,
			ref float[,] alphaLayer)
		{
			if (m_heights == null)
			{
				GetBase();
			}

			var results = new SplineSample[computer.iterations];
			computer.Evaluate(ref results, clipFrom, clipTo);
			Draw(results, ref drawLayer, ref alphaLayer);
		}


		private int ToHeightmapSize(float value)
		{
			float avgSize = (m_terrain.terrainData.size.x + m_terrain.terrainData.size.z) / 2f;
			int result = Mathf.RoundToInt(value / avgSize * m_terrain.terrainData.heightmapResolution);
			return result;
		}

		private Point ToHeightmapCoords(Vector3 pos)
		{
			Vector3 terrainPos = pos - m_terrain.transform.position;
			terrainPos.x /= m_terrain.terrainData.size.x;
			terrainPos.z /= m_terrain.terrainData.size.z;
			terrainPos.x = Mathf.Clamp01(terrainPos.x);
			terrainPos.z = Mathf.Clamp01(terrainPos.z);
			int x = Mathf.RoundToInt(terrainPos.z * m_terrain.terrainData.heightmapResolution);
			int y = Mathf.RoundToInt(terrainPos.x * m_terrain.terrainData.heightmapResolution);
			return new Point(x, y);
		}

		private float ToHeightmapValue(float y)
		{
			float terrainHeight = y - m_terrain.transform.position.y;
			terrainHeight /= m_terrain.terrainData.size.y;
			return terrainHeight;
		}

		private void PaintSegment(
			TerrainPaintPoint fromPoint,
			TerrainPaintPoint toPoint,
			ref float[,] layer,
			ref float[,] alphaLayer,
			bool writeAlpha = true,
			bool overWriteHeight = true)
		{
			//Flip the points if the forward one has a bigger radius so the lerp can work well
			if (Vector2.Distance(fromPoint.leftPoint.vector, fromPoint.rightPoint.vector) <
			    Vector2.Distance(toPoint.leftPoint.vector, toPoint.rightPoint.vector))
			{
				TerrainPaintPoint temp = fromPoint;
				fromPoint = toPoint;
				toPoint = temp;
			}

			var drawn = new List<Point>();
			Vector2 currentPosition = fromPoint.leftPoint.vector;
			Vector2 fromRight = fromPoint.rightPoint.vector;

			var alphaStartPercent = 0f;
			var alphaEndPercent = 1f;
			if (feather > 0)
			{
				currentPosition += (fromPoint.leftPoint.vector - fromPoint.center.vector).normalized * feather * 4f;
				fromRight += (fromPoint.rightPoint.vector - fromPoint.center.vector).normalized * feather * 4f;
				float span = (fromPoint.leftPoint.vector - fromPoint.rightPoint.vector).magnitude /
				             (fromRight - currentPosition).magnitude;
				float rest = (1f - span) / 2f;
				alphaStartPercent = rest;
				alphaEndPercent = 1f - rest;
			}

			float armLength = Vector2.Distance(currentPosition, fromRight);
			if (armLength < 1f)
			{
				return;
			}

			while (true)
			{
				float armDistance = Vector2.Distance(currentPosition, fromRight);
				float armPercent = 1f - armDistance / armLength;
				//This can be optimized, take it outside of the cycle
				var fromPos = new Point(currentPosition);
				Vector2 leftvector = toPoint.leftPoint.vector;
				Vector2 rightVector = toPoint.rightPoint.vector;
				if (feather > 0)
				{
					leftvector += (toPoint.leftPoint.vector - toPoint.center.vector).normalized * feather * 4f;
					rightVector += (toPoint.rightPoint.vector - toPoint.center.vector).normalized * feather * 4f;
				}

				Vector2 toArm = Vector2.Lerp(leftvector, rightVector, armPercent);

				var toPos = new Point(toArm);
				int dx = Mathf.Abs(toPos.x - fromPos.x), sx = fromPos.x < toPos.x ? 1 : -1;
				int dy = -Mathf.Abs(toPos.y - fromPos.y), sy = fromPos.y < toPos.y ? 1 : -1;
				int err = dx + dy, e2;
				Point current = fromPos;
				var target = new Vector2(toPos.x - fromPos.x, toPos.y - fromPos.y);

				float fromHeight = fromPoint.GetHeight(armPercent);
				float toHeight = toPoint.GetHeight(armPercent);
				while (true)
				{
					if (current.x >= 0 && current.x < layer.GetLength(0) && current.y >= 0 &&
					    current.y < layer.GetLength(1))
					{
						if (overWriteHeight || layer[current.x, current.y] == 0f)
						{
							if (!ContainsPoint(ref drawn, current))
							{
								var currentDist = new Vector2(current.x - fromPos.x, current.y - fromPos.y);
								float positionPercent = Mathf.Clamp01(currentDist.magnitude / target.magnitude);
								float height = Mathf.Lerp(fromHeight, toHeight, positionPercent);
								var alphaValue = 0f;
								if (armPercent >= alphaStartPercent && armPercent <= alphaEndPercent)
								{
									alphaValue = 1f;
								}

								if (writeAlpha)
								{
									Plot(current.x, current.y, height, alphaValue, ref alphaLayer, ref layer);
								}
								else
								{
									Plot(current.x, current.y, height, alphaLayer[current.x, current.y], ref alphaLayer,
										ref layer);
								}

								drawn.Add(current);
							}
						}
					}

					if (current.x == toPos.x && current.y == toPos.y)
					{
						break;
					}

					e2 = 2 * err;
					if (e2 > dy)
					{
						err += dy;
						current.x += sx;
					}
					else if (e2 < dx)
					{
						err += dx;
						current.y += sy;
					}
				}

				if (currentPosition == fromRight)
				{
					break;
				}

				currentPosition = Vector2.MoveTowards(currentPosition, fromRight, 1f);
			}
		}

		private bool ContainsPoint(ref List<Point> list, Point point)
		{
			for (var i = 0; i < list.Count; i++)
			{
				if (list[i].x == point.x && list[i].y == point.y)
				{
					return true;
				}
			}

			return false;
		}

		private void Draw(SplineSample[] points, ref float[,] drawLayer, ref float[,] alphaLayer)
		{
			var selectedPoints = new List<SplineSample>();
			var last = new Point();
			//Filter out points that are too close to each other
			for (var i = 0; i < points.Length; i++)
			{
				Point current = ToHeightmapCoords(points[i].position + points[i].up * offset);
				if (i == 0 || i == points.Length - 1)
				{
					last = new Point(current.x, current.y);
					selectedPoints.Add(points[i]);
				}
				else if (Vector2.Distance(new Vector2(current.x, current.y), new Vector2(last.x, last.y)) >= 1.5f)
				{
					selectedPoints.Add(points[i]);
					last = new Point(current.x, current.y);
				}
			}

			if (selectedPoints.Count <= 1)
			{
				return;
			}

			var paintPoints = new TerrainPaintPoint[selectedPoints.Count];
			for (var i = 0; i < selectedPoints.Count; i++)
			{
				ConvertToPaintPoint(selectedPoints[i], ref paintPoints[i]);
			}

			//Paint the points
			for (var i = 0; i < paintPoints.Length - 1; i++)
			{
				m_promptSave = true;
				PaintSegment(paintPoints[i], paintPoints[i + 1], ref drawLayer, ref alphaLayer);
			}

			SplineSample exResult = selectedPoints[0];
			exResult.position += exResult.position - selectedPoints[1].position;
			TerrainPaintPoint exPoint = null;
			ConvertToPaintPoint(exResult, ref exPoint);
			PaintSegment(paintPoints[0], exPoint, ref drawLayer, ref alphaLayer, false, false);

			exResult = selectedPoints[selectedPoints.Count - 1];
			exResult.position += exResult.position - selectedPoints[selectedPoints.Count - 2].position;
			ConvertToPaintPoint(exResult, ref exPoint);
			PaintSegment(paintPoints[paintPoints.Length - 1], exPoint, ref drawLayer, ref alphaLayer, false, false);
			//Extrapolate the ending and the begining
		}

		private TerrainPaintPoint ConvertToPaintPoint(SplineSample result, ref TerrainPaintPoint paintPoint)
		{
			paintPoint = new TerrainPaintPoint();
			Vector3 right = -Vector3.Cross(result.forward, result.up).normalized * size * 0.5f * result.size;
			Vector3 leftPoint = result.position - right + result.up * offset;
			Vector3 rightPoint = result.position + right + result.up * offset;
			paintPoint.center = ToHeightmapCoords(result.position + result.up * offset);
			paintPoint.leftPoint = ToHeightmapCoords(leftPoint);
			paintPoint.rightPoint = ToHeightmapCoords(rightPoint);
			paintPoint.leftHeight = ToHeightmapValue(leftPoint.y);
			paintPoint.rightHeight = ToHeightmapValue(rightPoint.y);
			paintPoint.floatDiameter = Vector2.Distance(new Vector2(leftPoint.x, leftPoint.z),
				new Vector2(rightPoint.x, rightPoint.z));
			if (paintPoint.leftHeight > m_maxDrawHeight)
			{
				m_maxDrawHeight = paintPoint.leftHeight;
			}

			if (paintPoint.rightHeight > m_maxDrawHeight)
			{
				m_maxDrawHeight = paintPoint.rightHeight;
			}

			return paintPoint;
		}


		private Point Project(Point fromPoint, Point toPoint, int x, int y)
		{
			Vector2 dir = toPoint.vector - fromPoint.vector;
			var point = new Vector2(x, y);
			dir.Normalize();
			Vector2 v = point - fromPoint.vector;
			float d = Vector2.Dot(v, dir);
			return new Point(fromPoint.vector + dir * d);
		}

		private void GaussBlur(ref float[,] source, ref float[,] target, int r)
		{
			int w = source.GetLength(0);
			int h = source.GetLength(1);
			int[] bxs = GbgetBoxes(r, 3);
			var flatSource = new float[source.GetLength(0) * source.GetLength(1)];
			var flatTarget = new float[source.GetLength(0) * source.GetLength(1)];
			for (var x = 0; x < source.GetLength(0); x++)
			{
				for (var y = 0; y < source.GetLength(1); y++)
				{
					if (r == 0)
					{
						target[x, y] = source[x, y];
					}
					else
					{
						flatSource[x * source.GetLength(0) + y] = source[x, y];
					}
				}
			}

			if (r == 0)
			{
				return;
			}

			BoxBlur(ref flatSource, ref flatTarget, w, h, (bxs[0] - 1) / 2);
			BoxBlur(ref flatTarget, ref flatSource, w, h, (bxs[1] - 1) / 2);
			BoxBlur(ref flatSource, ref flatTarget, w, h, (bxs[2] - 1) / 2);

			for (var i = 0; i < flatSource.Length; i++)
			{
				int x = Mathf.FloorToInt(i / source.GetLength(0));
				int y = i - x * source.GetLength(0);
				target[x, y] = flatTarget[i];
			}
		}

		private int[] GbgetBoxes(int sigma, int n)
		{
			float wIdeal = Mathf.Sqrt(12 * sigma * sigma / n + 1);
			int wl = Mathf.FloorToInt(wIdeal);
			if (wl % 2 == 0)
			{
				wl--;
			}

			int wu = wl + 2;

			float mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
			float m = Mathf.Round(mIdeal);

			var sizes = new int[n];
			for (var i = 0; i < n; i++)
			{
				sizes[i] = i < m ? wl : wu;
			}

			return sizes;
		}

		private void BoxBlur(ref float[] source, ref float[] target, int w, int h, int r)
		{
			for (var i = 0; i < source.Length; i++)
			{
				target[i] = source[i];
			}

			HorizontalBlur(ref target, ref source, w, h, r);
			VerticalBlur(ref source, ref target, w, h, r);
		}

		private void HorizontalBlur(ref float[] source, ref float[] target, int w, int h, int r)
		{
			float iarr = 1f / (r * 2f + 1f);
			for (var i = 0; i < h; i++)
			{
				int ti = i * w, li = ti, ri = ti + r;
				float fv = source[ti], lv = source[ti + w - 1], val = (r + 1) * fv;
				for (var j = 0; j < r; j++)
				{
					val += source[ti + j];
				}

				for (var j = 0; j <= r; j++)
				{
					val += source[ri++] - fv;
					target[ti++] = val * iarr;
				}

				for (int j = r + 1; j < w - r; j++)
				{
					val += source[ri++] - source[li++];
					target[ti++] = val * iarr;
				}

				for (int j = w - r; j < w; j++)
				{
					val += lv - source[li++];
					target[ti++] = val * iarr;
				}
			}
		}

		private void VerticalBlur(ref float[] source, ref float[] target, int w, int h, int r)
		{
			float iarr = 1f / (r * 2f + 1f);
			for (var i = 0; i < w; i++)
			{
				int ti = i, li = ti, ri = ti + r * w;
				float fv = source[ti], lv = source[ti + w * (h - 1)], val = (r + 1) * fv;
				for (var j = 0; j < r; j++)
				{
					val += source[ti + j * w];
				}

				for (var j = 0; j <= r; j++)
				{
					val += source[ri] - fv;
					target[ti] = val * iarr;
					ri += w;
					ti += w;
				}

				for (int j = r + 1; j < h - r; j++)
				{
					val += source[ri] - source[li];
					target[ti] = val * iarr;
					li += w;
					ri += w;
					ti += w;
				}

				for (int j = h - r; j < h; j++)
				{
					val += lv - source[li];
					target[ti] = val * iarr;
					li += w;
					ti += w;
				}
			}
		}


		private void Plot(int x, int y, float value, float alpha, ref float[,] alphaTarget, ref float[,] target)
		{
			if (x < 0 || x >= target.GetLength(0))
			{
				return;
			}

			if (y < 0 || y >= target.GetLength(1))
			{
				return;
			}

			if (value > target[x, y])
			{
				target[x, y] = value;
				alphaTarget[x, y] = alpha;
			}
		}

		public struct Point
		{
			public int x;
			public int y;

			public Vector2 vector
			{
				get => new(x, y);
				set
				{
					x = (int)value.x;
					y = (int)value.y;
				}
			}

			public Point(int newX, int newY)
			{
				x = newX;
				y = newY;
			}

			public Point(Vector2 input)
			{
				x = Mathf.RoundToInt(input.x);
				y = Mathf.RoundToInt(input.y);
			}
		}

		public class TerrainPaintPoint
		{
			public Point center;
			public float floatDiameter;
			public float leftHeight;
			public Point leftPoint;
			public float rightHeight;
			public Point rightPoint;

			public float GetHeight(float percent)
			{
				return Mathf.Lerp(leftHeight, rightHeight, percent);
			}
		}
	}
}