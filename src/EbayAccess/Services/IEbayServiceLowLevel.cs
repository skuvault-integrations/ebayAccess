using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Models.GetSellerListResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services.Parsers;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	internal interface IEbayServiceLowLevel
	{
		GetOrdersResponse GetOrders( DateTime createTimeFrom, DateTime createTimeTo, GetOrdersTimeRangeEnum getOrdersTimeRangeEnum );

		Task< GetOrdersResponse > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo, GetOrdersTimeRangeEnum getOrdersTimeRangeEnum );

		InventoryStatusResponse ReviseInventoryStatus( InventoryStatusRequest inventoryStatusResponse );

		Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusReq, InventoryStatusRequest inventoryStatusReq2 = null, InventoryStatusRequest inventoryStatusReq3 = null, InventoryStatusRequest inventoryStatusReq4 = null );

		GetSellerListResponse GetSellerList( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum );

		Task< GetSellerListResponse > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum );

		Item GetItem( string id );

		Task< Item > GetItemAsync( string id );

		IEnumerable< InventoryStatusResponse > ReviseInventoriesStatus( IEnumerable< InventoryStatusRequest > inventoryStatuses );

		Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > inventoryStatuses );

		string GetSessionId();

		void AuthenticateUser( string sessionId );

		string FetchToken( string sessionId );

		Uri GetAuthenticationUri( string sessionId );

		GetSellerListCustomResponse GetSellerListCustom( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum );

		Task< GetSellerListCustomResponse > GetSellerListCustomAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum );

		Task< WebRequest > CreateEbayStandartPostRequestToBulkExchangeServerAsync( string url, Dictionary< string, string > headers, string body );

		Task< CreateJobResponse > CreateUploadJobAsync( Guid guid );

		Task< AbortJobResponse > AbortJobAsync( string jobId );

		Task< IEnumerable< GetSellerListCustomResponse > > GetSellerListCustomResponsesAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum );
	}
}