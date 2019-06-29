using System;
using System.Collections.Generic;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// 内存保护选项
	/// </summary>
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
	public sealed class PageInfo {
		private readonly IntPtr _address;
		private readonly IntPtr _size;
		private readonly MemoryProtection _protection;
		private readonly MemoryType _type;

		/// <summary>
		/// 起始地址
		/// </summary>
		public IntPtr Address => _address;

		/// <summary>
		/// 大小
		/// </summary>
		public IntPtr Size => _size;

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
			bool is64Bit;

			is64Bit = (ulong)_address > uint.MaxValue;
			return $"Address=0x{_address.ToString(is64Bit ? "X16" : "X8")} Size=0x{_size.ToString(is64Bit ? "X16" : "X8")}";
		}
	}

	unsafe partial class NativeProcess {
		/// <summary>
		/// 获取所有页面信息
		/// </summary>
		/// <returns></returns>
		public PageInfo[] GetPageInfos() {
			QuickDemand(ProcessAccess.QueryInformation);
			return GetPageInfosInternal(_handle, IntPtr.Zero, (IntPtr)(-1));
		}

		/// <summary>
		/// 获取范围内页面信息
		/// </summary>
		/// <param name="startAddress">起始地址</param>
		/// <param name="endAddress">结束地址</param>
		/// <returns></returns>
		public PageInfo[] GetPageInfos(IntPtr startAddress, IntPtr endAddress) {
			QuickDemand(ProcessAccess.QueryInformation);
			return GetPageInfosInternal(_handle, startAddress, endAddress);
		}

		internal static PageInfo[] GetPageInfosInternal(IntPtr processHandle, IntPtr startAddress, IntPtr endAddress) {
			bool is64Bit;

			if (!Is64BitProcessInternal(processHandle, out is64Bit))
				return null;
			return is64Bit ? GetPageInfosInternal64(processHandle, (ulong)startAddress, (ulong)endAddress) : GetPageInfosInternal32(processHandle, (uint)startAddress, (uint)endAddress);
		}

		internal static PageInfo[] GetPageInfosInternal32(IntPtr processHandle, uint startAddress, uint endAddress) {
			uint nextAddress;
			List<PageInfo> pageInfos;

			nextAddress = startAddress;
			pageInfos = new List<PageInfo>();
			do {
				MEMORY_BASIC_INFORMATION mbi;

				if (!VirtualQueryEx(processHandle, (IntPtr)nextAddress, out mbi, MEMORY_BASIC_INFORMATION.UnmanagedSize))
					break;
				pageInfos.Add(new PageInfo(mbi));
				nextAddress = (uint)mbi.BaseAddress + (uint)mbi.RegionSize;
			} while ((int)nextAddress > 0 && nextAddress < endAddress);
			return pageInfos.Count == 0 ? null : pageInfos.ToArray();
		}

		internal static PageInfo[] GetPageInfosInternal64(IntPtr processHandle, ulong startAddress, ulong endAddress) {
			ulong nextAddress;
			List<PageInfo> pageInfos;

			nextAddress = startAddress;
			pageInfos = new List<PageInfo>();
			do {
				MEMORY_BASIC_INFORMATION mbi;

				if (!VirtualQueryEx(processHandle, (IntPtr)nextAddress, out mbi, MEMORY_BASIC_INFORMATION.UnmanagedSize))
					break;
				pageInfos.Add(new PageInfo(mbi));
				nextAddress = (ulong)mbi.BaseAddress + (ulong)mbi.RegionSize;
			} while ((long)nextAddress > 0 && nextAddress < endAddress);
			return pageInfos.Count == 0 ? null : pageInfos.ToArray();
		}
	}
}
