using System;

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class ErrorMsgContent : Asn1Encodable
	{
		private readonly PkiStatusInfo pkiStatusInfo;

		private readonly DerInteger errorCode;

		private readonly PkiFreeText errorDetails;

		public virtual PkiStatusInfo PkiStatusInfo
		{
			get
			{
				return this.pkiStatusInfo;
			}
		}

		public virtual DerInteger ErrorCode
		{
			get
			{
				return this.errorCode;
			}
		}

		public virtual PkiFreeText ErrorDetails
		{
			get
			{
				return this.errorDetails;
			}
		}

		private ErrorMsgContent(Asn1Sequence seq)
		{
			this.pkiStatusInfo = PkiStatusInfo.GetInstance(seq[0]);
			for (int i = 1; i < seq.Count; i++)
			{
				Asn1Encodable asn1Encodable = seq[i];
				if (asn1Encodable is DerInteger)
				{
					this.errorCode = DerInteger.GetInstance(asn1Encodable);
				}
				else
				{
					this.errorDetails = PkiFreeText.GetInstance(asn1Encodable);
				}
			}
		}

		public static ErrorMsgContent GetInstance(object obj)
		{
			if (obj is ErrorMsgContent)
			{
				return (ErrorMsgContent)obj;
			}
			if (obj is Asn1Sequence)
			{
				return new ErrorMsgContent((Asn1Sequence)obj);
			}
			throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
		}

		public ErrorMsgContent(PkiStatusInfo pkiStatusInfo) : this(pkiStatusInfo, null, null)
		{
		}

		public ErrorMsgContent(PkiStatusInfo pkiStatusInfo, DerInteger errorCode, PkiFreeText errorDetails)
		{
			if (pkiStatusInfo == null)
			{
				throw new ArgumentNullException("pkiStatusInfo");
			}
			this.pkiStatusInfo = pkiStatusInfo;
			this.errorCode = errorCode;
			this.errorDetails = errorDetails;
		}

		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new Asn1Encodable[]
			{
				this.pkiStatusInfo
			});
			asn1EncodableVector.AddOptional(new Asn1Encodable[]
			{
				this.errorCode
			});
			asn1EncodableVector.AddOptional(new Asn1Encodable[]
			{
				this.errorDetails
			});
			return new DerSequence(asn1EncodableVector);
		}
	}
}
