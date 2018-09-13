using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.Themes;

namespace DXSample21
{
    public class NodeBackgroundConverter : FrameworkElement, IMultiValueConverter
    {
        private static readonly Brush DefaultBrush;
        private static readonly Brush ShadowBaseBrush;
        private static readonly Brush ShadowBrush;

        static NodeBackgroundConverter()
        {
            DefaultBrush = new SolidColorBrush(Color.FromRgb(225, 225, 225));
            ShadowBaseBrush = new SolidColorBrush(Color.FromArgb(64, 128, 128, 128));
            ShadowBrush = CreateStripedBrush(ShadowBaseBrush);
        }

        private static Brush CreateStripedBrush(Brush stripeBrush, Brush backgroundBrush = null)
        {
            var brush = new VisualBrush
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(new Size(12, 12)),
                Viewbox = new Rect(new Point(3, 3), new Size(6, 6))
            };
            var grid = new Grid
            {
                Width = 10,
                Height = 10,
                Background = backgroundBrush ?? new SolidColorBrush(Colors.Transparent)
            };
            var path = new Path
            {
                Stroke = stripeBrush,
                Data = Geometry.Parse("M 0,3 L 3,0 M 0,6 L 6,0 M 0,9 L 9,0 M 2,10 L 10,2 M 5,10 L 10,5 M 8,10 L 10,8")
            };
            grid.Children.Add(path);
            brush.Visual = grid;
            return brush;
        }

        public Object Convert(Object[] values, Type targetType, Object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || !(values[0] is Boolean shadow) ||
                !(values[1] is SelectionState selection))
            {
                return null;
            }

            Brush selectionBrush = null;
            if (selection.In(SelectionState.Focused, SelectionState.FocusedAndSelected))
            {
                var themeKey = new GridRowThemeKeyExtension();
                themeKey.ThemeName = ThemeManager.ActualApplicationThemeName;
                themeKey.ResourceKey = GridRowThemeKeys.BorderFocusedBrush;
                selectionBrush = (Brush)FindResource(themeKey);
            }

            if (shadow)
            {
                if (selectionBrush != null)
                {
                    return CreateStripedBrush(ShadowBaseBrush, selectionBrush);
                }
                return ShadowBrush;
            }
            
            if (selectionBrush != null)
            {
                return selectionBrush;
            }

            return DefaultBrush;
        }

        public Object[] ConvertBack(Object value, Type[] targetTypes, Object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}