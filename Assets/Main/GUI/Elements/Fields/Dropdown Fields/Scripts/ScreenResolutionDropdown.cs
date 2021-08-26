using MPCore;
using System.Linq;
using UnityEngine;

namespace MPGUI
{
    public class ScreenResolutionDropdown : GenericDropdownField<ScreenResolutionDropdown.Dimension>
    {
        public Dimension Current
        {
            get => new Dimension(Screen.currentResolution);
            set => Screen.SetResolution(value.width, value.height, Screen.fullScreen);
        }

        public struct Dimension
        {
            public readonly int width;
            public readonly int height;
            public readonly string ratio;
            public readonly int rhash;

            public Dimension(Resolution res)
            {
                width = res.width;
                height = res.height;
                ratio = Fraction(width, height);
                rhash = res.height * res.width;
            }
        }

        void Awake()
        {
            SetReference(this, nameof(Current));
        }

        protected override string Write(Dimension value)
        {
            return $"{value.ratio} {value.width}/{value.height}";
        }

        protected override Color Colorize(Dimension option, Dimension current)
        {
            current = Current;

            if (option.ratio != current.ratio)
                return Color.grey;
            else if (option.height == current.height)
                return new Color(0.1f, 1f, 0.35f);
            else
                return Color.white;
        }

        protected override void InitDropdown()
        {
            Dimension current = Current;

            var all = Screen.resolutions.Select(r => new Dimension(r));
            var same = all.Where(dim => dim.ratio == current.ratio)
                .OrderByDescending(dim => dim.width * dim.height);
            var rest = all.Except(same)
                .OrderByDescending(dim => (float)dim.width / dim.height)
                .ThenByDescending(dim => dim.width * dim.height);
            var selection = same.Concat(rest);

            ClearOptions();
            AddOptions(selection);
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
