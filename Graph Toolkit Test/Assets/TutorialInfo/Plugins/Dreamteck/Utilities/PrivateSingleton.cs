namespace Dreamteck
{
    using System.Linq;
    using UnityEngine;

    public class PrivateSingleton<T> : MonoBehaviour where T : Component
    {
        [SerializeField] protected bool m_dontDestryOnLoad = true;
        [SerializeField] protected bool m_overrideInstance = false;

        protected static T s_instance;

        protected virtual void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                if (m_overrideInstance)
                {
                    Destroy(s_instance.gameObject);
                    s_instance = this as T;
                    Init();
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
            else
            {
                s_instance = this as T;

                if (m_dontDestryOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
                Init();
            }
        }

        protected virtual void Init()
        {
        }

        protected virtual void OnDestroy()
        {
            if (s_instance == this && !m_overrideInstance)
            {
                s_instance = null;
            }
        }
    }
}
