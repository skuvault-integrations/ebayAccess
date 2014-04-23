using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Interfaces
{
	public interface IEbayServiceRaw
	{
		IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo, bool includeDetails = false );

		Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, bool includeDetails = false );

		InventoryStatusResponse ReviseInventoryStatus( InventoryStatusRequest inventoryStatusResponse );

		Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusResponse );

		IEnumerable< Item > GetItems( DateTime startTimeFrom, DateTime startTimeTo );

		Task< IEnumerable< Item > > GetItemsAsync( DateTime startTimeFrom, DateTime startTimeTo );
	}
}