using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;

namespace D3bugDesign
{
	public partial class WindowButton : Button
	{
		public new object Content
		{
			get { return GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); RefreshContent(); }
		}

		public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(WindowButton), new UIPropertyMetadata());

		public object ContentDisabled
		{
			get { return GetValue(ContentDisabledProperty); }
			set { SetValue(ContentDisabledProperty, value); RefreshContent(); }
		}

		public static readonly DependencyProperty ContentDisabledProperty = DependencyProperty.Register("ContentDisabled", typeof(object), typeof(WindowButton), new UIPropertyMetadata());

		public CornerRadius CornerRadius
		{
			get { return (CornerRadius)GetValue(CornerRadiusDpProperty); }
			set { SetValue(CornerRadiusDpProperty, value); }
		}

		public static readonly DependencyProperty CornerRadiusDpProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(WindowButton), new UIPropertyMetadata(new CornerRadius()));


		[System.ComponentModel.Bindable(true)]
		public object ActiveContent
		{
			get { return GetValue(ActiveContentProperty); }
			set { SetValue(ActiveContentProperty, value); }
		}

		public static readonly DependencyProperty ActiveContentProperty = DependencyProperty.Register("ActiveContent", typeof(object), typeof(WindowButton), new UIPropertyMetadata());

		public virtual Brush BackgroundDefaultValue => (Brush)FindResource("DefaultBackgroundBrush");

		public WindowButton()
		{
			InitializeComponent();
			WindowChrome.SetIsHitTestVisibleInChrome(this, true);
			IsEnabledChanged += (s, e) => RefreshContent();
		}


		protected void RefreshContent()
		{
			// Button is enabled
			ActiveContent = IsEnabled ? Content : ContentDisabled;
		}
	}
}