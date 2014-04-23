using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Interfaces
{
	public interface IEbayService
	{
		IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo );

		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );

		IEnumerable< Item > GetProducts();

		Task< IEnumerable< Item > > GetProductsAsync();

		void UpdateProducts( IEnumerable< InventoryStatusRequest > products );

		Task UpdateProductsAsync( IEnumerable< InventoryStatusRequest > products );

		Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFrom );

		Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo );
	}
}