using System;

namespace NativeSharp {
	public sealed partial class NativeModule {
		private readonly NativeProcess _process;
		private readonly IntPtr _handle;

		public IntPtr Handle => _handle;

		public bool IsValid => _handle != IntPtr.Zero;

		public NativeModule(NativeProcess process, IntPtr handle) {
			if (process == null)
				throw new ArgumentNullException(nameof(process));

			_process = process;
			_handle = handle;
		}
	}
}
