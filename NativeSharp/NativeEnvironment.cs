using System;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	public static class NativeEnvironment {
		private static readonly bool _is64BitOperatingSystem;

		public static bool Is64BitOperatingSystem => _is64BitOperatingSystem;

		static NativeEnvironment() {
			bool isWow64;

			IsWow64Process(GetCurrentProcess(), out isWow64);
			_is64BitOperatingSystem = !isWow64 && IntPtr.Size == 8;
		}
	}
}
