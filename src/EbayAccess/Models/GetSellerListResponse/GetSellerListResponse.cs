using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetSellerListResponse
{
	public class GetSellerListResponse : EbayBaseResponse
	{
		public GetSellerListResponse()
		{
			this.Items = new List< Item >();
		}

		public List< Item > Items { get; set; }
		public bool HasMoreItems { get; set; }
	}
}