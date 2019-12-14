using System;
using System.IO;
using System.Text;
using static NativeSharp.NativeMethods;

namespace NativeSharp {
	/// <summary>
	/// 注入时使用的CLR版本
	/// </summary>
	public enum InjectionClrVersion {
		/// <summary>
		/// 自动选择，由要注入的程序集本身决定
		/// </summary>
		Auto,

		/// <summary>
		/// v2.0.50727
		/// </summary>
		V2,

		/// <summary>
		/// v4.0.30319
		/// </summary>
		V4
	}

	unsafe partial class NativeProcess {
		/// <summary>
		/// 注入托管DLL
		/// </summary>
		/// <param name="assemblyPath">要注入程序集的路径</param>
		/// <param name="typeName">类型名（命名空间+类型名，比如NamespaceA.ClassB）</param>
		/// <param name="methodName">方法名（比如MethodC），该方法必须具有此类签名static int MethodName(string)，比如private static int InjectingMain(string argument)</param>
		/// <param name="argument">参数，可传入 <see langword="null"/></param>
		/// <returns></returns>
		public bool InjectManaged(string assemblyPath, string typeName, string methodName, string argument) {
			return InjectManaged(assemblyPath, typeName, methodName, argument, InjectionClrVersion.Auto);
		}

		/// <summary>
		/// 注入托管DLL
		/// </summary>
		/// <param name="assemblyPath">要注入程序集的路径</param>
		/// <param name="typeName">类型名（命名空间+类型名，比如NamespaceA.ClassB）</param>
		/// <param name="methodName">方法名（比如MethodC），该方法必须具有此类签名static int MethodName(string)，比如private static int InjectingMain(string argument)</param>
		/// <param name="argument">参数，可传入 <see langword="null"/></param>
		/// <param name="clrVersion">使用的CLR版本</param>
		/// <returns></returns>
		public bool InjectManaged(string assemblyPath, string typeName, string methodName, string argument, InjectionClrVersion clrVersion) {
			if (string.IsNullOrEmpty(assemblyPath))
				throw new ArgumentNullException(nameof(assemblyPath));
			if (!File.Exists(assemblyPath))
				throw new FileNotFoundException();
			if (string.IsNullOrEmpty(typeName))
				throw new ArgumentNullException(nameof(typeName));
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentNullException(nameof(methodName));

			QuickDemand(ProcessAccess.CreateThread | ProcessAccess.MemoryOperation | ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation | ProcessAccess.Synchronize);
			return Injector.InjectManagedInternal(_handle, assemblyPath, typeName, methodName, argument, clrVersion, out _, false);
		}

		/// <summary>
		/// 注入托管DLL，并获取被调用方法的返回值（警告：被调用方法返回后才能获取到返回值，<see cref="InjectManaged(string, string, string, string, out int)"/>方法将一直等待到被调用方法返回。如果仅注入程序集而不需要获取返回值，请使用重载版本<see cref="InjectManaged(string, string, string, string)"/>）
		/// </summary>
		/// <param name="assemblyPath">要注入程序集的路径</param>
		/// <param name="typeName">类型名（命名空间+类型名，比如NamespaceA.ClassB）</param>
		/// <param name="methodName">方法名（比如MethodC），该方法必须具有此类签名static int MethodName(string)，比如private static int InjectingMain(string argument)</param>
		/// <param name="argument">参数，可传入 <see langword="null"/></param>
		/// <param name="returnValue">被调用方法返回的整数值</param>
		/// <returns></returns>
		public bool InjectManaged(string assemblyPath, string typeName, string methodName, string argument, out int returnValue) {
			return InjectManaged(assemblyPath, typeName, methodName, argument, InjectionClrVersion.Auto, out returnValue);
		}

		/// <summary>
		/// 注入托管DLL，并获取被调用方法的返回值（警告：被调用方法返回后才能获取到返回值，<see cref="InjectManaged(string, string, string, string, out int)"/>方法将一直等待到被调用方法返回。如果仅注入程序集而不需要获取返回值，请使用重载版本<see cref="InjectManaged(string, string, string, string)"/>）
		/// </summary>
		/// <param name="assemblyPath">要注入程序集的路径</param>
		/// <param name="typeName">类型名（命名空间+类型名，比如NamespaceA.ClassB）</param>
		/// <param name="methodName">方法名（比如MethodC），该方法必须具有此类签名static int MethodName(string)，比如private static int InjectingMain(string argument)</param>
		/// <param name="argument">参数，可传入 <see langword="null"/></param>
		/// <param name="clrVersion">使用的CLR版本</param>
		/// <param name="returnValue">被调用方法返回的整数值</param>
		/// <returns></returns>
		public bool InjectManaged(string assemblyPath, string typeName, string methodName, string argument, InjectionClrVersion clrVersion, out int returnValue) {
			if (string.IsNullOrEmpty(assemblyPath))
				throw new ArgumentNullException(nameof(assemblyPath));
			if (!File.Exists(assemblyPath))
				throw new FileNotFoundException();
			if (string.IsNullOrEmpty(typeName))
				throw new ArgumentNullException(nameof(typeName));
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentNullException(nameof(methodName));

			QuickDemand(ProcessAccess.CreateThread | ProcessAccess.MemoryOperation | ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation | ProcessAccess.Synchronize);
			return Injector.InjectManagedInternal(_handle, assemblyPath, typeName, methodName, argument, clrVersion, out returnValue, true);
		}

