using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetSellingManagerSoldListingsResponse
{
	public class GetSellingManagerSoldListingsResponse : EbayBaseResponse
	{
		public GetSellingManagerSoldListingsResponse()
		{
			this.Orders = new List< Order >();
		}

		public List< Order > Orders { get; set; }
		public bool IsLimitedResponse { get; set; }
	}
}