using UnityEngine;

namespace MPCore
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// non-allocating try get component in parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <param name="component"></param>
        /// <returns>true if the desired Component was found</returns>
        public static bool TryGetComponentInParent<T>(this Component c, out T component)
        {
            Transform t = c.transform;

            do
                if (t.TryGetComponent(out component))
                    return true;
            while (t = t.parent);

            return false;
        }

        public static bool TryGetComponentInParent<T>(this GameObject go, out T component)
        {
            return go.transform.TryGetComponentInParent(out component);
        }

        public static bool TryGetComponentInChildren<T>(this Component c, out T component)
        {
            if (c == null)
            {
                component = default;
                return false;
            }
            else if (c.TryGetComponent(out component))
                return true;
            else
                for (int cc = c.transform.childCount, i = 0; i < cc; i++)
                    if (c.transform.GetChild(i).TryGetComponent(out component))
                        return true;

            component = default;

            return false;
        }

        public static bool TryGetComponentInChildren<T>(this GameObject go, out T component)
        {
            return go.transform.TryGetComponentInChildren(out component);
        }
    }
}
