using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;

namespace Org.BouncyCastle.Crypto.Generators
{
	public class DHParametersGenerator
	{
		private int size;

		private int certainty;

		private SecureRandom random;

		public virtual void Init(int size, int certainty, SecureRandom random)
		{
			this.size = size;
			this.certainty = certainty;
			this.random = random;
		}

		public virtual DHParameters GenerateParameters()
		{
			BigInteger[] array = DHParametersHelper.GenerateSafePrimes(this.size, this.certainty, this.random);
			BigInteger p = array[0];
			BigInteger q = array[1];
			BigInteger g = DHParametersHelper.SelectGenerator(p, q, this.random);
			return new DHParameters(p, g, q, BigInteger.Two, null);
		}
	}
}
