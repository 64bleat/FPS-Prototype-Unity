using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MPGUI
{
    public class ScreenResolutionDropdown : DropdownField
    {
        private struct Dimension
        {
            internal int width;
            internal int height;
            internal string ratio;
            internal int rhash;

            public Dimension(Resolution res)
            {
                width = res.width;
                height = res.height;
                ratio = Fraction(width, height);
                rhash = (int)(100000f * res.height / res.width);
            }

            public override string ToString()
            {
                return $"{ratio} {width}/{height}";
            }
        }

        protected override void InitField()
        {
            valueText.SetText(new Dimension(Screen.currentResolution).ToString());
        }


        protected override void OpenMenu()
        {
            ButtonSet set = dropdown.SpawnDropdown(dropPosition);
            SortedList<int, Stack<Dimension>> sortedList = new SortedList<int, Stack<Dimension>>();
            Resolution cres = Screen.currentResolution;
            int chash = (int)(100000f * cres.height / cres.width);

            // Sort resolutions
            foreach (Resolution res in Screen.resolutions)
            {
                Dimension dim = new Dimension(res);

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
                    string text = dim.ToString();
                    void call()
                    {
                        Screen.SetResolution(dim.width, dim.height, Screen.fullScreen);
                        valueText.SetText(text);
                    }
                    GameObject go = set.AddButton(text, call);
                    Image image = go.GetComponent<Image>();

                    if (image && dim.rhash != chash)
                        image.color *= 0.5f;
                    else if (dim.height == cres.height)
                        image.color = new Color(0.1f, 1f, 0.35f);
                }
        }

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
    }
}
