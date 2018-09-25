using System;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	public sealed unsafe partial class NativeProcess : IDisposable {
		private static readonly NativeProcess _currentProcess = new NativeProcess(GetCurrentProcessId(), GetCurrentProcess(), PROCESS_ALL_ACCESS);

		private readonly uint _id;
		private readonly IntPtr _handle;
		private readonly uint? _access;
		private bool _isDisposed;

		public static NativeProcess CurrentProcess => _currentProcess;

		public uint Id {
			get {
				QuickDemand(0);
				return _id;
			}
		}

		public IntPtr Handle {
			get {
				QuickDemand(0);
				return _handle;
			}
		}

		public bool IsValid {
			get {
				QuickDemand(0);
				return _handle != IntPtr.Zero;
			}
		}

		private NativeProcess(uint processId, IntPtr processHandle, uint? access) {
			_id = processId;
			_handle = processHandle;
			_access = access;
		}

		public static NativeProcess Open(uint processId) => Open(processId, PROCESS_ALL_ACCESS);

		public static NativeProcess Open(uint processId, uint access) {
			IntPtr processHandle;

			processHandle = OpenProcess(access | PROCESS_QUERY_INFORMATION, false, processId);
			return processHandle == IntPtr.Zero ? null : new NativeProcess(processId, processHandle, access | PROCESS_QUERY_INFORMATION);
		}

		public static NativeProcess UnsafeOpen(IntPtr processHandle) {
			// 跳过权限检查
			if (processHandle == IntPtr.Zero)
				return null;

			uint processId;

			processId = GetProcessId(processHandle);
			return processId == 0 ? null : new NativeProcess(processId, processHandle, null);
		}

		public static NativeProcess UnsafeOpen(uint processId, IntPtr processHandle) => processHandle == IntPtr.Zero ? null : new NativeProcess(processId, processHandle, null);
		// 跳过权限检查

		public void QuickDemand(uint requireAccess) {
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(NativeProcess));
			if (_access != null && (_access.Value & requireAccess) != requireAccess)
				throw new NotSupportedException($"CurrentAccess={_access} RequireAccess={requireAccess}");
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
				if (!EnumProcessModulesEx(processHandle, p, size, out size, LIST_MODULES_ALL))
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

		public void Dispose() {
			if (_isDisposed)
				return;

			CloseHandle(_handle);
			_isDisposed = true;
		}
	}
}
