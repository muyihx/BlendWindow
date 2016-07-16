using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace D3bugDesign
{
	public class WindowCloseButton : WindowButton
	{
		private BlendWindow window;
		private Path p;
		public WindowCloseButton(BlendWindow window1)
		{
			window = window1;
			this.Width = 30;

			p = new Path
			{
				Data = Geometry.Parse("M 7.070,5.657 L 4.949,3.535 L 7.070,1.414 L 5.656,0.000 L 3.535,2.121 L 1.414,0.001 L 0.000,1.415 L 2.121,3.535 L 0.000,5.656 L 1.414,7.071 L 3.535,4.950 L 5.656,7.071 ZM 7.070,5.657 L 4.949,3.535 L 7.070,1.414 L 5.656,0.000 L 3.535,2.121 L 1.414,0.001 L 0.000,1.415 L 2.121,3.535 L 0.000,5.656 L 1.414,7.071 L 3.535,4.950 L 5.656,7.071 Z"),
				Fill = window.TitleBarButtonForeground,
				Stroke = window.TitleBarButtonBorder,
				StrokeThickness = 1,
				RenderTransformOrigin = new Point(0.5, 0.5),
				LayoutTransform = new ScaleTransform(1, 1),
				Stretch = Stretch.Fill,
				Width = 16,
				Height = 13
			};
			Content = p;
			ContentDisabled = p;
			Foreground = window.TitleBarButtonForeground;
			Background = window.TitleBarButtonBackground;
			this.CornerRadius = new CornerRadius(0, 3, 3, 0);
		}

		public void Enable()
		{
			Foreground = window.TitleBarButtonForeground;
			Background = window.TitleBarButtonBackground;
			p.Fill = window.TitleBarButtonForeground;
			p.Stroke = window.TitleBarButtonBorder;
		}

		public void Disable()
		{
			Foreground = window.TitleBarButtonForegroundDisabled;
			Background = window.TitleBarButtonBackgroundDisabled;
			p.Fill = window.TitleBarButtonForegroundDisabled;
			p.Stroke = window.TitleBarButtonForegroundDisabled;
		}
	}
}