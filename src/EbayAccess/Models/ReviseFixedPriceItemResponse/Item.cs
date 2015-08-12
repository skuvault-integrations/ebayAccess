using System;
using System.Globalization;
using EbayAccess.Misc;

namespace EbayAccess.Models.ReviseFixedPriceItemResponse
{
	public class Item : ISerializableManual
	{
		public long ItemId { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public string Sku { get; set; }

		public string ToJsonManual()
		{
			return string.Format( "{{\"Id\":\"{0}\", EndTime:\"{1}\", Sku:\"{2}\"}}",
				this.ItemId,
				this.EndTime.HasValue ? this.EndTime.Value.ToString( CultureInfo.InvariantCulture ) : PredefinedValues.NotAvailable,
				string.IsNullOrWhiteSpace( this.Sku ) ? PredefinedValues.NotAvailable : this.Sku.ToString( CultureInfo.InvariantCulture )
				);
		}
	}
}