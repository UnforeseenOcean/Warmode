using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using System;

namespace Org.BouncyCastle.Asn1.Crmf
{
	public class PKMacValue : Asn1Encodable
	{
		private readonly AlgorithmIdentifier algID;

		private readonly DerBitString macValue;

		public virtual AlgorithmIdentifier AlgID
		{
			get
			{
				return this.algID;
			}
		}

		public virtual DerBitString MacValue
		{
			get
			{
				return this.macValue;
			}
		}

		private PKMacValue(Asn1Sequence seq)
		{
			this.algID = AlgorithmIdentifier.GetInstance(seq[0]);
			this.macValue = DerBitString.GetInstance(seq[1]);
		}

		public static PKMacValue GetInstance(object obj)
		{
			if (obj is PKMacValue)
			{
				return (PKMacValue)obj;
			}
			if (obj is Asn1Sequence)
			{
				return new PKMacValue((Asn1Sequence)obj);
			}
			throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
		}

		public static PKMacValue GetInstance(Asn1TaggedObject obj, bool isExplicit)
		{
			return PKMacValue.GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
		}

		public PKMacValue(PbmParameter pbmParams, DerBitString macValue) : this(new AlgorithmIdentifier(CmpObjectIdentifiers.passwordBasedMac, pbmParams), macValue)
		{
		}

		public PKMacValue(AlgorithmIdentifier algID, DerBitString macValue)
		{
			this.algID = algID;
			this.macValue = macValue;
		}

		public override Asn1Object ToAsn1Object()
		{
			return new DerSequence(new Asn1Encodable[]
			{
				this.algID,
				this.macValue
			});
		}
	}
}