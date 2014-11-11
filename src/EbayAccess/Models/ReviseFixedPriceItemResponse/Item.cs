using System;
using System.Globalization;
using EbayAccess.Misc;

namespace EbayAccess.Models.ReviseFixedPriceItemResponse
{
	public class Item : ISerializableMnual
	{
		public long ItemId { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }

		public string ToJson()
		{
			return string.Format( "{{Id:{0},EndTime:{1}}}", this.ItemId, this.EndTime.HasValue ? this.EndTime.Value.ToString( CultureInfo.InvariantCulture ) : PredefinedValues.NotAvailable );
		}
	}
}