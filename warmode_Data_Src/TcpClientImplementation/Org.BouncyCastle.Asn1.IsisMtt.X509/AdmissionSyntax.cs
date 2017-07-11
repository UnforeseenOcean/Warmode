using Org.BouncyCastle.Asn1.X509;
using System;

namespace Org.BouncyCastle.Asn1.IsisMtt.X509
{
	public class AdmissionSyntax : Asn1Encodable
	{
		private readonly GeneralName admissionAuthority;

		private readonly Asn1Sequence contentsOfAdmissions;

		public virtual GeneralName AdmissionAuthority
		{
			get
			{
				return this.admissionAuthority;
			}
		}

		public static AdmissionSyntax GetInstance(object obj)
		{
			if (obj == null || obj is AdmissionSyntax)
			{
				return (AdmissionSyntax)obj;
			}
			if (obj is Asn1Sequence)
			{
				return new AdmissionSyntax((Asn1Sequence)obj);
			}
			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		private AdmissionSyntax(Asn1Sequence seq)
		{
			switch (seq.Count)
			{
			case 1:
				this.contentsOfAdmissions = Asn1Sequence.GetInstance(seq[0]);
				return;
			case 2:
				this.admissionAuthority = GeneralName.GetInstance(seq[0]);
				this.contentsOfAdmissions = Asn1Sequence.GetInstance(seq[1]);
				return;
			default:
				throw new ArgumentException("Bad sequence size: " + seq.Count);
			}
		}

		public AdmissionSyntax(GeneralName admissionAuthority, Asn1Sequence contentsOfAdmissions)
		{
			this.admissionAuthority = admissionAuthority;
			this.contentsOfAdmissions = contentsOfAdmissions;
		}

		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector asn1EncodableVector = new Asn1EncodableVector(new Asn1Encodable[0]);
			if (this.admissionAuthority != null)
			{
				asn1EncodableVector.Add(new Asn1Encodable[]
				{
					this.admissionAuthority
				});
			}
			asn1EncodableVector.Add(new Asn1Encodable[]
			{
				this.contentsOfAdmissions
			});
			return new DerSequence(asn1EncodableVector);
		}

		public virtual Admissions[] GetContentsOfAdmissions()
		{
			Admissions[] array = new Admissions[this.contentsOfAdmissions.Count];
			for (int i = 0; i < this.contentsOfAdmissions.Count; i++)
			{
				array[i] = Admissions.GetInstance(this.contentsOfAdmissions[i]);
			}
			return array;
		}
	}
}
