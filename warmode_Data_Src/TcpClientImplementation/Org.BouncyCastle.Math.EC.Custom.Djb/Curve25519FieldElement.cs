using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using System;

namespace Org.BouncyCastle.Math.EC.Custom.Djb
{
	internal class Curve25519FieldElement : ECFieldElement
	{
		public static readonly BigInteger Q = Curve25519.q;

		private static readonly uint[] PRECOMP_POW2 = new uint[]
		{
			1242472624u,
			3303938855u,
			2905597048u,
			792926214u,
			1039914919u,
			726466713u,
			1338105611u,
			730014848u
		};

		protected internal readonly uint[] x;

		public override bool IsZero
		{
			get
			{
				return Nat256.IsZero(this.x);
			}
		}

		public override bool IsOne
		{
			get
			{
				return Nat256.IsOne(this.x);
			}
		}

		public override string FieldName
		{
			get
			{
				return "Curve25519Field";
			}
		}

		public override int FieldSize
		{
			get
			{
				return Curve25519FieldElement.Q.BitLength;
			}
		}

		public Curve25519FieldElement(BigInteger x)
		{
			if (x == null || x.SignValue < 0 || x.CompareTo(Curve25519FieldElement.Q) >= 0)
			{
				throw new ArgumentException("value invalid for Curve25519FieldElement", "x");
			}
			this.x = Curve25519Field.FromBigInteger(x);
		}

		public Curve25519FieldElement()
		{
			this.x = Nat256.Create();
		}

		protected internal Curve25519FieldElement(uint[] x)
		{
			this.x = x;
		}

		public override bool TestBitZero()
		{
			return Nat256.GetBit(this.x, 0) == 1u;
		}

		public override BigInteger ToBigInteger()
		{
			return Nat256.ToBigInteger(this.x);
		}

		public override ECFieldElement Add(ECFieldElement b)
		{
			uint[] z = Nat256.Create();
			Curve25519Field.Add(this.x, ((Curve25519FieldElement)b).x, z);
			return new Curve25519FieldElement(z);
		}

		public override ECFieldElement AddOne()
		{
			uint[] z = Nat256.Create();
			Curve25519Field.AddOne(this.x, z);
			return new Curve25519FieldElement(z);
		}

		public override ECFieldElement Subtract(ECFieldElement b)
		{
			uint[] z = Nat256.Create();
			Curve25519Field.Subtract(this.x, ((Curve25519FieldElement)b).x, z);
			return new Curve25519FieldElement(z);
		}

		public override ECFieldElement Multiply(ECFieldElement b)
		{
			uint[] z = Nat256.Create();
			Curve25519Field.Multiply(this.x, ((Curve25519FieldElement)b).x, z);
			return new Curve25519FieldElement(z);
		}

		public override ECFieldElement Divide(ECFieldElement b)
		{
			uint[] z = Nat256.Create();
			Mod.Invert(Curve25519Field.P, ((Curve25519FieldElement)b).x, z);
			Curve25519Field.Multiply(z, this.x, z);
			return new Curve25519FieldElement(z);
		}

		public override ECFieldElement Negate()
		{
			uint[] z = Nat256.Create();
			Curve25519Field.Negate(this.x, z);
			return new Curve25519FieldElement(z);
		}

		public override ECFieldElement Square()
		{
			uint[] z = Nat256.Create();
			Curve25519Field.Square(this.x, z);
			return new Curve25519FieldElement(z);
		}

		public override ECFieldElement Invert()
		{
			uint[] z = Nat256.Create();
			Mod.Invert(Curve25519Field.P, this.x, z);
			return new Curve25519FieldElement(z);
		}

		public override ECFieldElement Sqrt()
		{
			uint[] y = this.x;
			if (Nat256.IsZero(y) || Nat256.IsOne(y))
			{
				return this;
			}
			uint[] array = Nat256.Create();
			Curve25519Field.Square(y, array);
			Curve25519Field.Multiply(array, y, array);
			uint[] array2 = array;
			Curve25519Field.Square(array, array2);
			Curve25519Field.Multiply(array2, y, array2);
			uint[] array3 = Nat256.Create();
			Curve25519Field.Square(array2, array3);
			Curve25519Field.Multiply(array3, y, array3);
			uint[] array4 = Nat256.Create();
			Curve25519Field.SquareN(array3, 3, array4);
			Curve25519Field.Multiply(array4, array2, array4);
			uint[] array5 = array2;
			Curve25519Field.SquareN(array4, 4, array5);
			Curve25519Field.Multiply(array5, array3, array5);
			uint[] array6 = array4;
			Curve25519Field.SquareN(array5, 4, array6);
			Curve25519Field.Multiply(array6, array3, array6);
			uint[] array7 = array3;
			Curve25519Field.SquareN(array6, 15, array7);
			Curve25519Field.Multiply(array7, array6, array7);
			uint[] array8 = array6;
			Curve25519Field.SquareN(array7, 30, array8);
			Curve25519Field.Multiply(array8, array7, array8);
			uint[] array9 = array7;
			Curve25519Field.SquareN(array8, 60, array9);
			Curve25519Field.Multiply(array9, array8, array9);
			uint[] z = array8;
			Curve25519Field.SquareN(array9, 11, z);
			Curve25519Field.Multiply(z, array5, z);
			uint[] array10 = array5;
			Curve25519Field.SquareN(z, 120, array10);
			Curve25519Field.Multiply(array10, array9, array10);
			uint[] z2 = array10;
			Curve25519Field.Square(z2, z2);
			uint[] array11 = array9;
			Curve25519Field.Square(z2, array11);
			if (Nat256.Eq(y, array11))
			{
				return new Curve25519FieldElement(z2);
			}
			Curve25519Field.Multiply(z2, Curve25519FieldElement.PRECOMP_POW2, z2);
			Curve25519Field.Square(z2, array11);
			if (Nat256.Eq(y, array11))
			{
				return new Curve25519FieldElement(z2);
			}
			return null;
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as Curve25519FieldElement);
		}

		public override bool Equals(ECFieldElement other)
		{
			return this.Equals(other as Curve25519FieldElement);
		}

		public virtual bool Equals(Curve25519FieldElement other)
		{
			return this == other || (other != null && Nat256.Eq(this.x, other.x));
		}

		public override int GetHashCode()
		{
			return Curve25519FieldElement.Q.GetHashCode() ^ Arrays.GetHashCode(this.x, 0, 8);
		}
	}
}
