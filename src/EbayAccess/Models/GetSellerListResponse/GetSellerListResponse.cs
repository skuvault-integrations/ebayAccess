using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetSellerListResponse
{
	public class GetSellerListResponse : EbayBaseResponse, IPaginationResponse< GetSellerListResponse >
	{
		public GetSellerListResponse()
		{
			this.Items = new List< Item >();
		}

		public List< Item > Items { get; set; }
		public bool HasMoreItems { get; set; }

		public bool HasMorePages => this.HasMoreItems;

		public void AddObjectsFromPage( GetSellerListResponse source )
		{
			if( this.Items == null )
			{
				this.Items = new List< Item >();
			}

			if( source?.Items != null )
			{
				this.Items.AddRange( source.Items );
			}
		}
	}
}