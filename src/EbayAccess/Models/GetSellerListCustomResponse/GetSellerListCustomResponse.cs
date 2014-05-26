using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	public class GetSellerListCustomResponse : EbayBaseResponse
	{
		public GetSellerListCustomResponse()
		{
			this.Items = new List< Item >();
		}

		public List< Item > Items { get; set; }
		public bool HasMoreItems { get; set; }
	}
}