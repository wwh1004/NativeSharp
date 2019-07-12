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
				throw new ArgumentNullException(nameof(moduleName));

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
				throw new ArgumentOutOfRangeException(nameof(_type));
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
		#region pointer
		/// <summary>
		/// 获取指针指向的地址
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <returns></returns>
		public IntPtr ToAddress(Pointer pointer) {
			if (pointer is null)
				throw new ArgumentNullException(nameof(pointer));

			IntPtr address;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ToAddressInternal(_handle, pointer, out address));
			return address;
		}

		/// <summary>
		/// 获取指针指向的地址
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <param name="address"></param>
		/// <returns></returns>
		public bool TryToAddress(Pointer pointer, out IntPtr address) {
			address = default;
			if (pointer is null)
				return false;

			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ToAddressInternal(_handle, pointer, out address);
		}
		#endregion

		#region read
		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public byte ReadByte(IntPtr address) {
			byte value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadByteInternal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public short ReadInt16(IntPtr address) {
			short value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadInt16Internal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public ushort ReadUInt16(IntPtr address) {
			ushort value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadUInt16Internal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public int ReadInt32(IntPtr address) {
			int value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadInt32Internal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public uint ReadUInt32(IntPtr address) {
			uint value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadUInt32Internal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public long ReadInt64(IntPtr address) {
			long value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadInt64Internal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public ulong ReadUInt64(IntPtr address) {
			ulong value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadUInt64Internal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public IntPtr ReadIntPtr(IntPtr address) {
			IntPtr value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadIntPtrInternal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public UIntPtr ReadUIntPtr(IntPtr address) {
			UIntPtr value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadUIntPtrInternal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public float ReadSingle(IntPtr address) {
			float value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadSingleInternal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <returns></returns>
		public double ReadDouble(IntPtr address) {
			double value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadDoubleInternal(_handle, address, out value));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void ReadBytes(IntPtr address, byte[] value) {
			if (value is null)
				throw new ArgumentNullException(nameof(value));

			ReadBytes(address, value, 0, (uint)value.Length);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="startIndex">从 <paramref name="value"/> 的指定偏移处开始</param>
		/// <param name="length">长度</param>
		public void ReadBytes(IntPtr address, byte[] value, uint startIndex, uint length) {
			ReadBytes(address, value, startIndex, length, out _);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="startIndex">从 <paramref name="value"/> 的指定偏移处开始</param>
		/// <param name="length">长度</param>
		/// <param name="readCount">实际读取字节数</param>
		public void ReadBytes(IntPtr address, byte[] value, uint startIndex, uint length, out uint readCount) {
			if (value is null)
				throw new ArgumentNullException(nameof(value));
			if (startIndex > value.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (startIndex + length > value.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadBytesInternal(_handle, address, value, startIndex, length, out readCount));
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="isEndWithDoubleZero">字符串是否以2个\0结尾</param>
		/// <param name="fromEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public string ReadString(IntPtr address, bool isEndWithDoubleZero, Encoding fromEncoding) {
			if (fromEncoding is null)
				throw new ArgumentNullException(nameof(fromEncoding));

			string value;

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadStringInternal(_handle, address, out value, isEndWithDoubleZero, fromEncoding));
			return value;
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadByte(IntPtr address, out byte value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadByteInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadInt16(IntPtr address, out short value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadInt16Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadUInt16(IntPtr address, out ushort value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadUInt16Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadInt32(IntPtr address, out int value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadInt32Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadUInt32(IntPtr address, out uint value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadUInt32Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadInt64(IntPtr address, out long value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadInt64Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadUInt64(IntPtr address, out ulong value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadUInt64Internal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadIntPtr(IntPtr address, out IntPtr value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadIntPtrInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadUIntPtr(IntPtr address, out UIntPtr value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadUIntPtrInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadSingle(IntPtr address, out float value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadSingleInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadDouble(IntPtr address, out double value) {
			value = default;
			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadDoubleInternal(_handle, address, out value);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryReadBytes(IntPtr address, byte[] value) {
			if (value is null)
				return false;

			return TryReadBytes(address, value, 0, (uint)value.Length);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="startIndex">从 <paramref name="value"/> 的指定偏移处开始</param>
		/// <param name="length">长度</param>
		/// <returns></returns>
		public bool TryReadBytes(IntPtr address, byte[] value, uint startIndex, uint length) {
			return TryReadBytes(address, value, startIndex, length, out _);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="startIndex">从 <paramref name="value"/> 的指定偏移处开始</param>
		/// <param name="length">长度</param>
		/// <param name="readCount">实际读取字节数</param>
		/// <returns></returns>
		public bool TryReadBytes(IntPtr address, byte[] value, uint startIndex, uint length, out uint readCount) {
			readCount = default;
			if (value is null)
				return false;
			if (startIndex > value.Length)
				return false;
			if (startIndex + length > value.Length)
				return false;

			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadBytesInternal(_handle, address, value, startIndex, length, out readCount);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="isEndWithDoubleZero">字符串是否以2个\0结尾</param>
		/// <param name="fromEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public bool TryReadString(IntPtr address, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
			value = default;
			if (fromEncoding is null)
				return false;

			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadStringInternal(_handle, address, out value, isEndWithDoubleZero, fromEncoding);
		}
		#endregion

		#region write
		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteByte(IntPtr address, byte value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteByteInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteInt16(IntPtr address, short value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteInt16Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteUInt16(IntPtr address, ushort value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteUInt16Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteInt32(IntPtr address, int value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteInt32Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteUInt32(IntPtr address, uint value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteUInt32Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteInt64(IntPtr address, long value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteInt64Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteUInt64(IntPtr address, ulong value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteUInt64Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteIntPtr(IntPtr address, IntPtr value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteIntPtrInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteUIntPtr(IntPtr address, UIntPtr value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteUIntPtrInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteSingle(IntPtr address, float value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteSingleInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteDouble(IntPtr address, double value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteDoubleInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteBytes(IntPtr address, byte[] value) {
			if (value is null)
				throw new ArgumentNullException(nameof(value));

			WriteBytes(address, value, 0, (uint)value.Length);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="startIndex">从 <paramref name="value"/> 的指定偏移处开始</param>
		/// <param name="length">长度</param>
		public void WriteBytes(IntPtr address, byte[] value, uint startIndex, uint length) {
			WriteBytes(address, value, startIndex, length, out _);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="startIndex">从 <paramref name="value"/> 的指定偏移处开始</param>
		/// <param name="length">长度</param>
		/// <param name="writtenCount">实际写入字节数</param>
		public void WriteBytes(IntPtr address, byte[] value, uint startIndex, uint length, out uint writtenCount) {
			if (value is null)
				throw new ArgumentNullException(nameof(value));
			if (startIndex > value.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (startIndex + length > value.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteBytesInternal(_handle, address, value, startIndex, length, out writtenCount));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="toEncoding">内存中字符串的编码</param>
		public void WriteString(IntPtr address, string value, Encoding toEncoding) {
			if (value is null)
				throw new ArgumentNullException(nameof(value));
			if (toEncoding is null)
				throw new ArgumentNullException(nameof(toEncoding));

			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteStringInternal(_handle, address, value, toEncoding));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteByte(IntPtr address, byte value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteByteInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteInt16(IntPtr address, short value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteInt16Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteUInt16(IntPtr address, ushort value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteUInt16Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteInt32(IntPtr address, int value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteInt32Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteUInt32(IntPtr address, uint value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteUInt32Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteInt64(IntPtr address, long value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteInt64Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteUInt64(IntPtr address, ulong value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteUInt64Internal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteIntPtr(IntPtr address, IntPtr value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteIntPtrInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteUIntPtr(IntPtr address, UIntPtr value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteUIntPtrInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteSingle(IntPtr address, float value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteSingleInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteDouble(IntPtr address, double value) {
			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteDoubleInternal(_handle, address, value);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns></returns>
		public bool TryWriteBytes(IntPtr address, byte[] value) {
			if (value is null)
				return false;

			return TryWriteBytes(address, value, 0, (uint)value.Length);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="startIndex">从 <paramref name="value"/> 的指定偏移处开始</param>
		/// <param name="length">长度</param>
		/// <returns></returns>
		public bool TryWriteBytes(IntPtr address, byte[] value, uint startIndex, uint length) {
			return TryWriteBytes(address, value, startIndex, length, out _);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="startIndex">从 <paramref name="value"/> 的指定偏移处开始</param>
		/// <param name="length">长度</param>
		/// <param name="writtenCount">实际写入字节数</param>
		/// <returns></returns>
		public bool TryWriteBytes(IntPtr address, byte[] value, uint startIndex, uint length, out uint writtenCount) {
			writtenCount = default;
			if (value is null)
				return false;
			if (startIndex > value.Length)
				return false;
			if (startIndex + length > value.Length)
				return false;

			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteBytesInternal(_handle, address, value, startIndex, length, out writtenCount);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="toEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public bool TryWriteString(IntPtr address, string value, Encoding toEncoding) {
			if (value is null)
				return false;
			if (toEncoding is null)
				return false;

			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteStringInternal(_handle, address, value, toEncoding);
		}
		#endregion

		#region pointer impl
		internal static bool ToAddressInternal(IntPtr processHandle, Pointer pointer, out IntPtr address) {
			bool is64Bit;

			if (!Is64BitProcessInternal(processHandle, out is64Bit)) {
				address = IntPtr.Zero;
				return false;
			}
			return is64Bit ? ToAddressPrivate64(processHandle, pointer, out address) : ToAddressPrivate32(processHandle, pointer, out address);
		}

		private static bool ToAddressPrivate32(IntPtr processHandle, Pointer pointer, out IntPtr address) {
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

		private static bool ToAddressPrivate64(IntPtr processHandle, Pointer pointer, out IntPtr address) {
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

		internal static bool ReadIntPtrInternal(IntPtr processHandle, IntPtr address, out IntPtr value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, (uint)IntPtr.Size, null);
		}

		internal static bool ReadUIntPtrInternal(IntPtr processHandle, IntPtr address, out UIntPtr value) {
			fixed (void* p = &value)
				return ReadProcessMemory(processHandle, address, p, (uint)UIntPtr.Size, null);
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
			return ReadBytesInternal(processHandle, address, value, 0, (uint)value.Length, out _);
		}

		internal static bool ReadBytesInternal(IntPtr processHandle, IntPtr address, byte[] value, uint startIndex, uint length) {
			return ReadBytesInternal(processHandle, address, value, startIndex, length, out _);
		}

		internal static bool ReadBytesInternal(IntPtr processHandle, IntPtr address, byte[] value, uint startIndex, uint length, out uint readCount) {
			fixed (void* p = &value[startIndex])
			fixed (uint* pReadCount = &readCount)
				return ReadProcessMemory(processHandle, address, p, length, pReadCount);
		}

		internal static bool ReadStringInternal(IntPtr processHandle, IntPtr address, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
			// 在出现时一些特殊字符可能导致字符串被过早截取！
			const uint BASE_BUFFER_SIZE = 0x100;

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
				} while (bytes is null);
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

		internal static bool WriteIntPtrInternal(IntPtr processHandle, IntPtr address, IntPtr value) {
			return WriteProcessMemory(processHandle, address, &value, (uint)IntPtr.Size, null);
		}

		internal static bool WriteUIntPtrInternal(IntPtr processHandle, IntPtr address, UIntPtr value) {
			return WriteProcessMemory(processHandle, address, &value, (uint)UIntPtr.Size, null);
		}

		internal static bool WriteSingleInternal(IntPtr processHandle, IntPtr address, float value) {
			return WriteProcessMemory(processHandle, address, &value, 4, null);
		}

		internal static bool WriteDoubleInternal(IntPtr processHandle, IntPtr address, double value) {
			return WriteProcessMemory(processHandle, address, &value, 8, null);
		}

		internal static bool WriteBytesInternal(IntPtr processHandle, IntPtr address, byte[] value) {
			return WriteBytesInternal(processHandle, address, value, 0, (uint)value.Length, out _);
		}

		internal static bool WriteBytesInternal(IntPtr processHandle, IntPtr address, byte[] value, uint startIndex, uint length) {
			return WriteBytesInternal(processHandle, address, value, startIndex, length, out _);
		}

		internal static bool WriteBytesInternal(IntPtr processHandle, IntPtr address, byte[] value, uint startIndex, uint length, out uint writtenCount) {
			fixed (void* p = &value[startIndex])
			fixed (uint* pWrittenCount = &writtenCount)
				return WriteProcessMemory(processHandle, address, p, length, pWrittenCount);
		}

		internal static bool WriteStringInternal(IntPtr processHandle, IntPtr address, string value, Encoding toEncoding) {
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
