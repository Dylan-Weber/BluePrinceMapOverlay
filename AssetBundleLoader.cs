using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.Bindings;

namespace BluePrinceMapOverlay
{
    // WHY: Since the game moved to Unity 6, Unity's managed bindings pass strings to native code
    // as a ManagedSpanWrapper built from a ReadOnlySpan<char>. AssetBundle methods are stripped
    // from the game, so Il2CppInterop "unstrips" them by rewriting the managed bodies, turning
    // System.ReadOnlySpan<char> into Il2CppSystem.ReadOnlySpan<char>, which has no
    // GetPinnableReference in the game's mscorlib. AssetBundle.LoadFromFile and
    // AssetBundle.LoadAsset then throw MissingMethodException. This calls the native icalls
    // directly with a span wrapper over a pinned .NET string.
    internal static unsafe class AssetBundleLoader
    {
        // Mirrors UnityEngine.Bindings.ManagedSpanWrapper (begin at 0, length at 8).
        [StructLayout(LayoutKind.Sequential)]
        private struct SpanWrapper
        {
            public void* Begin;
            public int Length;
        }

        private delegate IntPtr LoadFromFileICall(SpanWrapper* path, uint crc, ulong offset);
        private delegate IntPtr LoadAssetICall(IntPtr self, SpanWrapper* name, IntPtr type);

        private static LoadFromFileICall _loadFromFile;
        private static LoadAssetICall _loadAsset;

        public static AssetBundle LoadFromFile(string path)
        {
            _loadFromFile ??= IL2CPP.ResolveICall<LoadFromFileICall>("UnityEngine.AssetBundle::LoadFromFile_Internal_Injected");

            fixed (char* chars = path)
            {
                SpanWrapper span = new() { Begin = chars, Length = path.Length };
                // The icall returns a GC handle to the managed object, not the object itself.
                IntPtr handle = _loadFromFile(&span, 0, 0);
                return Unmarshal.UnmarshalUnityObject<AssetBundle>(handle);
            }
        }

        public static T LoadAsset<T>(AssetBundle bundle, string name) where T : UnityEngine.Object
        {
            _loadAsset ??= IL2CPP.ResolveICall<LoadAssetICall>("UnityEngine.AssetBundle::LoadAsset_Internal_Injected");

            IntPtr self = bundle.m_CachedPtr;
            if (self == IntPtr.Zero)
            {
                return null;
            }

            fixed (char* chars = name)
            {
                SpanWrapper span = new() { Begin = chars, Length = name.Length };
                IntPtr handle = _loadAsset(self, &span, Il2CppType.Of<T>().Pointer);
                return Unmarshal.UnmarshalUnityObject<UnityEngine.Object>(handle)?.TryCast<T>();
            }
        }
    }
}