		/// <summary>
		/// 注入非托管DLL
		/// </summary>
		/// <param name="dllPath">要注入DLL的路径</param>
		/// <returns></returns>
		public bool InjectUnmanaged(string dllPath) {
			if (string.IsNullOrEmpty(dllPath))
				throw new ArgumentNullException(nameof(dllPath));

			QuickDemand(ProcessAccess.CreateThread | ProcessAccess.MemoryOperation | ProcessAccess.MemoryRead | ProcessAccess.MemoryWrite | ProcessAccess.QueryInformation | ProcessAccess.Synchronize);
			return Injector.InjectUnmanagedInternal(_handle, dllPath);
		}
	}

	internal static unsafe class Injector {
		private const string CLR_V2 = "v2.0.50727";
		private const string CLR_V4 = "v4.0.30319";
		private const int AssemblyPathOffset = 0x200;
		private const int TypeNameOffset = 0x800;
		private const int MethodNameOffset = 0x980;
		private const int ReturnValueOffset = 0xA00;
		private const int CLRVersionOffset = 0xA10;
		private const int CLSID_CLRMetaHostOffset = 0xA60;
		private const int IID_ICLRMetaHostOffset = 0xA70;
		private const int IID_ICLRRuntimeInfoOffset = 0xA80;
		private const int CLSID_CLRRuntimeHostOffset = 0xA90;
		private const int IID_ICLRRuntimeHostOffset = 0xAA0;
		private const int ArgumentOffset = 0xB00;
		private readonly static byte[] CLSID_CLRMetaHost = new Guid(0x9280188D, 0x0E8E, 0x4867, 0xB3, 0x0C, 0x7F, 0xA8, 0x38, 0x84, 0xE8, 0xDE).ToByteArray();
		private readonly static byte[] IID_ICLRMetaHost = new Guid(0xD332DB9E, 0xB9B3, 0x4125, 0x82, 0x07, 0xA1, 0x48, 0x84, 0xF5, 0x32, 0x16).ToByteArray();
		private readonly static byte[] IID_ICLRRuntimeInfo = new Guid(0xBD39D1D2, 0xBA2F, 0x486A, 0x89, 0xB0, 0xB4, 0xB0, 0xCB, 0x46, 0x68, 0x91).ToByteArray();
		private readonly static byte[] CLSID_CLRRuntimeHost = new Guid(0x90F1A06E, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02).ToByteArray();
		private readonly static byte[] IID_ICLRRuntimeHost = new Guid(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02).ToByteArray();

		private struct SectionHeader {
			public uint VirtualSize;

			public uint VirtualAddress;

			public uint RawSize;

			public uint RawAddress;

			public SectionHeader(uint virtualSize, uint virtualAddress, uint rawSize, uint rawAddress) {
				VirtualSize = virtualSize;
				VirtualAddress = virtualAddress;
				RawSize = rawSize;
				RawAddress = rawAddress;
			}
		}

		internal static bool InjectManagedInternal(void* processHandle, string assemblyPath, string typeName, string methodName, string argument, InjectionClrVersion clrVersion, out int returnValue, bool wait) {
			bool isAssembly;
			InjectionClrVersion clrVersionTemp;
			void* pEnvironment;
			void* threadHandle;
			uint exitCode;

			returnValue = 0;
			assemblyPath = Path.GetFullPath(assemblyPath);
			// 获取绝对路径
			IsAssembly(assemblyPath, out isAssembly, out clrVersionTemp);
			if (clrVersion == InjectionClrVersion.Auto)
				clrVersion = clrVersionTemp;
			if (!isAssembly)
				throw new NotSupportedException("Not a valid .NET assembly.");
			if (!InjectUnmanagedInternal(processHandle, Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"System32\mscoree.dll")))
				return false;
			// 加载对应进程位数的mscoree.dll
			pEnvironment = WriteMachineCode(processHandle, clrVersion, assemblyPath, typeName, methodName, argument);
			// 获取远程进程中启动CLR的函数指针
			if (pEnvironment == null)
				return false;
			threadHandle = CreateRemoteThread(processHandle, null, 0, pEnvironment, ((byte*)pEnvironment + ReturnValueOffset), 0, null);
			if (threadHandle == null)
				return false;
			if (wait) {
				WaitForSingleObject(threadHandle, INFINITE);
				// 等待线程结束
				if (!GetExitCodeThread(threadHandle, out exitCode))
					return false;
				if (!NativeProcess.ReadInt32Internal(processHandle, ((byte*)pEnvironment + ReturnValueOffset), out returnValue))
					return false;
				// 获取程序集中被调用方法的返回值
				if (!NativeProcess.FreeMemoryInternal(processHandle, pEnvironment))
					return false;
				return exitCode == 0;
			}
			return true;
		}

		internal static bool InjectUnmanagedInternal(void* processHandle, string dllPath) {
			void* pLoadLibrary;
			void* pDllPath;
			void* threadHandle;
			uint exitCode;

			pLoadLibrary = NativeModule.GetFunctionAddressInternal(processHandle, "kernel32.dll", "LoadLibraryW");
			// 获取LoadLibrary的函数地址
			pDllPath = NativeProcess.AllocMemoryInternal(processHandle, (uint)dllPath.Length * 2 + 2, MemoryProtection.ExecuteRead);
			try {
				if (pDllPath == null)
					return false;
				if (!NativeProcess.WriteStringInternal(processHandle, pDllPath, dllPath, Encoding.Unicode))
					return false;
				threadHandle = CreateRemoteThread(processHandle, null, 0, pLoadLibrary, pDllPath, 0, null);
				if (threadHandle == null)
					return false;
				WaitForSingleObject(threadHandle, INFINITE);
				// 等待线程结束
				GetExitCodeThread(threadHandle, out exitCode);
				return exitCode != 0;
				// LoadLibrary返回值不为0则调用成功，否则失败
			}
			finally {
				NativeProcess.FreeMemoryInternal(processHandle, pDllPath);
			}
		}

