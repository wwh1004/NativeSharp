using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	unsafe partial class NativeModule {
		/// <summary>
		/// 名称
		/// </summary>
		public string Name {
			get {
				StringBuilder name;

				_process.QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
				name = new StringBuilder((int)MAX_MODULE_NAME32);
				return GetModuleBaseName(_process.Handle, _handle, name, MAX_MODULE_NAME32) ? name.ToString() : null;
			}
		}

		/// <summary>
		/// 文件路径
		/// </summary>
		public string ImagePath {
			get {
				StringBuilder iamgePath;

				_process.QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
				iamgePath = new StringBuilder((int)MAX_PATH);
				return GetModuleFileNameEx(_process.Handle, _handle, iamgePath, MAX_PATH) ? iamgePath.ToString() : null;
			}
		}

		/// <summary />
		public override string ToString() {
			return $"{Name} (Address: 0x{Handle.ToString(_process.Is64Bit ? "X16" : "X8")})";
		}
	}
}
