using Org.BouncyCastle.Utilities.Encoders;
using System;

namespace Org.BouncyCastle.Math.EC.Custom.Sec
{
	internal class SecP224R1Curve : AbstractFpCurve
	{
		private const int SecP224R1_DEFAULT_COORDS = 2;

		public static readonly BigInteger q = new BigInteger(1, Hex.Decode("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF000000000000000000000001"));

		protected readonly SecP224R1Point m_infinity;

		public virtual BigInteger Q
		{
			get
			{
				return SecP224R1Curve.q;
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
				return SecP224R1Curve.q.BitLength;
			}
		}

		public SecP224R1Curve() : base(SecP224R1Curve.q)
		{
			this.m_infinity = new SecP224R1Point(this, null, null);
			this.m_a = this.FromBigInteger(new BigInteger(1, Hex.Decode("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFE")));
			this.m_b = this.FromBigInteger(new BigInteger(1, Hex.Decode("B4050A850C04B3ABF54132565044B0B7D7BFD8BA270B39432355FFB4")));
			this.m_order = new BigInteger(1, Hex.Decode("FFFFFFFFFFFFFFFFFFFFFFFFFFFF16A2E0B8F03E13DD29455C5C2A3D"));
			this.m_cofactor = BigInteger.One;
			this.m_coord = 2;
		}

		protected override ECCurve CloneCurve()
		{
			return new SecP224R1Curve();
		}

		public override bool SupportsCoordinateSystem(int coord)
		{
			return coord == 2;
		}

		public override ECFieldElement FromBigInteger(BigInteger x)
		{
			return new SecP224R1FieldElement(x);
		}

		protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
		{
			return new SecP224R1Point(this, x, y, withCompression);
		}

		protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
		{
			return new SecP224R1Point(this, x, y, zs, withCompression);
		}
	}
}
