using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using System;

namespace Org.BouncyCastle.Math.EC.Custom.Sec
{
	internal class SecP384R1FieldElement : ECFieldElement
	{
		public static readonly BigInteger Q = SecP384R1Curve.q;

		protected internal readonly uint[] x;

		public override bool IsZero
		{
			get
			{
				return Nat.IsZero(12, this.x);
			}
		}

		public override bool IsOne
		{
			get
			{
				return Nat.IsOne(12, this.x);
			}
		}

		public override string FieldName
		{
			get
			{
				return "SecP384R1Field";
			}
		}

		public override int FieldSize
		{
			get
			{
				return SecP384R1FieldElement.Q.BitLength;
			}
		}

		public SecP384R1FieldElement(BigInteger x)
		{
			if (x == null || x.SignValue < 0 || x.CompareTo(SecP384R1FieldElement.Q) >= 0)
			{
				throw new ArgumentException("value invalid for SecP384R1FieldElement", "x");
			}
			this.x = SecP384R1Field.FromBigInteger(x);
		}

		public SecP384R1FieldElement()
		{
			this.x = Nat.Create(12);
		}

		protected internal SecP384R1FieldElement(uint[] x)
		{
			this.x = x;
		}

		public override bool TestBitZero()
		{
			return Nat.GetBit(this.x, 0) == 1u;
		}

		public override BigInteger ToBigInteger()
		{
			return Nat.ToBigInteger(12, this.x);
		}

		public override ECFieldElement Add(ECFieldElement b)
		{
			uint[] z = Nat.Create(12);
			SecP384R1Field.Add(this.x, ((SecP384R1FieldElement)b).x, z);
			return new SecP384R1FieldElement(z);
		}

		public override ECFieldElement AddOne()
		{
			uint[] z = Nat.Create(12);
			SecP384R1Field.AddOne(this.x, z);
			return new SecP384R1FieldElement(z);
		}

		public override ECFieldElement Subtract(ECFieldElement b)
		{
			uint[] z = Nat.Create(12);
			SecP384R1Field.Subtract(this.x, ((SecP384R1FieldElement)b).x, z);
			return new SecP384R1FieldElement(z);
		}

		public override ECFieldElement Multiply(ECFieldElement b)
		{
			uint[] z = Nat.Create(12);
			SecP384R1Field.Multiply(this.x, ((SecP384R1FieldElement)b).x, z);
			return new SecP384R1FieldElement(z);
		}

		public override ECFieldElement Divide(ECFieldElement b)
		{
			uint[] z = Nat.Create(12);
			Mod.Invert(SecP384R1Field.P, ((SecP384R1FieldElement)b).x, z);
			SecP384R1Field.Multiply(z, this.x, z);
			return new SecP384R1FieldElement(z);
		}

		public override ECFieldElement Negate()
		{
			uint[] z = Nat.Create(12);
			SecP384R1Field.Negate(this.x, z);
			return new SecP384R1FieldElement(z);
		}

		public override ECFieldElement Square()
		{
			uint[] z = Nat.Create(12);
			SecP384R1Field.Square(this.x, z);
			return new SecP384R1FieldElement(z);
		}

		public override ECFieldElement Invert()
		{
			uint[] z = Nat.Create(12);
			Mod.Invert(SecP384R1Field.P, this.x, z);
			return new SecP384R1FieldElement(z);
		}

		public override ECFieldElement Sqrt()
		{
			uint[] y = this.x;
			if (Nat.IsZero(12, y) || Nat.IsOne(12, y))
			{
				return this;
			}
			uint[] array = Nat.Create(12);
			uint[] array2 = Nat.Create(12);
			uint[] array3 = Nat.Create(12);
			uint[] array4 = Nat.Create(12);
			SecP384R1Field.Square(y, array);
			SecP384R1Field.Multiply(array, y, array);
			SecP384R1Field.SquareN(array, 2, array2);
			SecP384R1Field.Multiply(array2, array, array2);
			SecP384R1Field.Square(array2, array2);
			SecP384R1Field.Multiply(array2, y, array2);
			SecP384R1Field.SquareN(array2, 5, array3);
			SecP384R1Field.Multiply(array3, array2, array3);
			SecP384R1Field.SquareN(array3, 5, array4);
			SecP384R1Field.Multiply(array4, array2, array4);
			SecP384R1Field.SquareN(array4, 15, array2);
			SecP384R1Field.Multiply(array2, array4, array2);
			SecP384R1Field.SquareN(array2, 2, array3);
			SecP384R1Field.Multiply(array, array3, array);
			SecP384R1Field.SquareN(array3, 28, array3);
			SecP384R1Field.Multiply(array2, array3, array2);
			SecP384R1Field.SquareN(array2, 60, array3);
			SecP384R1Field.Multiply(array3, array2, array3);
			uint[] z = array2;
			SecP384R1Field.SquareN(array3, 120, z);
			SecP384R1Field.Multiply(z, array3, z);
			SecP384R1Field.SquareN(z, 15, z);
			SecP384R1Field.Multiply(z, array4, z);
			SecP384R1Field.SquareN(z, 33, z);
			SecP384R1Field.Multiply(z, array, z);
			SecP384R1Field.SquareN(z, 64, z);
			SecP384R1Field.Multiply(z, y, z);
			SecP384R1Field.SquareN(z, 30, array);
			SecP384R1Field.Square(array, array2);
			if (!Nat.Eq(12, y, array2))
			{
				return null;
			}
			return new SecP384R1FieldElement(array);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as SecP384R1FieldElement);
		}

		public override bool Equals(ECFieldElement other)
		{
			return this.Equals(other as SecP384R1FieldElement);
		}

		public virtual bool Equals(SecP384R1FieldElement other)
		{
			return this == other || (other != null && Nat.Eq(12, this.x, other.x));
		}

		public override int GetHashCode()
		{
			return SecP384R1FieldElement.Q.GetHashCode() ^ Arrays.GetHashCode(this.x, 0, 12);
		}
	}
}
