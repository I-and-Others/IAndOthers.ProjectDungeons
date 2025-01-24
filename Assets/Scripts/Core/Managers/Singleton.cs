using UnityEngine;
// Copyrights for dear Dilmer Valecillos, His channel is https://www.youtube.com/c/DilmerV

namespace Scripts.Core
{
    public class Singleton<T> : MonoBehaviour
        where T : Component
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var objs = FindObjectsOfType(typeof(T)) as T[];
                    if (objs.Length > 0)
                        _instance = objs[0];
                    if (objs.Length > 1)
                    {
                        Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                    }
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = string.Format("_{0}", typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void OnDestroy()
        {
            // When the Singleton is destroyed, set the instance to null
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}