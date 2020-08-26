using System;
using System.Collections.Generic;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// 内存保护选项
	/// </summary>
	[Flags]
	public enum MemoryProtection : uint {
		/// <summary />
		NoAccess = 0x01,
		/// <summary />
		ReadOnly = 0x02,
		/// <summary />
		ReadWrite = 0x04,
		/// <summary />
		WriteCopy = 0x08,
		/// <summary />
		Execute = 0x10,
		/// <summary />
		ExecuteRead = 0x20,
		/// <summary />
		ExecuteReadWrite = 0x40,
		/// <summary />
		ExecuteWriteCopy = 0x80,
		/// <summary />
		Guard = 0x100,
		/// <summary />
		NoCache = 0x200,
		/// <summary />
		WriteCombine = 0x400
	}

	/// <summary>
	/// 内存类型选项
	/// </summary>
	[Flags]
	public enum MemoryType : uint {
		/// <summary />
		Commit = 0x00001000,
		/// <summary />
		Reserve = 0x00002000,
		/// <summary />
		Decommit = 0x00004000,
		/// <summary />
		Release = 0x00008000,
		/// <summary />
		Free = 0x00010000,
		/// <summary />
		Private = 0x00020000,
		/// <summary />
		Mapped = 0x00040000,
		/// <summary />
		Reset = 0x00080000
	}

	/// <summary>
	/// 页面信息
	/// </summary>
	public sealed unsafe class PageInfo {
		private readonly void* _address;
		private readonly void* _size;
		private readonly MemoryProtection _protection;
		private readonly MemoryType _type;

		/// <summary>
		/// 起始地址
		/// </summary>
		public void* Address => _address;

		/// <summary>
		/// 大小
		/// </summary>
		public void* Size => _size;

		/// <summary>
		/// 保护
		/// </summary>
		public MemoryProtection Protection => _protection;

		/// <summary>
		/// 类型
		/// </summary>
		public MemoryType Type => _type;

		internal PageInfo(MEMORY_BASIC_INFORMATION mbi) {
			_address = mbi.BaseAddress;
			_size = mbi.RegionSize;
			_protection = (MemoryProtection)mbi.Protect;
			_type = (MemoryType)mbi.Type;
		}

		/// <summary />
		public override string ToString() {
			bool is64Bit = (ulong)_address > uint.MaxValue;
			return $"Address=0x{((IntPtr)_address).ToString(is64Bit ? "X16" : "X8")} Size=0x{((IntPtr)_size).ToString(is64Bit ? "X16" : "X8")}";
		}
	}

	unsafe partial class NativeProcess {
		/// <summary>
		/// 获取所有页面信息
		/// </summary>
		/// <returns></returns>
		public IEnumerable<PageInfo> EnumeratePageInfos() {
			QuickDemand(ProcessAccess.QueryInformation);
			return EnumeratePageInfosInternal((IntPtr)_handle, IntPtr.Zero, (IntPtr)(void*)-1);
		}

		/// <summary>
		/// 获取范围内页面信息
		/// </summary>
		/// <param name="startAddress">起始地址</param>
		/// <param name="endAddress">结束地址</param>
		/// <returns></returns>
		public IEnumerable<PageInfo> EnumeratePageInfos(void* startAddress, void* endAddress) {
			QuickDemand(ProcessAccess.QueryInformation);
			return EnumeratePageInfosInternal((IntPtr)_handle, (IntPtr)startAddress, (IntPtr)endAddress);
		}

		internal static IEnumerable<PageInfo> EnumeratePageInfosInternal(IntPtr processHandle, IntPtr startAddress, IntPtr endAddress) {
			if (!SafeIs64BitProcessInternal(processHandle, out bool is64Bit))
				yield break;
			var nextAddress = startAddress;
			do {
				if (!SafeVirtualQueryEx(processHandle, nextAddress, out var mbi, MEMORY_BASIC_INFORMATION.UnmanagedSize))
					break;
				yield return new PageInfo(mbi);
				nextAddress = SafeGetNextAddress(mbi);
			} while ((long)nextAddress > 0 && NextAddressLessThanEndAddress(nextAddress, endAddress));

			static bool SafeIs64BitProcessInternal(IntPtr processHandle_, out bool is64Bit_) {
				return Is64BitProcessInternal((void*)processHandle_, out is64Bit_);
			}

			static bool SafeVirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength) {
				return VirtualQueryEx((void*)hProcess, (void*)lpAddress, out lpBuffer, dwLength);
			}

			IntPtr SafeGetNextAddress(MEMORY_BASIC_INFORMATION mbi_) {
				return (IntPtr)(void*)(is64Bit ? ((ulong)mbi_.BaseAddress + (ulong)mbi_.RegionSize) : (uint)mbi_.BaseAddress + (uint)mbi_.RegionSize);
			}

			bool NextAddressLessThanEndAddress(IntPtr nextAddress_, IntPtr endAddress_) {
				return is64Bit ? (ulong)nextAddress_ < (ulong)endAddress_ : (uint)nextAddress_ < (uint)endAddress_;
			}
		}

		/// <summary>
		/// 设置内存保护选项
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="size">内存大小</param>
		/// <param name="protection">新内存保护选项</param>
		/// <returns></returns>
		public bool SetProtection(void* address, uint size, MemoryProtection protection) {
			return SetProtection(address, size, protection, out _);
		}

		/// <summary>
		/// 设置内存保护选项
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="size">内存大小</param>
		/// <param name="protection">新内存保护选项</param>
		/// <param name="oldProtection">原来的内存保护选项</param>
		/// <returns></returns>
		public bool SetProtection(void* address, uint size, MemoryProtection protection, out MemoryProtection oldProtection) {
			bool result = VirtualProtectEx(_handle, address, size, (uint)protection, out uint temp);
			oldProtection = (MemoryProtection)temp;
			return result;
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
