using Org.BouncyCastle.Security;
using System;

namespace Org.BouncyCastle.Crypto.Parameters
{
	public class DsaKeyGenerationParameters : KeyGenerationParameters
	{
		private readonly DsaParameters parameters;

		public DsaParameters Parameters
		{
			get
			{
				return this.parameters;
			}
		}

		public DsaKeyGenerationParameters(SecureRandom random, DsaParameters parameters) : base(random, parameters.P.BitLength - 1)
		{
			this.parameters = parameters;
		}
	}
}
