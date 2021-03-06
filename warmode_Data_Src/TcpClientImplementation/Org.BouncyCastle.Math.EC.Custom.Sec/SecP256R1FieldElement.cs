using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using System;

namespace Org.BouncyCastle.Math.EC.Custom.Sec
{
	internal class SecP256R1FieldElement : ECFieldElement
	{
		public static readonly BigInteger Q = SecP256R1Curve.q;

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
				return "SecP256R1Field";
			}
		}

		public override int FieldSize
		{
			get
			{
				return SecP256R1FieldElement.Q.BitLength;
			}
		}

		public SecP256R1FieldElement(BigInteger x)
		{
			if (x == null || x.SignValue < 0 || x.CompareTo(SecP256R1FieldElement.Q) >= 0)
			{
				throw new ArgumentException("value invalid for SecP256R1FieldElement", "x");
			}
			this.x = SecP256R1Field.FromBigInteger(x);
		}

		public SecP256R1FieldElement()
		{
			this.x = Nat256.Create();
		}

		protected internal SecP256R1FieldElement(uint[] x)
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
			SecP256R1Field.Add(this.x, ((SecP256R1FieldElement)b).x, z);
			return new SecP256R1FieldElement(z);
		}

		public override ECFieldElement AddOne()
		{
			uint[] z = Nat256.Create();
			SecP256R1Field.AddOne(this.x, z);
			return new SecP256R1FieldElement(z);
		}

		public override ECFieldElement Subtract(ECFieldElement b)
		{
			uint[] z = Nat256.Create();
			SecP256R1Field.Subtract(this.x, ((SecP256R1FieldElement)b).x, z);
			return new SecP256R1FieldElement(z);
		}

		public override ECFieldElement Multiply(ECFieldElement b)
		{
			uint[] z = Nat256.Create();
			SecP256R1Field.Multiply(this.x, ((SecP256R1FieldElement)b).x, z);
			return new SecP256R1FieldElement(z);
		}

		public override ECFieldElement Divide(ECFieldElement b)
		{
			uint[] z = Nat256.Create();
			Mod.Invert(SecP256R1Field.P, ((SecP256R1FieldElement)b).x, z);
			SecP256R1Field.Multiply(z, this.x, z);
			return new SecP256R1FieldElement(z);
		}

		public override ECFieldElement Negate()
		{
			uint[] z = Nat256.Create();
			SecP256R1Field.Negate(this.x, z);
			return new SecP256R1FieldElement(z);
		}

		public override ECFieldElement Square()
		{
			uint[] z = Nat256.Create();
			SecP256R1Field.Square(this.x, z);
			return new SecP256R1FieldElement(z);
		}

		public override ECFieldElement Invert()
		{
			uint[] z = Nat256.Create();
			Mod.Invert(SecP256R1Field.P, this.x, z);
			return new SecP256R1FieldElement(z);
		}

		public override ECFieldElement Sqrt()
		{
			uint[] y = this.x;
			if (Nat256.IsZero(y) || Nat256.IsOne(y))
			{
				return this;
			}
			uint[] array = Nat256.Create();
			uint[] array2 = Nat256.Create();
			SecP256R1Field.Square(y, array);
			SecP256R1Field.Multiply(array, y, array);
			SecP256R1Field.SquareN(array, 2, array2);
			SecP256R1Field.Multiply(array2, array, array2);
			SecP256R1Field.SquareN(array2, 4, array);
			SecP256R1Field.Multiply(array, array2, array);
			SecP256R1Field.SquareN(array, 8, array2);
			SecP256R1Field.Multiply(array2, array, array2);
			SecP256R1Field.SquareN(array2, 16, array);
			SecP256R1Field.Multiply(array, array2, array);
			SecP256R1Field.SquareN(array, 32, array);
			SecP256R1Field.Multiply(array, y, array);
			SecP256R1Field.SquareN(array, 96, array);
			SecP256R1Field.Multiply(array, y, array);
			SecP256R1Field.SquareN(array, 94, array);
			SecP256R1Field.Multiply(array, array, array2);
			if (!Nat256.Eq(y, array2))
			{
				return null;
			}
			return new SecP256R1FieldElement(array);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as SecP256R1FieldElement);
		}

		public override bool Equals(ECFieldElement other)
		{
			return this.Equals(other as SecP256R1FieldElement);
		}

		public virtual bool Equals(SecP256R1FieldElement other)
		{
			return this == other || (other != null && Nat256.Eq(this.x, other.x));
		}

		public override int GetHashCode()
		{
			return SecP256R1FieldElement.Q.GetHashCode() ^ Arrays.GetHashCode(this.x, 0, 8);
		}
	}
}
