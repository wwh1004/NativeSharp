using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NativeSharp {
	internal static unsafe class NativeMethods {
		public const uint STANDARD_RIGHTS_REQUIRED = 0xF0000;
		public const uint LIST_MODULES_32BIT = 0x1;
		public const uint LIST_MODULES_64BIT = 0x2;
		public const uint LIST_MODULES_ALL = 0x3;
		public const uint MAX_MODULE_NAME32 = 255;
		public const uint MAX_PATH = 260;
		public const uint INFINITE = 0xFFFFFFFF;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct MEMORY_BASIC_INFORMATION {
			public static readonly uint UnmanagedSize = (uint)sizeof(MEMORY_BASIC_INFORMATION);

			public IntPtr BaseAddress;
			public IntPtr AllocationBase;
			public uint AllocationProtect;
			public IntPtr RegionSize;
			public uint State;
			public uint Protect;
			public uint Type;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct IMAGE_EXPORT_DIRECTORY {
			public static readonly uint UnmanagedSize = (uint)sizeof(MEMORY_BASIC_INFORMATION);

			public uint Characteristics;
			public uint TimeDateStamp;
			public ushort MajorVersion;
			public ushort MinorVersion;
			public uint Name;
			public uint Base;
			public uint NumberOfFunctions;
			public uint NumberOfNames;
			public uint AddressOfFunctions;
			public uint AddressOfNames;
			public uint AddressOfNameOrdinals;
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern uint GetCurrentProcessId();

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern uint GetProcessId(IntPtr Process);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, void* lpBuffer, uint nSize, uint* lpNumberOfBytesRead);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, void* lpBuffer, uint nSize, uint* lpNumberOfBytesWritten);

		[DllImport("psapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumProcesses(uint* pProcessIds, uint cb, out uint pBytesReturned);

		[DllImport("psapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumProcessModulesEx(IntPtr hProcess, IntPtr* lphModule, uint cb, out uint lpcbNeeded, uint dwFilterFlag);

		[DllImport("psapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetProcessImageFileName(IntPtr hProcess, StringBuilder lpImageFileName, uint nSize);

		[DllImport("psapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);

		[DllImport("psapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, uint nSize);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr CreateRemoteThread(IntPtr hProcess, void* lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, uint* lpThreadId);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
	}
}
