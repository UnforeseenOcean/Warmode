using System;

namespace Org.BouncyCastle.Crypto
{
	public abstract class BufferedCipherBase : IBufferedCipher
	{
		protected static readonly byte[] EmptyBuffer = new byte[0];

		public abstract string AlgorithmName
		{
			get;
		}

		public abstract void Init(bool forEncryption, ICipherParameters parameters);

		public abstract int GetBlockSize();

		public abstract int GetOutputSize(int inputLen);

		public abstract int GetUpdateOutputSize(int inputLen);

		public abstract byte[] ProcessByte(byte input);

		public virtual int ProcessByte(byte input, byte[] output, int outOff)
		{
			byte[] array = this.ProcessByte(input);
			if (array == null)
			{
				return 0;
			}
			if (outOff + array.Length > output.Length)
			{
				throw new DataLengthException("output buffer too short");
			}
			array.CopyTo(output, outOff);
			return array.Length;
		}

		public virtual byte[] ProcessBytes(byte[] input)
		{
			return this.ProcessBytes(input, 0, input.Length);
		}

		public abstract byte[] ProcessBytes(byte[] input, int inOff, int length);

		public virtual int ProcessBytes(byte[] input, byte[] output, int outOff)
		{
			return this.ProcessBytes(input, 0, input.Length, output, outOff);
		}

		public virtual int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
		{
			byte[] array = this.ProcessBytes(input, inOff, length);
			if (array == null)
			{
				return 0;
			}
			if (outOff + array.Length > output.Length)
			{
				throw new DataLengthException("output buffer too short");
			}
			array.CopyTo(output, outOff);
			return array.Length;
		}

		public abstract byte[] DoFinal();

		public virtual byte[] DoFinal(byte[] input)
		{
			return this.DoFinal(input, 0, input.Length);
		}

		public abstract byte[] DoFinal(byte[] input, int inOff, int length);

		public virtual int DoFinal(byte[] output, int outOff)
		{
			byte[] array = this.DoFinal();
			if (outOff + array.Length > output.Length)
			{
				throw new DataLengthException("output buffer too short");
			}
			array.CopyTo(output, outOff);
			return array.Length;
		}

		public virtual int DoFinal(byte[] input, byte[] output, int outOff)
		{
			return this.DoFinal(input, 0, input.Length, output, outOff);
		}

		public virtual int DoFinal(byte[] input, int inOff, int length, byte[] output, int outOff)
		{
			int num = this.ProcessBytes(input, inOff, length, output, outOff);
			return num + this.DoFinal(output, outOff + num);
		}

		public abstract void Reset();
	}
}
