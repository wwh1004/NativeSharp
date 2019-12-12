using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// 指针
	/// </summary>
	public unsafe sealed class Pointer {
		private string _moduleName;
		private void* _baseAddress;
		private readonly List<uint> _offsets;

		/// <summary>
		/// 模块名。若 <see cref="BaseAddress"/> 为 <see langword="null"/>，下次使用当前指针实例获取地址时，<see cref="BaseAddress"/> 将被设置为 <see cref="ModuleName"/> 对应的句柄（模块基址）
		/// </summary>
		public string ModuleName {
			get => _moduleName;
			set => _moduleName = value;
		}

		/// <summary>
		/// 基址
		/// </summary>
		public void* BaseAddress {
			get => _baseAddress;
			set => _baseAddress = value;
		}

		/// <summary>
		/// 多级偏移
		/// </summary>
		public IList<uint> Offsets => _offsets;

		/// <summary>
		/// 构造器
		/// </summary>
		public Pointer() {
			_offsets = new List<uint>();
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <param name="offsets">多级偏移</param>
		public Pointer(string moduleName, params uint[] offsets) {
			_moduleName = moduleName;
			_offsets = new List<uint>(offsets);
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="baseAddress">基址</param>
		/// <param name="offsets">偏移</param>
		public Pointer(void* baseAddress, params uint[] offsets) {
			_baseAddress = baseAddress;
			_offsets = new List<uint>(offsets);
		}

		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="pointer">指针</param>
		public Pointer(Pointer pointer) {
			_moduleName = pointer._moduleName;
			_baseAddress = pointer._baseAddress;
			_offsets = new List<uint>(pointer._offsets);
		}
	}

	unsafe partial class NativeProcess {
		#region pointer
		/// <summary>
		/// 获取指针指向的地址
		/// </summary>
		/// <param name="pointer">指针</param>
		/// <returns></returns>
		public void* ToAddress(Pointer pointer) {
			if (pointer is null)
				throw new ArgumentNullException(nameof(pointer));

			void* address;

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
		public bool TryToAddress(Pointer pointer, out void* address) {
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
		public byte ReadByte(void* address) {
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
		public short ReadInt16(void* address) {
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
		public ushort ReadUInt16(void* address) {
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
		public int ReadInt32(void* address) {
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
		public uint ReadUInt32(void* address) {
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
		public long ReadInt64(void* address) {
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
		public ulong ReadUInt64(void* address) {
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
		public IntPtr ReadIntPtr(void* address) {
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
		public UIntPtr ReadUIntPtr(void* address) {
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
		public float ReadSingle(void* address) {
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
		public double ReadDouble(void* address) {
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
		public void ReadBytes(void* address, byte[] value) {
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
		public void ReadBytes(void* address, byte[] value, uint startIndex, uint length) {
			if (value is null)
				throw new ArgumentNullException(nameof(value));
			if (startIndex > value.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (startIndex + length > value.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			QuickDemand(ProcessAccess.MemoryRead);
			ThrowWin32ExceptionIfFalse(ReadBytesInternal(_handle, address, value, startIndex, length));
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="isEndWithDoubleZero">字符串是否以2个\0结尾</param>
		/// <param name="fromEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public string ReadString(void* address, bool isEndWithDoubleZero, Encoding fromEncoding) {
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
		public bool TryReadByte(void* address, out byte value) {
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
		public bool TryReadInt16(void* address, out short value) {
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
		public bool TryReadUInt16(void* address, out ushort value) {
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
		public bool TryReadInt32(void* address, out int value) {
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
		public bool TryReadUInt32(void* address, out uint value) {
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
		public bool TryReadInt64(void* address, out long value) {
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
		public bool TryReadUInt64(void* address, out ulong value) {
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
		public bool TryReadIntPtr(void* address, out IntPtr value) {
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
		public bool TryReadUIntPtr(void* address, out UIntPtr value) {
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
		public bool TryReadSingle(void* address, out float value) {
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
		public bool TryReadDouble(void* address, out double value) {
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
		public bool TryReadBytes(void* address, byte[] value) {
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
		public bool TryReadBytes(void* address, byte[] value, uint startIndex, uint length) {
			if (value is null)
				return false;
			if (startIndex > value.Length)
				return false;
			if (startIndex + length > value.Length)
				return false;

			if (!QuickDemandNoThrow(ProcessAccess.MemoryRead))
				return false;
			return ReadBytesInternal(_handle, address, value, startIndex, length);
		}

		/// <summary>
		/// 读取内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="isEndWithDoubleZero">字符串是否以2个\0结尾</param>
		/// <param name="fromEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public bool TryReadString(void* address, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
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
		public void WriteByte(void* address, byte value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteByteInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteInt16(void* address, short value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteInt16Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteUInt16(void* address, ushort value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteUInt16Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteInt32(void* address, int value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteInt32Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteUInt32(void* address, uint value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteUInt32Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteInt64(void* address, long value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteInt64Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteUInt64(void* address, ulong value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteUInt64Internal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteIntPtr(void* address, IntPtr value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteIntPtrInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteUIntPtr(void* address, UIntPtr value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteUIntPtrInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteSingle(void* address, float value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteSingleInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteDouble(void* address, double value) {
			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteDoubleInternal(_handle, address, value));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		public void WriteBytes(void* address, byte[] value) {
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
		public void WriteBytes(void* address, byte[] value, uint startIndex, uint length) {
			if (value is null)
				throw new ArgumentNullException(nameof(value));
			if (startIndex > value.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (startIndex + length > value.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			QuickDemand(ProcessAccess.MemoryWrite);
			ThrowWin32ExceptionIfFalse(WriteBytesInternal(_handle, address, value, startIndex, length));
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="toEncoding">内存中字符串的编码</param>
		public void WriteString(void* address, string value, Encoding toEncoding) {
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
		public bool TryWriteByte(void* address, byte value) {
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
		public bool TryWriteInt16(void* address, short value) {
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
		public bool TryWriteUInt16(void* address, ushort value) {
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
		public bool TryWriteInt32(void* address, int value) {
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
		public bool TryWriteUInt32(void* address, uint value) {
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
		public bool TryWriteInt64(void* address, long value) {
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
		public bool TryWriteUInt64(void* address, ulong value) {
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
		public bool TryWriteIntPtr(void* address, IntPtr value) {
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
		public bool TryWriteUIntPtr(void* address, UIntPtr value) {
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
		public bool TryWriteSingle(void* address, float value) {
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
		public bool TryWriteDouble(void* address, double value) {
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
		public bool TryWriteBytes(void* address, byte[] value) {
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
		public bool TryWriteBytes(void* address, byte[] value, uint startIndex, uint length) {
			if (value is null)
				return false;
			if (startIndex > value.Length)
				return false;
			if (startIndex + length > value.Length)
				return false;

			if (!QuickDemandNoThrow(ProcessAccess.MemoryWrite))
				return false;
			return WriteBytesInternal(_handle, address, value, startIndex, length);
		}

		/// <summary>
		/// 写入内存
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <param name="toEncoding">内存中字符串的编码</param>
		/// <returns></returns>
		public bool TryWriteString(void* address, string value, Encoding toEncoding) {
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
		internal static bool ToAddressInternal(void* processHandle, Pointer pointer, out void* address) {
			bool is64Bit;

			if (!Is64BitProcessInternal(processHandle, out is64Bit)) {
				address = default;
				return false;
			}
			return is64Bit ? ToAddressPrivate64(processHandle, pointer, out address) : ToAddressPrivate32(processHandle, pointer, out address);
		}

		private static bool ToAddressPrivate32(void* processHandle, Pointer pointer, out void* address) {
			uint newAddress;
			IList<uint> offsets;

			address = default;
			if (pointer.BaseAddress == null) {
				if (string.IsNullOrEmpty(pointer.ModuleName))
					throw new ArgumentNullException(nameof(Pointer.ModuleName));
				pointer.BaseAddress = GetModuleHandleInternal(processHandle, false, pointer.ModuleName);
			}
			if (pointer.BaseAddress == null)
				throw new ArgumentNullException(nameof(Pointer.BaseAddress));
			newAddress = (uint)pointer.BaseAddress;
			offsets = pointer.Offsets;
			if (offsets.Count > 0) {
				for (int i = 0; i < offsets.Count - 1; i++) {
					newAddress += offsets[i];
					if (!ReadUInt32Internal(processHandle, (void*)newAddress, out newAddress))
						return false;
				}
				newAddress += offsets[offsets.Count - 1];
			}
			address = (void*)newAddress;
			return true;
		}

		private static bool ToAddressPrivate64(void* processHandle, Pointer pointer, out void* address) {
			ulong newAddress;
			IList<uint> offsets;

			address = default;
			if (pointer.BaseAddress == null) {
				if (string.IsNullOrEmpty(pointer.ModuleName))
					throw new ArgumentNullException(nameof(Pointer.ModuleName));
				pointer.BaseAddress = GetModuleHandleInternal(processHandle, false, pointer.ModuleName);
			}
			if (pointer.BaseAddress == null)
				throw new ArgumentNullException(nameof(Pointer.BaseAddress));
			newAddress = (ulong)pointer.BaseAddress;
			offsets = pointer.Offsets;
			if (offsets.Count > 0) {
				for (int i = 0; i < offsets.Count - 1; i++) {
					newAddress += offsets[i];
					if (!ReadUInt64Internal(processHandle, (void*)newAddress, out newAddress))
						return false;
				}
				newAddress += offsets[offsets.Count - 1];
			}
			address = (void*)newAddress;
			return true;
		}
		#endregion

		#region read impl
		internal static bool ReadByteInternal(void* processHandle, void* address, out byte value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 1);
		}

		internal static bool ReadInt16Internal(void* processHandle, void* address, out short value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 2);
		}

		internal static bool ReadUInt16Internal(void* processHandle, void* address, out ushort value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 2);
		}

		internal static bool ReadInt32Internal(void* processHandle, void* address, out int value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 4);
		}

		internal static bool ReadUInt32Internal(void* processHandle, void* address, out uint value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 4);
		}

		internal static bool ReadInt64Internal(void* processHandle, void* address, out long value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 8);
		}

		internal static bool ReadUInt64Internal(void* processHandle, void* address, out ulong value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 8);
		}

		internal static bool ReadIntPtrInternal(void* processHandle, void* address, out IntPtr value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, (uint)IntPtr.Size);
		}

		internal static bool ReadUIntPtrInternal(void* processHandle, void* address, out UIntPtr value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, (uint)UIntPtr.Size);
		}

		internal static bool ReadSingleInternal(void* processHandle, void* address, out float value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 4);
		}

		internal static bool ReadDoubleInternal(void* processHandle, void* address, out double value) {
			fixed (void* p = &value)
				return ReadInternal(processHandle, address, p, 8);
		}

		internal static bool ReadBytesInternal(void* processHandle, void* address, byte[] value) {
			return ReadBytesInternal(processHandle, address, value, 0, (uint)value.Length);
		}

		internal static bool ReadBytesInternal(void* processHandle, void* address, byte[] value, uint startIndex, uint length) {
			fixed (void* p = &value[startIndex])
				return ReadInternal(processHandle, address, p, length);
		}

		internal static bool ReadStringInternal(void* processHandle, void* address, out string value, bool isEndWithDoubleZero, Encoding fromEncoding) {
			// 在出现时一些特殊字符可能导致字符串被过早截取！
			const uint BASE_BUFFER_SIZE = 0x100;

			uint dummy;

			if (!ReadInternal(processHandle, address, &dummy, isEndWithDoubleZero ? 2u : 1)) {
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
					address = (byte*)address + bufferSize;
					bufferSize += BASE_BUFFER_SIZE;
				} while (bytes is null);
				if (fromEncoding.CodePage != Encoding.Unicode.CodePage)
					bytes = Encoding.Convert(fromEncoding, Encoding.Unicode, bytes);
				fixed (void* p = bytes)
					value = new string((char*)p);
			}
			return true;
		}

		internal static bool ReadInternal(void* processHandle, void* address, void* value, uint length) {
			return ReadProcessMemory(processHandle, address, value, length, null);
		}
		#endregion

		#region write impl
		internal static bool WriteByteInternal(void* processHandle, void* address, byte value) {
			return WriteInternal(processHandle, address, &value, 1);
		}

		internal static bool WriteInt16Internal(void* processHandle, void* address, short value) {
			return WriteInternal(processHandle, address, &value, 2);
		}

		internal static bool WriteUInt16Internal(void* processHandle, void* address, ushort value) {
			return WriteInternal(processHandle, address, &value, 2);
		}

		internal static bool WriteInt32Internal(void* processHandle, void* address, int value) {
			return WriteInternal(processHandle, address, &value, 4);
		}

		internal static bool WriteUInt32Internal(void* processHandle, void* address, uint value) {
			return WriteInternal(processHandle, address, &value, 4);
		}

		internal static bool WriteInt64Internal(void* processHandle, void* address, long value) {
			return WriteInternal(processHandle, address, &value, 8);
		}

		internal static bool WriteUInt64Internal(void* processHandle, void* address, ulong value) {
			return WriteInternal(processHandle, address, &value, 8);
		}

		internal static bool WriteIntPtrInternal(void* processHandle, void* address, IntPtr value) {
			return WriteInternal(processHandle, address, &value, (uint)IntPtr.Size);
		}

		internal static bool WriteUIntPtrInternal(void* processHandle, void* address, UIntPtr value) {
			return WriteInternal(processHandle, address, &value, (uint)UIntPtr.Size);
		}

		internal static bool WriteSingleInternal(void* processHandle, void* address, float value) {
			return WriteInternal(processHandle, address, &value, 4);
		}

		internal static bool WriteDoubleInternal(void* processHandle, void* address, double value) {
			return WriteInternal(processHandle, address, &value, 8);
		}

		internal static bool WriteBytesInternal(void* processHandle, void* address, byte[] value) {
			return WriteBytesInternal(processHandle, address, value, 0, (uint)value.Length);
		}

		internal static bool WriteBytesInternal(void* processHandle, void* address, byte[] value, uint startIndex, uint length) {
			fixed (void* p = &value[startIndex])
				return WriteInternal(processHandle, address, p, length);
		}

		internal static bool WriteStringInternal(void* processHandle, void* address, string value, Encoding toEncoding) {
			byte[] buffer;

			value += "\0";
			if (toEncoding.CodePage == Encoding.Unicode.CodePage)
				fixed (void* p = value)
					return WriteInternal(processHandle, address, p, (uint)(value.Length * 2));
			buffer = Encoding.Convert(Encoding.Unicode, toEncoding, Encoding.Unicode.GetBytes(value));
			fixed (void* p = buffer)
				return WriteInternal(processHandle, address, p, (uint)buffer.Length);
		}

		internal static bool WriteInternal(void* processHandle, void* address, void* value, uint length) {
			return WriteProcessMemory(processHandle, address, value, length, null);
		}
		#endregion
	}
}
