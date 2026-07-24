using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CWFramework
{
    public static class CWComponentEx
    {
        private static List<Component> m_ComponentCache = new List<Component>();

        public static Component GetComponentNoAlloc(this GameObject @this, System.Type componentType)
        {
            @this.GetComponents(componentType, m_ComponentCache);
            Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
            m_ComponentCache.Clear();
            return component;
        }

        public static T GetComponentNoAlloc<T>(this GameObject @this) where T : Component
        {
            @this.GetComponents(typeof(T), m_ComponentCache);
            Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
            m_ComponentCache.Clear();
            return component as T;
        }

        public static T GetComponentAround<T>(this GameObject @this) where T : Component
        {
            T component = @this.GetComponentInChildren<T>(true);
            if (component == null)
            {
                component = @this.GetComponentInParent<T>();
            }

            return component;
        }

        public static T[] GetComponentsInOnlyChildren<T>(this GameObject @this) where T : Component
        {
            T[] components = @this.GetComponentsInChildren<T>(true);
            if (components == default)
            {
                return default;
            }

            T componentInSelf = @this.GetComponent<T>();

            if (componentInSelf == default)
            {
                return components;
            }
            else
            {
                List<T> result = new List<T>();
                result.AddRange(components);
                if (result.Contains(componentInSelf))
                {
                    result.Remove(componentInSelf);
                }

                return result.ToArray();
            }
        }

        // Get

        public static Component GetComponentNoAlloc(this Component @this, System.Type componentType)
        {
            if (@this != null)
            {
                @this.GetComponents(componentType, m_ComponentCache);
                Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
                m_ComponentCache.Clear();
                return component;
            }
            else
            {
                return default;
            }
        }

        public static T GetComponentNoAlloc<T>(this Component @this) where T : Component
        {
            if (@this != null)
            {
                @this.GetComponents(typeof(T), m_ComponentCache);
                Component component = m_ComponentCache.Count > 0 ? m_ComponentCache[0] : null;
                m_ComponentCache.Clear();
                return component as T;
            }
            else
            {
                return default;
            }
        }

        public static T[] GetComponentsInOnlyChildren<T>(this Component @this) where T : Component
        {
            T[] components = @this.GetComponentsInChildren<T>(true);
            if (components == default)
            {
                return default;
            }

            T componentInSelf = @this.GetComponent<T>();

            if (componentInSelf == default)
            {
                return components;
            }
            else
            {
                List<T> result = new List<T>();
                result.AddRange(components);
                if (result.Contains(componentInSelf))
                {
                    result.Remove(componentInSelf);
                }

                return result.ToArray();
            }
        }

        // Get

        public static string GetHierarchyPath(this GameObject self)
        {
            InfiniteLoopDetector.Reset();

            Stack<string> stack = new Stack<string>();
            Transform node = self.transform;
            do
            {
                stack.Push(node.gameObject.name);

                //stack.Push(text);
                node = node.parent;

                InfiniteLoopDetector.Run();
            }
            while (node != null);

            InfiniteLoopDetector.Reset();

            StringBuilder builder = new StringBuilder("[");
            while (stack.Count > 0)
            {
                string name = stack.Pop();

                if (stack.Count > 0)
                {
                    builder.Append(name);
                    builder.Append("/");
                }
                else
                {
                    builder.Append(name.ToColorString(CWColors.DarkViolet));
                    builder.Append("]");
                }

                InfiniteLoopDetector.Run();
            }

            return builder.ToString();
        }

        public static string GetHierarchyPath(this Component self)
        {
#if UNITY_EDITOR
            if (self == null)
            {
                return "<color=red>NULL</color>";
            }

            if (self.gameObject == null)
            {
                return "<color=red>NULL</color>";
            }

            return self.gameObject.GetHierarchyPath();
#else
	        return "";
#endif
        }

        public static string GetHierarchyName(this GameObject self)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || TS_DEVELOPMENT_BUILD || BETA_BUILD
            return self.name.ToColorString(CWColors.DarkViolet);
#else
                return self.name;
#endif
        }

        public static string GetHierarchyName(this Component self)
        {
            if (self == null || self.gameObject == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || TS_DEVELOPMENT_BUILD || BETA_BUILD
                return "NULL".ToErrorString();
#else
                return "NULL";
#endif
            }

            return GetHierarchyName(self.gameObject);
        }

        // Find

        public static T FindFirstParentComponent<T>(this GameObject self) where T : Component
        {
            Transform node = self.transform.parent;
            while (node != null)
            {
                T component = node.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                node = node.transform.parent;
            }

            return default;
        }

        public static T FindFirstParentComponent<T>(this Component self) where T : Component
        {
            Transform node = self.transform.parent;
            while (node != null)
            {
                T component = node.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                node = node.transform.parent;
            }

            return default;
        }

        public static T FindFirstChildComponent<T>(this Component self) where T : Component
        {
            Transform child;
            T result;
            for (int i = 0; i < self.transform.childCount; i++)
            {
                child = self.transform.GetChild(i);
                if (child != null)
                {
                    result = child.GetComponent<T>();
                    if (result != default)
                    {
                        return result;
                    }
                }
            }

            return default;
        }

        public static T FindComponent<T>(this Component self, string path)
        {
            Transform child = self.transform.Find(path);
            if (child != null)
            {
                return child.GetComponent<T>();
            }

            return default;
        }

        public static T FindComponent<T>(this Component self, string path, T defaultComponent)
        {
            Transform child = self.transform.Find(path);
            if (child != null)
            {
                return child.GetComponent<T>();
            }

            return defaultComponent;
        }

        public static T FindComponentInChildren<T>(this Component self, string path)
        {
            Transform child = self.transform.Find(path);
            if (child != null)
            {
                return child.GetComponentInChildren<T>(true);
            }

            return default;
        }

        public static T[] FindComponentsInChildren<T>(this Component self, string path)
        {
            Transform child = self.transform.Find(path);
            if (child != null)
            {
                return child.GetComponentsInChildren<T>(true);
            }

            return default;
        }

        // Find

        public static Transform FindTransform(this Component self, string path)
        {
            return self.transform.Find(path);
        }

        public static Transform[] FindTransformsInChildren(this Component self, string path)
        {
            List<Transform> result = new List<Transform>();
            Transform child = self.transform.Find(path);
            if (child != null)
            {
                for (int i = 0; i < child.childCount; i++)
                {
                    result.Add(child.GetChild(i));
                }
            }

            return result.Count > 0 ? result.ToArray() : null;
        }

        public static Transform[] FindTransformsInChildrenWithName(this Component self, string name)
        {
            List<Transform> result = new List<Transform>();

            Transform[] children = self.GetComponentsInChildren<Transform>();

            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name.Contains(name))
                {
                    result.Add(children[i]);
                }
            }

            return result.ToArray();
        }

        public static GameObject FindGameObject(this Component self, string path)
        {
            Transform child = self.transform.Find(path);
            if (child != null)
            {
                return child.gameObject;
            }

            return default;
        }

        public static GameObject[] FindGameObjectsInChildren(this Component self, string path)
        {
            List<GameObject> result = new List<GameObject>();

            Transform child = self.transform.Find(path);
            if (child != null)
            {
                for (int i = 0; i < child.childCount; i++)
                {
                    result.Add(child.GetChild(i).gameObject);
                }
            }

            return result.ToArray();
        }

        //

        public static void SetActive(this Component self, bool isActive)
        {
            if (self != null)
            {
                self.gameObject.SetActive(isActive);
            }
        }
    }
}