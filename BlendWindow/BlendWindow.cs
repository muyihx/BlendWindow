using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
[assembly: CLSCompliant(true)]
namespace D3bugDesign
{
	[CLSCompliant(true)]
	public class BlendWindow : Window
	{
		public enum WindowButtonState { Normal, Disabled, None }
		public enum CloseActionOption { Hide, Close, Minimize }
		private Border captionControl;
		private WindowCloseButton closeButton;
		private WindowButtonState closeButtonState;
		private Border contentWindowBackgroundBorder;
		private Border contentWindowBorder;
		private int lastMouseCaptionClick;
		private WindowMaximizeButton maximizeButton;
		private WindowButtonState maximizeButtonState;
		private WindowMinimizeButton minimizeButton;
		private WindowButtonState minimizeButtonState;
		private WindowRestoreButton restoreButton;
		private HwndSource hwndSource;
		private int captionHeight;
		public bool ContentExtend { get; set; }

		public WindowButtonState MinimizeButtonState
		{
			get { return minimizeButtonState; }
			set
			{
				minimizeButtonState = value;
				OnWindowButtonStateChange(value, minimizeButton);
			}
		}

		public WindowButtonState MaximizeButtonState
		{
			get { return maximizeButtonState; }
			set
			{
				maximizeButtonState = value;
				OnWindowButtonStateChange(value, restoreButton);
				OnWindowButtonStateChange(value, maximizeButton);
			}
		}

		public WindowButtonState CloseButtonState
		{
			get { return closeButtonState; }
			set
			{
				closeButtonState = value;
				OnWindowButtonStateChange(value, closeButton);
			}
		}

		public new virtual WindowState WindowState
		{
			get { return base.WindowState; }
			set { SetWindowState(value); }
		}

