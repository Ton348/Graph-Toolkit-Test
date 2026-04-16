namespace Dreamteck.Splines.Editor
{
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using System.Collections.Generic;

    public class PointModule : EditorModule
    {
        protected bool isClosed
        {
            get { return m_editor.GetSplineClosed(); }
        }
        protected int sampleRate
        {
            get { return m_editor.GetSplineSampleRate(); }
        }
        protected Spline.Type splineType
        {
            get { return m_editor.GetSplineType(); }
        }
        protected Color color {
            get { return m_editor.drawColor; }
        }
        protected SplineEditor m_editor;

        protected SerializedSplinePoint[] points {
            get { return m_editor.points; }    
            set { m_editor.points = value; }    
        }

        protected List<int> selectedPoints
        {
            get { return m_editor.selectedPoints; }
            set { m_editor.selectedPoints = value; }
        }

        public Vector3 center
        {
            get
            {
                Vector3 avg = Vector3.zero;
                if (points.Length == 0) return avg;
                for (int i = 0; i < points.Length; i++) avg += points[i].position;
                return avg / points.Length;
            }
        }

        public Vector3 selectionCenter
        {
            get
            {
                Vector3 avg = Vector3.zero;
                if (selectedPoints.Count == 0) return avg;
                for (int i = 0; i < selectedPoints.Count; i++) avg += points[selectedPoints[i]].position;
                return avg / selectedPoints.Count;
            }
        }

        protected EditorGuievents m_eventModule;

        public delegate void UndoHandler(string title);
        public delegate void EmptyHandler();
        public delegate void IntHandler(int value);
        public delegate void IntArrayHandler(int[] values);

        public Spline.Direction duplicationDirection = Spline.Direction.Forward;
        public Color highlightColor = Color.white;
        public bool showPointNumbers = false;

        public event EmptyHandler onBeforeDeleteSelectedPoints;
        public event EmptyHandler onSelectionChanged;
        public event IntArrayHandler onDuplicatePoint;

        private bool m_movePivot = false;
        private Vector3 m_idealPivot = Vector3.zero;

        

        public PointModule(SplineEditor editor)
        {
            this.m_editor = editor;
            m_eventModule = editor.eventModule;
        }

        protected override void RecordUndo(string title)
        {
            if (m_editor.undoHandler != null) m_editor.undoHandler(title);
        }

        protected override void Repaint()
        {
            if (m_editor.repaintHandler != null) m_editor.repaintHandler();
        }

        public override void BeforeSceneDraw(SceneView current)
        {
            base.BeforeSceneDraw(current);
            Event e = Event.current;

            if (m_movePivot)
            {
                SceneView.lastActiveSceneView.pivot = Vector3.Lerp(SceneView.lastActiveSceneView.pivot, m_idealPivot, 0.02f);
                if (e.type == EventType.MouseDown || e.type == EventType.MouseUp) m_movePivot = false;
                if (Vector3.Distance(SceneView.lastActiveSceneView.pivot, m_idealPivot) <= 0.05f)
                {
                    SceneView.lastActiveSceneView.pivot = m_idealPivot;
                    m_movePivot = false;
                }
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && HasSelection())
            {
                DeleteSelectedPoints();
                e.Use();
            }

            if(e.type == EventType.ExecuteCommand && Tools.current == Tool.None)
            {
                switch (e.commandName)
                {
                    case "FrameSelected":
                        if (points.Length > 0)
                        {
                            e.commandName = "";
                            FramePoints();
                            e.Use();
                        }
                        break;
                    case "SelectAll":
                        e.commandName = "";
                        ClearSelection();
                        for (int i = 0; i < points.Length; i++)
                        {
                            AddPointSelection(i);
                        }
                        e.Use();
                        break;

                    case "Duplicate":
                        if (points.Length > 0 && selectedPoints.Count > 0)
                        {
                            e.commandName = "";
                            DuplicateSelected();
                            e.Use();
                        }
                        break;
                }
            }
        }

        public virtual void DuplicateSelected()
        {
            if (selectedPoints.Count == 0) return;
            SplinePoint[] newPoints = new SplinePoint[points.Length + selectedPoints.Count];
            SplinePoint[] duplicated = new SplinePoint[selectedPoints.Count];
            m_editor.SetPointsCount(newPoints.Length);
            int index = 0;
            for (int i = 0; i < selectedPoints.Count; i++) duplicated[index++] = points[selectedPoints[i]].CreateSplinePoint();
            int min = points.Length - 1, max = 0;
            for (int i = 0; i < selectedPoints.Count; i++)
            {
                if (selectedPoints[i] < min) min = selectedPoints[i];
                if (selectedPoints[i] > max) max = selectedPoints[i];
            }
            int[] selected = selectedPoints.ToArray();
            selectedPoints.Clear();
            if (duplicationDirection == Spline.Direction.Backward)
            {
                for (int i = 0; i < min; i++) newPoints[i] = points[i].CreateSplinePoint();
                for (int i = 0; i < duplicated.Length; i++)
                {
                    newPoints[i + min] = duplicated[i];
                    selectedPoints.Add(i + min);
                }
                for (int i = min; i < points.Length; i++) newPoints[i + duplicated.Length] = points[i].CreateSplinePoint();
            }
            else
            {
                for (int i = 0; i <= max; i++) newPoints[i] = points[i].CreateSplinePoint();
                for (int i = 0; i < duplicated.Length; i++)
                {
                    newPoints[i + max + 1] = duplicated[i];
                    selectedPoints.Add(i + max + 1);
                }
                for (int i = max + 1; i < points.Length; i++) newPoints[i + duplicated.Length] = points[i].CreateSplinePoint();
            }
            m_editor.SetPointsArray(newPoints);
            RegisterChange();
            if (onDuplicatePoint != null) onDuplicatePoint(selected);
        }

        public virtual void Reset()
        {
        }

        public bool HasSelection()
        {
            return selectedPoints.Count > 0;
        }

        public void ClearSelection()
        {
            selectedPoints.Clear();
            Repaint();
            if (m_editor.selectionChangeHandler != null) m_editor.selectionChangeHandler();
            if (onSelectionChanged != null) onSelectionChanged();
        }

        protected void DeleteSelectedPoints()
        {
            if (onBeforeDeleteSelectedPoints != null)
            {
                onBeforeDeleteSelectedPoints();
            }

            for (int i = 0; i < selectedPoints.Count; i++)
            {
                DeletePoint(selectedPoints[i]);
                for (int n = i; n < selectedPoints.Count; n++)
                {
                    selectedPoints[n]--;
                }
            }
            ClearSelection();
            RegisterChange();
            m_editor.ApplyModifiedProperties(true);
        }

        protected void DeletePoint(int index)
        {
            m_editor.DeletePoint(index);
            RegisterChange();
        }


        public void InverseSelection()
        {
            List<int> inverse = new List<int>();
            for (int i = 0; i < (isClosed ? points.Length - 1 : points.Length); i++)
            {
                bool found = false;
                for (int j = 0; j < selectedPoints.Count; j++)
                {
                    if (selectedPoints[j] == i)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) inverse.Add(i);
            }
            selectedPoints = new List<int>(inverse);
            Repaint();
            if (m_editor.selectionChangeHandler != null) m_editor.selectionChangeHandler();
            if (onSelectionChanged != null) onSelectionChanged();
        }

        protected void SelectPoint(int index)
        {
            if (selectedPoints.Count == 1 && selectedPoints[0] == index) return;
            selectedPoints.Clear();
            selectedPoints.Add(index);
            Repaint();
            if (m_editor.selectionChangeHandler != null) m_editor.selectionChangeHandler();
            if (onSelectionChanged != null) onSelectionChanged();
        }

        protected void DeselectPoint(int index)
        {
            if (selectedPoints.Contains(index))
            {
                selectedPoints.Remove(index);
                Repaint();
                if (m_editor.selectionChangeHandler != null) m_editor.selectionChangeHandler();
                if (onSelectionChanged != null) onSelectionChanged();
            }
        }

        protected void SelectPoints(List<int> indices)
        {
            selectedPoints.Clear();
            for (int i = 0; i < indices.Count; i++)
            {
                selectedPoints.Add(indices[i]);
            }
            Repaint();
            if (m_editor.selectionChangeHandler != null) m_editor.selectionChangeHandler();
            if (onSelectionChanged != null) onSelectionChanged();
        }

        protected void AddPointSelection(int index)
        {
            if (selectedPoints.Contains(index)) return;
            selectedPoints.Add(index);
            Repaint();
            if (m_editor.selectionChangeHandler != null) m_editor.selectionChangeHandler();
            if (onSelectionChanged != null) onSelectionChanged();
        }

        protected void FramePoints()
        {
            if (points.Length == 0) return;
            Vector3 center = Vector3.zero;
            Camera camera = SceneView.lastActiveSceneView.camera;
            Transform cam = camera.transform;
            Vector3 min = Vector3.zero, max = Vector3.zero;
            if (HasSelection())
            {
                for (int i = 0; i < selectedPoints.Count; i++)
                {
                    center += points[selectedPoints[i]].position;
                    Vector3 local = cam.InverseTransformPoint(points[selectedPoints[i]].position);
                    if (local.x < min.x) min.x = local.x;
                    if (local.y < min.y) min.y = local.y;
                    if (local.z < min.z) min.z = local.z;
                    if (local.x > max.x) max.x = local.x;
                    if (local.y > max.y) max.y = local.y;
                    if (local.z > max.z) max.z = local.z;
                }
                center /= selectedPoints.Count;
            }
            else
            {
                for (int i = 0; i < points.Length; i++)
                {
                    center += points[i].position;
                    Vector3 local = cam.InverseTransformPoint(points[i].position);
                    if (local.x < min.x) min.x = local.x;
                    if (local.y < min.y) min.y = local.y;
                    if (local.z < min.z) min.z = local.z;
                    if (local.x > max.x) max.x = local.x;
                    if (local.y > max.y) max.y = local.y;
                    if (local.z > max.z) max.z = local.z;
                }
                center /= points.Length;
            }
            m_movePivot = true;
            m_idealPivot = center;
        }

        
    }
}
