using System.Windows;
using System.Windows.Media;
using Path = System.Windows.Shapes.Path;

namespace D3bugDesign
{
	public class WindowMinimizeButton : WindowButton
	{
		private BlendWindow window;
		private Path p;
		public WindowMinimizeButton(BlendWindow window1)
		{
			window = window1;
			p = new Path
			{
				Data = Geometry.Parse("M0,0 L8,0 8,1 8,2 0,2 0,1 z"),
				Fill = window.TitleBarButtonForeground,
				Stroke = window.TitleBarButtonBorder,
				StrokeThickness = 1,
				RenderTransformOrigin = new Point(0.5,0.5),
				LayoutTransform = new ScaleTransform(1,1),
				Stretch = Stretch.Fill,
				VerticalAlignment = VerticalAlignment.Bottom,
				Width = 15,
				Height = 3,
				Margin = new Thickness(3)
			};
			Content = p;
			ContentDisabled = p;
			CornerRadius = new CornerRadius(3, 0, 0, 3);
			Enable();
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
