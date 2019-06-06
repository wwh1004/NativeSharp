using System;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// Win32环境
	/// </summary>
	public static class NativeEnvironment {
		private static readonly bool _is64BitOperatingSystem;

		/// <summary>
		/// 是否为64位操作系统
		/// </summary>
		public static bool Is64BitOperatingSystem => _is64BitOperatingSystem;

		static NativeEnvironment() {
			bool isWow64;

			IsWow64Process(GetCurrentProcess(), out isWow64);
			_is64BitOperatingSystem = !isWow64 && IntPtr.Size == 8;
		}
	}
}
