using System;

namespace NativeSharp {
	/// <summary>
	/// Win32模块
	/// </summary>
	public sealed unsafe partial class NativeModule {
		private readonly NativeProcess _process;
		private readonly void* _handle;

		/// <summary>
		/// 模块句柄
		/// </summary>
		public IntPtr Handle => (IntPtr)UnsafeHandle;

		/// <summary>
		/// 模块句柄
		/// </summary>
		public void* UnsafeHandle => _handle;

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="process">Win32进程</param>
		/// <param name="handle">模块句柄</param>
		public NativeModule(NativeProcess process, IntPtr handle) : this(process, (void*)handle) {
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="process">Win32进程</param>
		/// <param name="handle">模块句柄</param>
		public NativeModule(NativeProcess process, void* handle) {
			if (process is null)
				throw new ArgumentNullException(nameof(process));

			_process = process;
			_handle = handle;
		}
	}
}