		private static void* WriteMachineCode(void* processHandle, InjectionClrVersion clrVersion, string assemblyPath, string typeName, string methodName, string argument) {
			bool is64Bit;
			string clrVersionString;
			byte[] machineCode;
			void* pEnvironment;
			void* pCorBindToRuntimeEx;
			void* pCLRCreateInstance;

			if (!NativeProcess.Is64BitProcessInternal(processHandle, out is64Bit))
				return null;
			clrVersionString = clrVersion switch
			{
				InjectionClrVersion.V2 => CLR_V2,
				InjectionClrVersion.V4 => CLR_V4,
				_ => throw new ArgumentOutOfRangeException(nameof(clrVersion)),
			};
			machineCode = GetMachineCodeTemplate(clrVersionString, assemblyPath, typeName, methodName, argument);
			pEnvironment = NativeProcess.AllocMemoryInternal(processHandle, 0x1000 + (argument is null ? 0 : (uint)argument.Length * 2 + 2), MemoryProtection.ExecuteReadWrite);
			if (pEnvironment == null)
				return null;
			try {
				fixed (byte* p = machineCode)
					switch (clrVersion) {
					case InjectionClrVersion.V2:
						pCorBindToRuntimeEx = NativeModule.GetFunctionAddressInternal(processHandle, "mscoree.dll", "CorBindToRuntimeEx");
						if (pCorBindToRuntimeEx == null)
							return null;
						if (is64Bit)
							WriteMachineCode64v2(p, (ulong)pEnvironment, (ulong)pCorBindToRuntimeEx);
						else
							WriteMachineCode32v2(p, (uint)pEnvironment, (uint)pCorBindToRuntimeEx);
						break;
					case InjectionClrVersion.V4:
						pCLRCreateInstance = NativeModule.GetFunctionAddressInternal(processHandle, "mscoree.dll", "CLRCreateInstance");
						if (pCLRCreateInstance == null)
							return null;
						if (is64Bit)
							WriteMachineCode64v4(p, (ulong)pEnvironment, (ulong)pCLRCreateInstance);
						else
							WriteMachineCode32v4(p, (uint)pEnvironment, (uint)pCLRCreateInstance);
						break;
					}
				if (!NativeProcess.WriteBytesInternal(processHandle, pEnvironment, machineCode))
					return null;
			}
			catch {
				NativeProcess.FreeMemoryInternal(processHandle, pEnvironment);
				return null;
			}
			return pEnvironment;
		}

		private static byte[] GetMachineCodeTemplate(string clrVersion, string assemblyPath, string typeName, string methodName, string argument) {
			byte[] buffer;

			using (MemoryStream stream = new MemoryStream(0x1000 + (argument is null ? 0 : argument.Length * 2))) {
				buffer = Encoding.Unicode.GetBytes(assemblyPath);
				stream.Position = AssemblyPathOffset;
				stream.Write(buffer, 0, buffer.Length);
				// assemblyPath
				buffer = Encoding.Unicode.GetBytes(typeName);
				stream.Position = TypeNameOffset;
				stream.Write(buffer, 0, buffer.Length);
				// typeName
				buffer = Encoding.Unicode.GetBytes(methodName);
				stream.Position = MethodNameOffset;
				stream.Write(buffer, 0, buffer.Length);
				// methodName
				buffer = argument is null ? Array2.Empty<byte>() : Encoding.Unicode.GetBytes(argument);
				stream.Position = ArgumentOffset;
				stream.Write(buffer, 0, buffer.Length);
				// argument
				buffer = Encoding.Unicode.GetBytes(clrVersion);
				stream.Position = CLRVersionOffset;
				stream.Write(buffer, 0, buffer.Length);
				// clrVersion
				stream.Position = CLSID_CLRMetaHostOffset;
				stream.Write(CLSID_CLRMetaHost, 0, CLSID_CLRMetaHost.Length);
				stream.Position = IID_ICLRMetaHostOffset;
				stream.Write(IID_ICLRMetaHost, 0, IID_ICLRMetaHost.Length);
				stream.Position = IID_ICLRRuntimeInfoOffset;
				stream.Write(IID_ICLRRuntimeInfo, 0, IID_ICLRRuntimeInfo.Length);
				stream.Position = CLSID_CLRRuntimeHostOffset;
				stream.Write(CLSID_CLRRuntimeHost, 0, CLSID_CLRRuntimeHost.Length);
				stream.Position = IID_ICLRRuntimeHostOffset;
				stream.Write(IID_ICLRRuntimeHost, 0, IID_ICLRRuntimeHost.Length);
				stream.SetLength(stream.Capacity);
				return stream.ToArray();
			}
		}

