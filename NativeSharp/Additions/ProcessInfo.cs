using System;
using System.IO;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	unsafe partial class NativeProcess {
		/// <summary>
		/// 是否为64位进程
		/// </summary>
		public bool Is64Bit {
			get {
				bool is64Bit;

				QuickDemand(ProcessAccess.QueryInformation);
				Is64BitProcessInternal(_handle, out is64Bit);
				return is64Bit;
			}
		}

		/// <summary>
		/// 名称
		/// </summary>
		public string Name => Path.GetFileName(ImagePath);

		/// <summary>
		/// 文件路径
		/// </summary>
		public string ImagePath {
			get {
				StringBuilder iamgePath;

				QuickDemand(ProcessAccess.QueryInformation);
				iamgePath = new StringBuilder((int)MAX_PATH);
				return GetProcessImageFileName(_handle, iamgePath, MAX_PATH) ? iamgePath.ToString() : null;
			}
		}

		/// <summary>
		/// 获取所有模块
		/// </summary>
		/// <returns></returns>
		public NativeModule[] GetModules() {
			IntPtr moduleHandle;
			uint size;
			IntPtr[] moduleHandles;
			NativeModule[] modules;

			QuickDemand(ProcessAccess.QueryInformation);
			if (!EnumProcessModulesEx(_handle, &moduleHandle, (uint)IntPtr.Size, out size, LIST_MODULES_ALL))
				return null;
			moduleHandles = new IntPtr[size / IntPtr.Size];
			fixed (IntPtr* p = moduleHandles)
				if (!EnumProcessModulesEx(_handle, p, size, out _, LIST_MODULES_ALL))
					return null;
			modules = new NativeModule[moduleHandles.Length];
			for (int i = 0; i < modules.Length; i++)
				modules[i] = new NativeModule(this, moduleHandles[i]);
			return modules;
		}

		/// <summary>
		/// 获取主模块
		/// </summary>
		/// <returns></returns>
		public NativeModule GetMainModule() {
			IntPtr moduleHandle;

			QuickDemand(ProcessAccess.QueryInformation);
			moduleHandle = GetModuleHandleInternal(_handle, true, null);
			return moduleHandle == IntPtr.Zero ? null : new NativeModule(this, moduleHandle);
		}

		/// <summary>
		/// 获取模块
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <returns></returns>
		public NativeModule GetModule(string moduleName) {
			IntPtr moduleHandle;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			moduleHandle = GetModuleHandleInternal(_handle, false, moduleName);
			return moduleHandle == IntPtr.Zero ? null : new NativeModule(this, moduleHandle);
		}

		/// <summary />
		public override string ToString() {
			return $"{Name} (Id: 0x{Id.ToString("X")})";
		}
	}
}
