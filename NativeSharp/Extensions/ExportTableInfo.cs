using System;
using System.Collections.Generic;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	public sealed class ExportFunctionInfo {
		private readonly IntPtr _address;
		private readonly string _name;
		private readonly ushort _ordinal;

		public IntPtr Address => _address;

		public string Name => _name;

		public ushort Ordinal => _ordinal;

		public ExportFunctionInfo(IntPtr address, string name, ushort ordinal) {
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			_address = address;
			_name = name;
			_ordinal = ordinal;
		}
	}

	unsafe partial class NativeModule {
		public IntPtr GetFunctionAddress(string functionName) {
			_process.QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			return GetFunctionAddressInternal(_process.Handle, _handle, functionName);
		}

		internal static IntPtr GetFunctionAddressInternal(IntPtr processHandle, string moduleName, string functionName) {
			IntPtr moduleHandle;

			moduleHandle = NativeProcess.GetModuleHandleInternal(processHandle, false, moduleName);
			if (moduleHandle == IntPtr.Zero)
				return IntPtr.Zero;
			return GetFunctionAddressInternal(processHandle, moduleHandle, functionName);
		}

		internal static IntPtr GetFunctionAddressInternal(IntPtr processHandle, IntPtr moduleHandle, string functionName) {
			IMAGE_EXPORT_DIRECTORY ied;
			uint[] nameOffsets;
			string name;
			ushort ordinal;
			uint addressOffset;

			if (!GetExportTableInfo(processHandle, moduleHandle, out ied, out nameOffsets))
				return IntPtr.Zero;
			for (uint i = 0; i < ied.NumberOfNames; i++) {
				if (!NativeProcess.ReadStringInternal(processHandle, (IntPtr)((byte*)moduleHandle + nameOffsets[i]), out name, false, Encoding.ASCII))
					continue;
				if (name == functionName) {
					if (!NativeProcess.ReadUInt16Internal(processHandle, (IntPtr)((byte*)moduleHandle + ied.AddressOfNameOrdinals + i * 2), out ordinal))
						continue;
					if (!NativeProcess.ReadUInt32Internal(processHandle, (IntPtr)((byte*)moduleHandle + ied.AddressOfFunctions + ordinal * 4), out addressOffset))
						continue;
					return (IntPtr)((byte*)moduleHandle + addressOffset);
				}
			}
			return IntPtr.Zero;
		}

		public ExportFunctionInfo[] GetFunctionInfos() {
			_process.QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			return GetFunctionInfosInternal(_process.Handle, _handle);
		}

		internal static ExportFunctionInfo[] GetFunctionInfosInternal(IntPtr processHandle, string moduleName) {
			IntPtr moduleHandle;

			moduleHandle = NativeProcess.GetModuleHandleInternal(processHandle, false, moduleName);
			if (moduleHandle == IntPtr.Zero)
				return null;
			return GetFunctionInfosInternal(processHandle, moduleHandle);
		}

		internal static ExportFunctionInfo[] GetFunctionInfosInternal(IntPtr processHandle, IntPtr moduleHandle) {
			IMAGE_EXPORT_DIRECTORY ied;
			uint[] nameOffsets;
			string functionName;
			ushort ordinal;
			uint addressOffset;
			List<ExportFunctionInfo> exportFunctionInfoList;

			if (!GetExportTableInfo(processHandle, moduleHandle, out ied, out nameOffsets))
				return null;
			exportFunctionInfoList = new List<ExportFunctionInfo>(nameOffsets.Length);
			for (uint i = 0; i < ied.NumberOfNames; i++) {
				if (!NativeProcess.ReadStringInternal(processHandle, (IntPtr)((byte*)moduleHandle + nameOffsets[i]), out functionName, false, Encoding.ASCII))
					continue;
				if (!NativeProcess.ReadUInt16Internal(processHandle, (IntPtr)((byte*)moduleHandle + ied.AddressOfNameOrdinals + i * 2), out ordinal))
					continue;
				if (!NativeProcess.ReadUInt32Internal(processHandle, (IntPtr)((byte*)moduleHandle + ied.AddressOfFunctions + ordinal * 4), out addressOffset))
					continue;
				exportFunctionInfoList.Add(new ExportFunctionInfo((IntPtr)((byte*)moduleHandle + addressOffset), functionName, ordinal));
			}
			return exportFunctionInfoList.ToArray();
		}

		private static bool GetExportTableInfo(IntPtr processHandle, IntPtr moduleHandle, out IMAGE_EXPORT_DIRECTORY ied, out uint[] nameOffsets) {
			uint ntHeaderOffset;
			bool is64Bit;
			uint iedRVA;

			ied = default(IMAGE_EXPORT_DIRECTORY);
			nameOffsets = null;
			if (!NativeProcess.ReadUInt32Internal(processHandle, (IntPtr)((byte*)moduleHandle + 0x3C), out ntHeaderOffset))
				return false;
			if (!NativeProcess.Is64BitProcessInternal(processHandle, out is64Bit))
				return false;
			if (is64Bit) {
				if (!NativeProcess.ReadUInt32Internal(processHandle, (IntPtr)((byte*)moduleHandle + ntHeaderOffset + 0x88), out iedRVA))
					return false;
			}
			else {
				if (!NativeProcess.ReadUInt32Internal(processHandle, (IntPtr)((byte*)moduleHandle + ntHeaderOffset + 0x78), out iedRVA))
					return false;
			}
			fixed (void* p = &ied)
				if (!ReadProcessMemory(processHandle, (IntPtr)((byte*)moduleHandle + iedRVA), p, IMAGE_EXPORT_DIRECTORY.UnmanagedSize, null))
					return false;
			if (ied.NumberOfNames == 0)
				return true;
			nameOffsets = new uint[ied.NumberOfNames];
			fixed (void* p = nameOffsets)
				if (!ReadProcessMemory(processHandle, (IntPtr)((byte*)moduleHandle + ied.AddressOfNames), p, ied.NumberOfNames * 4, null))
					return false;
			return true;
		}
	}
}