		static BlendWindow()
		{
			if (Assembly.GetEntryAssembly() != null)
			{
				CloseActionOptionProperty = DependencyProperty.Register("CloseAction", typeof(CloseActionOption), typeof(BlendWindow), new UIPropertyMetadata(CloseActionOption.Close, CloseActionPropertyChanged));
				HideTitlebarButtonsProperty = DependencyProperty.Register("HideTitlebarButtons", typeof(bool), typeof(BlendWindow), new UIPropertyMetadata(false, HideTitlebarButtonsPropertyChanged));
				CaptionHeightMaximizedProperty = DependencyProperty.Register("CaptionHeightMaximized", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(25));
				ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(null, ContentChangedCallback));
				BackgroundProperty = DependencyProperty.Register("Background", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.Transparent, BackgroundChangedCallback));
				BackgroundDisabledProperty = DependencyProperty.Register("BackgroundDisabled", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.Transparent, BackgroundDisabledChangedCallback));
				BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.Transparent, BorderBrushChangedCallback));
				BorderBrushDisabledProperty = DependencyProperty.Register("BorderBrushDisabled", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.Transparent, BorderBrushDisabledChangedCallback));
				TitleBarBackgroundProperty = DependencyProperty.Register("TitleBarBackground", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.DarkBlue, TitlebarBackgroundBrushChangedCallback));
				TitleBarBackgroundDisabledProperty = DependencyProperty.Register("TitleBarBackgroundDisabled", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.DarkBlue, TitlebarBackgroundDisabledBrushChangedCallback));
				TitleBarForegroundProperty = DependencyProperty.Register("TitleBarForeground", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.White, TitlebarForegroundBrushChangedCallback));
				TitleBarForegroundDisabledProperty = DependencyProperty.Register("TitleBarForegroundDisabled", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.DarkGray, TitlebarForegroundDisabledBrushChangedCallback));
				TitleBarButtonBackgroundProperty = DependencyProperty.Register("TitleBarButtonBackground", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.White, TitlebarButtonBackgroundBrushChangedCallback));
				TitleBarButtonBackgroundDisabledProperty = DependencyProperty.Register("TitleBarButtonBackgroundDisabled", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.DarkGray, TitlebarButtonBackgroundDisabledBrushChangedCallback));
				TitleBarButtonForegroundProperty = DependencyProperty.Register("TitleBarButtonForeground", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.White, TitlebarButtonForegroundBrushChangedCallback));
				TitleBarButtonForegroundDisabledProperty = DependencyProperty.Register("TitleBarButtonForegroundDisabled", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.DarkGray, TitlebarButtonForegroundDisabledBrushChangedCallback));
				TitleBarButtonBorderProperty = DependencyProperty.Register("TitleBarButtonBorder", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.White, TitlebarButtonBorderBrushChangedCallback));
				TitleBarButtonBorderDisabledProperty = DependencyProperty.Register("TitleBarButtonBorderDisabled", typeof(object), typeof(BlendWindow), new UIPropertyMetadata(Brushes.DarkGray, TitlebarButtonBorderDisabledBrushChangedCallback));
			}
		}

		public BlendWindow()
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
			fromHwnd?.AddHook(WindowProc);
		}

		public static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
		{
			var a = (WindowsMessage)msg;
			switch (a)
			{
				case WindowsMessage.WM_GETMINMAXINFO:
					NativeMethods.WmGetMinMaxInfo(hwnd, lparam);
					break;
			}
			return IntPtr.Zero;
		}

		protected void InitializeContentControls()
		{
			WindowStyle = WindowStyle.None;
			base.BorderBrush = BorderBrush;
			BorderThickness = new Thickness(3);
			minimizeButton = new WindowMinimizeButton(this);
			minimizeButton.Click += OnButtonMinimize_Click;
			restoreButton = new WindowRestoreButton(this);
			restoreButton.Click += OnButtonRestore_Click;
			restoreButton.Margin = new Thickness(-1, 0, 0, 0);
			maximizeButton = new WindowMaximizeButton(this);
			maximizeButton.Click += OnButtonMaximize_Click;
			maximizeButton.Margin = new Thickness(-1, 0, 0, 0);
			closeButton = new WindowCloseButton(this);
			closeButton.Click += OnButtonClose_Click;
			closeButton.Margin = new Thickness(-1, 0, 0, 0);

			// put buttons into StackPanel
			var buttonsStackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center };
			buttonsStackPanel.Children.Add(minimizeButton);
			buttonsStackPanel.Children.Add(restoreButton);
			buttonsStackPanel.Children.Add(maximizeButton);
			buttonsStackPanel.Children.Add(closeButton);

			// put stack into border
			var buttonsBorder = new Border
			{
				BorderThickness = new Thickness(0),
				VerticalAlignment = VerticalAlignment.Center,
				Child = buttonsStackPanel
			};
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
			contentWindowBackgroundBorder = new Border { Background = Background };
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
			TextBlock.SetForeground(captionControl, TitleBarForeground);
			contentWindowBorder.Margin = ContentExtend ? new Thickness(0) : new Thickness(0, CaptionHeight, 0, 0);
			Height = Height + 1;
			Height = Height - 1;
			StateChanged += StandardWindow_StateChanged;
			Activated += OnStandardWindowActivated;
			Deactivated += OnStandardWindowDeactivated;
			OnStateChanged(new EventArgs());
		}

		private void SetWindowState(WindowState state)
		{
			if (state == WindowState.Maximized)
			{

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
				ResizeMode = ResizeMode.CanResize;
				restoreButton.Visibility = Visibility.Collapsed;
				if (maximizeButtonState != WindowButtonState.None)
				{
					maximizeButton.Visibility = HideTitlebarButtons ? Visibility.Hidden : Visibility.Visible;
				}
				captionControl.Height = CaptionHeight;
				var par = CaptionHeight * 0.7;
				minimizeButton.Height = par;
				maximizeButton.Height = par;
				restoreButton.Height = par;
				closeButton.Height = par;

			}
			else if (WindowState == WindowState.Maximized)
			{
				maximizeButton.Visibility = Visibility.Collapsed;
				ResizeMode = ResizeMode.NoResize;
				if (maximizeButtonState != WindowButtonState.None)
				{
					restoreButton.Visibility = HideTitlebarButtons ? Visibility.Hidden : Visibility.Visible;
				}
				captionControl.Height = CaptionHeightMaximized;
				var par = CaptionHeightMaximized * 0.7;
				minimizeButton.Height = par;
				maximizeButton.Height = par;
				restoreButton.Height = par;
				closeButton.Height = par;
			}
		}

		protected virtual void OnStandardWindowActivated(object sender, EventArgs e)
		{
			minimizeButton.Enable();
			maximizeButton.Enable();
			restoreButton.Enable();
			closeButton.Enable();
			captionControl.Background = TitleBarBackground;
			contentWindowBackgroundBorder.Background = Background;
			base.BorderBrush = BorderBrush;
			TextBlock.SetForeground(captionControl, TitleBarForeground);
		}

		protected virtual void OnStandardWindowDeactivated(object sender, EventArgs e)
		{
			minimizeButton.Disable();
			maximizeButton.Disable();
			restoreButton.Disable();
			closeButton.Disable();
			captionControl.Background = TitleBarBackgroundDisabled;
			contentWindowBackgroundBorder.Background = BackgroundDisabled;
			base.BorderBrush = BorderBrushDisabled;
			TextBlock.SetForeground(captionControl, TitleBarForegroundDisabled);
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

		#region Titlebar Button Event Handlers
		protected virtual void OnButtonMinimize_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		protected virtual void OnButtonRestore_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Normal;
		}

		protected virtual void OnButtonMaximize_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Maximized;
		}

		protected virtual void OnButtonClose_Click(object sender, RoutedEventArgs e)
		{
			switch (CloseAction)
			{
				case CloseActionOption.Hide:
					Hide();
					break;
				case CloseActionOption.Close:
					Close();
					break;
				case CloseActionOption.Minimize:
					WindowState = WindowState.Minimized;
					break;
			}
		}

		#endregion

		#region Dependency Properties

		#region Window

		public static readonly DependencyProperty CloseActionOptionProperty;

		private static void CloseActionPropertyChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.CloseAction = (CloseActionOption)args.NewValue;
		}

		public CloseActionOption CloseAction
		{
			get { return (CloseActionOption)GetValue(CloseActionOptionProperty); }
			set
			{
				SetValue(CloseActionOptionProperty, value);
			}
		}

		public static readonly DependencyProperty HideTitlebarButtonsProperty;

		private static void HideTitlebarButtonsPropertyChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.HideTitlebarButtons = (bool)args.NewValue;
			window.closeButton.Visibility = window.HideTitlebarButtons ? Visibility.Hidden : Visibility.Visible;
			window.minimizeButton.Visibility = window.HideTitlebarButtons ? Visibility.Hidden : Visibility.Visible;
			window.maximizeButton.Visibility = window.HideTitlebarButtons ? Visibility.Hidden : Visibility.Visible;
			window.restoreButton.Visibility = window.HideTitlebarButtons ? Visibility.Hidden : Visibility.Visible;
		}

		public bool HideTitlebarButtons
		{
			get { return (bool)GetValue(HideTitlebarButtonsProperty); }
			set
			{
				SetValue(HideTitlebarButtonsProperty, value);
			}
		}

		#endregion

		#region Caption

		[TypeConverter(typeof(TypeConverterStringToUiElement))]
		public UIElement Caption
		{
			get { return captionControl.Child; }
			set { captionControl.Child = value; }
		}

		public int CaptionHeight
		{
			get { return captionHeight; }
			set { captionHeight = value; }
		}

		public static readonly DependencyProperty CaptionHeightMaximizedProperty;

		public int CaptionHeightMaximized
		{
			get { return (int)GetValue(CaptionHeightMaximizedProperty); }
			set { SetValue(CaptionHeightMaximizedProperty, value); }
		}

		#endregion

		#region Titlebar

		public static readonly DependencyProperty TitleBarBackgroundProperty;

		private static void TitlebarBackgroundBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarBackground = (Brush)args.NewValue;
		}

		public Brush TitleBarBackground
		{
			get { return (Brush)GetValue(TitleBarBackgroundProperty); }
			set
			{
				SetValue(TitleBarBackgroundProperty, value);
				captionControl.Background = value;
			}
		}


		public static readonly DependencyProperty TitleBarBackgroundDisabledProperty;

		private static void TitlebarBackgroundDisabledBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarBackgroundDisabled = (Brush)args.NewValue;
		}

		public Brush TitleBarBackgroundDisabled
		{
			get { return (Brush)GetValue(TitleBarBackgroundDisabledProperty); }
			set { SetValue(TitleBarBackgroundDisabledProperty, value); }
		}


		public static readonly DependencyProperty TitleBarForegroundProperty;

		private static void TitlebarForegroundBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarForeground = (Brush)args.NewValue;
		}

		public Brush TitleBarForeground
		{
			get { return (Brush)GetValue(TitleBarForegroundProperty); }
			set
			{
				SetValue(TitleBarForegroundProperty, value);
				TextBlock.SetForeground(captionControl, value);
			}
		}


		public static readonly DependencyProperty TitleBarForegroundDisabledProperty;

		private static void TitlebarForegroundDisabledBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarForegroundDisabled = (Brush)args.NewValue;
		}

		public Brush TitleBarForegroundDisabled
		{
			get { return (Brush)GetValue(TitleBarForegroundDisabledProperty); }
			set { SetValue(TitleBarForegroundDisabledProperty, value); }
		}


		public static readonly DependencyProperty TitleBarButtonBackgroundProperty;

		private static void TitlebarButtonBackgroundBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarButtonBackground = (Brush)args.NewValue;
		}

		public Brush TitleBarButtonBackground
		{
			get { return (Brush)GetValue(TitleBarButtonBackgroundProperty); }
			set { SetValue(TitleBarButtonBackgroundProperty, value); }
		}


		public static readonly DependencyProperty TitleBarButtonBackgroundDisabledProperty;

		private static void TitlebarButtonBackgroundDisabledBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarButtonBackgroundDisabled = (Brush)args.NewValue;
		}

		public Brush TitleBarButtonBackgroundDisabled
		{
			get { return (Brush)GetValue(TitleBarButtonBackgroundDisabledProperty); }
			set { SetValue(TitleBarButtonBackgroundDisabledProperty, value); }
		}


		public static readonly DependencyProperty TitleBarButtonForegroundProperty;

		private static void TitlebarButtonForegroundBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarButtonForeground = (Brush)args.NewValue;
		}

		public Brush TitleBarButtonForeground
		{
			get { return (Brush)GetValue(TitleBarButtonForegroundDisabledProperty); }
			set { SetValue(TitleBarButtonForegroundDisabledProperty, value); }
		}


		public static readonly DependencyProperty TitleBarButtonForegroundDisabledProperty;

		private static void TitlebarButtonForegroundDisabledBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarButtonForegroundDisabled = (Brush)args.NewValue;
		}

		public Brush TitleBarButtonForegroundDisabled
		{
			get { return (Brush)GetValue(TitleBarButtonForegroundDisabledProperty); }
			set { SetValue(TitleBarButtonForegroundDisabledProperty, value); }
		}


		public static readonly DependencyProperty TitleBarButtonBorderProperty;

		private static void TitlebarButtonBorderBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarButtonBorder = (Brush)args.NewValue;
		}

		public Brush TitleBarButtonBorder
		{
			get { return (Brush)GetValue(TitleBarButtonBorderProperty); }
			set { SetValue(TitleBarButtonBorderProperty, value); }
		}

		public static readonly DependencyProperty TitleBarButtonBorderDisabledProperty;

		private static void TitlebarButtonBorderDisabledBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.TitleBarButtonBorderDisabled = (Brush)args.NewValue;
		}

		public Brush TitleBarButtonBorderDisabled
		{
			get { return (Brush)GetValue(TitleBarButtonBorderDisabledProperty); }
			set { SetValue(TitleBarButtonBorderDisabledProperty, value); }
		}

		#endregion

		#region Content

		public new static readonly DependencyProperty ContentProperty;

		private static void ContentChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.contentWindowBorder.Child = (UIElement)args.NewValue;
		}

		public new object Content
		{
			get { return GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public new static readonly DependencyProperty BackgroundProperty;

		private static void BackgroundChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.contentWindowBackgroundBorder.Background = (Brush)args.NewValue;
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

		public static readonly DependencyProperty BackgroundDisabledProperty;

		private static void BackgroundDisabledChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.contentWindowBackgroundBorder.Background = (Brush)args.NewValue;
		}

		public Brush BackgroundDisabled
		{
			get { return (Brush)GetValue(BackgroundDisabledProperty); }
			set { SetValue(BackgroundDisabledProperty, value); }
		}

		public new static readonly DependencyProperty BorderBrushProperty;

		private static void BorderBrushChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.BorderBrush = (Brush)args.NewValue;
		}

		public new Brush BorderBrush
		{
			get { return (Brush)GetValue(BorderBrushProperty); }
			set { SetValue(BorderBrushProperty, value); }
		}

		public static readonly DependencyProperty BorderBrushDisabledProperty;

		private static void BorderBrushDisabledChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
		{
			var window = (BlendWindow)property;
			window.BorderBrushDisabled = (Brush)args.NewValue;
		}

		public Brush BorderBrushDisabled
		{
			get { return (Brush)GetValue(BorderBrushDisabledProperty); }
			set { SetValue(BorderBrushDisabledProperty, value); }
		}

		#endregion

		#endregion
	}
}