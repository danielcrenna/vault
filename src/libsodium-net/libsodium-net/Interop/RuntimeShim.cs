using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Sodium.Interop
{
  internal static class RuntimeShim
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
    {
#if NETSTANDARD1_3
      Array.Copy(sourceArray, (int)sourceIndex, destinationArray, (int)destinationIndex, (int)length);
#else
      Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
#endif
    }

#if NETSTANDARD1_3
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long GetLongLength(this Array array, int dimension)
    {
      return array.GetLength(dimension);
    }
#endif

    [Conditional("NET46")]
    internal static void ProtectMemory(byte[] data)
    {
#if NET46
      if (!SodiumRuntimeConfig.IsRunningOnMono)
      {
        ProtectedMemory.Protect(data, MemoryProtectionScope.SameProcess);
      }
#endif
    }

    [Conditional("NET46")]
    internal static void UnprotectMemory(byte[] data)
    {
#if NET46
      if (!SodiumRuntimeConfig.IsRunningOnMono)
      {
        ProtectedMemory.Unprotect(data, MemoryProtectionScope.SameProcess);
      }
#endif
    }

    [Conditional("NET46")]
    internal static void PinDllImportLibrary(string library)
    {
#if NET46
      if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        return;
      }

      // mimic LoadLibraryEx behaviour,
      // see https://github.com/dotnet/coreclr/issues/8902
      var dllName = Path.GetFileName(library);
      if (!dllName.Contains("."))
      {
        library += ".dll";
        dllName += ".dll";
      }

      string dllPath;
      if (!Path.IsPathRooted(library))
      {
        var appDirectory = AppContext.BaseDirectory;
        if (File.Exists(Path.Combine(appDirectory, library)))
        {
          return;
        }
        dllPath = Path.Combine(appDirectory, SodiumRuntimeConfig.ArchitectureDirectory, library);
      }
      else
      {
        dllPath = library;
      }

      if (Windows.LoadLibraryEx(dllPath, IntPtr.Zero, LoadLibraryExFlags.LoadWithAlteredSearchPath) == IntPtr.Zero)
      {
        var cause = GetLastWin32ErrorAsException();
        throw new DllNotFoundException($"Failed to load the native library \"{library}\".", cause);
      }

      // prevent accidental unloading
      var handle = IntPtr.Zero;
      if (Windows.GetModuleHandleEx(GetModuleHandleExFlags.Pin, dllName, ref handle) == 0)
      {
        var cause = GetLastWin32ErrorAsException();
        throw new DllNotFoundException($"Failed to pin the native library \"{library}\".", cause);
      }
#endif
    }

    public static void ThrowLastWin32Error()
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
      }
    }

    public static Exception GetLastWin32ErrorAsException()
    {
      try
      {
        ThrowLastWin32Error();
      }
      catch (Exception e)
      {
        return e;
      }
      return null;
    }

    private static class Windows
    {
      [DllImport("kernel32.dll", SetLastError = true)]
      internal static extern IntPtr LoadLibraryEx(string filename, IntPtr reserved, LoadLibraryExFlags flags);

      [DllImport("kernel32.dll", SetLastError = true)]
      internal static extern int GetModuleHandleEx(GetModuleHandleExFlags flags, string lpModuleName, ref IntPtr handle);
    }

    [Flags]
    private enum GetModuleHandleExFlags : uint
    {
      None              = 0x00_00_00_00,
      Pin               = 0x00_00_00_01,
      UnchangedRefCount = 0x00_00_00_02,
      FromAddress       = 0x00_00_00_04,
    }

    [Flags]
    private enum LoadLibraryExFlags : uint
    {
      None                            = 0x00_00_00_00,
      DontResolveDllReferences        = 0x00_00_00_01,
      LoadIgnoreCodeAuthzLevel        = 0x00_00_00_10,
      LoadLibraryAsDatafile           = 0x00_00_00_02,
      LoadLibraryAsDatafileExclusive  = 0x00_00_00_40,
      LoadLibraryAsImageResource      = 0x00_00_00_20,
      LoadLibrarySearchApplicationDir = 0x00_00_02_00,
      LoadLibrarySearchDefaultDirs    = 0x00_00_10_00,
      LoadLibrarySearchDllLoadDir     = 0x00_00_01_00,
      LoadLibrarySearchSystem32       = 0x00_00_08_00,
      LoadLibrarySearchUserDirs       = 0x00_00_04_00,
      LoadWithAlteredSearchPath       = 0x00_00_00_08,
    }
  }
}
