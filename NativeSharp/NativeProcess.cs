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
		private static readonly NativeProcess _invalidProcess = new NativeProcess(0, null, 0) { _isDisposed = true };
		private static readonly NativeProcess _currentProcess = new NativeProcess(GetCurrentProcessId(), GetCurrentProcess(), ProcessAccess.AllAccess);

		private readonly uint _id;
		private readonly void* _handle;
		private readonly ProcessAccess? _access;
		private bool _isDisposed;

		/// <summary>
		/// 表示一个无效进程
		/// </summary>
		public static NativeProcess InvalidProcess => _invalidProcess;

		/// <summary>
		/// 当前进程
		/// </summary>
		public static NativeProcess CurrentProcess => _currentProcess;

		/// <summary>
		/// 当前实例是否当前进程
		/// </summary>
		public bool IsCurrentProcess => this == CurrentProcess;

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
		public void* Handle {
			get {
				QuickDemand(0);
				return _handle;
			}
		}

		/// <summary>
		/// 当前句柄是否为无效句柄
		/// </summary>
		public bool IsInvalid => _handle == null;

		private NativeProcess(uint id, void* handle, ProcessAccess? access) {
			_id = id;
			_handle = handle;
			_access = access;
		}

		/// <summary>
		/// 打开进程，失败时返回 <see cref="InvalidProcess"/>
		/// </summary>
		/// <param name="id">进程ID</param>
		/// <returns></returns>
		public static NativeProcess Open(uint id) {
			return Open(id, ProcessAccess.AllAccess);
		}

		/// <summary>
		/// 打开进程，失败时返回 <see cref="InvalidProcess"/>
		/// </summary>
		/// <param name="id">进程ID</param>
		/// <param name="access">权限</param>
		/// <returns></returns>
		public static NativeProcess Open(uint id, ProcessAccess access) {
			access |= ProcessAccess.QueryInformation;
			void* processHandle = OpenProcess((uint)access, false, id);
			return processHandle == null ? InvalidProcess : new NativeProcess(id, processHandle, access);
		}

		/// <summary>
		/// 通过已有句柄打开进程，并且跳过权限检查，失败时返回 <see cref="InvalidProcess"/>
		/// </summary>
		/// <param name="handle">进程句柄</param>
		/// <returns></returns>
		public static NativeProcess UnsafeOpen(void* handle) {
			if (handle == null)
				return InvalidProcess;

			uint id = GetProcessId(handle);
			return id != 0 ? new NativeProcess(id, handle, null) : InvalidProcess;
		}

		/// <summary>
		/// 通过已有句柄打开进程，并且跳过权限检查，失败时返回 <see cref="InvalidProcess"/>
		/// </summary>
		/// <param name="id">进程ID</param>
		/// <param name="handle">进程句柄</param>
		/// <returns></returns>
		public static NativeProcess UnsafeOpen(uint id, void* handle) {
			return handle != null ? new NativeProcess(id, handle, null) : InvalidProcess;
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
			return !_access.HasValue || (_access.Value & requireAccess) == requireAccess;
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
