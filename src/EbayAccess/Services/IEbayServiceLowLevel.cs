using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Models.GetSellerListResponse;
using EbayAccess.Models.GetSellingManagerSoldListingsResponse;
using EbayAccess.Models.ReviseFixedPriceItemRequest;
using EbayAccess.Models.ReviseFixedPriceItemResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services.Parsers;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	internal interface IEbayServiceLowLevel
	{
		GetOrdersResponse GetOrders( DateTime createTimeFrom, DateTime createTimeTo, GetOrdersTimeRangeEnum getOrdersTimeRangeEnum, string mark );

		Task< GetOrdersResponse > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo, GetOrdersTimeRangeEnum getOrdersTimeRangeEnum, string mark = "" );

		InventoryStatusResponse ReviseInventoryStatus( InventoryStatusRequest inventoryStatusResponse, string mark );

		Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusReq, InventoryStatusRequest inventoryStatusReq2 = null, InventoryStatusRequest inventoryStatusReq3 = null, InventoryStatusRequest inventoryStatusReq4 = null, string mark = "" );

		GetSellerListResponse GetSellerList( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark );

		Task< GetSellerListResponse > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark );

		Item GetItem( string id, string mark );

		Task< Item > GetItemAsync( string id, string mark );

		IEnumerable< InventoryStatusResponse > ReviseInventoriesStatus( IEnumerable< InventoryStatusRequest > inventoryStatuses, string mark );

		Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > inventoryStatuses, string mark );

		Task< ReviseFixedPriceItemResponse > ReviseFixedPriceItemAsync( ReviseFixedPriceItemRequest fixedPriceItem, string mark, bool isVariation );

		string GetSessionId( string mark );

		void AuthenticateUser( string sessionId );

		string FetchToken( string sessionId, string mark );

		Uri GetAuthenticationUri( string sessionId );

		GetSellerListCustomResponse GetSellerListCustom( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark );

		Task< GetSellerListCustomResponse > GetSellerListCustomAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark );

		Task< WebRequest > CreateEbayStandartPostRequestToBulkExchangeServerAsync( string url, Dictionary< string, string > headers, string body, string mark );

		Task< CreateJobResponse > CreateUploadJobAsync( Guid guid, string mark );

		Task< AbortJobResponse > AbortJobAsync( string jobId, string mark );

		Task< IEnumerable< GetSellerListCustomResponse > > GetSellerListCustomResponsesAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark );

		Task< GetOrdersResponse > GetOrdersAsync( string mark = "", params string[] ordersIds );

		Task< GetSellingManagerSoldListingsResponse > GetSellngManagerOrderByRecordNumberAsync( string salerecordNumber, string mark );

		string ToJson();

		int MaxThreadsCount { get; }
	}
}