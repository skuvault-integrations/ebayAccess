using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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
using Netco.Logging;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	internal interface IEbayServiceLowLevel
	{
		Task< GetOrdersResponse > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo, GetOrdersTimeRangeEnum getOrdersTimeRangeEnum, CancellationToken cts, Mark mark = null );

		Task< InventoryStatusResponse > ReviseInventoryStatusAsync( CancellationToken token, InventoryStatusRequest inventoryStatusReq, InventoryStatusRequest inventoryStatusReq2 = null, InventoryStatusRequest inventoryStatusReq3 = null, InventoryStatusRequest inventoryStatusReq4 = null, Mark mark = null );

		Task< GetSellerListResponse > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, Mark mark );

		Task< Item > GetItemAsync( string id, Mark mark );

		Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > inventoryStatuses, CancellationToken token, Mark mark );

		Task< ReviseFixedPriceItemResponse > ReviseFixedPriceItemAsync( ReviseFixedPriceItemRequest fixedPriceItem, CancellationToken token, Mark mark );

		string GetSessionId( Mark mark );

		void AuthenticateUser( string sessionId );

		string FetchToken( string sessionId, Mark mark );

		Uri GetAuthenticationUri( string sessionId );

		Task< GetSellerListCustomResponse > GetSellerListCustomAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, Mark mark );

		Task< WebRequest > CreateEbayStandardPostRequestToBulkExchangeServerAsync( string url, Dictionary< string, string > headers, string body, Mark mark );

		Task< CreateJobResponse > CreateUploadJobAsync( Guid guid, Mark mark );

		Task< AbortJobResponse > AbortJobAsync( string jobId, Mark mark );

		Task< GetOrdersResponse > GetOrdersAsync( CancellationToken cts, Mark mark = null, params string[] ordersIds );

		Task< GetSellingManagerSoldListingsResponse > GetSellingManagerOrderByRecordNumberAsync( string saleRecordNumber, Mark mark, CancellationToken cts );

		Task< GetSellingManagerSoldListingsResponse > GetSellingManagerSoldListingsByPeriodAsync( DateTime timeFrom, DateTime timeTo, CancellationToken ct, int pageLimit, Mark mark );

		string ToJson();

		int MaxThreadsCount { get; }

		Func< string > AdditionalLogInfo { get; set; }

		Task< IEnumerable< GetSellerListCustomResponse > > GetSellerListCustomResponsesWithMaxThreadsRestrictionAsync( CancellationToken ct, DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, Mark mark );

		Task< IEnumerable< GetSellerListCustomProductResponse > > GetSellerListCustomProductResponsesWithMaxThreadsRestrictionAsync( CancellationToken ct, DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, Mark mark );
	}
}