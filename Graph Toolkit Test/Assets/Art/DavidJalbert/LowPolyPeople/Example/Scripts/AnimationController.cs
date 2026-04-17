using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DavidJalbert.LowPolyPeople
{
    public class AnimationController : MonoBehaviour
    {
        public Animator[] characters;
        public Text label;
        public Material[] palettes;
        public Camera[] cameras;

        private int m_currentCamera = 0;

        void Start()
        {
            SetAnimation("idle");
            SetCamera(0);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetAnimation("idle");
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetAnimation("walk");
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetAnimation("run");
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetAnimation("wave");
            if (Input.GetKeyDown(KeyCode.R)) RandomizePalette();
            if (Input.GetKeyDown(KeyCode.C)) ChangeCamera();
        }

        public void SetAnimation(string tag)
        {
            label.text = "Current animation: " + tag;
            foreach (Animator animator in characters) animator.SetTrigger(tag);
        }

        public void RandomizePalette()
        {
            foreach (Animator animator in characters)
            {
                SkinnedMeshRenderer renderer = animator.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = palettes[(int)(Random.value * palettes.Length) % palettes.Length];
                }
            }
        }

        public void SetCamera(int c)
        {
            m_currentCamera = c % cameras.Length;
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].gameObject.SetActive(m_currentCamera == i);
            }
        }

        public void ChangeCamera()
        {
            SetCamera(m_currentCamera + 1);
        }
    }
}