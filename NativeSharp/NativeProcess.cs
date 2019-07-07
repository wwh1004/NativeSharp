using System;
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
		/// 是否为零
		/// </summary>
		public bool IsZero {
			get {
				QuickDemand(0);
				return _handle == IntPtr.Zero;
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
		/// 确保当前实例未被释放并且确保拥有所需权限
		/// </summary>
		/// <param name="requireAccess">需要的权限</param>
		public void QuickDemand(ProcessAccess requireAccess) {
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(NativeProcess));
			if (!(_access is null) && (_access.Value & requireAccess) != requireAccess)
				throw new NotSupportedException($"CurrentAccess={_access} RequireAccess={requireAccess}");
		}

		/// <summary>
		/// 确保当前实例未被释放并且确保拥有所需权限
		/// </summary>
		/// <param name="requireAccess">需要的权限</param>
		/// <returns></returns>
		public bool QuickDemandNoThrow(ProcessAccess requireAccess) {
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(NativeProcess));
			return _access is null || (_access.Value & requireAccess) == requireAccess;
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
