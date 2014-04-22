using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetOrdersResponse
{
	public class GetOrdersResponse : EbayBaseResponse
	{
		public List< Order > Orders { get; set; }
	}
}