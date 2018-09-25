using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	unsafe partial class NativeModule {
		public string Name {
			get {
				StringBuilder name;

				_process.QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
				name = new StringBuilder((int)MAX_MODULE_NAME32);
				return GetModuleBaseName(_process.Handle, _handle, name, MAX_MODULE_NAME32) ? name.ToString() : null;
			}
		}

		public string ImagePath {
			get {
				StringBuilder iamgePath;

				_process.QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
				iamgePath = new StringBuilder((int)MAX_PATH);
				return GetModuleFileNameEx(_process.Handle, _handle, iamgePath, MAX_PATH) ? iamgePath.ToString() : null;
			}
		}

		public override string ToString() => $"{Name} (Address: 0x{Handle.ToString(_process.Is64Bit ? "X16" : "X8")})";
	}
}
