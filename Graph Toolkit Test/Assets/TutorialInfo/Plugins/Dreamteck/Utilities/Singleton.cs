namespace Dreamteck
{
    using System.Linq;
    using UnityEngine;

    public class Singleton<T> : PrivateSingleton<T> where T : Component
    {
        public static T instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Object.FindObjectsOfType<T>().FirstOrDefault();
                }

                return s_instance;
            }
        }
    }
}