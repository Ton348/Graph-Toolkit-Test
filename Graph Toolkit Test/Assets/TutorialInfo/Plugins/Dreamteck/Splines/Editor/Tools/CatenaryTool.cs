using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace Dreamteck.Splines
{
    public class CatenaryTool : SplineTool
    {
        protected GameObject m_obj;
        protected ObjectController m_spawner;
        private float m_sag = 0f;
        private float m_minSagDistance = 0f;
        private float m_maxSagDistance = 10f;
        private Dictionary<SplineComputer, SplinePoint[]> m_editSplines = new Dictionary<SplineComputer, SplinePoint[]>();

        public override string GetName()
        {
            return "Catenary Tool";
        }

        protected override string GetPrefix()
        {
            return "CatenaryTool";
        }

        public override void Open(EditorWindow window)
        {
            base.Open(window);
            m_sag = EditorPrefs.GetFloat("DreamteckSplines.CatenaryTool._sag", 0f);
            m_minSagDistance = EditorPrefs.GetFloat("DreamteckSplines.CatenaryTool._minSagDistance", 0f);
            m_maxSagDistance = EditorPrefs.GetFloat("DreamteckSplines.CatenaryTool._maxSagDistance", 10f);
        }

        public override void Close()
        {
            base.Close();
            EditorPrefs.SetFloat("DreamteckSplines.CatenaryTool._sag", m_sag);
            EditorPrefs.SetFloat("DreamteckSplines.CatenaryTool._minSagDistance", m_minSagDistance);
            EditorPrefs.SetFloat("DreamteckSplines.CatenaryTool._maxSagDistance", m_minSagDistance);
        }

        public override void Draw(Rect windowRect)
        {
            base.Draw(windowRect);
            if(m_editSplines.Keys.Count == 0 && m_splines.Count > 0)
            {
                if(GUILayout.Button("Convert Selected"))
                {
                    ConvertSelected();
                }
            } else
            {
                EditorGUI.BeginChangeCheck();
                m_sag = EditorGUILayout.FloatField("Sag", m_sag);
                m_minSagDistance = EditorGUILayout.FloatField("Min Distance", m_minSagDistance);
                m_maxSagDistance = EditorGUILayout.FloatField("Max Distance", m_maxSagDistance);

                var keys = m_editSplines.Keys;
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                    foreach (var key in keys)
                    {
                        for (int i = 0; i < key.pointCount; i++)
                        {
                            ModifyPoint(key, i);
                        }
                        key.SetPoints(m_editSplines[key]);
                    }
                }
                
                if (GUILayout.Button("Apply"))
                {
                    foreach (var key in keys)
                    {
                        EditorUtility.SetDirty(key);
                    }
                    m_editSplines.Clear();
                }
            }
        }

        private void ModifyPoint(SplineComputer spline, int index)
        {
            var current = m_editSplines[spline][index];
            if(index > 0)
            {
                var previous = m_editSplines[spline][index - 1];
                Vector3 prevDirection = (previous.position - current.position)/3f;
                float sagAmount = Mathf.InverseLerp(m_minSagDistance, m_maxSagDistance, prevDirection.magnitude) * m_sag;
                current.SetTangentPosition(current.position + prevDirection + Vector3.down * sagAmount);
            }

            if(index < m_editSplines[spline].Length - 1)
            {
                var next = m_editSplines[spline][index + 1];
                Vector3 nextDirection = (next.position - current.position) / 3f;
                float sagAmount = Mathf.InverseLerp(m_minSagDistance, m_maxSagDistance, nextDirection.magnitude) * m_sag;
                current.SetTangent2Position(current.position + nextDirection + Vector3.down * sagAmount);
            }
            m_editSplines[spline][index] = current;
        }

        private void ConvertSelected()
        {
            m_editSplines.Clear();
            foreach(var spline in m_splines)
            {
                var points = spline.GetPoints();
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].type = SplinePoint.Type.Broken;
                }
                spline.type = Spline.Type.Bezier;
                m_editSplines.Add(spline, points);
            }
        }
    }
}
