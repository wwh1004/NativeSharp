using System;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	unsafe partial class NativeProcess {
		private static MemoryProtectionFlags ToProtectionFlags(bool writable, bool executable) {
#pragma warning disable IDE0046
			if (writable) {
				if (executable)
					return (MemoryProtectionFlags)PAGE_EXECUTE_READWRITE;
				else
					return (MemoryProtectionFlags)PAGE_READWRITE;
			}
			else {
				if (executable)
					return (MemoryProtectionFlags)PAGE_EXECUTE_READ;
				else
					return (MemoryProtectionFlags)PAGE_READONLY;
			}
#pragma warning restore IDE0046
		}

		public IntPtr AllocMemory(uint size, bool writable, bool executable) {
			QuickDemand(PROCESS_VM_OPERATION);
			return AllocMemoryInternal(_handle, size, ToProtectionFlags(writable, executable));
		}

		public IntPtr AllocMemory(uint size, MemoryProtectionFlags allocFlags) {
			QuickDemand(PROCESS_VM_OPERATION);
			return AllocMemoryInternal(_handle, size, allocFlags);
		}

		public IntPtr AllocMemory(IntPtr address, uint size, MemoryProtectionFlags allocFlags) {
			QuickDemand(PROCESS_VM_OPERATION);
			return AllocMemoryInternal(_handle, address, size, allocFlags);
		}

		internal static IntPtr AllocMemoryInternal(IntPtr processHandle, uint size, bool writable, bool executable) => AllocMemoryInternal(processHandle, size, ToProtectionFlags(writable, executable));

		internal static IntPtr AllocMemoryInternal(IntPtr processHandle, uint size, MemoryProtectionFlags allocFlags) => VirtualAllocEx(processHandle, IntPtr.Zero, size, MEM_COMMIT, (uint)allocFlags);

		internal static IntPtr AllocMemoryInternal(IntPtr processHandle, IntPtr address, uint size, MemoryProtectionFlags allocFlags) => VirtualAllocEx(processHandle, address, size, MEM_COMMIT, (uint)allocFlags);

		public bool FreeMemory(IntPtr address) {
			QuickDemand(PROCESS_VM_OPERATION);
			return FreeMemoryInternal(_handle, address);
		}

		public bool FreeMemory(IntPtr address, uint size, MemoryTypeFlags freeTyp) {
			QuickDemand(PROCESS_VM_OPERATION);
			return FreeMemoryInternal(_handle, address, size, freeTyp);
		}

		internal static bool FreeMemoryInternal(IntPtr processHandle, IntPtr address) => VirtualFreeEx(processHandle, address, 0, MEM_DECOMMIT);

		internal static bool FreeMemoryInternal(IntPtr processHandle, IntPtr address, uint size, MemoryTypeFlags freeFlags) => VirtualFreeEx(processHandle, address, size, (uint)freeFlags);
	}
}
