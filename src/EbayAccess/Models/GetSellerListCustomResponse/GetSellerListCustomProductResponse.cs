using System.Collections.Generic;
using System.Linq;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetSellerListCustomResponse
{
	public class GetSellerListCustomProductResponse : EbayBaseResponse, IPaginationResponse< GetSellerListCustomProductResponse >
	{
		public GetSellerListCustomProductResponse()
		{
			this.Items = new List< Product >();
		}

		public List< Product > Items { get; set; }
		public bool HasMoreItems { get; set; }

		public bool HasMorePages => this.HasMoreItems;

		public void AddObjectsFromPage( GetSellerListCustomProductResponse source )
		{
			if( this.Items == null )
			{
				this.Items = new List< Product >();
			}

			if( source?.Items != null )
			{
				this.Items.AddRange( source.Items );
			}
		}
	}
}