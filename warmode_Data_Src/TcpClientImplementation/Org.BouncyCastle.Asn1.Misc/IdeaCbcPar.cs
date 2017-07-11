using System;

namespace Org.BouncyCastle.Asn1.Misc
{
	public class IdeaCbcPar : Asn1Encodable
	{
		internal Asn1OctetString iv;

		public static IdeaCbcPar GetInstance(object o)
		{
			if (o is IdeaCbcPar)
			{
				return (IdeaCbcPar)o;
			}
			if (o is Asn1Sequence)
			{
				return new IdeaCbcPar((Asn1Sequence)o);
			}
			throw new ArgumentException("unknown object in IDEACBCPar factory");
		}

		public IdeaCbcPar(byte[] iv)
		{
			this.iv = new DerOctetString(iv);
		}

		private IdeaCbcPar(Asn1Sequence seq)
		{
			if (seq.Count == 1)
			{
				this.iv = (Asn1OctetString)seq[0];
			}
		}

		public byte[] GetIV()
		{
			if (this.iv != null)
			{
				return this.iv.GetOctets();
			}
			return null;
		}

		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new Asn1Encodable[0]);
			if (this.iv != null)
			{
				asn1EncodableVector.Add(new Asn1Encodable[]
				{
					this.iv
				});
			}
			return new DerSequence(asn1EncodableVector);
		}
	}
}