		private static void WriteMachineCode32v2(byte* p, uint pFunction, uint pCorBindToRuntimeEx) {
			// HRESULT WINAPI LoadCLR2(DWORD *pReturnValue)
			#region {
			p[0] = 0x55;
			p += 1;
			// push ebp
			p[0] = 0x89;
			p[1] = 0xE5;
			p += 2;
			// mov ebp,esp
			p[0] = 0x83;
			p[1] = 0xEC;
			p[2] = 0x44;
			p += 3;
			// sub esp,byte +0x44
			p[0] = 0x53;
			p += 1;
			// push ebx
			p[0] = 0x56;
			p += 1;
			// push esi
			p[0] = 0x57;
			p += 1;
			// push edi
			p[0] = 0xC7;
			p[1] = 0x45;
			p[2] = 0xFC;
			p[3] = 0x00;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p += 7;
			#endregion
			#region ICLRRuntimeHost *pRuntimeHost = nullptr;
			// mov dword [ebp-0x4],0x0
			p[0] = 0x8D;
			p[1] = 0x45;
			p[2] = 0xFC;
			p += 3;
			#endregion
			#region CorBindToRuntimeEx(L"v2.0.50727", nullptr, 0, CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&pRuntimeHost);
			// lea eax,[ebp-0x4]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + IID_ICLRRuntimeHostOffset;
			p += 5;
			// push dword PIID_ICLRRuntimeHost
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + CLSID_CLRRuntimeHostOffset;
			p += 5;
			// push dword pCLSID_CLRRuntimeHost
			p[0] = 0x6A;
			p[1] = 0x00;
			p += 2;
			// push byte +0x0
			p[0] = 0x6A;
			p[1] = 0x00;
			p += 2;
			// push byte +0x0
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + CLRVersionOffset;
			p += 5;
			// push dword pCLRVersion
			p[0] = 0xB9;
			*(uint*)(p + 1) = pCorBindToRuntimeEx;
			p += 5;
			// mov ecx,pCorBindToRuntimeEx
			p[0] = 0xFF;
			p[1] = 0xD1;
			p += 2;
			// call ecx
			#endregion
			#region pRuntimeHost->Start();
			p[0] = 0x8B;
			p[1] = 0x45;
			p[2] = 0xFC;
			p += 3;
			// mov eax,[ebp-0x4]
			p[0] = 0x8B;
			p[1] = 0x08;
			p += 2;
			// mov ecx,[eax]
			p[0] = 0x8B;
			p[1] = 0x55;
			p[2] = 0xFC;
			p += 3;
			// mov edx,[ebp-0x4]
			p[0] = 0x52;
			p += 1;
			// push edx
			p[0] = 0x8B;
			p[1] = 0x41;
			p[2] = 0x0C;
			p += 3;
			// mov eax,[ecx+0xc]
			p[0] = 0xFF;
			p[1] = 0xD0;
			p += 2;
			// call eax
			#endregion
			#region return pRuntimeHost->ExecuteInDefaultAppDomain(L"assemblyPath", L"typeName", L"methodName", L"argument", pReturnValue);
			p[0] = 0x8B;
			p[1] = 0x45;
			p[2] = 0x08;
			p += 3;
			// mov eax,[ebp+0x8]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + ArgumentOffset;
			p += 5;
			// push dword pArgument
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + MethodNameOffset;
			p += 5;
			// push dword pMethodName
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + TypeNameOffset;
			p += 5;
			// push dword pTypeName
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + AssemblyPathOffset;
			p += 5;
			// push dword pAssemblyPath
			p[0] = 0x8B;
			p[1] = 0x4D;
			p[2] = 0xFC;
			p += 3;
			// mov ecx,[ebp-0x4]
			p[0] = 0x8B;
			p[1] = 0x11;
			p += 2;
			// mov edx,[ecx]
			p[0] = 0x8B;
			p[1] = 0x45;
			p[2] = 0xFC;
			p += 3;
			// mov eax,[ebp-0x4]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x8B;
			p[1] = 0x4A;
			p[2] = 0x2C;
			p += 3;
			// mov ecx,[edx+0x2c]
			p[0] = 0xFF;
			p[1] = 0xD1;
			p += 2;
			// call ecx
			#endregion
			#region }
			p[0] = 0x5F;
			p += 1;
			// pop edi
			p[0] = 0x5E;
			p += 1;
			// pop esi
			p[0] = 0x5B;
			p += 1;
			// pop ebx
			p[0] = 0x89;
			p[1] = 0xEC;
			p += 2;
			// mov esp,ebp
			p[0] = 0x5D;
			p += 1;
			// pop ebp
			p[0] = 0xC2;
			p[1] = 0x04;
			p[2] = 0x00;
			// ret 0x4
			#endregion
		}

