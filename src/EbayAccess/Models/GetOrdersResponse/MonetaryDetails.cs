using System.Collections.Generic;

namespace EbayAccess.Models.GetOrdersResponse
{
	public class MonetaryDetails
	{
		public IEnumerable< Refund > Refunds { get; set; }
	}
}