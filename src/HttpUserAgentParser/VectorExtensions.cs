// Copyright © https://myCSharp.de - all rights reserved

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace MyCSharp.HttpUserAgentParser;

internal static class VectorExtensions
{
    extension(ref char c)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<byte> ReadVector128AsBytes(int offset)
        {
            ref short ptr = ref Unsafe.As<char, short>(ref c);

#if NET10_0_OR_GREATER
            return Vector128.NarrowWithSaturation(
                Vector128.LoadUnsafe(ref ptr, (uint)offset),
                Vector128.LoadUnsafe(ref ptr, (uint)(offset + Vector128<short>.Count))
            ).AsByte();
#else
            if (Sse2.IsSupported)
            {
                return Sse2.PackUnsignedSaturate(
                    Vector128.LoadUnsafe(ref ptr, (uint)offset),
                    Vector128.LoadUnsafe(ref ptr, (uint)(offset + Vector128<short>.Count)));
            }
            else if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.UnzipEven(
                    Vector128.LoadUnsafe(ref ptr, (uint)offset).AsByte(),
                    Vector128.LoadUnsafe(ref ptr, (uint)(offset + Vector128<short>.Count)).AsByte());
            }
            else
            {
                return Vector128.Narrow(
                    Vector128.LoadUnsafe(ref ptr, (uint)offset),
                    Vector128.LoadUnsafe(ref ptr, (uint)(offset + Vector128<short>.Count))
                ).AsByte();
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector256<byte> ReadVector256AsBytes(int offset)
        {
            ref short ptr = ref Unsafe.As<char, short>(ref c);

#if NET10_0_OR_GREATER
            return Vector256.NarrowWithSaturation(
                Vector256.LoadUnsafe(ref ptr, (uint)offset),
                Vector256.LoadUnsafe(ref ptr, (uint)offset + (uint)Vector256<short>.Count)
            ).AsByte();
#else
            if (Avx2.IsSupported)
            {
                Vector256<byte> tmp = Avx2.PackUnsignedSaturate(
                    Vector256.LoadUnsafe(ref ptr, (uint)offset),
                    Vector256.LoadUnsafe(ref ptr, (uint)offset + (uint)Vector256<short>.Count));

                Vector256<long> tmp1 = Avx2.Permute4x64(tmp.AsInt64(), 0b_11_01_10_00);

                return tmp1.AsByte();
            }
            else
            {
                return Vector256.Narrow(
                    Vector256.LoadUnsafe(ref ptr, (uint)offset),
                    Vector256.LoadUnsafe(ref ptr, (uint)offset + (uint)Vector256<short>.Count)
                ).AsByte();
            }
#endif
        }
    }
}
