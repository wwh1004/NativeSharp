using System;

namespace NativeSharp {
	/// <summary>
	/// Win32模块
	/// </summary>
	public sealed partial class NativeModule {
		private readonly NativeProcess _process;
		private readonly IntPtr _handle;

		/// <summary>
		/// 模块句柄
		/// </summary>
		public IntPtr Handle => _handle;

		/// <summary>
		/// 是否为零
		/// </summary>
		public bool IsZero => _handle == IntPtr.Zero;

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="process">Win32进程</param>
		/// <param name="handle">模块句柄</param>
		public NativeModule(NativeProcess process, IntPtr handle) {
			if (process == null)
				throw new ArgumentNullException(nameof(process));

			_process = process;
			_handle = handle;
		}
	}
}
