using Org.BouncyCastle.Math.Raw;
using Org.BouncyCastle.Utilities;
using System;

namespace Org.BouncyCastle.Math.EC.Custom.Sec
{
	internal class SecP160R1FieldElement : ECFieldElement
	{
		public static readonly BigInteger Q = SecP160R1Curve.q;

		protected internal readonly uint[] x;

		public override bool IsZero
		{
			get
			{
				return Nat160.IsZero(this.x);
			}
		}

		public override bool IsOne
		{
			get
			{
				return Nat160.IsOne(this.x);
			}
		}

		public override string FieldName
		{
			get
			{
				return "SecP160R1Field";
			}
		}

		public override int FieldSize
		{
			get
			{
				return SecP160R1FieldElement.Q.BitLength;
			}
		}

		public SecP160R1FieldElement(BigInteger x)
		{
			if (x == null || x.SignValue < 0 || x.CompareTo(SecP160R1FieldElement.Q) >= 0)
			{
				throw new ArgumentException("value invalid for SecP160R1FieldElement", "x");
			}
			this.x = SecP160R1Field.FromBigInteger(x);
		}

		public SecP160R1FieldElement()
		{
			this.x = Nat160.Create();
		}

		protected internal SecP160R1FieldElement(uint[] x)
		{
			this.x = x;
		}

		public override bool TestBitZero()
		{
			return Nat160.GetBit(this.x, 0) == 1u;
		}

		public override BigInteger ToBigInteger()
		{
			return Nat160.ToBigInteger(this.x);
		}

		public override ECFieldElement Add(ECFieldElement b)
		{
			uint[] z = Nat160.Create();
			SecP160R1Field.Add(this.x, ((SecP160R1FieldElement)b).x, z);
			return new SecP160R1FieldElement(z);
		}

		public override ECFieldElement AddOne()
		{
			uint[] z = Nat160.Create();
			SecP160R1Field.AddOne(this.x, z);
			return new SecP160R1FieldElement(z);
		}

		public override ECFieldElement Subtract(ECFieldElement b)
		{
			uint[] z = Nat160.Create();
			SecP160R1Field.Subtract(this.x, ((SecP160R1FieldElement)b).x, z);
			return new SecP160R1FieldElement(z);
		}

		public override ECFieldElement Multiply(ECFieldElement b)
		{
			uint[] z = Nat160.Create();
			SecP160R1Field.Multiply(this.x, ((SecP160R1FieldElement)b).x, z);
			return new SecP160R1FieldElement(z);
		}

		public override ECFieldElement Divide(ECFieldElement b)
		{
			uint[] z = Nat160.Create();
			Mod.Invert(SecP160R1Field.P, ((SecP160R1FieldElement)b).x, z);
			SecP160R1Field.Multiply(z, this.x, z);
			return new SecP160R1FieldElement(z);
		}

		public override ECFieldElement Negate()
		{
			uint[] z = Nat160.Create();
			SecP160R1Field.Negate(this.x, z);
			return new SecP160R1FieldElement(z);
		}

		public override ECFieldElement Square()
		{
			uint[] z = Nat160.Create();
			SecP160R1Field.Square(this.x, z);
			return new SecP160R1FieldElement(z);
		}

		public override ECFieldElement Invert()
		{
			uint[] z = Nat160.Create();
			Mod.Invert(SecP160R1Field.P, this.x, z);
			return new SecP160R1FieldElement(z);
		}

		public override ECFieldElement Sqrt()
		{
			uint[] y = this.x;
			if (Nat160.IsZero(y) || Nat160.IsOne(y))
			{
				return this;
			}
			uint[] array = Nat160.Create();
			SecP160R1Field.Square(y, array);
			SecP160R1Field.Multiply(array, y, array);
			uint[] array2 = Nat160.Create();
			SecP160R1Field.SquareN(array, 2, array2);
			SecP160R1Field.Multiply(array2, array, array2);
			uint[] array3 = array;
			SecP160R1Field.SquareN(array2, 4, array3);
			SecP160R1Field.Multiply(array3, array2, array3);
			uint[] array4 = array2;
			SecP160R1Field.SquareN(array3, 8, array4);
			SecP160R1Field.Multiply(array4, array3, array4);
			uint[] array5 = array3;
			SecP160R1Field.SquareN(array4, 16, array5);
			SecP160R1Field.Multiply(array5, array4, array5);
			uint[] array6 = array4;
			SecP160R1Field.SquareN(array5, 32, array6);
			SecP160R1Field.Multiply(array6, array5, array6);
			uint[] array7 = array5;
			SecP160R1Field.SquareN(array6, 64, array7);
			SecP160R1Field.Multiply(array7, array6, array7);
			uint[] array8 = array6;
			SecP160R1Field.Square(array7, array8);
			SecP160R1Field.Multiply(array8, y, array8);
			uint[] z = array8;
			SecP160R1Field.SquareN(z, 29, z);
			uint[] array9 = array7;
			SecP160R1Field.Square(z, array9);
			if (!Nat160.Eq(y, array9))
			{
				return null;
			}
			return new SecP160R1FieldElement(z);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as SecP160R1FieldElement);
		}

		public override bool Equals(ECFieldElement other)
		{
			return this.Equals(other as SecP160R1FieldElement);
		}

		public virtual bool Equals(SecP160R1FieldElement other)
		{
			return this == other || (other != null && Nat160.Eq(this.x, other.x));
		}

		public override int GetHashCode()
		{
			return SecP160R1FieldElement.Q.GetHashCode() ^ Arrays.GetHashCode(this.x, 0, 5);
		}
	}
}
