using System;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// 进程权限
	/// </summary>
	public enum ProcessAccess : uint {
		/// <summary />
		CreateProcess = 0x0080,

		/// <summary />
		CreateThread = 0x0002,

		/// <summary />
		DuplicateHandle = 0x0040,

		/// <summary />
		QueryInformation = 0x0400,

		/// <summary />
		QueryLimitedInformation = 0x1000,

		/// <summary />
		SetInformation = 0x0200,

		/// <summary />
		SetQuota = 0x0100,

		/// <summary />
		SuspendResume = 0x0800,

		/// <summary />
		Synchronize = 0x00100000,

		/// <summary />
		Terminate = 0x0001,

		/// <summary />
		MemoryOperation = 0x0008,

		/// <summary />
		MemoryRead = 0x0010,

		/// <summary />
		MemoryWrite = 0x0020,

		/// <summary />
		AllAccess = STANDARD_RIGHTS_REQUIRED | Synchronize | 0xFFFF
	}

	/// <summary>
	/// Win32进程
	/// </summary>
	public sealed unsafe partial class NativeProcess : IDisposable {
		private static readonly NativeProcess _currentProcess = new NativeProcess(GetCurrentProcessId(), GetCurrentProcess(), ProcessAccess.AllAccess);

		private readonly uint _id;
		private readonly IntPtr _handle;
		private readonly ProcessAccess? _access;
		private bool _isDisposed;

		/// <summary>
		/// 当前进程
		/// </summary>
		public static NativeProcess CurrentProcess => _currentProcess;

		/// <summary>
		/// 进程ID
		/// </summary>
		public uint Id {
			get {
				QuickDemand(0);
				return _id;
			}
		}

		/// <summary>
		/// 打开进程时获取的句柄
		/// </summary>
		public IntPtr Handle {
			get {
				QuickDemand(0);
				return _handle;
			}
		}

		/// <summary>
		/// 是否为有效句柄
		/// </summary>
		public bool IsValid {
			get {
				QuickDemand(0);
				return _handle != IntPtr.Zero;
			}
		}

		private NativeProcess(uint id, IntPtr handle, ProcessAccess? access) {
			_id = id;
			_handle = handle;
			_access = access;
		}

		/// <summary>
		/// 打开进程
		/// </summary>
		/// <param name="id">进程ID</param>
		/// <returns></returns>
		public static NativeProcess Open(uint id) {
			return Open(id, ProcessAccess.AllAccess);
		}

		/// <summary>
		/// 打开进程
		/// </summary>
		/// <param name="id">进程ID</param>
		/// <param name="access">权限</param>
		/// <returns></returns>
		public static NativeProcess Open(uint id, ProcessAccess access) {
			IntPtr processHandle;

			access |= ProcessAccess.QueryInformation;
			processHandle = OpenProcess((uint)access, false, id);
			return processHandle == IntPtr.Zero ? null : new NativeProcess(id, processHandle, access);
		}

		/// <summary>
		/// 通过已有句柄打开进程，并且跳过权限检查
		/// </summary>
		/// <param name="handle">进程句柄</param>
		/// <returns></returns>
		public static NativeProcess UnsafeOpen(IntPtr handle) {
			if (handle == IntPtr.Zero)
				return null;

			uint id;

			id = GetProcessId(handle);
			return id == 0 ? null : new NativeProcess(id, handle, null);
		}

		/// <summary>
		/// 通过已有句柄打开进程，并且跳过权限检查
		/// </summary>
		/// <param name="id">进程ID</param>
		/// <param name="handle">进程句柄</param>
		/// <returns></returns>
		public static NativeProcess UnsafeOpen(uint id, IntPtr handle) {
			return handle == IntPtr.Zero ? null : new NativeProcess(id, handle, null);
		}

		/// <summary>
		/// 快速严重当前实例是否有效
		/// </summary>
		/// <param name="requireAccess">需要的权限</param>
		public void QuickDemand(ProcessAccess requireAccess) {
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

		/// <summary />
		public void Dispose() {
			if (_isDisposed)
				return;

			CloseHandle(_handle);
			_isDisposed = true;
		}
	}
}
