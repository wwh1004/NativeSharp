using System;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// Win32环境
	/// </summary>
	public static unsafe class NativeEnvironment {
		private static readonly bool _is64BitOperatingSystem = GetIs64BitOperatingSystem();

		/// <summary>
		/// 是否为64位操作系统
		/// </summary>
		public static bool Is64BitOperatingSystem => _is64BitOperatingSystem;

		private static bool GetIs64BitOperatingSystem() {
			bool is64BitOperatingSystem;
			if (IntPtr.Size == 8)
				is64BitOperatingSystem = true;
			else
				IsWow64Process(GetCurrentProcess(), out is64BitOperatingSystem);
			return is64BitOperatingSystem;
		}
	}
}
