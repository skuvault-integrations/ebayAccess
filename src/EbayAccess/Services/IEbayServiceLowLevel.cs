using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	public interface IEbayServiceLowLevel
	{
		GetOrdersResponse GetOrders( DateTime createTimeFrom, DateTime createTimeTo );

		Task< GetOrdersResponse > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo );

		InventoryStatusResponse ReviseInventoryStatus( InventoryStatusRequest inventoryStatusResponse );

		Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusResponse );

		GetSellerListResponse GetSellerList( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum, GetSellerListDetailsLevelEnum detailsLevel );

		Task< GetSellerListResponse > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum, GetSellerListDetailsLevelEnum detailsLevel );

		Item GetItem( string id );

		Task< Item > GetItemAsync( string id );

		IEnumerable< InventoryStatusResponse > ReviseInventoriesStatus( IEnumerable< InventoryStatusRequest > inventoryStatuses );

		Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > inventoryStatuses );

		Task< string > GetSessionIdAsync();

		void AuthenticateUser( string sessionId );

		Task< string > FetchTokenAsync( string sessionId );

		Uri GetAuthenticationUri( string sessionId );
	}
}