		private static void WriteMachineCode32v4(byte* p, uint pFunction, uint pCLRCreateInstance) {
			// HRESULT WINAPI LoadCLR4(DWORD *pReturnValue)
			#region {
			p[0] = 0x55;
			p += 1;
			// push ebp
			p[0] = 0x89;
			p[1] = 0xE5;
			p += 2;
			// mov ebp,esp
			p[0] = 0x83;
			p[1] = 0xEC;
			p[2] = 0x4C;
			p += 3;
			// sub esp,byte +0x4c
			p[0] = 0x53;
			p += 1;
			// push ebx
			p[0] = 0x56;
			p += 1;
			// push esi
			p[0] = 0x57;
			p += 1;
			// push edi
			#endregion
			#region ICLRMetaHost *pMetaHost = nullptr;
			p[0] = 0xC7;
			p[1] = 0x45;
			p[2] = 0xFC;
			p[3] = 0x00;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p += 7;
			// mov dword [ebp-0x4],0x0
			#endregion
			#region ICLRRuntimeInfo *pRuntimeInfo = nullptr;
			p[0] = 0xC7;
			p[1] = 0x45;
			p[2] = 0xF8;
			p[3] = 0x00;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p += 7;
			// mov dword [ebp-0x8],0x0
			#endregion
			#region ICLRRuntimeHost *pRuntimeHost = nullptr;
			p[0] = 0xC7;
			p[1] = 0x45;
			p[2] = 0xF4;
			p[3] = 0x00;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p += 7;
			// mov dword [ebp-0xc],0x0
			#endregion
			#region CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
			p[0] = 0x8D;
			p[1] = 0x45;
			p[2] = 0xFC;
			p += 3;
			// lea eax,[ebp-0x4]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + IID_ICLRMetaHostOffset;
			p += 5;
			// push dword pIID_ICLRMetaHost
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + CLSID_CLRMetaHostOffset;
			p += 5;
			// push dword pCLSID_CLRMetaHost
			p[0] = 0xB9;
			*(uint*)(p + 1) = pCLRCreateInstance;
			p += 5;
			// mov ecx,pCLRCreateInstance
			p[0] = 0xFF;
			p[1] = 0xD1;
			p += 2;
			// call ecx
			#endregion
			#region pMetaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&pRuntimeInfo);
			p[0] = 0x8D;
			p[1] = 0x45;
			p[2] = 0xF8;
			p += 3;
			// lea eax,[ebp-0x8]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + IID_ICLRRuntimeInfoOffset;
			p += 5;
			// push dword pIID_ICLRRuntimeInfo
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + CLRVersionOffset;
			p += 5;
			// push dword pCLRVersion
			p[0] = 0x8B;
			p[1] = 0x4D;
			p[2] = 0xFC;
			p += 3;
			// mov ecx,[ebp-0x4]
			p[0] = 0x8B;
			p[1] = 0x11;
			p += 2;
			// mov edx,[ecx]
			p[0] = 0x8B;
			p[1] = 0x45;
			p[2] = 0xFC;
			p += 3;
			// mov eax,[ebp-0x4]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x8B;
			p[1] = 0x4A;
			p[2] = 0x0C;
			p += 3;
			// mov ecx,[edx+0xc]
			p[0] = 0xFF;
			p[1] = 0xD1;
			p += 2;
			// call ecx
			#endregion
			#region pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&pRuntimeHost);
			p[0] = 0x8D;
			p[1] = 0x45;
			p[2] = 0xF4;
			p += 3;
			// lea eax,[ebp-0xc]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + IID_ICLRRuntimeHostOffset;
			p += 5;
			// push dword pIID_ICLRRuntimeHost
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + CLSID_CLRRuntimeHostOffset;
			p += 5;
			// push dword pCLSID_CLRRuntimeHost
			p[0] = 0x8B;
			p[1] = 0x4D;
			p[2] = 0xF8;
			p += 3;
			// mov ecx,[ebp-0x8]
			p[0] = 0x8B;
			p[1] = 0x11;
			p += 2;
			// mov edx,[ecx]
			p[0] = 0x8B;
			p[1] = 0x45;
			p[2] = 0xF8;
			p += 3;
			// mov eax,[ebp-0x8]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x8B;
			p[1] = 0x4A;
			p[2] = 0x24;
			p += 3;
			// mov ecx,[edx+0x24]
			p[0] = 0xFF;
			p[1] = 0xD1;
			p += 2;
			// call ecx
			#endregion
			#region pRuntimeHost->Start();
			p[0] = 0x8B;
			p[1] = 0x45;
			p[2] = 0xF4;
			p += 3;
			// mov eax,[ebp-0xc]
			p[0] = 0x8B;
			p[1] = 0x08;
			p += 2;
			// mov ecx,[eax]
			p[0] = 0x8B;
			p[1] = 0x55;
			p[2] = 0xF4;
			p += 3;
			// mov edx,[ebp-0xc]
			p[0] = 0x52;
			p += 1;
			// push edx
			p[0] = 0x8B;
			p[1] = 0x41;
			p[2] = 0x0C;
			p += 3;
			// mov eax,[ecx+0xc]
			p[0] = 0xFF;
			p[1] = 0xD0;
			p += 2;
			// call eax
			#endregion
			#region return pRuntimeHost->ExecuteInDefaultAppDomain(L"assemblyPath", L"typeName", L"methodName", L"argument", pReturnValue);
			p[0] = 0x8B;
			p[1] = 0x45;
			p[2] = 0x08;
			p += 3;
			// mov eax,[ebp+0x8]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + ArgumentOffset;
			p += 5;
			// push dword pArgument
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + MethodNameOffset;
			p += 5;
			// push dword pMethodName
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + TypeNameOffset;
			p += 5;
			// push dword pTypeName
			p[0] = 0x68;
			*(uint*)(p + 1) = pFunction + AssemblyPathOffset;
			p += 5;
			// push dword pAssemblyPath
			p[0] = 0x8B;
			p[1] = 0x4D;
			p[2] = 0xF4;
			p += 3;
			// mov ecx,[ebp-0xc]
			p[0] = 0x8B;
			p[1] = 0x11;
			p += 2;
			// mov edx,[ecx]
			p[0] = 0x8B;
			p[1] = 0x45;
			p[2] = 0xF4;
			p += 3;
			// mov eax,[ebp-0xc]
			p[0] = 0x50;
			p += 1;
			// push eax
			p[0] = 0x8B;
			p[1] = 0x4A;
			p[2] = 0x2C;
			p += 3;
			// mov ecx,[edx+0x2c]
			p[0] = 0xFF;
			p[1] = 0xD1;
			p += 2;
			// call ecx
			#endregion
			#region }
			p[0] = 0x5F;
			p += 1;
			// pop edi
			p[0] = 0x5E;
			p += 1;
			// pop esi
			p[0] = 0x5B;
			p += 1;
			// pop ebx
			p[0] = 0x89;
			p[1] = 0xEC;
			p += 2;
			// mov esp,ebp
			p[0] = 0x5D;
			p += 1;
			// pop ebp
			p[0] = 0xC2;
			p[1] = 0x04;
			p[2] = 0x00;
			// ret 0x4
			#endregion
		}

