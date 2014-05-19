using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	public interface IEbayServiceLowLevel
	{
		IEnumerable< Order > GetOrders( DateTime createTimeFrom, DateTime createTimeTo );

		Task< IEnumerable< Order > > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo );

		InventoryStatusResponse ReviseInventoryStatus( InventoryStatusRequest inventoryStatusResponse );

		Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusResponse );

		IEnumerable< Item > GetSellerList( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum );

		Task< IEnumerable< Item > > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum );

		Item GetItem( string id );

		Task< Item > GetItemAsync( string id );

		IEnumerable< InventoryStatusResponse > ReviseInventoriesStatus( IEnumerable< InventoryStatusRequest > inventoryStatuses );

		Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > inventoryStatuses );

		Task< string > GetSessionIdAsync();

		void AuthenticateUser( string sessionId );

		Task< string > FetchTokenAsync( string sessionId );
	}
}