using System;
using System.Runtime.InteropServices;

namespace D3bugDesign
{
	public static class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct Point
		{
			public int x;
			public int y;

			public Point(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Minmaxinfo
		{
			public Point ptReserved;
			public Point ptMaxSize;
			public Point ptMaxPosition;
			public Point ptMinTrackSize;
			public Point ptMaxTrackSize;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 0)]
		public struct Rect
		{
			public readonly int left;
			public readonly int top;
			public readonly int right;
			public readonly int bottom;

			public static readonly Rect Empty;

			public int Width => Math.Abs(right - left);

			public int Height => bottom - top;

			public Rect(int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			public Rect(Rect rcSrc)
			{
				left = rcSrc.left;
				top = rcSrc.top;
				right = rcSrc.right;
				bottom = rcSrc.bottom;
			}

			public bool IsEmpty => left >= right || top >= bottom;

			public override string ToString()
			{
				if (this == Empty)
				{
					return "Rect {Empty}";
				}
				return "Rect { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
			}

			public override bool Equals(object obj)
			{
				if (!(obj is System.Windows.Rect))
				{
					return false;
				}
				return this == (Rect)obj;
			}

			public override int GetHashCode()
			{
				return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
			}

			public static bool operator ==(Rect rect1, Rect rect2)
			{
				return rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom;
			}

			public static bool operator !=(Rect rect1, Rect rect2)
			{
				return !(rect1 == rect2);
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class Monitorinfo
		{
			public int cbSize = Marshal.SizeOf(typeof(Monitorinfo));
			public Rect rcMonitor = new Rect();
			public Rect rcWork = new Rect();
			public int dwFlags = 0;
		}

		[DllImport("user32.dll")]
		internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

		[DllImport("user32.dll")]
		internal static extern bool GetMonitorInfo(IntPtr hMonitor, Monitorinfo lpmi);

		public static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
		{
			var mmi = (Minmaxinfo)Marshal.PtrToStructure(lParam, typeof(Minmaxinfo));
			var MONITOR_DEFAULTTONEAREST = 0x00000002;
			var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
			if (monitor != IntPtr.Zero)
			{
				var monitorInfo = new Monitorinfo();
				GetMonitorInfo(monitor, monitorInfo);
				var rcWorkArea = monitorInfo.rcWork;
				var rcMonitorArea = monitorInfo.rcMonitor;
				mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
				mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
				mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
				mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
				mmi.ptMaxTrackSize.x = mmi.ptMaxSize.x;
				mmi.ptMaxTrackSize.y = mmi.ptMaxSize.y;
			}

			Marshal.StructureToPtr(mmi, lParam, true);
		}
	}
}
