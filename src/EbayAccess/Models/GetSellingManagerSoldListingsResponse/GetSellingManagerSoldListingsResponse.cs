using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetSellingManagerSoldListingsResponse
{
	public class GetSellingManagerSoldListingsResponse : EbayBaseResponse
	{
		public List< Order > Orders { get; set; }
	}
}