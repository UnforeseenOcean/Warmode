using Org.BouncyCastle.Utilities;
using System;
using System.Collections;
using System.IO;

namespace Org.BouncyCastle.Crypto.Tls
{
	public class CertificateUrl
	{
		internal class ListBuffer16 : MemoryStream
		{
			internal ListBuffer16()
			{
				TlsUtilities.WriteUint16(0, this);
			}

			internal void EncodeTo(Stream output)
			{
				long num = this.Length - 2L;
				TlsUtilities.CheckUint16(num);
				this.Position = 0L;
				TlsUtilities.WriteUint16((int)num, this);
				this.WriteTo(output);
				this.Close();
			}
		}

		protected readonly byte mType;

		protected readonly IList mUrlAndHashList;

		public virtual byte Type
		{
			get
			{
				return this.mType;
			}
		}

		public virtual IList UrlAndHashList
		{
			get
			{
				return this.mUrlAndHashList;
			}
		}

		public CertificateUrl(byte type, IList urlAndHashList)
		{
			if (!CertChainType.IsValid(type))
			{
				throw new ArgumentException("not a valid CertChainType value", "type");
			}
			if (urlAndHashList == null || urlAndHashList.Count < 1)
			{
				throw new ArgumentException("must have length > 0", "urlAndHashList");
			}
			this.mType = type;
			this.mUrlAndHashList = urlAndHashList;
		}

		public virtual void Encode(Stream output)
		{
			TlsUtilities.WriteUint8(this.mType, output);
			CertificateUrl.ListBuffer16 listBuffer = new CertificateUrl.ListBuffer16();
			foreach (UrlAndHash urlAndHash in this.mUrlAndHashList)
			{
				urlAndHash.Encode(listBuffer);
			}
			listBuffer.EncodeTo(output);
		}

		public static CertificateUrl parse(TlsContext context, Stream input)
		{
			byte b = TlsUtilities.ReadUint8(input);
			if (!CertChainType.IsValid(b))
			{
				throw new TlsFatalAlert(50);
			}
			int num = TlsUtilities.ReadUint16(input);
			if (num < 1)
			{
				throw new TlsFatalAlert(50);
			}
			byte[] buffer = TlsUtilities.ReadFully(num, input);
			MemoryStream memoryStream = new MemoryStream(buffer, false);
			IList list = Platform.CreateArrayList();
			while (memoryStream.Position < memoryStream.Length)
			{
				UrlAndHash value = UrlAndHash.Parse(context, memoryStream);
				list.Add(value);
			}
			return new CertificateUrl(b, list);
		}
	}
}
