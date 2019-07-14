using System;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// Win32环境
	/// </summary>
	public static unsafe class NativeEnvironment {
		private static readonly bool _is64BitOperatingSystem;

		/// <summary>
		/// 是否为64位操作系统
		/// </summary>
		public static bool Is64BitOperatingSystem => _is64BitOperatingSystem;

		static NativeEnvironment() {
			if (IntPtr.Size == 8)
				_is64BitOperatingSystem = true;
			else
				IsWow64Process(GetCurrentProcess(), out _is64BitOperatingSystem);
		}
	}
}
