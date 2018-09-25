using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NativeSharp {
	internal static unsafe class NativeMethods {
		public const uint DELETE = 0x00010000;
		public const uint READ_CONTROL = 0x00020000;
		public const uint SYNCHRONIZE = 0x00100000;
		public const uint WRITE_DAC = 0x00040000;
		public const uint WRITE_OWNER = 0x00080000;
		public const uint STANDARD_RIGHTS_REQUIRED = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER;
		public const uint PROCESS_CREATE_PROCESS = 0x0080;
		public const uint PROCESS_CREATE_THREAD = 0x0002;
		public const uint PROCESS_DUP_HANDLE = 0x0040;
		public const uint PROCESS_QUERY_INFORMATION = 0x0400;
		public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
		public const uint PROCESS_SET_INFORMATION = 0x0200;
		public const uint PROCESS_SET_QUOTA = 0x0100;
		public const uint PROCESS_SUSPEND_RESUME = 0x0800;
		public const uint PROCESS_TERMINATE = 0x0001;
		public const uint PROCESS_VM_OPERATION = 0x0008;
		public const uint PROCESS_VM_READ = 0x0010;
		public const uint PROCESS_VM_WRITE = 0x0020;
		public const uint PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFFF;
		public const uint LIST_MODULES_32BIT = 0x1;
		public const uint LIST_MODULES_64BIT = 0x2;
		public const uint LIST_MODULES_ALL = 0x3;
		public const uint MAX_MODULE_NAME32 = 255;
		public const uint MAX_PATH = 260;
		public const uint PAGE_NOACCESS = 0x01;
		public const uint PAGE_READONLY = 0x02;
		public const uint PAGE_READWRITE = 0x04;
		public const uint PAGE_WRITECOPY = 0x08;
		public const uint PAGE_EXECUTE = 0x10;
		public const uint PAGE_EXECUTE_READ = 0x20;
		public const uint PAGE_EXECUTE_READWRITE = 0x40;
		public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
		public const uint PAGE_GUARD = 0x100;
		public const uint PAGE_NOCACHE = 0x200;
		public const uint PAGE_WRITECOMBINE = 0x400;
		public const uint PAGE_REVERT_TO_FILE_MAP = 0x80000000;
		public const uint PAGE_ENCLAVE_THREAD_CONTROL = 0x80000000;
		public const uint PAGE_TARGETS_NO_UPDATE = 0x40000000;
		public const uint PAGE_TARGETS_INVALID = 0x40000000;
		public const uint PAGE_ENCLAVE_UNVALIDATED = 0x20000000;
		public const uint MEM_COMMIT = 0x00001000;
		public const uint MEM_RESERVE = 0x00002000;
		public const uint MEM_DECOMMIT = 0x00004000;
		public const uint MEM_RELEASE = 0x00008000;
		public const uint MEM_FREE = 0x00010000;
		public const uint MEM_PRIVATE = 0x00020000;
		public const uint MEM_MAPPED = 0x00040000;
		public const uint MEM_RESET = 0x00080000;
		public const uint MEM_TOP_DOWN = 0x00100000;
		public const uint MEM_WRITE_WATCH = 0x00200000;
		public const uint MEM_PHYSICAL = 0x00400000;
		public const uint MEM_ROTATE = 0x00800000;
		public const uint MEM_DIFFERENT_IMAGE_BASE_OK = 0x00800000;
		public const uint MEM_RESET_UNDO = 0x01000000;
		public const uint MEM_LARGE_PAGES = 0x20000000;
		public const uint MEM_4MB_PAGES = 0x80000000;
		public const uint MEM_64K_PAGES = MEM_LARGE_PAGES | MEM_PHYSICAL;
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
