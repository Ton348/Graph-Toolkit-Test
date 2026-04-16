namespace Dreamteck.Splines.Editor
{
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

    public class CreatePointModule : PointModule
    {
        public enum AppendMode { Beginning = 0, End = 1}
        public enum PlacementMode { YPlane, XPlane, ZPlane, CameraPlane, Surface, Insert }
        public enum NormalMode { Default, LookAtCamera, AlignWithCamera, Calculate, Left, Right, Up, Down, Forward, Back }
        protected PlacementMode m_placementMode = PlacementMode.YPlane;
        public AppendMode appendMode = AppendMode.End;
        public float offset = 0f;
        public NormalMode normalMode = NormalMode.Default;
        public LayerMask surfaceLayerMask = new LayerMask();
        public float createPointSize = 1f;
        public Color createPointColor = Color.white;
        protected Spline m_visualizer;
        protected Camera m_editorCamera;
        protected Vector3 m_createPoint = Vector3.zero, m_createNormal = Vector3.up;
        protected SplineSample m_evalResult = new SplineSample();
        protected int m_lastCreated = -1;

        public CreatePointModule(SplineEditor editor) : base(editor)
        {

        }

        public override GUIContent GetIconOff()
        {
            return IconContent("+", "add", "Add Points");
        }

        public override GUIContent GetIconOn()
        {
            return IconContent("+", "add_on", "Add Points");
        }

        public override void LoadState()
        {
            base.LoadState();
            normalMode = (NormalMode)LoadInt("normalMode");
            m_placementMode = (PlacementMode)LoadInt("placementMode");
            appendMode = (AppendMode)LoadInt("appendMode", 1);
            offset = LoadFloat("offset");
            surfaceLayerMask = LoadInt("surfaceLayerMask", ~0);
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveInt("normalMode", (int)normalMode);
            SaveInt("placementMode", (int)m_placementMode);
            SaveInt("appendMode", (int)appendMode);
            SaveFloat("offset", offset);
            SaveInt("surfaceLayerMask", surfaceLayerMask);
        }

        public override void Deselect()
        {
            base.Deselect();
            GUIUtility.hotControl = -1;
            if (Event.current != null)
            {
                Event.current.Use();
            }
        }

        protected override void OnDrawInspector()
        {
            m_placementMode = (PlacementMode)EditorGUILayout.EnumPopup("Placement Mode", m_placementMode);
            if (m_placementMode != PlacementMode.Insert)
            {
                normalMode = (NormalMode)EditorGUILayout.EnumPopup("Normal Mode", normalMode);
                appendMode = (AppendMode)EditorGUILayout.EnumPopup("Append To", appendMode);
            }
            string offsetLabel = "Grid Offset";
            if (m_placementMode == PlacementMode.CameraPlane) offsetLabel = "Far Plane";
            if (m_placementMode == PlacementMode.Surface) offsetLabel = "Surface Offset";
            offset = EditorGUILayout.FloatField(offsetLabel, offset);
            if (m_placementMode == PlacementMode.Surface)
            {
                surfaceLayerMask = DreamteckEditorGui.LayermaskField("Surface Mask", surfaceLayerMask);
            }
        }

        protected override void OnDrawScene()
        {
            m_editorCamera = SceneView.currentDrawingSceneView.camera;
            bool canCreate = false;
            if (m_placementMode == PlacementMode.CameraPlane)
            {
                GetCreatePointOnPlane(-m_editorCamera.transform.forward, m_editorCamera.transform.position + m_editorCamera.transform.forward * offset, out m_createPoint);
                Handles.color = new Color(1f, 0.78f, 0.12f);
                DrawGrid(m_createPoint, m_editorCamera.transform.forward, Vector2.one * 10, 2.5f);
                Handles.color = Color.white;
                canCreate = true;
                m_createNormal = -m_editorCamera.transform.forward;
            }

            if (m_placementMode == PlacementMode.Surface)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, surfaceLayerMask))
                {
                    canCreate = true;
                    m_createPoint = hit.point + hit.normal * offset;
                    Handles.color = Color.blue;
                    Handles.DrawLine(hit.point, m_createPoint);
                    SplineEditorHandles.DrawRectangle(m_createPoint, Quaternion.LookRotation(-m_editorCamera.transform.forward, m_editorCamera.transform.up), HandleUtility.GetHandleSize(m_createPoint) * 0.1f);
                    Handles.color = Color.white;
                    m_createNormal = hit.normal;
                }
            }

            if (m_placementMode == PlacementMode.XPlane)
            {
                canCreate = AxisGrid(Vector3.right, new Color(0.85f, 0.24f, 0.11f, 0.92f), out m_createPoint);
                m_createNormal = Vector3.right;
            }

            if (m_placementMode == PlacementMode.YPlane)
            {
                canCreate = AxisGrid(Vector3.up, new Color(0.6f, 0.95f, 0.28f, 0.92f), out m_createPoint);
                m_createNormal = Vector3.up;
            }

            if (m_placementMode == PlacementMode.ZPlane)
            {
                canCreate = AxisGrid(Vector3.forward, new Color(0.22f, 0.47f, 0.97f, 0.92f), out m_createPoint);
                m_createNormal = Vector3.back;
            }

            if (m_placementMode == PlacementMode.Insert)
            {
                canCreate = true;
                if (points.Length < 2)
                {
                    m_placementMode = PlacementMode.YPlane;
                }
                else
                {
                    InsertMode(Event.current.mousePosition);
                }
            }
            else if (m_eventModule.mouseLeftDown && canCreate && !m_eventModule.mouseRight && !m_eventModule.alt)
            {
                CreateSplinePoint(m_createPoint, m_createNormal);
            }

            if (m_lastCreated >= 0 && m_lastCreated < points.Length && m_editor.eventModule.mouseLeft)
            {
                Vector3 tangent = points[m_lastCreated].position - m_createPoint;
                if (appendMode == AppendMode.End)
                {
                    tangent = m_createPoint - points[m_lastCreated].position;
                }
                points[m_lastCreated].SetTangent2Position(points[m_lastCreated].position + tangent);
                RegisterChange();
            }
            else if (!m_editor.eventModule.mouseLeft)
            {
                m_lastCreated = -1;
            }


            if (!canCreate) DrawMouseCross();
            UpdateVisualizer();
            SplineDrawer.DrawSpline(m_visualizer, color);
            Repaint();
        }

        protected virtual void CreateSplinePoint(Vector3 position, Vector3 normal)
        {
            GUIUtility.hotControl = -1;
            AddPoint();
        }

        protected void AddPoint()
        {
            SplinePoint newPoint = new SplinePoint(m_createPoint, m_createPoint);
            newPoint.size = createPointSize;
            newPoint.color = createPointColor;
            SplinePoint[] newPoints = m_editor.GetPointsArray();
            if (appendMode == AppendMode.End)
            {
                Dreamteck.ArrayUtility.Add(ref newPoints, newPoint);
                m_lastCreated = newPoints.Length - 1;
            }
            else
            {
                Dreamteck.ArrayUtility.Insert(ref newPoints, 0, newPoint);
                m_lastCreated = 0;
            }

            m_editor.SetPointsArray(newPoints);
            SetPointNormal(m_lastCreated, m_createNormal);
            SelectPoint(m_lastCreated);
            RegisterChange();
        }

        protected void SetPointNormal(int index, Vector3 defaultNormal)
        {
            if (m_editor.is2D)
            {
                points[index].normal = Vector3.back;
                return;
            }
            if (normalMode == NormalMode.Default) points[index].normal = defaultNormal;
            else
            {
                Camera editorCamera = SceneView.lastActiveSceneView.camera;
                switch (normalMode)
                {
                    case NormalMode.AlignWithCamera: points[index].normal = editorCamera.transform.forward; break;
                    case NormalMode.LookAtCamera: points[index].normal = Vector3.Normalize(editorCamera.transform.position - points[index].position); break;
                    case NormalMode.Calculate: PointNormalModule.CalculatePointNormal(points, index, isClosed); break;
                    case NormalMode.Left: points[index].normal = Vector3.left; break;
                    case NormalMode.Right: points[index].normal = Vector3.right; break;
                    case NormalMode.Up: points[index].normal = Vector3.up; break;
                    case NormalMode.Down: points[index].normal = Vector3.down; break;
                    case NormalMode.Forward: points[index].normal = Vector3.forward; break;
                    case NormalMode.Back: points[index].normal = Vector3.back; break;
                }
            }
        }

        protected virtual void InsertMode(Vector3 screenCoordinates)
        {
           
            double percent = ProjectScreenSpace(screenCoordinates);
            m_editor.evaluate(percent, ref m_evalResult);
            if (m_editor.eventModule.mouseRight)
            {
                SplineEditorHandles.DrawCircle(m_evalResult.position, Quaternion.LookRotation(m_editorCamera.transform.position - m_evalResult.position), HandleUtility.GetHandleSize(m_evalResult.position) * 0.2f);
                return;
            }
            if (SplineEditorHandles.CircleButton(m_evalResult.position, Quaternion.LookRotation(m_editorCamera.transform.position - m_evalResult.position), HandleUtility.GetHandleSize(m_evalResult.position) * 0.2f, 1.5f, color))
            {
                SplinePoint newPoint = new SplinePoint(m_evalResult.position, m_evalResult.position);
                newPoint.size = m_evalResult.size;
                newPoint.color = m_evalResult.color;
                newPoint.normal = m_evalResult.up;
                double floatIndex = (points.Length - 1) * percent;
                int pointIndex = Mathf.Clamp(Dmath.FloorInt(floatIndex), 0, points.Length - 2);
                m_editor.AddPointAt(pointIndex + 1);
                points[pointIndex + 1].SetPoint(newPoint); 
                SelectPoint(pointIndex);
                RegisterChange();
            }
        }

        protected double ProjectScreenSpace(Vector2 screenPoint)
        {
            float closestDistance = (screenPoint - HandleUtility.WorldToGUIPoint(points[0].position)).sqrMagnitude;
            double closestPercent = 0.0;
            double moveStep = 1.0 / ((m_editor.points.Length - 1) * sampleRate);
            double add = moveStep;
            if (splineType == Spline.Type.Linear) add /= 2.0;
            int count = 0;
            for (double i = add; i < 1.0; i += add)
            {
                m_editor.evaluate(i, ref m_evalResult);
                Vector2 point = HandleUtility.WorldToGUIPoint(m_evalResult.position);
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

        bool GetCreatePointOnPlane(Vector3 normal, Vector3 origin, out Vector3 result)
        {
            Plane plane = new Plane(normal, origin);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance))
            {
                result = ray.GetPoint(rayDistance);
                return true;
            }
            else if (normal == Vector3.zero)
            {
                result = origin;
                return true;
            }
            else
            {
                result = ray.GetPoint(0f);
                return true;
            }
        }


        bool AxisGrid(Vector3 axis, Color color, out Vector3 origin)
        {
            float dot = Vector3.Dot(m_editorCamera.transform.position.normalized, axis);
            if (dot < 0f) axis = -axis;
            Plane plane = new Plane(axis, Vector3.zero);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance))
            {
                origin = ray.GetPoint(rayDistance) + axis * offset;
                Handles.color = color;
                float distance = 1f;
                ray = new Ray(m_editorCamera.transform.position, -axis);
                if (!m_editorCamera.orthographic && plane.Raycast(ray, out rayDistance)) distance = Vector3.Distance(m_editorCamera.transform.position + axis * offset, origin);
                else if (m_editorCamera.orthographic) distance = 2f * m_editorCamera.orthographicSize;
                DrawGrid(origin, axis, Vector2.one * distance * 0.3f, distance * 2.5f * 0.03f);
                Handles.DrawLine(origin, origin - axis * offset);
                Handles.color = Color.white;
                return true;
            }
            else
            {
                origin = Vector3.zero;
                return false;
            }
        }

        void DrawGrid(Vector3 center, Vector3 normal, Vector2 size, float scale)
        {
            Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;
            if (Mathf.Abs(Vector3.Dot(Vector3.up, normal)) >= 0.9999f) right = Vector3.Cross(Vector3.forward, normal).normalized;
            Vector3 up = Vector3.Cross(normal, right).normalized;
            Vector3 startPoint = center - right * size.x * 0.5f + up * size.y * 0.5f;
            float i = 0f;
            float add = scale;
            while (i <= size.x)
            {
                Vector3 point = startPoint + right * i;
                Handles.DrawLine(point, point - up * size.y);
                i += add;
            }

            i = 0f;
            add = scale;
            while (i <= size.x)
            {
                Vector3 point = startPoint - up * i;
                Handles.DrawLine(point, point + right * size.x);
                i += add;
            }
        }

        void DrawMouseCross()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Vector3 origin = ray.GetPoint(1f);
            float size = 0.4f * HandleUtility.GetHandleSize(origin);
            Vector3 a = origin + m_editorCamera.transform.up * size - m_editorCamera.transform.right * size;
            Vector3 b = origin - m_editorCamera.transform.up * size + m_editorCamera.transform.right * size;
            Handles.color = Color.red;
            Handles.DrawLine(a, b);
            a = origin - m_editorCamera.transform.up * size - m_editorCamera.transform.right * size;
            b = origin + m_editorCamera.transform.up * size + m_editorCamera.transform.right * size;
            Handles.DrawLine(a, b);
            Handles.color = Color.white;
        }

        private void UpdateVisualizer()
        {
            if(m_visualizer == null) m_visualizer = new Spline(splineType);
            m_visualizer.type = splineType;
            m_visualizer.sampleRate = sampleRate;
            if(m_placementMode == PlacementMode.Insert)
            {
                m_visualizer.points = m_editor.GetPointsArray();
                if (isClosed) m_visualizer.Close();
                else if (m_visualizer.isClosed) m_visualizer.Break();
                return;
            }

            if (m_visualizer.points.Length != points.Length + 1)
            {
                m_visualizer.points = new SplinePoint[points.Length + 1];
            }

            SplinePoint newPoint = new SplinePoint(m_createPoint, m_createPoint, m_createNormal, 1f, Color.white);
            if (appendMode == AppendMode.End)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    m_visualizer.points[i] = points[i].CreateSplinePoint();
                }
                m_visualizer.points[m_visualizer.points.Length - 1] = newPoint;
            }
            else
            {
                for (int i = 1; i < m_visualizer.points.Length; i++)
                {
                    m_visualizer.points[i] = points[i - 1].CreateSplinePoint();
                }
                m_visualizer.points[0] = newPoint;
            }

            if (isClosed && !m_visualizer.isClosed)
            {
                if(m_visualizer.points.Length >= 3)
                {
                    m_visualizer.Close();
                } else
                {
                    m_visualizer.Break();
                }
            }
            else if (!isClosed && m_visualizer.isClosed)
            {
                m_visualizer.Break();
            }
        }
    }
}
