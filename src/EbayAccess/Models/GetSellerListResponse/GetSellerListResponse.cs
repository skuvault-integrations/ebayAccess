using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetSellerListResponse
{
	public class GetSellerListResponse : EbayBaseResponse
	{
		public List< Item > Items { get; set; }
		public bool HasMoreItems { get; set; }
	}
}