using Org.BouncyCastle.Security;
using System;

namespace Org.BouncyCastle.Crypto.Paddings
{
	public interface IBlockCipherPadding
	{
		string PaddingName
		{
			get;
		}

		void Init(SecureRandom random);

		int AddPadding(byte[] input, int inOff);

		int PadCount(byte[] input);
	}
}
