using System;
using System.Collections.Generic;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	public enum MemoryProtectionFlags : uint {
		NoAccess = 0x01,

		ReadOnly = 0x02,

		ReadWrite = 0x04,

		WriteCopy = 0x08,

		Execute = 0x10,

		ExecuteRead = 0x20,

		ExecuteReadWrite = 0x40,

		ExecuteWriteCopy = 0x80,

		Guard = 0x100,

		NoCache = 0x200,

		WriteCombine = 0x400
	}

	public enum MemoryTypeFlags : uint {
		Commit = 0x00001000,

		Reserve = 0x00002000,

		Decommit = 0x00004000,

		Release = 0x00008000,

		Free = 0x00010000,

		Private = 0x00020000,

		Mapped = 0x00040000,

		Reset = 0x00080000
	}

	public sealed class PageInfo {
		private readonly IntPtr _address;
		private readonly IntPtr _size;
		private readonly MemoryProtectionFlags _protectionFlags;
		private readonly MemoryTypeFlags _typeFlags;

		public IntPtr Address => _address;

		public IntPtr Size => _size;

		public MemoryProtectionFlags ProtectionFlags => _protectionFlags;

		public MemoryTypeFlags TypeFlags => _typeFlags;

		internal PageInfo(MEMORY_BASIC_INFORMATION mbi) {
			_address = mbi.BaseAddress;
			_size = mbi.RegionSize;
			_protectionFlags = (MemoryProtectionFlags)mbi.Protect;
			_typeFlags = (MemoryTypeFlags)mbi.Type;
		}

		public override string ToString() {
			bool is64Bit;

			is64Bit = (ulong)_address > uint.MaxValue;
			return $"Address=0x{_address.ToString(is64Bit ? "X16" : "X8")} Size=0x{_size.ToString(is64Bit ? "X16" : "X8")}";
		}
	}

	unsafe partial class NativeProcess {
		public PageInfo[] GetPageInfos() {
			QuickDemand(PROCESS_QUERY_INFORMATION);
			return GetPageInfosInternal(_handle, IntPtr.Zero, (IntPtr)(-1));
		}

		public PageInfo[] GetPageInfos(IntPtr startAddress, IntPtr endAddress) {
			QuickDemand(PROCESS_QUERY_INFORMATION);
			return GetPageInfosInternal(_handle, startAddress, endAddress);
		}

		internal static PageInfo[] GetPageInfosInternal(IntPtr processHandle, IntPtr startAddress, IntPtr endAddress) {
			bool is64Bit;

#pragma warning disable IDE0046
			if (Is64BitProcessInternal(processHandle, out is64Bit))
				return is64Bit ? GetPageInfosInternal64(processHandle, (ulong)startAddress, (ulong)endAddress) : GetPageInfosInternal32(processHandle, (uint)startAddress, (uint)endAddress);
			else
				return null;
#pragma warning restore IDE0046
		}

		internal static PageInfo[] GetPageInfosInternal32(IntPtr processHandle, uint startAddress, uint endAddress) {
			uint nextAddress;
			List<PageInfo> pageInfoList;

			nextAddress = startAddress;
			pageInfoList = new List<PageInfo>();
			do {
				MEMORY_BASIC_INFORMATION mbi;

				if (!VirtualQueryEx(processHandle, (IntPtr)nextAddress, out mbi, MEMORY_BASIC_INFORMATION.UnmanagedSize))
					break;
				pageInfoList.Add(new PageInfo(mbi));
				nextAddress = (uint)mbi.BaseAddress + (uint)mbi.RegionSize;
			} while ((int)nextAddress > 0 && nextAddress < endAddress);
			return pageInfoList.Count == 0 ? null : pageInfoList.ToArray();
		}

		internal static PageInfo[] GetPageInfosInternal64(IntPtr processHandle, ulong startAddress, ulong endAddress) {
			ulong nextAddress;
			List<PageInfo> pageInfoList;

			nextAddress = startAddress;
			pageInfoList = new List<PageInfo>();
			do {
				MEMORY_BASIC_INFORMATION mbi;

				if (!VirtualQueryEx(processHandle, (IntPtr)nextAddress, out mbi, MEMORY_BASIC_INFORMATION.UnmanagedSize))
					break;
				pageInfoList.Add(new PageInfo(mbi));
				nextAddress = (ulong)mbi.BaseAddress + (ulong)mbi.RegionSize;
			} while ((long)nextAddress > 0 && nextAddress < endAddress);
			return pageInfoList.Count == 0 ? null : pageInfoList.ToArray();
		}
	}
}
