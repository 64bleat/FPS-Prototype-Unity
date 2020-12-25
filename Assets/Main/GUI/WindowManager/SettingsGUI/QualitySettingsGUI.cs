using MPCore;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class QualitySettingsGUI : ScriptableObject
    {
        public DropdownSpawn spawner;
        public ObjectEvent qualityChannel;

        private struct Dimension
        {
            internal int width;
            internal int height;
            internal string ratio;
            internal int rhash;
        }

        public void SetQualityLevel(int level)
        {
            QualitySettings.SetQualityLevel(level);

            if (qualityChannel)
                qualityChannel.Invoke(level);
        }

        /// <summary> Maybe one day this will work on more than just mobile devices... :( </summary>
        public void OpenFullScreenModeDropdown(RectTransform button)
        {
            GUIButtonSet set = spawner.SpawnDropdown(button);
            FullScreenMode current = Screen.fullScreenMode;

            foreach (FullScreenMode mode in Enum.GetValues(typeof(FullScreenMode)))
            {
                FullScreenMode m = mode;
                Image image = set.AddButton(m.ToString(), () => Screen.fullScreenMode = m).GetComponent<Image>();

                if (!m.Equals(current))
                    image.color *= 0.5f;
            }
        }

        public void OpenScreenOrientationDropdown(RectTransform button)
        {
            GUIButtonSet set = spawner.SpawnDropdown(button);
            ScreenOrientation current = Screen.orientation;

            foreach (ScreenOrientation mode in Enum.GetValues(typeof(ScreenOrientation)))
            {
                ScreenOrientation o = mode;
                Image image = set.AddButton(o.ToString(), () => Screen.orientation = o).GetComponent<Image>();

                if (!o.Equals(current))
                    image.color *= 0.5f;
            }
        }

        /// <summary> Populates a dropdown menu with resolution options</summary>
        /// <param name="button"> Dropdown spawns directly under <c>button</c></param>
        public void OpenResolutionDropdown(RectTransform button)
        {
            GUIButtonSet set = spawner.SpawnDropdown(button);
            SortedList<int, Stack<Dimension>> sortedList = new SortedList<int, Stack<Dimension>>();
            Resolution cres = Screen.currentResolution;
            int chash = (int)(100000f * cres.height / cres.width);

            // Sort resolutions
            foreach (Resolution res in Screen.resolutions)
            {
                Dimension dim;
                dim.width = res.width;
                dim.height = res.height;
                dim.ratio = Fraction(dim.width, dim.height);
                dim.rhash = (int)(100000f * res.height / res.width);

                if (dim.rhash == chash)
                    dim.rhash *= -1;

                if (!sortedList.ContainsKey(dim.rhash))
                    sortedList.Add(dim.rhash, new Stack<Dimension>());

                if (sortedList.TryGetValue(dim.rhash, out Stack<Dimension> queue))
                    if (!queue.Contains(dim))
                        queue.Push(dim);
            }

            chash *= -1;

            // Display resolutions
            foreach (var queue in sortedList.Values)
                foreach (var dim in queue)
                {
                    string text = dim.ratio + " " + dim.width + " / " + dim.height;
                    void action() => Screen.SetResolution(dim.width, dim.height, Screen.fullScreen);
                    GameObject go = set.AddButton(text, action);
                    Image image = go.GetComponent<Image>();

                    if (image && dim.rhash != chash)
                        image.color *= 0.5f;
                    else if (dim.height == cres.height)
                        image.color = new Color(0.1f, 1f, 0.35f);
                }
        }

        /// <summary> Reduced fraction, quick and dirty </summary>
        private static string Fraction(int width, int height)
        {
            int d = 2;

            while (d < width && d < height)
            {
                while (width % d == 0 && height % d == 0)
                {
                    width /= d;
                    height /= d;
                }

                d++;
            }

            return "(" + width + ":" + height + ")";
        }

        public void OpenQualityDropdown(RectTransform rect)
        {
            GUIButtonSet set = spawner.SpawnDropdown(rect);
            string[] names = QualitySettings.names;
            int len = names.Length;
            Color good = new Color(0.5f, 1f, 0.5f);
            Color bad = new Color(0.8f, 0.5f, 0.2f);

            for (int i = len - 1; i >= 0; i--)
            {
                int l = i;
                Image image = set.AddButton(names[l], () => SetQualityLevel(l)).GetComponent<Image>();

                if (image)
                    if (i == QualitySettings.GetQualityLevel())
                        image.color = Color.Lerp(bad, good, (float)i / len);
                    else
                        image.color *= 0.5f;
            }
        }

        public void OpenGUIScaleDropdown(RectTransform rect)
        {
            GUIButtonSet set = spawner.SpawnDropdown(rect);
            CanvasScaler scaler = rect.GetComponentInParent<CanvasScaler>();

            for (int i = 1; i <= 4; i++)
            {
                int factor = i;
                Image image = set.AddButton(factor.ToString(), () => scaler.scaleFactor = factor).GetComponent<Image>();

                if (image && factor != scaler.scaleFactor)
                    image.color *= 0.5f;
            }
        }
    }
}
