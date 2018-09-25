using System;
using System.IO;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	unsafe partial class NativeProcess {
		public bool Is64Bit {
			get {
				bool is64Bit;

				QuickDemand(PROCESS_QUERY_INFORMATION);
				Is64BitProcessInternal(_handle, out is64Bit);
				return is64Bit;
			}
		}

		public string Name => Path.GetFileName(ImagePath);

		public string ImagePath {
			get {
				StringBuilder iamgePath;

				QuickDemand(PROCESS_QUERY_INFORMATION);
				iamgePath = new StringBuilder((int)MAX_PATH);
				return GetProcessImageFileName(_handle, iamgePath, MAX_PATH) ? iamgePath.ToString() : null;
			}
		}

		public NativeModule[] GetModules() {
			IntPtr moduleHandle;
			uint size;
			IntPtr[] moduleHandles;
			NativeModule[] modules;

			QuickDemand(PROCESS_QUERY_INFORMATION);
			if (!EnumProcessModulesEx(_handle, &moduleHandle, (uint)IntPtr.Size, out size, LIST_MODULES_ALL))
				return null;
			moduleHandles = new IntPtr[size / IntPtr.Size];
			fixed (IntPtr* p = moduleHandles)
				if (!EnumProcessModulesEx(_handle, p, size, out size, LIST_MODULES_ALL))
					return null;
			modules = new NativeModule[moduleHandles.Length];
			for (int i = 0; i < modules.Length; i++)
				modules[i] = new NativeModule(this, moduleHandles[i]);
			return modules;
		}

		public NativeModule GetModule() {
			IntPtr moduleHandle;

			QuickDemand(PROCESS_QUERY_INFORMATION);
			moduleHandle = GetModuleHandleInternal(_handle, true, null);
			return moduleHandle == IntPtr.Zero ? null : new NativeModule(this, moduleHandle);
		}

		public NativeModule GetModule(string moduleName) {
			IntPtr moduleHandle;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			moduleHandle = GetModuleHandleInternal(_handle, false, moduleName);
			return moduleHandle == IntPtr.Zero ? null : new NativeModule(this, moduleHandle);
		}

		public override string ToString() => $"{Name} (Id: 0x{Id.ToString("X")})";
	}
}
