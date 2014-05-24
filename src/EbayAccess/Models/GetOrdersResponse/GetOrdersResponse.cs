using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetOrdersResponse
{
	public class GetOrdersResponse : EbayBaseResponse
	{
		public GetOrdersResponse()
		{
			Orders = new List< Order >();
		}

		public List< Order > Orders { get; set; }
		public bool HasMoreOrders { get; set; }
	}
}