using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace D3bugDesign
{
	public class StandardWindow : Window
	{
		private Border captionControl;
		protected WindowCloseButton CloseButton;
		private WindowButtonState closeButtonState;
		private Border contentWindowBackgroundBorder;
		private Border contentWindowBorder;
		private int lastMouseCaptionClick;
		protected WindowMaximizeButton MaximizeButton;
		protected WindowButtonState _maximizeButtonState; 
		protected WindowMinimizeButton MinimizeButton;
		protected WindowButtonState _minimizeButtonState;
		private WindowResizingAdorner resizingAdorner;
		protected WindowRestoreButton RestoreButton;
		private Border windowBorderBorder;
		private HwndSource hwndSource;
		
		static StandardWindow()
		{
			if (Assembly.GetEntryAssembly() != null)
			{
				ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(StandardWindow), new UIPropertyMetadata(null, ContentChangedCallback));
				BackgroundProperty = DependencyProperty.Register("Background", typeof(object), typeof(StandardWindow), new UIPropertyMetadata(Brushes.Transparent, BackgroundChangedCallback));
				TitlebarBrushProperty = DependencyProperty.Register("TitlebarBrush", typeof(object), typeof(StandardWindow), new UIPropertyMetadata(Brushes.DarkBlue, TitlebarBrushChangedCallback));
			}
		}

		public StandardWindow()
		{
			InitializeContentControls();
			CaptionHeight = (int)SystemParameters.CaptionHeight;
			ContentExtend = false;
			WindowStyle = WindowStyle.None;
			AllowsTransparency = false;
			WindowChrome.SetWindowChrome(this, new WindowChrome());
			Loaded += OnLoaded;
			SourceInitialized += StandardWindow_SourceInitialized;
		}

		private void StandardWindow_SourceInitialized(object sender, EventArgs e)
		{
			hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
			if (hwndSource == null) return;
			var handle = hwndSource.Handle;
			var fromHwnd = HwndSource.FromHwnd(handle);
			fromHwnd?.AddHook(Graphics.WindowProc);
		}

		protected void InitializeContentControls()
		{
			WindowStyle = WindowStyle.None;
			MinimizeButton = new WindowMinimizeButton();
			MinimizeButton.Click += OnButtonMinimize_Click;
			RestoreButton = new WindowRestoreButton();
			RestoreButton.Click += OnButtonRestore_Click;
			RestoreButton.Margin = new Thickness(-1, 0, 0, 0);
			MaximizeButton = new WindowMaximizeButton();
			MaximizeButton.Click += OnButtonMaximize_Click;
			MaximizeButton.Margin = new Thickness(-1, 0, 0, 0);
			CloseButton = new WindowCloseButton();
			CloseButton.Click += OnButtonClose_Click;
			CloseButton.Margin = new Thickness(-1, 0, 0, 0);

			// put buttons into StackPanel
			var buttonsStackPanel = new StackPanel();
			buttonsStackPanel.Orientation = Orientation.Horizontal;
			buttonsStackPanel.Children.Add(MinimizeButton);
			buttonsStackPanel.Children.Add(RestoreButton);
			buttonsStackPanel.Children.Add(MaximizeButton);
			buttonsStackPanel.Children.Add(CloseButton);

			// put stack into border
			var buttonsBorder = new Border();
			buttonsBorder.BorderThickness = new Thickness(0, 1, 0, 0);
			buttonsBorder.BorderBrush = new SolidColorBrush(new Color { R = 118, G = 124, B = 132, A = 255 });
			buttonsBorder.VerticalAlignment = VerticalAlignment.Top;
			buttonsBorder.Child = buttonsStackPanel;
			buttonsBorder.VerticalAlignment = VerticalAlignment.Top;
			buttonsBorder.HorizontalAlignment = HorizontalAlignment.Right;

			//
			// Caption
			captionControl = new Border();
			captionControl.MouseMove += OnWindowDragMove;
			captionControl.MouseLeftButtonDown += OnCaptionBarClick;
			DockPanel.SetDock(captionControl, Dock.Top);

			//
			// Window
			contentWindowBackgroundBorder = new Border();
			contentWindowBackgroundBorder.Background = Brushes.White;
			var windowDockPanel = new DockPanel();
			windowDockPanel.Children.Add(captionControl);
			windowDockPanel.Children.Add(contentWindowBackgroundBorder);

			// all wrap into grid
			contentWindowBorder = new Border();
			var topGrid = new Grid();
			topGrid.Children.Add(windowDockPanel);
			topGrid.Children.Add(contentWindowBorder);
			topGrid.Children.Add(buttonsBorder);
			base.Content = topGrid;
		}
		protected void OnLoaded(object sender, RoutedEventArgs e)
		{
			captionControl.Height = CaptionHeight;
			if (ContentExtend)
				contentWindowBorder.Margin = new Thickness(0);
			else
				contentWindowBorder.Margin = new Thickness(0, CaptionHeight, 0, 0);

			Height = Height + 1;
			Height = Height - 1;
			TextBlock.SetForeground(captionControl, SystemColors.ActiveCaptionTextBrush);
			TextBlock.SetFontFamily(captionControl, SystemFonts.CaptionFontFamily);
			TextBlock.SetFontSize(captionControl, SystemFonts.CaptionFontSize);
			TextBlock.SetFontStyle(captionControl, SystemFonts.CaptionFontStyle);
			TextBlock.SetFontWeight(captionControl, SystemFonts.CaptionFontWeight);
			StateChanged += StandardWindow_StateChanged;
			Activated += OnStandardWindowActivated;
			Deactivated += OnStandardWindowDeactivated;
			OnStateChanged(new EventArgs());
		}

		// called when Minimize button clicked
		protected virtual void OnButtonMinimize_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		// called when Restore button clicked
		protected virtual void OnButtonRestore_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Normal;
		}

		// called when Maximize button clicked
		protected virtual void OnButtonMaximize_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Maximized;
		}

		// called when Close button clicked
		protected virtual void OnButtonClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void SetWindowState(WindowState state)
		{
			ResizeMode = ResizeMode.CanResize;
			if (state == WindowState.Maximized)
			{
				ResizeMode = ResizeMode.NoResize; 
				base.WindowState = WindowState.Maximized;
			}
			else if (state == WindowState.Normal)
			{
				base.WindowState = WindowState.Normal;
			}
			else if (state == WindowState.Minimized)
			{
				base.WindowState = WindowState.Minimized;
			}
		}

		protected void StandardWindow_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Normal)
			{
				RestoreButton.Visibility = Visibility.Collapsed;
				
				if (_maximizeButtonState != WindowButtonState.None)
					MaximizeButton.Visibility = Visibility.Visible;
			}
			else if (WindowState == WindowState.Maximized)
			{
				MaximizeButton.Visibility = Visibility.Collapsed;
				if (_maximizeButtonState != WindowButtonState.None)
					RestoreButton.Visibility = Visibility.Visible;
			}
		}

		protected virtual void OnWindowDragMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}
		protected virtual void OnCaptionBarClick(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				if (Environment.TickCount - lastMouseCaptionClick < 400)
				{
					if (WindowState == WindowState.Maximized)
						WindowState = WindowState.Normal;
					else if (WindowState == WindowState.Normal)
						WindowState = WindowState.Maximized;
					lastMouseCaptionClick = 0;
				}
				lastMouseCaptionClick = Environment.TickCount;
			}
		}

		protected virtual void OnWindowButtonStateChange(WindowButtonState state, WindowButton button)
		{
			switch (state)
			{
				case WindowButtonState.Normal:
					button.Visibility = Visibility.Visible;
					button.IsEnabled = true;
					break;
				case WindowButtonState.Disabled:
					button.Visibility = Visibility.Visible;
					button.IsEnabled = false;
					break;
				case WindowButtonState.None:
					button.Visibility = Visibility.Collapsed;
					break;
			}
		}

		protected virtual void OnStandardWindowActivated(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Maximized)
				TextBlock.SetForeground(captionControl, Brushes.White); // caption text color when window is maximized and active
			else
				TextBlock.SetForeground(captionControl, SystemColors.ActiveCaptionTextBrush); // caption text color when window is active
		}

		protected virtual void OnStandardWindowDeactivated(object sender, EventArgs e)
		{
			TextBlock.SetForeground(captionControl, SystemColors.InactiveCaptionTextBrush); // caption text color when window is inactive
		}

		public int CaptionHeight { get; set; }

		public WindowButtonState MinimizeButtonState
		{
			get { return _minimizeButtonState; }
			set
			{
				_minimizeButtonState = value;
				OnWindowButtonStateChange(value, MinimizeButton);
			}
		}

		public WindowButtonState MaximizeButtonState
		{
			get { return _maximizeButtonState; }
			set
			{
				_maximizeButtonState = value;
				OnWindowButtonStateChange(value, RestoreButton);
				OnWindowButtonStateChange(value, MaximizeButton);
			}
		}

		public WindowButtonState CloseButtonState
		{
			get { return closeButtonState; }
			set
			{
				closeButtonState = value;
				OnWindowButtonStateChange(value, CloseButton);
			}
		}

		[TypeConverter(typeof(TypeConverterStringToUIElement))]
		public UIElement Caption
		{
			get { return captionControl.Child; }
			set { captionControl.Child = value; }
		}

		public bool ContentExtend { get; set; }

		public new object Content
		{
			get { return GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public new static readonly DependencyProperty ContentProperty;

		private static void ContentChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (StandardWindow)property;
			window.contentWindowBorder.Child = (UIElement)args.NewValue;
		}

		public new Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set
			{
				SetValue(BackgroundProperty, value);
				contentWindowBackgroundBorder.Background = value;
			}
		}

		public new static readonly DependencyProperty BackgroundProperty;

		private static void BackgroundChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (StandardWindow)property;
			window.contentWindowBackgroundBorder.Background = (Brush)args.NewValue;
		}

		public Brush TitlebarBrush
		{
			get { return (Brush)GetValue(TitlebarBrushProperty); }
			set
			{
				SetValue(TitlebarBrushProperty, value);
				captionControl.Background = value;
			}
		}

		public static readonly DependencyProperty TitlebarBrushProperty;

		private static void TitlebarBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (StandardWindow)property;
			window.TitlebarBrush = (Brush)args.NewValue;
		}

		public new virtual WindowState WindowState
		{
			get { return base.WindowState; }
			set { SetWindowState(value); }
		}
	}
}