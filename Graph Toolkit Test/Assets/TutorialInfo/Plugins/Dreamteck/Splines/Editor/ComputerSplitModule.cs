namespace Dreamteck.Splines.Editor
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;

    public class ComputerSplitModule : ComputerEditorModule
    {

        public ComputerSplitModule(SplineComputer spline) : base(spline)
        {

        }

        public override GUIContent GetIconOff()
        {
            return IconContent("Split", "split", "Split Spline");
        }

        public override GUIContent GetIconOn()
        {
            return IconContent("Split", "split_on", "Split Spline");
        }

        protected override void OnDrawScene()
        {
            bool change = false;
            Camera editorCamera = SceneView.currentDrawingSceneView.camera;

            for (int i = 0; i < m_spline.pointCount; i++)
            {
                Vector3 pos = m_spline.GetPointPosition(i);
                if (SplineEditorHandles.CircleButton(pos, Quaternion.LookRotation(editorCamera.transform.position - pos), HandleUtility.GetHandleSize(pos) * 0.12f, 1f, m_spline.editorPathColor))
                {
                    SplitAtPoint(i);
                    change = true;
                    break;
                }
            }
            SplineSample projected  = m_spline.Evaluate(ProjectMouse());
            if (!change)
            {
                float pointValue = (float)projected.percent * (m_spline.pointCount - 1);
                int pointIndex = Mathf.FloorToInt(pointValue);
                    float size = HandleUtility.GetHandleSize(projected.position) * 0.3f;
                    Vector3 up = Vector3.Cross(editorCamera.transform.forward, projected.forward).normalized * size + projected.position;
                    Vector3 down = Vector3.Cross(projected.forward, editorCamera.transform.forward).normalized * size + projected.position;
                    Handles.color = m_spline.editorPathColor;
                    Handles.DrawLine(up, down);
                    Handles.color = Color.white;
                if (pointValue - pointIndex > m_spline.moveStep) { 
                    if (SplineEditorHandles.CircleButton(projected.position, Quaternion.LookRotation(editorCamera.transform.position - projected.position), HandleUtility.GetHandleSize(projected.position) * 0.12f, 1f, m_spline.editorPathColor))
                    {
                        SplitAtPercent(projected.percent);
                        change = true;
                    }
                }
                SceneView.RepaintAll();
            }
            Handles.color = Color.white;
            DssplineDrawer.DrawSplineComputer(m_spline, 0.0, projected.percent, 1f);
            DssplineDrawer.DrawSplineComputer(m_spline, projected.percent, 1.0, 0.4f);
        }
        
        
        void HandleNodes(SplineComputer newSpline, int splitIndex)
        {
            List<Node> nodes = new List<Node>();
            List<int> indices = new List<int>();

            for (int i = splitIndex; i < m_spline.pointCount; i++)
            {
                Node node = m_spline.GetNode(i);
                if(node != null)
                {
                    nodes.Add(node);
                    indices.Add(i);
                    m_spline.DisconnectNode(i);
                    i--;
                }
            }
            for (int i = 0; i < nodes.Count; i++) newSpline.ConnectNode(nodes[i], indices[i] - splitIndex);
        }

       void SplitAtPercent(double percent)
       {
            RecordUndo("Split Spline");
            float pointValue = (m_spline.pointCount - 1) * (float)percent;
            int lastPointIndex = Mathf.FloorToInt(pointValue);
            int nextPointIndex = Mathf.CeilToInt(pointValue);
            SplinePoint[] splitPoints = new SplinePoint[m_spline.pointCount - lastPointIndex];
            float lerpPercent = Mathf.InverseLerp(lastPointIndex, nextPointIndex, pointValue);
            SplinePoint splitPoint = SplinePoint.Lerp(m_spline.GetPoint(lastPointIndex), m_spline.GetPoint(nextPointIndex), lerpPercent);
            splitPoint.SetPosition(m_spline.EvaluatePosition(percent));
            splitPoints[0] = splitPoint;
            for (int i = 1; i < splitPoints.Length; i++) splitPoints[i] = m_spline.GetPoint(lastPointIndex + i);
            SplineComputer newSpline = CreateNewSpline();
            newSpline.SetPoints(splitPoints);

            HandleNodes(newSpline, lastPointIndex);

            SplineUser[] users = newSpline.GetSubscribers();
            for (int i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = Dmath.InverseLerp(percent, 1.0, users[i].clipFrom);
                users[i].clipTo = Dmath.InverseLerp(percent, 1.0, users[i].clipTo);
            }
            splitPoints = new SplinePoint[lastPointIndex + 2];
            for (int i = 0; i <= lastPointIndex; i++) splitPoints[i] = m_spline.GetPoint(i);
            splitPoints[splitPoints.Length - 1] = splitPoint;
            m_spline.SetPoints(splitPoints);
            users = m_spline.GetSubscribers();
            for (int i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = Dmath.InverseLerp(0.0, percent, users[i].clipFrom);
                users[i].clipTo = Dmath.InverseLerp(0.0, percent, users[i].clipTo);
            }
        }

        void SplitAtPoint(int index)
        {
            RecordUndo("Split Spline");
            SplinePoint[] splitPoints = new SplinePoint[m_spline.pointCount - index];
            for(int i = 0; i < splitPoints.Length; i++) splitPoints[i] = m_spline.GetPoint(index + i);
            SplineComputer newSpline = CreateNewSpline();
            newSpline.SetPoints(splitPoints);

            HandleNodes(newSpline, index);

            SplineUser[] users = newSpline.GetSubscribers();
            for (int i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = Dmath.InverseLerp((double)index / (m_spline.pointCount - 1), 1.0, users[i].clipFrom);
                users[i].clipTo = Dmath.InverseLerp((double)index / (m_spline.pointCount - 1), 1.0, users[i].clipTo);
            }
            splitPoints = new SplinePoint[index + 1];
            for (int i = 0; i <= index; i++) splitPoints[i] = m_spline.GetPoint(i);
            m_spline.SetPoints(splitPoints);
            users = m_spline.GetSubscribers();
            for (int i = 0; i < users.Length; i++)
            {
                users[i].clipFrom = Dmath.InverseLerp(0.0, ((double)index) / (m_spline.pointCount - 1), users[i].clipFrom);
                users[i].clipTo = Dmath.InverseLerp(0.0, ((double)index) / (m_spline.pointCount - 1), users[i].clipTo);
            }

        }

        SplineComputer CreateNewSpline()
        {
            GameObject go = Object.Instantiate(m_spline.gameObject);
            Undo.RegisterCreatedObjectUndo(go, "New Spline");
            go.name = m_spline.name + "_split";
            SplineUser[] users = go.GetComponents<SplineUser>();
            SplineComputer newSpline = go.GetComponent<SplineComputer>();
            for (int i = 0; i < users.Length; i++)
            {
                m_spline.Unsubscribe(users[i]);
                users[i].spline = newSpline;
                newSpline.Subscribe(users[i]);
            }
            for(int i = go.transform.childCount-1; i>=0; i--)
            {
                Undo.DestroyObjectImmediate(go.transform.GetChild(i).gameObject);
            }
            return newSpline;
        }

        private double ProjectMouse()
        {
            if (m_spline.pointCount == 0) return 0.0;
            float closestDistance = (Event.current.mousePosition - HandleUtility.WorldToGUIPoint(m_spline.GetPointPosition(0))).sqrMagnitude;
            double closestPercent = 0.0;
            double add = m_spline.moveStep;
            if (m_spline.type == Spline.Type.Linear) add /= 2.0;
            int count = 0;
            for (double i = add; i < 1.0; i += add)
            {
                SplineSample result = m_spline.Evaluate(i);
                Vector2 point = HandleUtility.WorldToGUIPoint(result.position);
                float dist = (point - Event.current.mousePosition).sqrMagnitude;
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPercent = i;
                }
                count++;
            }
            return closestPercent;
        }
    }
}
