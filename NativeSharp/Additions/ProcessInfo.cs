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
			void* moduleHandle;
			uint size;
			void*[] moduleHandles;
			NativeModule[] modules;

			QuickDemand(ProcessAccess.QueryInformation);
			if (!EnumProcessModulesEx(_handle, &moduleHandle, (uint)IntPtr.Size, out size, LIST_MODULES_ALL))
				return null;
			moduleHandles = new void*[size / (uint)IntPtr.Size];
			fixed (void** p = moduleHandles)
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
			void* moduleHandle;

			QuickDemand(ProcessAccess.QueryInformation);
			moduleHandle = GetModuleHandleInternal(_handle, true, null);
			return moduleHandle is null ? null : UnsafeGetModule(moduleHandle);
		}

		/// <summary>
		/// 获取模块
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <returns></returns>
		public NativeModule GetModule(string moduleName) {
			if (string.IsNullOrEmpty(moduleName))
				throw new ArgumentNullException(nameof(moduleName));

			void* moduleHandle;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			moduleHandle = GetModuleHandleInternal(_handle, false, moduleName);
			return moduleHandle is null ? null : UnsafeGetModule(moduleHandle);
		}

		/// <summary>
		/// 通过模块句柄直接获取模块，只要 <paramref name="moduleHandle"/> 不为零，均会返回一个 <see cref="NativeModule"/> 实例
		/// </summary>
		/// <param name="moduleHandle">模块句柄</param>
		/// <returns></returns>
		public NativeModule UnsafeGetModule(void* moduleHandle) {
			if (moduleHandle is null)
				return null;

			return new NativeModule(this, moduleHandle);
		}

		/// <summary />
		public override string ToString() {
			return $"{Name} (Id: 0x{Id.ToString("X")})";
		}
	}
}
