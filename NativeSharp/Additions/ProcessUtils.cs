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

		internal static bool Is64BitProcessInternal(void* processHandle, out bool is64Bit) {
			bool isWow64;

			is64Bit = false;
			if (!NativeEnvironment.Is64BitOperatingSystem)
				return true;
			if (!IsWow64Process(processHandle, out isWow64))
				return false;
			is64Bit = !isWow64;
			return true;
		}

		internal static void* GetModuleHandleInternal(void* processHandle, bool first, string moduleName) {
			void* moduleHandle;
			uint size;
			void*[] moduleHandles;
			StringBuilder moduleNameBuffer;

			if (!EnumProcessModulesEx(processHandle, &moduleHandle, (uint)IntPtr.Size, out size, LIST_MODULES_ALL))
				return null;
			if (first)
				return moduleHandle;
			moduleHandles = new void*[size / (uint)IntPtr.Size];
			fixed (void** p = moduleHandles)
				if (!EnumProcessModulesEx(processHandle, p, size, out _, LIST_MODULES_ALL))
					return null;
			moduleNameBuffer = new StringBuilder((int)MAX_MODULE_NAME32);
			for (int i = 0; i < moduleHandles.Length; i++) {
				if (!GetModuleBaseName(processHandle, moduleHandles[i], moduleNameBuffer, MAX_MODULE_NAME32))
					return null;
				if (moduleNameBuffer.ToString().Equals(moduleName, StringComparison.OrdinalIgnoreCase))
					return moduleHandles[i];
			}
			return null;
		}

		internal static void ThrowWin32ExceptionIfFalse(bool result) {
			if (!result)
				throw new Win32Exception();
		}
	}
}
