using Org.BouncyCastle.Math.Raw;
using System;

namespace Org.BouncyCastle.Math.EC.Custom.Sec
{
	internal class SecT113Field
	{
		private const ulong M49 = 562949953421311uL;

		private const ulong M57 = 144115188075855871uL;

		public static void Add(ulong[] x, ulong[] y, ulong[] z)
		{
			z[0] = (x[0] ^ y[0]);
			z[1] = (x[1] ^ y[1]);
		}

		public static void AddExt(ulong[] xx, ulong[] yy, ulong[] zz)
		{
			zz[0] = (xx[0] ^ yy[0]);
			zz[1] = (xx[1] ^ yy[1]);
			zz[2] = (xx[2] ^ yy[2]);
			zz[3] = (xx[3] ^ yy[3]);
		}

		public static void AddOne(ulong[] x, ulong[] z)
		{
			z[0] = (x[0] ^ 1uL);
			z[1] = x[1];
		}

		public static ulong[] FromBigInteger(BigInteger x)
		{
			ulong[] array = Nat128.FromBigInteger64(x);
			SecT113Field.Reduce15(array, 0);
			return array;
		}

		public static void Multiply(ulong[] x, ulong[] y, ulong[] z)
		{
			ulong[] array = Nat128.CreateExt64();
			SecT113Field.ImplMultiply(x, y, array);
			SecT113Field.Reduce(array, z);
		}

		public static void MultiplyAddToExt(ulong[] x, ulong[] y, ulong[] zz)
		{
			ulong[] array = Nat128.CreateExt64();
			SecT113Field.ImplMultiply(x, y, array);
			SecT113Field.AddExt(zz, array, zz);
		}

		public static void Reduce(ulong[] xx, ulong[] z)
		{
			ulong num = xx[0];
			ulong num2 = xx[1];
			ulong num3 = xx[2];
			ulong num4 = xx[3];
			num2 ^= (num4 << 15 ^ num4 << 24);
			num3 ^= (num4 >> 49 ^ num4 >> 40);
			num ^= (num3 << 15 ^ num3 << 24);
			num2 ^= (num3 >> 49 ^ num3 >> 40);
			ulong num5 = num2 >> 49;
			z[0] = (num ^ num5 ^ num5 << 9);
			z[1] = (num2 & 562949953421311uL);
		}

		public static void Reduce15(ulong[] z, int zOff)
		{
			ulong num = z[zOff + 1];
			ulong num2 = num >> 49;
			z[zOff] ^= (num2 ^ num2 << 9);
			z[zOff + 1] = (num & 562949953421311uL);
		}

		public static void Square(ulong[] x, ulong[] z)
		{
			ulong[] array = Nat128.CreateExt64();
			SecT113Field.ImplSquare(x, array);
			SecT113Field.Reduce(array, z);
		}

		public static void SquareAddToExt(ulong[] x, ulong[] zz)
		{
			ulong[] array = Nat128.CreateExt64();
			SecT113Field.ImplSquare(x, array);
			SecT113Field.AddExt(zz, array, zz);
		}

		public static void SquareN(ulong[] x, int n, ulong[] z)
		{
			ulong[] array = Nat128.CreateExt64();
			SecT113Field.ImplSquare(x, array);
			SecT113Field.Reduce(array, z);
			while (--n > 0)
			{
				SecT113Field.ImplSquare(z, array);
				SecT113Field.Reduce(array, z);
			}
		}

		protected static void ImplMultiply(ulong[] x, ulong[] y, ulong[] zz)
		{
			ulong num = x[0];
			ulong num2 = x[1];
			num2 = ((num >> 57 ^ num2 << 7) & 144115188075855871uL);
			num &= 144115188075855871uL;
			ulong num3 = y[0];
			ulong num4 = y[1];
			num4 = ((num3 >> 57 ^ num4 << 7) & 144115188075855871uL);
			num3 &= 144115188075855871uL;
			ulong[] array = new ulong[6];
			SecT113Field.ImplMulw(num, num3, array, 0);
			SecT113Field.ImplMulw(num2, num4, array, 2);
			SecT113Field.ImplMulw(num ^ num2, num3 ^ num4, array, 4);
			ulong num5 = array[1] ^ array[2];
			ulong num6 = array[0];
			ulong num7 = array[3];
			ulong num8 = array[4] ^ num6 ^ num5;
			ulong num9 = array[5] ^ num7 ^ num5;
			zz[0] = (num6 ^ num8 << 57);
			zz[1] = (num8 >> 7 ^ num9 << 50);
			zz[2] = (num9 >> 14 ^ num7 << 43);
			zz[3] = num7 >> 21;
		}

		protected static void ImplMulw(ulong x, ulong y, ulong[] z, int zOff)
		{
			ulong[] array = new ulong[8];
			array[1] = y;
			array[2] = array[1] << 1;
			array[3] = (array[2] ^ y);
			array[4] = array[2] << 1;
			array[5] = (array[4] ^ y);
			array[6] = array[3] << 1;
			array[7] = (array[6] ^ y);
			uint num = (uint)x;
			ulong num2 = 0uL;
			ulong num3 = array[(int)((UIntPtr)(num & 7u))];
			int num4 = 48;
			do
			{
				num = (uint)(x >> num4);
				ulong num5 = array[(int)((UIntPtr)(num & 7u))] ^ array[(int)((UIntPtr)(num >> 3 & 7u))] << 3 ^ array[(int)((UIntPtr)(num >> 6 & 7u))] << 6;
				num3 ^= num5 << num4;
				num2 ^= num5 >> -num4;
			}
			while ((num4 -= 9) > 0);
			num2 ^= (x & 72198606942111744uL & y << 7 >> 63) >> 8;
			z[zOff] = (num3 & 144115188075855871uL);
			z[zOff + 1] = (num3 >> 57 ^ num2 << 7);
		}

		protected static void ImplSquare(ulong[] x, ulong[] zz)
		{
			Interleave.Expand64To128(x[0], zz, 0);
			Interleave.Expand64To128(x[1], zz, 2);
		}
	}
}