		private static void WriteMachineCode64v2(byte* p, ulong pFunction, ulong pCorBindToRuntimeEx) {
			// HRESULT WINAPI LoadCLR2(DWORD *pReturnValue)
			#region {
			p[0] = 0x48;
			p[1] = 0x89;
			p[2] = 0x4C;
			p[3] = 0x24;
			p[4] = 0x08;
			p += 5;
			// mov [rsp+0x8],rcx
			p[0] = 0x55;
			p += 1;
			// push rbp
			p[0] = 0x48;
			p[1] = 0x81;
			p[2] = 0xEC;
			p[3] = 0x80;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p += 7;
			// sub rsp,0x80
			p[0] = 0x48;
			p[1] = 0x8D;
			p[2] = 0x6C;
			p[3] = 0x24;
			p[4] = 0x30;
			p += 5;
			// lea rbp,[rsp+0x30]
			#endregion
			#region ICLRRuntimeHost *pRuntimeHost = nullptr;
			p[0] = 0x48;
			p[1] = 0xC7;
			p[2] = 0x45;
			p[3] = 0x00;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p[7] = 0x00;
			p += 8;
			// mov qword [rbp+0x0],0x0
			#endregion
			#region CorBindToRuntimeEx(L"v2.0.50727", nullptr, 0, CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&pRuntimeHost);
			p[0] = 0x48;
			p[1] = 0x8D;
			p[2] = 0x45;
			p[3] = 0x00;
			p += 4;
			// lea rax,[rbp+0x0]
			p[0] = 0x48;
			p[1] = 0x89;
			p[2] = 0x44;
			p[3] = 0x24;
			p[4] = 0x28;
			p += 5;
			// mov [rsp+0x28],rax
			p[0] = 0x48;
			p[1] = 0xB8;
			*(ulong*)(p + 2) = pFunction + IID_ICLRRuntimeHostOffset;
			p += 10;
			// mov rax,pIID_ICLRRuntimeHost
			p[0] = 0x48;
			p[1] = 0x89;
			p[2] = 0x44;
			p[3] = 0x24;
			p[4] = 0x20;
			p += 5;
			// mov [rsp+0x20],rax
			p[0] = 0x49;
			p[1] = 0xB9;
			*(ulong*)(p + 2) = pFunction + CLSID_CLRRuntimeHostOffset;
			p += 10;
			// mov r9,pCLSID_CLRRuntimeHost
			p[0] = 0x45;
			p[1] = 0x31;
			p[2] = 0xC0;
			p += 3;
			// xor r8d,r8d
			p[0] = 0x31;
			p[1] = 0xD2;
			p += 2;
			// xor edx,edx
			p[0] = 0x48;
			p[1] = 0xB9;
			*(ulong*)(p + 2) = pFunction + CLRVersionOffset;
			p += 10;
			// mov rcx,pCLRVersion
			p[0] = 0x49;
			p[1] = 0xBF;
			*(ulong*)(p + 2) = pCorBindToRuntimeEx;
			p += 10;
			// mov r15,pCorBindToRuntimeEx
			p[0] = 0x41;
			p[1] = 0xFF;
			p[2] = 0xD7;
			p += 3;
			// call r15
			#endregion
			#region pRuntimeHost->Start();
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x45;
			p[3] = 0x00;
			p += 4;
			// mov rax,[rbp+0x0]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x00;
			p += 3;
			// mov rax,[rax]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x4D;
			p[3] = 0x00;
			p += 4;
			// mov rcx,[rbp+0x0]
			p[0] = 0xFF;
			p[1] = 0x50;
			p[2] = 0x18;
			p += 3;
			// call [rax+0x18]
			#endregion
			#region return pRuntimeHost->ExecuteInDefaultAppDomain(L"assemblyPath", L"typeName", L"methodName", L"argument", pReturnValue);
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x45;
			p[3] = 0x00;
			p += 4;
			// mov rax,[rbp+0x0]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x00;
			p += 3;
			// mov rax,[rax]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x4D;
			p[3] = 0x60;
			p += 4;
			// mov rcx,[rbp+0x60]
			p[0] = 0x48;
			p[1] = 0x89;
			p[2] = 0x4C;
			p[3] = 0x24;
			p[4] = 0x28;
			p += 5;
			// mov [rsp+0x28],rcx
			p[0] = 0x48;
			p[1] = 0xB9;
			*(ulong*)(p + 2) = pFunction + ArgumentOffset;
			p += 10;
			// mov rcx,pArgument
			p[0] = 0x48;
			p[1] = 0x89;
			p[2] = 0x4C;
			p[3] = 0x24;
			p[4] = 0x20;
			p += 5;
			// mov [rsp+0x20],rcx
			p[0] = 0x49;
			p[1] = 0xB9;
			*(ulong*)(p + 2) = pFunction + MethodNameOffset;
			p += 10;
			// mov r9,pMethodName
			p[0] = 0x49;
			p[1] = 0xB8;
			*(ulong*)(p + 2) = pFunction + TypeNameOffset;
			p += 10;
			// mov r8,pTypeName
			p[0] = 0x48;
			p[1] = 0xBA;
			*(ulong*)(p + 2) = pFunction + AssemblyPathOffset;
			p += 10;
			// mov rdx,pAssemblyPath
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x4D;
			p[3] = 0x00;
			p += 4;
			// mov rcx,[rbp+0x0]
			p[0] = 0xFF;
			p[1] = 0x50;
			p[2] = 0x58;
			p += 3;
			// call [rax+0x58]
			#endregion
			#region }
			p[0] = 0x48;
			p[1] = 0x8D;
			p[2] = 0x65;
			p[3] = 0x50;
			p += 4;
			// lea rsp,[rbp+0x50]
			p[0] = 0x5D;
			p += 1;
			// pop rbp
			p[0] = 0xC3;
			// ret
			#endregion
		}

