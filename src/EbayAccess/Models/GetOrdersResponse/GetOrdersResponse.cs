using System.Collections.Generic;
using EbayAccess.Models.BaseResponse;

namespace EbayAccess.Models.GetOrdersResponse
{
	public class GetOrdersResponse : EbayBaseResponse, IPaginationResponse< GetOrdersResponse >
	{
		public GetOrdersResponse()
		{
			this.Orders = new List< Order >();
		}

		public List< Order > Orders { get; set; }
		public bool HasMoreOrders { get; set; }

		public bool HasMorePages => this.HasMoreOrders;

		public void AddObjectsFromPage( GetOrdersResponse source )
		{
			if( this.Orders == null )
			{
				this.Orders = new List< Order >();
			}

			if( source?.Orders != null )
			{
				this.Orders.AddRange( source.Orders );
			}
		}
	}
}