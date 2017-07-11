using System;

namespace Org.BouncyCastle.Asn1.Cms
{
	public class Time : Asn1Encodable, IAsn1Choice
	{
		private readonly Asn1Object time;

		public string TimeString
		{
			get
			{
				if (this.time is DerUtcTime)
				{
					return ((DerUtcTime)this.time).AdjustedTimeString;
				}
				return ((DerGeneralizedTime)this.time).GetTime();
			}
		}

		public DateTime Date
		{
			get
			{
				DateTime result;
				try
				{
					if (this.time is DerUtcTime)
					{
						result = ((DerUtcTime)this.time).ToAdjustedDateTime();
					}
					else
					{
						result = ((DerGeneralizedTime)this.time).ToDateTime();
					}
				}
				catch (FormatException ex)
				{
					throw new InvalidOperationException("invalid date string: " + ex.Message);
				}
				return result;
			}
		}

		public static Time GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return Time.GetInstance(obj.GetObject());
		}

		public Time(Asn1Object time)
		{
			if (!(time is DerUtcTime) && !(time is DerGeneralizedTime))
			{
				throw new ArgumentException("unknown object passed to Time");
			}
			this.time = time;
		}

		public Time(DateTime date)
		{
			string text = date.ToString("yyyyMMddHHmmss") + "Z";
			int num = int.Parse(text.Substring(0, 4));
			if (num < 1950 || num > 2049)
			{
				this.time = new DerGeneralizedTime(text);
				return;
			}
			this.time = new DerUtcTime(text.Substring(2));
		}

		public static Time GetInstance(object obj)
		{
			if (obj == null || obj is Time)
			{
				return (Time)obj;
			}
			if (obj is DerUtcTime)
			{
				return new Time((DerUtcTime)obj);
			}
			if (obj is DerGeneralizedTime)
			{
				return new Time((DerGeneralizedTime)obj);
			}
			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public override Asn1Object ToAsn1Object()
		{
			return this.time;
		}
	}
}