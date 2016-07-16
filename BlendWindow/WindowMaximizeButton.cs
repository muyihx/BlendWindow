using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace D3bugDesign
{
	public class WindowMaximizeButton : WindowButton
	{
		private BlendWindow window;
		private Path p;
		public WindowMaximizeButton(BlendWindow window1)
		{
			window = window1;
			p = new Path
			{
				Data = Geometry.Parse("M 1,3 L 11,3 L 11,11 L 1,11 Z M 0,0 L 12,0 L12,12 L 0,12 ZM 1,3 L 11,3 L 11,11 L 1,11 Z M 0,0 L 12,0 L12,12 L 0,12 Z"),
				Fill = window.TitleBarButtonForeground,
				Stroke = window.TitleBarButtonBorder,
				StrokeThickness = 1,
				RenderTransformOrigin = new Point(0.5, 0.5),
				LayoutTransform = new ScaleTransform(1, 1),
				Stretch = Stretch.Fill,
				Width = 10,
				Height = 10
			};
			Content = p;
			ContentDisabled = p;
			Foreground = window.TitleBarButtonForeground;
			Background = window.TitleBarButtonBackground;
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