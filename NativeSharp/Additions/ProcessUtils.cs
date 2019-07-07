using System;
using System.ComponentModel;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	unsafe partial class NativeProcess {
		/// <summary>
		/// 获取所有进程ID
		/// </summary>
		/// <returns></returns>
		public static uint[] GetAllProcessIds() {
			uint[] buffer;
			uint bytesReturned;
			uint[] processIds;

			buffer = null;
			do {
				if (buffer is null)
					buffer = new uint[0x200];
				else
					buffer = new uint[buffer.Length * 2];
				fixed (uint* p = buffer)
					if (!EnumProcesses(p, (uint)(buffer.Length * 4), out bytesReturned))
						return null;
			} while (bytesReturned == buffer.Length * 4);
			processIds = new uint[bytesReturned / 4];
			for (int i = 0; i < processIds.Length; i++)
				processIds[i] = buffer[i];
			return processIds;
		}

		internal static bool Is64BitProcessInternal(IntPtr processHandle, out bool is64Bit) {
			bool isWow64;

			if (!NativeEnvironment.Is64BitOperatingSystem) {
				is64Bit = false;
				return true;
			}
			if (!IsWow64Process(processHandle, out isWow64)) {
				is64Bit = false;
				return false;
			}
			is64Bit = !isWow64;
			return true;
		}

		internal static IntPtr GetModuleHandleInternal(IntPtr processHandle, bool first, string moduleName) {
			IntPtr moduleHandle;
			uint size;
			IntPtr[] moduleHandles;
			StringBuilder moduleNameBuffer;

			if (!EnumProcessModulesEx(processHandle, &moduleHandle, (uint)IntPtr.Size, out size, LIST_MODULES_ALL))
				return IntPtr.Zero;
			if (first)
				return moduleHandle;
			moduleHandles = new IntPtr[size / IntPtr.Size];
			fixed (IntPtr* p = moduleHandles)
				if (!EnumProcessModulesEx(processHandle, p, size, out _, LIST_MODULES_ALL))
					return IntPtr.Zero;
			moduleNameBuffer = new StringBuilder((int)MAX_MODULE_NAME32);
			for (int i = 0; i < moduleHandles.Length; i++) {
				if (!GetModuleBaseName(processHandle, moduleHandles[i], moduleNameBuffer, MAX_MODULE_NAME32))
					return IntPtr.Zero;
				if (moduleNameBuffer.ToString().Equals(moduleName, StringComparison.OrdinalIgnoreCase))
					return moduleHandles[i];
			}
			return IntPtr.Zero;
		}

		internal static void ThrowWin32ExceptionIfFalse(bool result) {
			if (!result)
				throw new Win32Exception();
		}
	}
}
