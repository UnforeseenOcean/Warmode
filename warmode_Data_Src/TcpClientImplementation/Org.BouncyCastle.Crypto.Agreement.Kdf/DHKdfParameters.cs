using Org.BouncyCastle.Asn1;
using System;

namespace Org.BouncyCastle.Crypto.Agreement.Kdf
{
	public class DHKdfParameters : IDerivationParameters
	{
		private readonly DerObjectIdentifier algorithm;

		private readonly int keySize;

		private readonly byte[] z;

		private readonly byte[] extraInfo;

		public DerObjectIdentifier Algorithm
		{
			get
			{
				return this.algorithm;
			}
		}

		public int KeySize
		{
			get
			{
				return this.keySize;
			}
		}

		public DHKdfParameters(DerObjectIdentifier algorithm, int keySize, byte[] z) : this(algorithm, keySize, z, null)
		{
		}

		public DHKdfParameters(DerObjectIdentifier algorithm, int keySize, byte[] z, byte[] extraInfo)
		{
			this.algorithm = algorithm;
			this.keySize = keySize;
			this.z = z;
			this.extraInfo = extraInfo;
		}

		public byte[] GetZ()
		{
			return this.z;
		}

		public byte[] GetExtraInfo()
		{
			return this.extraInfo;
		}
	}
}