		private static void WriteMachineCode64v4(byte* p, ulong pFunction, ulong pCLRCreateInstance) {
			// HRESULT WINAPI LoadCLR4(DWORD *pReturnValue)
			#region {
			p[0] = 0x48;
			p[1] = 0x89;
			p[2] = 0x4C;
			p[3] = 0x24;
			p[4] = 0x08;
			p += 5;
			// mov [rsp+0x8],rcx
			p[0] = 0x55;
			p += 1;
			// push rbp
			p[0] = 0x48;
			p[1] = 0x81;
			p[2] = 0xEC;
			p[3] = 0x90;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p += 7;
			// sub rsp,0x90
			p[0] = 0x48;
			p[1] = 0x8D;
			p[2] = 0x6C;
			p[3] = 0x24;
			p[4] = 0x30;
			p += 5;
			// lea rbp,[rsp+0x30]
			#endregion
			#region ICLRMetaHost *pMetaHost = nullptr;
			p[0] = 0x48;
			p[1] = 0xC7;
			p[2] = 0x45;
			p[3] = 0x00;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p[7] = 0x00;
			p += 8;
			// mov qword [rbp+0x0],0x0
			#endregion
			#region ICLRRuntimeInfo *pRuntimeInfo = nullptr;
			p[0] = 0x48;
			p[1] = 0xC7;
			p[2] = 0x45;
			p[3] = 0x08;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p[7] = 0x00;
			p += 8;
			// mov qword [rbp+0x8],0x0
			#endregion
			#region ICLRRuntimeHost *pRuntimeHost = nullptr;
			p[0] = 0x48;
			p[1] = 0xC7;
			p[2] = 0x45;
			p[3] = 0x10;
			p[4] = 0x00;
			p[5] = 0x00;
			p[6] = 0x00;
			p[7] = 0x00;
			p += 8;
			// mov qword [rbp+0x10],0x0
			#endregion
			#region CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&pMetaHost);
			p[0] = 0x4C;
			p[1] = 0x8D;
			p[2] = 0x45;
			p[3] = 0x00;
			p += 4;
			// lea r8,[rbp+0x0]
			p[0] = 0x48;
			p[1] = 0xBA;
			*(ulong*)(p + 2) = pFunction + IID_ICLRMetaHostOffset;
			p += 10;
			// mov rdx,pIID_ICLRMetaHost
			p[0] = 0x48;
			p[1] = 0xB9;
			*(ulong*)(p + 2) = pFunction + CLSID_CLRMetaHostOffset;
			p += 10;
			// mov rcx,pCLSID_CLRMetaHost
			p[0] = 0x49;
			p[1] = 0xBF;
			*(ulong*)(p + 2) = pCLRCreateInstance;
			p += 10;
			// mov r15,pCLRCreateInstance
			p[0] = 0x41;
			p[1] = 0xFF;
			p[2] = 0xD7;
			p += 3;
			// call r15
			#endregion
			#region pMetaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&pRuntimeInfo);
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x45;
			p[3] = 0x00;
			p += 4;
			// mov rax,[rbp+0x0]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x00;
			p += 3;
			// mov rax,[rax]
			p[0] = 0x4C;
			p[1] = 0x8D;
			p[2] = 0x4D;
			p[3] = 0x08;
			p += 4;
			// lea r9,[rbp+0x8]
			p[0] = 0x49;
			p[1] = 0xB8;
			*(ulong*)(p + 2) = pFunction + IID_ICLRRuntimeInfoOffset;
			p += 10;
			// mov r8,pIID_ICLRRuntimeInfo
			p[0] = 0x48;
			p[1] = 0xBA;
			*(ulong*)(p + 2) = pFunction + CLRVersionOffset;
			p += 10;
			// mov rdx,pCLRVersion
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x4D;
			p[3] = 0x00;
			p += 4;
			// mov rcx,[rbp+0x0]
			p[0] = 0xFF;
			p[1] = 0x50;
			p[2] = 0x18;
			p += 3;
			// call [rax+0x18]
			#endregion
			#region pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&pRuntimeHost);
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x45;
			p[3] = 0x08;
			p += 4;
			// mov rax,[rbp+0x8]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x00;
			p += 3;
			// mov rax,[rax]
			p[0] = 0x4C;
			p[1] = 0x8D;
			p[2] = 0x4D;
			p[3] = 0x10;
			p += 4;
			// lea r9,[rbp+0x10]
			p[0] = 0x49;
			p[1] = 0xB8;
			*(ulong*)(p + 2) = pFunction + IID_ICLRRuntimeHostOffset;
			p += 10;
			// mov r8,pIID_ICLRRuntimeHost
			p[0] = 0x48;
			p[1] = 0xBA;
			*(ulong*)(p + 2) = pFunction + CLSID_CLRRuntimeHostOffset;
			p += 10;
			// mov rdx,pCLSID_CLRRuntimeHost
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x4D;
			p[3] = 0x08;
			p += 4;
			// mov rcx,[rbp+0x8]
			p[0] = 0xFF;
			p[1] = 0x50;
			p[2] = 0x48;
			p += 3;
			// call [rax+0x48]
			#endregion
			#region pRuntimeHost->Start();
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x45;
			p[3] = 0x10;
			p += 4;
			// mov rax,[rbp+0x10]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x00;
			p += 3;
			// mov rax,[rax]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x4D;
			p[3] = 0x10;
			p += 4;
			// mov rcx,[rbp+0x10]
			p[0] = 0xFF;
			p[1] = 0x50;
			p[2] = 0x18;
			p += 3;
			// call [rax+0x18]
			#endregion
			#region return pRuntimeHost->ExecuteInDefaultAppDomain(L"assemblyPath", L"typeName", L"methodName", L"argument", pReturnValue);
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x45;
			p[3] = 0x10;
			p += 4;
			// mov rax,[rbp+0x10]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x00;
			p += 3;
			// mov rax,[rax]
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x4D;
			p[3] = 0x70;
			p += 4;
			// mov rcx,[rbp+0x70]
			p[0] = 0x48;
			p[1] = 0x89;
			p[2] = 0x4C;
			p[3] = 0x24;
			p[4] = 0x28;
			p += 5;
			// mov [rsp+0x28],rcx
			p[0] = 0x48;
			p[1] = 0xB9;
			*(ulong*)(p + 2) = pFunction + ArgumentOffset;
			p += 10;
			// mov rcx,pArgument
			p[0] = 0x48;
			p[1] = 0x89;
			p[2] = 0x4C;
			p[3] = 0x24;
			p[4] = 0x20;
			p += 5;
			// mov [rsp+0x20],rcx
			p[0] = 0x49;
			p[1] = 0xB9;
			*(ulong*)(p + 2) = pFunction + MethodNameOffset;
			p += 10;
			// mov r9,pMethodName
			p[0] = 0x49;
			p[1] = 0xB8;
			*(ulong*)(p + 2) = pFunction + TypeNameOffset;
			p += 10;
			// mov r8,pTypeName
			p[0] = 0x48;
			p[1] = 0xBA;
			*(ulong*)(p + 2) = pFunction + AssemblyPathOffset;
			p += 10;
			// mov rdx,pAssemblyPath
			p[0] = 0x48;
			p[1] = 0x8B;
			p[2] = 0x4D;
			p[3] = 0x10;
			p += 4;
			// mov rcx,[rbp+0x10]
			p[0] = 0xFF;
			p[1] = 0x50;
			p[2] = 0x58;
			p += 3;
			// call [rax+0x58]
			#endregion
			#region }
			p[0] = 0x48;
			p[1] = 0x8D;
			p[2] = 0x65;
			p[3] = 0x60;
			p += 4;
			// lea rsp,[rbp+0x60]
			p[0] = 0x5D;
			p += 1;
			// pop rbp
			p[0] = 0xC3;
			// ret
			#endregion
		}

