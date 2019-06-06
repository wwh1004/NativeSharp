using System;
using System.IO;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// 指针类型
	/// </summary>
	public enum PointerType {
		/// <summary>
		/// 模块名+偏移
		/// </summary>
		ModuleNameWithOffset,

		/// <summary>
		/// 基址+偏移
		/// </summary>
		BaseAddressWithOffset
	}

	/// <summary>
	/// 指针
	/// </summary>
	public sealed class Pointer {
		private readonly string _moduleName;

		private readonly uint _moduleOffset;

		private readonly IntPtr _baseAddress;

		private readonly uint[] _offsets;

		private readonly PointerType _type;

		/// <summary>
		/// 模块名
		/// </summary>
		public string ModuleName => _type == PointerType.ModuleNameWithOffset ? _moduleName : throw new InvalidOperationException($"Type={_type}");

		/// <summary>
		/// 模块偏移
		/// </summary>
		public uint ModuleOffset => _type == PointerType.ModuleNameWithOffset ? _moduleOffset : throw new InvalidOperationException($"Type={_type}");

		/// <summary>
		/// 基址
		/// </summary>
		public IntPtr BaseAddress => _type == PointerType.BaseAddressWithOffset ? _baseAddress : throw new InvalidOperationException($"Type={_type}");

		/// <summary>
		/// 多级偏移
		/// </summary>
		public uint[] Offsets => _offsets;

		/// <summary>
		/// 类型
		/// </summary>
		public PointerType Type => _type;

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <param name="moduleOffset">模块偏移</param>
		/// <param name="offsets">多级偏移</param>
		public Pointer(string moduleName, uint moduleOffset, params uint[] offsets) {
			if (string.IsNullOrEmpty(moduleName))
				throw new ArgumentOutOfRangeException();

			_moduleName = moduleName;
			_moduleOffset = moduleOffset;
			_offsets = offsets;
			_type = PointerType.ModuleNameWithOffset;
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="baseAddress">基址</param>
		/// <param name="offsets">偏移</param>
		public Pointer(IntPtr baseAddress, params uint[] offsets) {
			_baseAddress = baseAddress;
			_offsets = offsets;
			_type = PointerType.BaseAddressWithOffset;
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="basePointer">指针</param>
		/// <param name="offsets">分级偏移</param>
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
				newAddress += offsets[i];
			}
			address = (IntPtr)newAddress;
			return true;
		}

		private static bool GetPointerAddress64(IntPtr processHandle, Pointer pointer, out IntPtr address) {
			ulong newAddress;
			uint[] offsets;

			address = IntPtr.Zero;
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
				newAddress += offsets[i];
			}
			address = (IntPtr)newAddress;
			return true;
		}

		#region read
		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadByte(IntPtr address, out byte value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadByteInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadInt16(IntPtr address, out short value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadInt16Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadUInt16(IntPtr address, out ushort value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadUInt16Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadInt32(IntPtr address, out int value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadInt32Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadUInt32(IntPtr address, out uint value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadUInt32Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadInt64(IntPtr address, out long value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadInt64Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadUInt64(IntPtr address, out ulong value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadUInt64Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadSingle(IntPtr address, out float value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadSingleInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadDouble(IntPtr address, out double value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadDoubleInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadBytes(IntPtr address, byte[] value) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadBytesInternal(_handle, address, value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="readCount">实际读取字节数</param>
		/// <returns></returns>
		public bool ReadBytes(IntPtr address, byte[] value, out uint readCount) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadBytesInternal(_handle, address, value, out readCount);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="isEndWithDoubleZero">字符串是否以2个\0结尾</param>
		/// <param name="fromEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public bool ReadString(IntPtr address, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
			QuickDemand(ProcessAccess.MemoryRead);
			return ReadStringInternal(_handle, address, out value, isEndWithDoubleZero, fromEncoding);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadByte(Pointer pointer, out byte value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadByteInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadInt16(Pointer pointer, out short value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadInt16Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadUInt16(Pointer pointer, out ushort value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadUInt16Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadInt32(Pointer pointer, out int value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadInt32Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadUInt32(Pointer pointer, out uint value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadUInt32Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadInt64(Pointer pointer, out long value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadInt64Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadUInt64(Pointer pointer, out ulong value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadUInt64Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadSingle(Pointer pointer, out float value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadSingleInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadDouble(Pointer pointer, out double value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = default;
				return false;
			}
			return ReadDoubleInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool ReadBytes(Pointer pointer, byte[] value) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out address) && ReadBytesInternal(_handle, address, value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <param name="readCount">实际读取字节数</param>
		/// <returns></returns>
		public bool ReadBytes(Pointer pointer, byte[] value, out uint readCount) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				readCount = 0;
				return false;
			}
			return ReadBytesInternal(_handle, address, value, out readCount);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <param name="isEndWithDoubleZero">字符串是否以2个\0结尾</param>
		/// <param name="fromEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public bool ReadString(Pointer pointer, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out address)) {
				value = null;
				return false;
			}
			return ReadStringInternal(_handle, address, out value, isEndWithDoubleZero, fromEncoding);
		}
		#endregion

		#region write
		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteByte(IntPtr address, byte value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteByteInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteInt16(IntPtr address, short value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteInt16Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteUInt16(IntPtr address, ushort value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteUInt16Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteInt32(IntPtr address, int value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteInt32Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteUInt32(IntPtr address, uint value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteUInt32Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteInt64(IntPtr address, long value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteInt64Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteUInt64(IntPtr address, ulong value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteUInt64Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteSingle(IntPtr address, float value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteSingleInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteDouble(IntPtr address, double value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteDoubleInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteBytes(IntPtr address, byte[] value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteBytesInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="writtenCount">实际写入字节数</param>
		/// <returns></returns>
		public bool WriteBytes(IntPtr address, byte[] value, out uint writtenCount) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteBytesInternal(_handle, address, value, out writtenCount);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="toEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public bool WriteString(IntPtr address, string value, Encoding toEncoding) {
			QuickDemand(ProcessAccess.MemoryWrite);
			return WriteStringInternal(_handle, address, value, toEncoding);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteByte(Pointer pointer, byte value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteByteInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteInt16(Pointer pointer, short value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteInt16Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteUInt16(Pointer pointer, ushort value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteUInt16Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteInt32(Pointer pointer, int value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteInt32Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteUInt32(Pointer pointer, uint value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteUInt32Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteInt64(Pointer pointer, long value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteInt64Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteUInt64(Pointer pointer, ulong value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteUInt64Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteSingle(Pointer pointer, float value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteSingleInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteDouble(Pointer pointer, double value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteDoubleInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool WriteBytes(Pointer pointer, byte[] value) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			return GetPointerAddress(_handle, pointer, out IntPtr address) && WriteBytesInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <param name="writtenCount">实际写入字节数</param>
		/// <returns></returns>
		public bool WriteBytes(Pointer pointer, byte[] value, out uint writtenCount) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
			if (!GetPointerAddress(_handle, pointer, out IntPtr address)) {
				writtenCount = 0;
				return false;
			}
			return WriteBytesInternal(_handle, address, value, out writtenCount);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="value">值</param>
		/// <param name="toEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public bool WriteString(Pointer pointer, string value, Encoding toEncoding) {
			QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation);
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

		internal static bool ReadBytesInternal(IntPtr processHandle, IntPtr address, byte[] value, out uint readCount) {
			fixed (void* p = value)
			fixed (uint* pReadCount = &readCount)
				return ReadProcessMemory(processHandle, address, p, (uint)value.Length, pReadCount);
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
		internal static bool WriteByteInternal(IntPtr processHandle, IntPtr address, byte value) {
			return WriteProcessMemory(processHandle, address, &value, 1, null);
		}

		internal static bool WriteInt16Internal(IntPtr processHandle, IntPtr address, short value) {
			return WriteProcessMemory(processHandle, address, &value, 2, null);
		}

		internal static bool WriteUInt16Internal(IntPtr processHandle, IntPtr address, ushort value) {
			return WriteProcessMemory(processHandle, address, &value, 2, null);
		}

		internal static bool WriteInt32Internal(IntPtr processHandle, IntPtr address, int value) {
			return WriteProcessMemory(processHandle, address, &value, 4, null);
		}

		internal static bool WriteUInt32Internal(IntPtr processHandle, IntPtr address, uint value) {
			return WriteProcessMemory(processHandle, address, &value, 4, null);
		}

		internal static bool WriteInt64Internal(IntPtr processHandle, IntPtr address, long value) {
			return WriteProcessMemory(processHandle, address, &value, 8, null);
		}

		internal static bool WriteUInt64Internal(IntPtr processHandle, IntPtr address, ulong value) {
			return WriteProcessMemory(processHandle, address, &value, 8, null);
		}

		internal static bool WriteSingleInternal(IntPtr processHandle, IntPtr address, float value) {
			return WriteProcessMemory(processHandle, address, &value, 4, null);
		}

		internal static bool WriteDoubleInternal(IntPtr processHandle, IntPtr address, double value) {
			return WriteProcessMemory(processHandle, address, &value, 8, null);
		}

		internal static bool WriteBytesInternal(IntPtr processHandle, IntPtr address, byte[] value) {
			fixed (void* p = value)
				return WriteProcessMemory(processHandle, address, p, (uint)value.Length, null);
		}

		internal static bool WriteBytesInternal(IntPtr processHandle, IntPtr address, byte[] value, out uint writtenCount) {
			fixed (void* p = value)
			fixed (uint* pWrittenCount = &writtenCount)
				return WriteProcessMemory(processHandle, address, p, (uint)value.Length, pWrittenCount);
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
