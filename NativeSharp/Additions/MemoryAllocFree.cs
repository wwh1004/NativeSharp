using static NativeSharp.NativeMethods;

namespace NativeSharp {
	unsafe partial class NativeProcess {
		private static MemoryProtection ToProtection(bool writable, bool executable) {
			if (writable) {
				if (executable)
					return MemoryProtection.ExecuteReadWrite;
				else
					return MemoryProtection.ReadWrite;
			}
			else {
				if (executable)
					return MemoryProtection.ExecuteRead;
				else
					return MemoryProtection.ReadOnly;
			}
		}

		/// <summary>
		/// 分配内存
		/// </summary>
		/// <param name="size">大小</param>
		/// <param name="writable">是否可写</param>
		/// <param name="executable">是否可执行</param>
		/// <returns></returns>
		public void* AllocMemory(uint size, bool writable, bool executable) {
			QuickDemand(ProcessAccess.MemoryOperation);
			return AllocMemoryInternal(_handle, size, ToProtection(writable, executable));
		}

		/// <summary>
		/// 分配内存
		/// </summary>
		/// <param name="size">大小</param>
		/// <param name="protection">选项</param>
		/// <returns></returns>
		public void* AllocMemory(uint size, MemoryProtection protection) {
			QuickDemand(ProcessAccess.MemoryOperation);
			return AllocMemoryInternal(_handle, size, protection);
		}

		/// <summary>
		/// 分配内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="size">大小</param>
		/// <param name="protection">选项</param>
		/// <returns></returns>
		public void* AllocMemory(void* address, uint size, MemoryProtection protection) {
			QuickDemand(ProcessAccess.MemoryOperation);
			return AllocMemoryInternal(_handle, address, size, protection);
		}

		internal static void* AllocMemoryInternal(void* processHandle, uint size, bool writable, bool executable) {
			return AllocMemoryInternal(processHandle, size, ToProtection(writable, executable));
		}

		internal static void* AllocMemoryInternal(void* processHandle, uint size, MemoryProtection protection) {
			return VirtualAllocEx(processHandle, null, size, (uint)MemoryType.Commit, (uint)protection);
		}

		internal static void* AllocMemoryInternal(void* processHandle, void* address, uint size, MemoryProtection protection) {
			return VirtualAllocEx(processHandle, address, size, (uint)MemoryType.Commit, (uint)protection);
		}

		/// <summary>
		/// 释放内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public bool FreeMemory(void* address) {
			QuickDemand(ProcessAccess.MemoryOperation);
			return FreeMemoryInternal(_handle, address);
		}

		/// <summary>
		/// 释放内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="size">大小</param>
		/// <param name="type">选项</param>
		/// <returns></returns>
		public bool FreeMemory(void* address, uint size, MemoryType type) {
			QuickDemand(ProcessAccess.MemoryOperation);
			return FreeMemoryInternal(_handle, address, size, type);
		}

		internal static bool FreeMemoryInternal(void* processHandle, void* address) {
			return VirtualFreeEx(processHandle, address, 0, (uint)MemoryType.Decommit);
		}

		internal static bool FreeMemoryInternal(void* processHandle, void* address, uint size, MemoryType type) {
			return VirtualFreeEx(processHandle, address, size, (uint)type);
		}
	}
}
