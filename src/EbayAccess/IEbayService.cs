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

		InventoryStatusResponse ReviseInventoryStatus( InventoryStatusRequest inventoryStatusResponse );

		Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusResponse );

		IEnumerable< Item > GetItems( DateTime startTimeFrom, DateTime startTimeTo );

		Task< IEnumerable< Item > > GetItemsAsync( DateTime startTimeFrom, DateTime startTimeTo );
	}
}