using System;
using System.IO;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	public enum PointerType {
		ModuleNameWithOffset,

		BaseAddressWithOffset
	}

	public sealed class Pointer {
		private readonly string _moduleName;

		private readonly uint _moduleOffset;

		private readonly IntPtr _baseAddress;

		private readonly uint[] _offsets;

		private readonly PointerType _type;

		public string ModuleName => _type == PointerType.ModuleNameWithOffset ? _moduleName : throw new InvalidOperationException($"Type={_type}");

		public uint ModuleOffset => _type == PointerType.ModuleNameWithOffset ? _moduleOffset : throw new InvalidOperationException($"Type={_type}");

		public IntPtr BaseAddress => _type == PointerType.BaseAddressWithOffset ? _baseAddress : throw new InvalidOperationException($"Type={_type}");

		public uint[] Offsets => _offsets;

		public PointerType Type => _type;

		public Pointer(string moduleName, uint moduleOffset, params uint[] offsets) {
			if (string.IsNullOrEmpty(moduleName))
				throw new ArgumentOutOfRangeException();

			_moduleName = moduleName;
			_moduleOffset = moduleOffset;
			_offsets = offsets;
			_type = PointerType.ModuleNameWithOffset;
		}

		public Pointer(IntPtr baseAddress, params uint[] offsets) {
			_baseAddress = baseAddress;
			_offsets = offsets;
			_type = PointerType.BaseAddressWithOffset;
		}

		public Pointer(Pointer basePointer, params uint[] offsets) {
			int baseLength;
			int newLength;

			switch (basePointer._type) {
			case PointerType.ModuleNameWithOffset:
				_moduleName = basePointer._moduleName;
				_moduleOffset = basePointer._moduleOffset;
				break;
			case PointerType.BaseAddressWithOffset:
				_baseAddress = basePointer._baseAddress;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(basePointer._type));
			}
			baseLength = basePointer._offsets.Length;
			if (baseLength == 0) {
				_offsets = offsets;
				return;
			}
			newLength = offsets.Length;
			if (newLength == 0) {
				_offsets = basePointer._offsets;
				return;
			}
			_offsets = new uint[baseLength + newLength];
			Array.Copy(basePointer._offsets, 0, _offsets, 0, baseLength);
			Array.Copy(offsets, 0, _offsets, baseLength, newLength);
		}
	}

	unsafe partial class NativeProcess {
		private static bool GetPointerAddress(IntPtr processHandle, Pointer pointer, out IntPtr address) {
			bool is64Bit;

			if (!Is64BitProcessInternal(processHandle, out is64Bit)) {
				address = IntPtr.Zero;
				return false;
			}
			return is64Bit ? GetPointerAddress64(processHandle, pointer, out address) : GetPointerAddress32(processHandle, pointer, out address);
		}

		private static bool GetPointerAddress32(IntPtr processHandle, Pointer pointer, out IntPtr address) {
			uint newAddress;
			uint[] offsets;

			address = IntPtr.Zero;
			newAddress = 0;
			if (pointer.Type == PointerType.BaseAddressWithOffset) {
				if (!ReadUInt32Internal(processHandle, pointer.BaseAddress, out newAddress))
					return false;
			}
			else {
				newAddress = (uint)GetModuleHandleInternal(processHandle, false, pointer.ModuleName);
				if (newAddress == 0)
					return false;
				newAddress += pointer.ModuleOffset;
			}
			offsets = pointer.Offsets;
			for (int i = 0; i < offsets.Length; i++) {
				if (!ReadUInt32Internal(processHandle, (IntPtr)newAddress, out newAddress))
					return false;
				newAddress = newAddress + offsets[i];
			}
			address = (IntPtr)newAddress;
			return true;
		}

		private static bool GetPointerAddress64(IntPtr processHandle, Pointer pointer, out IntPtr address) {
			ulong newAddress;
			uint[] offsets;

			address = IntPtr.Zero;
			newAddress = 0;
			if (pointer.Type == PointerType.BaseAddressWithOffset) {
				if (!ReadUInt64Internal(processHandle, pointer.BaseAddress, out newAddress))
					return false;
			}
			else {
				newAddress = (ulong)GetModuleHandleInternal(processHandle, false, pointer.ModuleName);
				if (newAddress == 0)
					return false;
				newAddress += pointer.ModuleOffset;
			}
			offsets = pointer.Offsets;
			for (int i = 0; i < offsets.Length; i++) {
				if (!ReadUInt64Internal(processHandle, (IntPtr)newAddress, out newAddress))
					return false;
				newAddress = newAddress + offsets[i];
			}
			address = (IntPtr)newAddress;
			return true;
		}

		#region read
		public bool ReadByte(IntPtr address, out byte value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadByteInternal(_handle, address, out value);
		}

		public bool ReadInt16(IntPtr address, out short value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadInt16Internal(_handle, address, out value);
		}

		public bool ReadUInt16(IntPtr address, out ushort value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadUInt16Internal(_handle, address, out value);
		}

		public bool ReadInt32(IntPtr address, out int value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadInt32Internal(_handle, address, out value);
		}

		public bool ReadUInt32(IntPtr address, out uint value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadUInt32Internal(_handle, address, out value);
		}

		public bool ReadInt64(IntPtr address, out long value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadInt64Internal(_handle, address, out value);
		}

		public bool ReadUInt64(IntPtr address, out ulong value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadUInt64Internal(_handle, address, out value);
		}

		public bool ReadSingle(IntPtr address, out float value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadSingleInternal(_handle, address, out value);
		}

		public bool ReadDouble(IntPtr address, out double value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadDoubleInternal(_handle, address, out value);
		}

		public bool ReadBytes(IntPtr address, byte[] value) {
			QuickDemand(PROCESS_VM_READ);
			return ReadBytesInternal(_handle, address, value);
		}

		public bool ReadBytes(IntPtr address, byte[] value, out uint numberOfRead) {
			QuickDemand(PROCESS_VM_READ);
			return ReadBytesInternal(_handle, address, value, out numberOfRead);
		}

		public bool ReadString(IntPtr address, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
			QuickDemand(PROCESS_VM_READ);
			return ReadStringInternal(_handle, address, out value, isEndWithDoubleZero, fromEncoding);
		}

		public bool ReadByte(Pointer pointer, out byte value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(byte);
				return false;
			}
			return ReadByteInternal(_handle, address, out value);
		}

		public bool ReadInt16(Pointer pointer, out short value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(short);
				return false;
			}
			return ReadInt16Internal(_handle, address, out value);
		}

		public bool ReadUInt16(Pointer pointer, out ushort value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(ushort);
				return false;
			}
			return ReadUInt16Internal(_handle, address, out value);
		}

		public bool ReadInt32(Pointer pointer, out int value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(int);
				return false;
			}
			return ReadInt32Internal(_handle, address, out value);
		}

		public bool ReadUInt32(Pointer pointer, out uint value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(uint);
				return false;
			}
			return ReadUInt32Internal(_handle, address, out value);
		}

		public bool ReadInt64(Pointer pointer, out long value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(long);
				return false;
			}
			return ReadInt64Internal(_handle, address, out value);
		}

		public bool ReadUInt64(Pointer pointer, out ulong value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(ulong);
				return false;
			}
			return ReadUInt64Internal(_handle, address, out value);
		}

		public bool ReadSingle(Pointer pointer, out float value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(float);
				return false;
			}
			return ReadSingleInternal(_handle, address, out value);
		}

		public bool ReadDouble(Pointer pointer, out double value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default(double);
				return false;
			}
			return ReadDoubleInternal(_handle, address, out value);
		}

		public bool ReadBytes(Pointer pointer, byte[] value) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out address) && ReadBytesInternal(_handle, address, value);
		}

		public bool ReadBytes(Pointer pointer, byte[] value, out uint numberOfRead) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				numberOfRead = 0;
				return false;
			}
			return ReadBytesInternal(_handle, address, value, out numberOfRead);
		}

		public bool ReadString(Pointer pointer, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
			IntPtr address;

			QuickDemand(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = null;
				return false;
			}
			return ReadStringInternal(_handle, address, out value, isEndWithDoubleZero, fromEncoding);
		}
		#endregion

		#region write
		public bool WriteByte(IntPtr address, byte value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteByteInternal(_handle, address, value);
		}

		public bool WriteInt16(IntPtr address, short value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteInt16Internal(_handle, address, value);
		}

		public bool WriteUInt16(IntPtr address, ushort value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteUInt16Internal(_handle, address, value);
		}

		public bool WriteInt32(IntPtr address, int value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteInt32Internal(_handle, address, value);
		}

		public bool WriteUInt32(IntPtr address, uint value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteUInt32Internal(_handle, address, value);
		}

		public bool WriteInt64(IntPtr address, long value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteInt64Internal(_handle, address, value);
		}

		public bool WriteUInt64(IntPtr address, ulong value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteUInt64Internal(_handle, address, value);
		}

		public bool WriteSingle(IntPtr address, float value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteSingleInternal(_handle, address, value);
		}

		public bool WriteDouble(IntPtr address, double value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteDoubleInternal(_handle, address, value);
		}

		public bool WriteBytes(IntPtr address, byte[] value) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteBytesInternal(_handle, address, value);
		}

		public bool WriteBytes(IntPtr address, byte[] value, out uint numberOfWritten) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteBytesInternal(_handle, address, value, out numberOfWritten);
		}

		public bool WriteString(IntPtr address, string value, Encoding toEncoding) {
			QuickDemand(PROCESS_VM_WRITE);
			return WriteStringInternal(_handle, address, value, toEncoding);
		}

		public bool WriteByte(Pointer pointer, byte value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteByteInternal(_handle, address, value);
		}

		public bool WriteInt16(Pointer pointer, short value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteInt16Internal(_handle, address, value);
		}

		public bool WriteUInt16(Pointer pointer, ushort value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteUInt16Internal(_handle, address, value);
		}

		public bool WriteInt32(Pointer pointer, int value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteInt32Internal(_handle, address, value);
		}

		public bool WriteUInt32(Pointer pointer, uint value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteUInt32Internal(_handle, address, value);
		}

		public bool WriteInt64(Pointer pointer, long value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteInt64Internal(_handle, address, value);
		}

		public bool WriteUInt64(Pointer pointer, ulong value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteUInt64Internal(_handle, address, value);
		}

		public bool WriteSingle(Pointer pointer, float value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteSingleInternal(_handle, address, value);
		}

		public bool WriteDouble(Pointer pointer, double value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteDoubleInternal(_handle, address, value);
		}

		public bool WriteBytes(Pointer pointer, byte[] value) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteBytesInternal(_handle, address, value);
		}

		public bool WriteBytes(Pointer pointer, byte[] value, out uint numberOfWritten) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			if (!GetPointerAddress(_handle, pointer, out IntPtr address)) {
				numberOfWritten = 0;
				return false;
			}
			return WriteBytesInternal(_handle, address, value, out numberOfWritten);
		}

		public bool WriteString(Pointer pointer, string value, Encoding toEncoding) {
			QuickDemand(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteStringInternal(_handle, address, value, toEncoding);
		}
		#endregion

		#region read impl
		internal static bool ReadByteInternal(IntPtr processHandle, IntPtr address, out byte value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 1, null);
		}

		internal static bool ReadInt16Internal(IntPtr processHandle, IntPtr address, out short value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 2, null);
		}

		internal static bool ReadUInt16Internal(IntPtr processHandle, IntPtr address, out ushort value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 2, null);
		}

		internal static bool ReadInt32Internal(IntPtr processHandle, IntPtr address, out int value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 4, null);
		}

		internal static bool ReadUInt32Internal(IntPtr processHandle, IntPtr address, out uint value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 4, null);
		}

		internal static bool ReadInt64Internal(IntPtr processHandle, IntPtr address, out long value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 8, null);
		}

		internal static bool ReadUInt64Internal(IntPtr processHandle, IntPtr address, out ulong value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 8, null);
		}

		internal static bool ReadSingleInternal(IntPtr processHandle, IntPtr address, out float value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 4, null);
		}

		internal static bool ReadDoubleInternal(IntPtr processHandle, IntPtr address, out double value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, 8, null);
		}

		internal static bool ReadBytesInternal(IntPtr processHandle, IntPtr address, byte[] value) {
			fixed (void* p = value)
				return ReadProcessMemory(processHandle, address, p, (uint)value.Length, null);
		}

		internal static bool ReadBytesInternal(IntPtr processHandle, IntPtr address, byte[] value, out uint numberOfRead) {
			fixed (void* p = value)
			fixed (uint* pNumberOfRead = &numberOfRead)
				return ReadProcessMemory(processHandle, address, p, (uint)value.Length, pNumberOfRead);
		}

		internal static bool ReadStringInternal(IntPtr processHandle, IntPtr address, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
			// 在出现时一些特殊字符可能导致字符串被过早截取！
			const uint BASE_BUFFER_SIZE = 0x100;

			if (fromEncoding == null)
				throw new ArgumentNullException(nameof(fromEncoding));

			uint dummy;

			if (!ReadProcessMemory(processHandle, address, &dummy, isEndWithDoubleZero ? 2u : 1, null)) {
				value = null;
				return false;
			}
			using (MemoryStream stream = new MemoryStream((int)BASE_BUFFER_SIZE)) {
				uint bufferSize;
				byte[] bytes;
				bool isZero;
				bool isLastZero;

				bufferSize = BASE_BUFFER_SIZE;
				bytes = null;
				isLastZero = false;
				do {
					byte[] buffer;
					long oldPostion;
					int length;

					buffer = new byte[bufferSize];
					ReadBytesInternal(processHandle, address, buffer);
					oldPostion = stream.Position == 0 ? 0 : stream.Position - (isEndWithDoubleZero ? 2 : 1);
					stream.Write(buffer, 0, buffer.Length);
					length = (int)(stream.Length - oldPostion);
					stream.Position = oldPostion;
					for (int i = 0; i < length; i++) {
						isZero = stream.ReadByte() == 0;
						if ((isEndWithDoubleZero && !isLastZero) || !isZero) {
							isLastZero = isZero;
							continue;
						}
						bytes = new byte[stream.Position];
						stream.Position = 0;
						stream.Read(bytes, 0, bytes.Length);
						break;
					}
					address = (IntPtr)((byte*)address + bufferSize);
					bufferSize += BASE_BUFFER_SIZE;
				} while (bytes == null);
				if (fromEncoding.CodePage != Encoding.Unicode.CodePage)
					bytes = Encoding.Convert(fromEncoding, Encoding.Unicode, bytes);
				fixed (void* p = bytes)
					value = new string((char*)p);
			}
			return true;
		}
		#endregion

		#region write impl
		internal static bool WriteByteInternal(IntPtr processHandle, IntPtr address, byte value) => WriteProcessMemory(processHandle, address, &value, 1, null);

		internal static bool WriteInt16Internal(IntPtr processHandle, IntPtr address, short value) => WriteProcessMemory(processHandle, address, &value, 2, null);

		internal static bool WriteUInt16Internal(IntPtr processHandle, IntPtr address, ushort value) => WriteProcessMemory(processHandle, address, &value, 2, null);

		internal static bool WriteInt32Internal(IntPtr processHandle, IntPtr address, int value) => WriteProcessMemory(processHandle, address, &value, 4, null);

		internal static bool WriteUInt32Internal(IntPtr processHandle, IntPtr address, uint value) => WriteProcessMemory(processHandle, address, &value, 4, null);

		internal static bool WriteInt64Internal(IntPtr processHandle, IntPtr address, long value) => WriteProcessMemory(processHandle, address, &value, 8, null);

		internal static bool WriteUInt64Internal(IntPtr processHandle, IntPtr address, ulong value) => WriteProcessMemory(processHandle, address, &value, 8, null);

		internal static bool WriteSingleInternal(IntPtr processHandle, IntPtr address, float value) => WriteProcessMemory(processHandle, address, &value, 4, null);

		internal static bool WriteDoubleInternal(IntPtr processHandle, IntPtr address, double value) => WriteProcessMemory(processHandle, address, &value, 8, null);

		internal static bool WriteBytesInternal(IntPtr processHandle, IntPtr address, byte[] value) {
			fixed (void* p = value)
				return WriteProcessMemory(processHandle, address, p, (uint)value.Length, null);
		}

		internal static bool WriteBytesInternal(IntPtr processHandle, IntPtr address, byte[] value, out uint numberOfWritten) {
			fixed (void* p = value)
			fixed (uint* pNumberOfWritten = &numberOfWritten)
				return WriteProcessMemory(processHandle, address, p, (uint)value.Length, pNumberOfWritten);
		}

		internal static bool WriteStringInternal(IntPtr processHandle, IntPtr address, string value, Encoding toEncoding) {
			if (value == null)
				throw new ArgumentNullException();
			if (toEncoding == null)
				throw new ArgumentNullException();

			byte[] buffer;

			value += "\0";
			if (toEncoding.CodePage == Encoding.Unicode.CodePage)
				fixed (void* p = value)
					return WriteProcessMemory(processHandle, address, p, (uint)(value.Length * 2), null);
			buffer = Encoding.Convert(Encoding.Unicode, toEncoding, Encoding.Unicode.GetBytes(value));
			fixed (void* p = buffer)
				return WriteProcessMemory(processHandle, address, p, (uint)buffer.Length, null);
		}
		#endregion
	}
}
