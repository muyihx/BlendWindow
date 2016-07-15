using System;

namespace D3bugDesign
{
	public static class Graphics
	{
		//public static bool InitializeAero(Window window, int captionHeight)
		//{
		//	bool aeroEnabled = false;

		//	// XP is version 5, Vista is version 6
		//	if (Environment.OSVersion.Version.Major >= 6)
		//	{
		//		try
		//		{
		//			VistaApi.DwmIsCompositionEnabled(ref aeroEnabled);

		//			if (aeroEnabled)
		//			{
		//				IntPtr mainWindowPtr = new WindowInteropHelper(window).Handle;
		//				HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
		//				mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

		//				// extend glass effect
		//				//VistaApi.Margins margins = new VistaApi.Margins();
		//				//margins.Set(0, 0, captionHeight, 0);

		//				//// inicialize aero
		//				//VistaApi.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
		//			}
		//		}
		//		// glass effect is not supported
		//		catch (DllNotFoundException)
		//		{
		//			aeroEnabled = false;
		//		}
		//	}

		//	return aeroEnabled;
		//}

		public static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
		{
			var a = (WindowsMessage)msg;
			if (a == WindowsMessage.WM_GETICON || a == WindowsMessage.WM_MOUSEFIRST || a == WindowsMessage.WM_NCMOUSELEAVE || a == WindowsMessage.WM_NCHITTEST || a == WindowsMessage.WM_SETCURSOR || a == WindowsMessage.WM_NCMOUSEMOVE) return IntPtr.Zero;

			switch (a)
			{
				case WindowsMessage.WM_GETMINMAXINFO:
					NativeMethods.WmGetMinMaxInfo(hwnd, lparam);
					break;
			}
			return IntPtr.Zero;
		}
	}
}
