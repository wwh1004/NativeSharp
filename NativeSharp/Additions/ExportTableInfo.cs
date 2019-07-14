using System;
using System.Collections.Generic;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// 导出函数信息
	/// </summary>
	public sealed unsafe class ExportFunctionInfo {
		private readonly void* _address;
		private readonly string _name;
		private readonly ushort _ordinal;

		/// <summary>
		/// 地址
		/// </summary>
		public void* Address => _address;

		/// <summary>
		/// 名称
		/// </summary>
		public string Name => _name;

		/// <summary>
		/// 序号
		/// </summary>
		public ushort Ordinal => _ordinal;

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="name">名称</param>
		/// <param name="ordinal">序号</param>
		public ExportFunctionInfo(void* address, string name, ushort ordinal) {
			_address = address;
			_name = name ?? string.Empty;
			_ordinal = ordinal;
		}
	}

	unsafe partial class NativeModule {
		/// <summary>
		/// 通过函数名获取导出函数地址
		/// </summary>
		/// <param name="functionName">函数名</param>
		/// <returns></returns>
		public void* GetFunctionAddress(string functionName) {
			_process.QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			return GetFunctionAddressInternal(_process.Handle, _handle, functionName);
		}

		internal static void* GetFunctionAddressInternal(void* processHandle, string moduleName, string functionName) {
			void* moduleHandle;

			moduleHandle = NativeProcess.GetModuleHandleInternal(processHandle, false, moduleName);
			if (moduleHandle is null)
				return null;
			return GetFunctionAddressInternal(processHandle, moduleHandle, functionName);
		}

		internal static void* GetFunctionAddressInternal(void* processHandle, void* moduleHandle, string functionName) {
			IMAGE_EXPORT_DIRECTORY ied;
			uint[] nameOffsets;
			string name;
			ushort ordinal;
			uint addressOffset;

			if (!SafeGetExportTableInfo((IntPtr)processHandle, (IntPtr)moduleHandle, out ied, out nameOffsets))
				return null;
			for (uint i = 0; i < ied.NumberOfNames; i++) {
				if (!NativeProcess.ReadStringInternal(processHandle, (byte*)moduleHandle + nameOffsets[i], out name, false, Encoding.ASCII) || name != functionName)
					continue;
				if (!NativeProcess.ReadUInt16Internal(processHandle, (byte*)moduleHandle + ied.AddressOfNameOrdinals + i * 2, out ordinal))
					continue;
				if (!NativeProcess.ReadUInt32Internal(processHandle, (byte*)moduleHandle + ied.AddressOfFunctions + ordinal * 4, out addressOffset))
					continue;
				return (byte*)moduleHandle + addressOffset;
			}
			return null;
		}

		/// <summary>
		/// 获取所有导出函数信息
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ExportFunctionInfo> EnumerateFunctionInfos() {
			_process.QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			return EnumerateFunctionInfosInternal((IntPtr)_process.Handle, (IntPtr)_handle);
		}

		internal static IEnumerable<ExportFunctionInfo> EnumerateFunctionInfosInternal(IntPtr processHandle, string moduleName) {
			IntPtr moduleHandle;

			moduleHandle = (IntPtr)NativeProcess.GetModuleHandleInternal((void*)processHandle, false, moduleName);
			if (moduleHandle == IntPtr.Zero)
				return null;
			return EnumerateFunctionInfosInternal(processHandle, moduleHandle);
		}

		internal static IEnumerable<ExportFunctionInfo> EnumerateFunctionInfosInternal(IntPtr processHandle, IntPtr moduleHandle) {
			IMAGE_EXPORT_DIRECTORY ied;
			uint[] nameOffsets;

			if (!SafeGetExportTableInfo(processHandle, moduleHandle, out ied, out nameOffsets))
				yield break;
			for (uint i = 0; i < ied.NumberOfNames; i++) {
				ExportFunctionInfo functionInfo;

				if (SafeGetExportFunctionInfo(processHandle, moduleHandle, ied, nameOffsets, i, out functionInfo))
					yield return functionInfo;
				else
					yield break;
			}
		}

		private static bool SafeGetExportTableInfo(IntPtr processHandle, IntPtr moduleHandle, out IMAGE_EXPORT_DIRECTORY ied, out uint[] nameOffsets) {
			uint ntHeaderOffset;
			bool is64Bit;
			uint iedRVA;

			ied = default;
			nameOffsets = null;
			if (!NativeProcess.ReadUInt32Internal((void*)processHandle, (byte*)moduleHandle + 0x3C, out ntHeaderOffset))
				return false;
			if (!NativeProcess.Is64BitProcessInternal((void*)processHandle, out is64Bit))
				return false;
			if (is64Bit) {
				if (!NativeProcess.ReadUInt32Internal((void*)processHandle, (byte*)moduleHandle + ntHeaderOffset + 0x88, out iedRVA))
					return false;
			}
			else {
				if (!NativeProcess.ReadUInt32Internal((void*)processHandle, (byte*)moduleHandle + ntHeaderOffset + 0x78, out iedRVA))
					return false;
			}
			fixed (void* p = &ied)
				if (!NativeProcess.ReadInternal((void*)processHandle, (byte*)moduleHandle + iedRVA, p, IMAGE_EXPORT_DIRECTORY.UnmanagedSize))
					return false;
			if (ied.NumberOfNames == 0)
				return true;
			nameOffsets = new uint[ied.NumberOfNames];
			fixed (void* p = nameOffsets)
				if (!NativeProcess.ReadInternal((void*)processHandle, (byte*)moduleHandle + ied.AddressOfNames, p, ied.NumberOfNames * 4))
					return false;
			return true;
		}

		private static bool SafeGetExportFunctionInfo(IntPtr processHandle, IntPtr moduleHandle, IMAGE_EXPORT_DIRECTORY ied, uint[] nameOffsets, uint i, out ExportFunctionInfo functionInfo) {
			string functionName;
			ushort ordinal;
			uint addressOffset;

			functionInfo = default;
			if (!NativeProcess.ReadStringInternal((void*)processHandle, (byte*)moduleHandle + nameOffsets[i], out functionName, false, Encoding.ASCII))
				return false;
			if (!NativeProcess.ReadUInt16Internal((void*)processHandle, (byte*)moduleHandle + ied.AddressOfNameOrdinals + i * 2, out ordinal))
				return false;
			if (!NativeProcess.ReadUInt32Internal((void*)processHandle, (byte*)moduleHandle + ied.AddressOfFunctions + ordinal * 4, out addressOffset))
				return false;
			functionInfo = new ExportFunctionInfo((byte*)moduleHandle + addressOffset, functionName, ordinal);
			return true;
		}
	}
}
