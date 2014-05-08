using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess
{
	public interface IEbayService
	{
		IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo );

		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo );

		void UpdateProducts( IEnumerable< InventoryStatusRequest > products );

		Task< IEnumerable< InventoryStatusResponse > > UpdateProductsAsync( IEnumerable< InventoryStatusRequest > products );

		IEnumerable< Item > GetProducts();

		Task< IEnumerable< Item > > GetProductsAsync();

		Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFrom );

		Task< IEnumerable< Item > > GetProductsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo );

		Task< IEnumerable< Item > > GetProductsDetailsAsync( DateTime createTimeFromStart, DateTime createTimeFromTo );
	}
}