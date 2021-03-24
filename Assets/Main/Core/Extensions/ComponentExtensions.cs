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
            if(c)
                if (c.TryGetComponent(out component))
                    return true;
                else
                    for (int i = 0, count = c.transform.childCount; i < count; i++)
                        if (c.transform.GetChild(i).TryGetComponentInChildren(out component))
                            return true;

            component = default;
            return false;
        }

        public static bool TryGetComponentInChildren<T>(this GameObject go, out T component)
        {
            return go.transform.TryGetComponentInChildren(out component);
        }

        public static bool TryFindChild<T>(this Component c, string name, out T component)
        {
            Transform t = c.transform;

            if (t.gameObject.name.Contains(name) && t.TryGetComponent(out component))
                return true;
            else
                for (int i = 0, count = t.childCount; i < count; i++)
                    if (t.GetChild(i).TryFindChild(name, out component))
                        return true;

            component = default;

            return false;
        }
    }
}
