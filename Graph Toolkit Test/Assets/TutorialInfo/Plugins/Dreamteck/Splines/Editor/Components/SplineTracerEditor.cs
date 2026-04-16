namespace Dreamteck.Splines.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(SplineTracer), true)]
    public class SplineTracerEditor : SplineUserEditor
    {
        private bool m_cameraFoldout = false;
        private TransformModuleEditor m_motionEditor;
        private RenderTexture m_rt;
        private Texture2D m_renderCanvas = null;
        private Camera m_cam;
        SplineTracer[] m_tracers = new SplineTracer[0];

        public delegate void DistanceReceiver(float distance);

        protected override void OnEnable()
        {
            base.OnEnable();
            SplineTracer tracer = (SplineTracer)target;
            m_motionEditor = new TransformModuleEditor(tracer, this, tracer.motion);
            m_tracers = new SplineTracer[targets.Length];
            for (int i = 0; i < m_tracers.Length; i++)
            {
                m_tracers[i] = (SplineTracer)targets[i];
            }
        }

        private int GetRtwidth()
        {
            return Mathf.RoundToInt(EditorGUIUtility.currentViewWidth)-50;
        }

        private int GetRtheight()
        {
            return Mathf.RoundToInt(GetRtwidth()/m_cam.aspect);
        }

        private void CreateRt()
        {
            if(m_rt != null)
            {
                DestroyImmediate(m_rt);
                DestroyImmediate(m_renderCanvas);
            }
            m_rt = new RenderTexture(GetRtwidth(), GetRtheight(), 16, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            m_renderCanvas = new Texture2D(m_rt.width, m_rt.height, TextureFormat.RGB24, false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyImmediate(m_rt);
        }

        protected override void BodyGui()
        {
            base.BodyGui();
            EditorGUILayout.LabelField("Tracing", EditorStyles.boldLabel);
            SplineTracer tracer = (SplineTracer)target;
            serializedObject.Update();
            SerializedProperty useTriggers = serializedObject.FindProperty("useTriggers");
            SerializedProperty triggerGroup = serializedObject.FindProperty("triggerGroup");
            SerializedProperty direction = serializedObject.FindProperty("_direction");
            SerializedProperty physicsMode = serializedObject.FindProperty("_physicsMode");
            SerializedProperty dontLerpDirection = serializedObject.FindProperty("_dontLerpDirection");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(useTriggers);
            if (useTriggers.boolValue) EditorGUILayout.PropertyField(triggerGroup);
            EditorGUILayout.PropertyField(direction, new GUIContent("Direction"));
            EditorGUILayout.PropertyField(dontLerpDirection, new GUIContent("Don't Lerp Direction"));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(physicsMode, new GUIContent("Physics Mode"));

            if (tracer.physicsMode == SplineTracer.PhysicsMode.Rigidbody)
            {
                Rigidbody rb = tracer.GetComponent<Rigidbody>();
                if (rb == null) EditorGUILayout.HelpBox("Assign a Rigidbody component.", MessageType.Error);
                else if (rb.interpolation == RigidbodyInterpolation.None && tracer.updateMethod != SplineUser.UpdateMethod.FixedUpdate) EditorGUILayout.HelpBox("Switch to FixedUpdate mode to ensure smooth update for non-interpolated rigidbodies", MessageType.Warning);

            }
            else if (tracer.physicsMode == SplineTracer.PhysicsMode.Rigidbody2D)
            {
                Rigidbody2D rb = tracer.GetComponent<Rigidbody2D>();
                if (rb == null) EditorGUILayout.HelpBox("Assign a Rigidbody2D component.", MessageType.Error);
                else if (rb.interpolation == RigidbodyInterpolation2D.None && tracer.updateMethod != SplineUser.UpdateMethod.FixedUpdate) EditorGUILayout.HelpBox("Switch to FixedUpdate mode to ensure smooth update for non-interpolated rigidbodies", MessageType.Warning);
            }
            if (m_tracers.Length == 1)
            {
                bool mightBe2d = false;
                if(m_tracers[0].spline != null)
                {
                    mightBe2d = m_tracers[0].spline.is2D;
                }
                if (!mightBe2d)
                {
                    mightBe2d = physicsMode.intValue == (int)SplineTracer.PhysicsMode.Rigidbody2D;
                }
                if (!mightBe2d)
                {
                    if(tracer.GetComponentInChildren<SpriteRenderer>() != null)
                    {
                        mightBe2d = true;
                    }
                }
                m_motionEditor.DrawInspector();

                if (mightBe2d && !tracer.motion.is2D)
                {
                    EditorGUILayout.HelpBox(
                        "The object is possibly set up for 2D but the rotation is applied in 3D. If the intention is for the object to be 2D, switch to 2D in the Motion panel.",
                        MessageType.Warning);
                }

                m_cameraFoldout = EditorGUILayout.Foldout(m_cameraFoldout, "Camera preview");
                if (m_cameraFoldout)
                {
                    if (m_cam == null)
                    {
                        m_cam = tracer.GetComponentInChildren<Camera>();
                    }
                    if (m_cam != null)
                    {
                        if (m_rt == null || m_rt.width != GetRtwidth() || m_rt.height != GetRtheight()) CreateRt();
                        GUILayout.Box("", GUILayout.Width(m_rt.width), GUILayout.Height(m_rt.height));
                        RenderTexture prevTarget = m_cam.targetTexture;
                        RenderTexture prevActive = RenderTexture.active;
                        CameraClearFlags lastFlags = m_cam.clearFlags;
                        Color lastColor = m_cam.backgroundColor;
                        m_cam.targetTexture = m_rt;
                        m_cam.clearFlags = CameraClearFlags.Color;
                        m_cam.backgroundColor = Color.black;
                        m_cam.Render();
                        RenderTexture.active = m_rt;
                        m_renderCanvas.SetPixels(new Color[m_renderCanvas.width * m_renderCanvas.height]);
                        m_renderCanvas.ReadPixels(new Rect(0, 0, m_rt.width, m_rt.height), 0, 0);
                        m_renderCanvas.Apply();
                        RenderTexture.active = prevActive;
                        m_cam.targetTexture = prevTarget;
                        m_cam.clearFlags = lastFlags;
                        m_cam.backgroundColor = lastColor;
                        GUI.DrawTexture(GUILayoutUtility.GetLastRect(), m_renderCanvas, ScaleMode.StretchToFill);
                    }
                    else EditorGUILayout.HelpBox("There is no camera attached to the selected object or its children.", MessageType.Info);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                for (int i = 0; i < m_tracers.Length; i++) m_tracers[i].Rebuild();
            }
        }

        protected override void DuringSceneGui(SceneView currentSceneView)
        {
            base.DuringSceneGui(currentSceneView);
            SplineTracer tracer = (SplineTracer)target;
        }

        protected void DrawResult(SplineSample result)
        {
            SplineTracer tracer = (SplineTracer)target;
            Handles.color = Color.white;
            Handles.DrawLine(tracer.transform.position, result.position);
            SplineEditorHandles.DrawSolidSphere(result.position, HandleUtility.GetHandleSize(result.position) * 0.2f);
            Handles.color = Color.blue;
            Handles.DrawLine(result.position, result.position + result.forward * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.green;
            Handles.DrawLine(result.position, result.position + result.up * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.red;
            Handles.DrawLine(result.position, result.position + result.right * HandleUtility.GetHandleSize(result.position) * 0.5f);
            Handles.color = Color.white;
        }


    }
}
