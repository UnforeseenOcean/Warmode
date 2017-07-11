using Org.BouncyCastle.Utilities.Encoders;
using System;

namespace Org.BouncyCastle.Math.EC.Custom.Sec
{
	internal class SecP160K1Curve : AbstractFpCurve
	{
		private const int SECP160K1_DEFAULT_COORDS = 2;

		public static readonly BigInteger q = SecP160R2Curve.q;

		protected readonly SecP160K1Point m_infinity;

		public virtual BigInteger Q
		{
			get
			{
				return SecP160K1Curve.q;
			}
		}

		public override ECPoint Infinity
		{
			get
			{
				return this.m_infinity;
			}
		}

		public override int FieldSize
		{
			get
			{
				return SecP160K1Curve.q.BitLength;
			}
		}

		public SecP160K1Curve() : base(SecP160K1Curve.q)
		{
			this.m_infinity = new SecP160K1Point(this, null, null);
			this.m_a = this.FromBigInteger(BigInteger.Zero);
			this.m_b = this.FromBigInteger(BigInteger.ValueOf(7L));
			this.m_order = new BigInteger(1, Hex.Decode("0100000000000000000001B8FA16DFAB9ACA16B6B3"));
			this.m_cofactor = BigInteger.One;
			this.m_coord = 2;
		}

		protected override ECCurve CloneCurve()
		{
			return new SecP160K1Curve();
		}

		public override bool SupportsCoordinateSystem(int coord)
		{
			return coord == 2;
		}

		public override ECFieldElement FromBigInteger(BigInteger x)
		{
			return new SecP160R2FieldElement(x);
		}

		protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
		{
			return new SecP160K1Point(this, x, y, withCompression);
		}

		protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		{
			return new SecP160K1Point(this, x, y, zs, withCompression);
		}
	}
}
