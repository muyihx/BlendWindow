using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace D3bugDesign
{
	public class WindowRestoreButton : WindowButton
	{
		private BlendWindow window;
		private Path p;
		public WindowRestoreButton(BlendWindow window1)
		{
			window = window1;
			p = new Path
			{
				Data = Geometry.Parse("M 0,3L 3,3L 3,0L 10,0L 10,7L 7,7L 7,10L 0,10 Z M 8,2L 5,2L 5,3L 7,3L 7, 5L 8, 5 Z M 2,5L 2, 8L 5, 8L 5,5 ZM 0,3L 3,3L 3,0L 10,0L 10,7L 7,7L 7,10L 0,10 Z M 8,2L 5,2L 5,3L 7,3L 7, 5L 8, 5 Z M 2,5L 2, 8L 5, 8L 5,5 Z"),
				Fill = window.TitleBarButtonForeground,
				Stroke = window.TitleBarButtonBorder,
				StrokeThickness = 1,
				RenderTransformOrigin = new Point(0.5, 0.5),
				LayoutTransform = new ScaleTransform(1, 1),
				Stretch = Stretch.Fill,
				Width = 11,
				Height = 11
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