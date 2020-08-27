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
				QuickDemand(ProcessAccess.QueryInformation);
				Is64BitProcessInternal(_handle, out bool is64Bit);
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
				QuickDemand(ProcessAccess.QueryInformation);
				var iamgePath = new StringBuilder((int)MAX_PATH);
				uint size = MAX_PATH;
				return QueryFullProcessImageName(_handle, 0, iamgePath, &size) ? iamgePath.ToString() : string.Empty;
			}
		}

		/// <summary>
		/// 获取所有模块
		/// </summary>
		/// <returns></returns>
		public NativeModule[] GetModules() {
			QuickDemand(ProcessAccess.QueryInformation);
			void* moduleHandle;
			if (!EnumProcessModulesEx(_handle, &moduleHandle, (uint)IntPtr.Size, out uint size, LIST_MODULES_ALL))
				return Array2.Empty<NativeModule>();
			void*[] moduleHandles = new void*[size / (uint)IntPtr.Size];
			fixed (void** p = moduleHandles) {
				if (!EnumProcessModulesEx(_handle, p, size, out _, LIST_MODULES_ALL))
					return Array2.Empty<NativeModule>();
			}
			var modules = new NativeModule[moduleHandles.Length];
			for (int i = 0; i < modules.Length; i++)
				modules[i] = UnsafeGetModule(moduleHandles[i]);
			return modules;
		}

		/// <summary>
		/// 获取主模块
		/// </summary>
		/// <returns></returns>
		public NativeModule GetMainModule() {
			QuickDemand(ProcessAccess.QueryInformation);
			return UnsafeGetModule(IsCurrentProcess ? GetModuleHandle(null) : GetModuleHandleInternal(_handle, true, string.Empty));
		}

		/// <summary>
		/// 获取模块
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <returns></returns>
		public NativeModule GetModule(string moduleName) {
			if (string.IsNullOrEmpty(moduleName))
				throw new ArgumentNullException(nameof(moduleName));

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			return UnsafeGetModule(IsCurrentProcess ? GetModuleHandle(moduleName) : GetModuleHandleInternal(_handle, false, moduleName));
		}

		/// <summary>
		/// 通过模块句柄直接获取模块，只要 <paramref name="moduleHandle"/> 不为零，均会返回一个 <see cref="NativeModule"/> 实例
		/// </summary>
		/// <param name="moduleHandle">模块句柄</param>
		/// <returns></returns>
		public NativeModule UnsafeGetModule(void* moduleHandle) {
			return new NativeModule(this, moduleHandle);
		}

		/// <summary />
		public override string ToString() {
			return $"{Name} (Id: 0x{Id:X})";
		}
	}
}