		private static void IsAssembly(string path, out bool isAssembly, out InjectionClrVersion clrVersion) {
			try {
				using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
					clrVersion = GetVersionString(reader) switch
					{
						CLR_V2 => InjectionClrVersion.V2,
						CLR_V4 => InjectionClrVersion.V4,
						_ => default,
					};
				isAssembly = true;
			}
			catch {
				clrVersion = default;
				isAssembly = false;
			}
		}

		private static string GetVersionString(BinaryReader binaryReader) {
			uint peOffset;
			bool is64Bit;
			SectionHeader[] sectionHeaders;
			uint rva;
			SectionHeader sectionHeader;

			GetPEInfo(binaryReader, out peOffset, out is64Bit);
			binaryReader.BaseStream.Position = peOffset + (is64Bit ? 0xF8 : 0xE8);
			rva = binaryReader.ReadUInt32();
			// .Net Metadata Directory RVA
			if (rva == 0)
				throw new BadImageFormatException("File isn't a valid .NET assembly.");
			sectionHeaders = GetSectionHeaders(binaryReader);
			sectionHeader = GetSectionHeader(rva, sectionHeaders);
			binaryReader.BaseStream.Position = sectionHeader.RawAddress + rva - sectionHeader.VirtualAddress + 0x8;
			// .Net Metadata Directory FileOffset
			rva = binaryReader.ReadUInt32();
			// .Net Metadata RVA
			if (rva == 0)
				throw new BadImageFormatException("File isn't a valid .NET assembly.");
			sectionHeader = GetSectionHeader(rva, sectionHeaders);
			binaryReader.BaseStream.Position = sectionHeader.RawAddress + rva - sectionHeader.VirtualAddress + 0xC;
			// .Net Metadata FileOffset
			return Encoding.UTF8.GetString(binaryReader.ReadBytes(binaryReader.ReadInt32() - 2));
		}

		private static void GetPEInfo(BinaryReader binaryReader, out uint peOffset, out bool is64Bit) {
			ushort machine;

			binaryReader.BaseStream.Position = 0x3C;
			peOffset = binaryReader.ReadUInt32();
			binaryReader.BaseStream.Position = peOffset + 0x4;
			machine = binaryReader.ReadUInt16();
			if (machine != 0x14C && machine != 0x8664)
				throw new BadImageFormatException("Invalid \"Machine\" in FileHeader.");
			is64Bit = machine == 0x8664;
		}

		private static SectionHeader[] GetSectionHeaders(BinaryReader binaryReader) {
			uint ntHeaderOffset;
			bool is64Bit;
			ushort numberOfSections;
			SectionHeader[] sectionHeaders;

			GetPEInfo(binaryReader, out ntHeaderOffset, out is64Bit);
			numberOfSections = binaryReader.ReadUInt16();
			binaryReader.BaseStream.Position = ntHeaderOffset + (is64Bit ? 0x108 : 0xF8);
			sectionHeaders = new SectionHeader[numberOfSections];
			for (int i = 0; i < numberOfSections; i++) {
				binaryReader.BaseStream.Position += 0x8;
				sectionHeaders[i] = new SectionHeader(binaryReader.ReadUInt32(), binaryReader.ReadUInt32(), binaryReader.ReadUInt32(), binaryReader.ReadUInt32());
				binaryReader.BaseStream.Position += 0x10;
			}
			return sectionHeaders;
		}

		private static SectionHeader GetSectionHeader(uint rva, SectionHeader[] sectionHeaders) {
			foreach (SectionHeader sectionHeader in sectionHeaders)
				if (rva >= sectionHeader.VirtualAddress && rva < sectionHeader.VirtualAddress + Math.Max(sectionHeader.VirtualSize, sectionHeader.RawSize))
					return sectionHeader;
			throw new BadImageFormatException("Can't get section from specific RVA.");
		}
	}
}
