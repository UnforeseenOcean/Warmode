using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;

namespace Org.BouncyCastle.Crypto
{
	public class BufferedAeadBlockCipher : BufferedCipherBase
	{
		private readonly IAeadBlockCipher cipher;

		public override string AlgorithmName
		{
			get
			{
				return this.cipher.AlgorithmName;
			}
		}

		public BufferedAeadBlockCipher(IAeadBlockCipher cipher)
		{
			if (cipher == null)
			{
				throw new ArgumentNullException("cipher");
			}
			this.cipher = cipher;
		}

		public override void Init(bool forEncryption, ICipherParameters parameters)
		{
			if (parameters is ParametersWithRandom)
			{
				parameters = ((ParametersWithRandom)parameters).Parameters;
			}
			this.cipher.Init(forEncryption, parameters);
		}

		public override int GetBlockSize()
		{
			return this.cipher.GetBlockSize();
		}

		public override int GetUpdateOutputSize(int length)
		{
			return this.cipher.GetUpdateOutputSize(length);
		}

		public override int GetOutputSize(int length)
		{
			return this.cipher.GetOutputSize(length);
		}

		public override int ProcessByte(byte input, byte[] output, int outOff)
		{
			return this.cipher.ProcessByte(input, output, outOff);
		}

		public override byte[] ProcessByte(byte input)
		{
			int updateOutputSize = this.GetUpdateOutputSize(1);
			byte[] array = (updateOutputSize > 0) ? new byte[updateOutputSize] : null;
			int num = this.ProcessByte(input, array, 0);
			if (updateOutputSize > 0 && num < updateOutputSize)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, 0, array2, 0, num);
				array = array2;
			}
			return array;
		}

		public override byte[] ProcessBytes(byte[] input, int inOff, int length)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (length < 1)
			{
				return null;
			}
			int updateOutputSize = this.GetUpdateOutputSize(length);
			byte[] array = (updateOutputSize > 0) ? new byte[updateOutputSize] : null;
			int num = this.ProcessBytes(input, inOff, length, array, 0);
			if (updateOutputSize > 0 && num < updateOutputSize)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, 0, array2, 0, num);
				array = array2;
			}
			return array;
		}

		public override int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
		{
			return this.cipher.ProcessBytes(input, inOff, length, output, outOff);
		}

		public override byte[] DoFinal()
		{
			byte[] array = new byte[this.GetOutputSize(0)];
			int num = this.DoFinal(array, 0);
			if (num < array.Length)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, 0, array2, 0, num);
				array = array2;
			}
			return array;
		}

		public override byte[] DoFinal(byte[] input, int inOff, int inLen)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			byte[] array = new byte[this.GetOutputSize(inLen)];
			int num = (inLen > 0) ? this.ProcessBytes(input, inOff, inLen, array, 0) : 0;
			num += this.DoFinal(array, num);
			if (num < array.Length)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, 0, array2, 0, num);
				array = array2;
			}
			return array;
		}

		public override int DoFinal(byte[] output, int outOff)
		{
			return this.cipher.DoFinal(output, outOff);
		}

		public override void Reset()
		{
			this.cipher.Reset();
		}
	}
}
