using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using System;

namespace Org.BouncyCastle.Cms
{
	internal interface CmsSecureReadable
	{
		AlgorithmIdentifier Algorithm
		{
			get;
		}

		object CryptoObject
		{
			get;
		}

		CmsReadable GetReadable(KeyParameter key);
	}
}
