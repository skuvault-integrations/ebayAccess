using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Misc;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Models.GetSellerListResponse;
using EbayAccess.Models.GetSellingManagerSoldListingsResponse;
using EbayAccess.Models.ReviseFixedPriceItemRequest;
using EbayAccess.Models.ReviseFixedPriceItemResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services.Parsers;
using Netco.Extensions;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	internal sealed class EbayServiceLowLevel : IEbayServiceLowLevel
	{
		private const int MaxRetryCountOnEbayInternalError = 3;
		private readonly EbayUserCredentials _userCredentials;
		private readonly EbayConfig _ebayConfig;
		private readonly string _endPoint;
		private readonly int _itemsPerPage;
		private readonly IWebRequestServices _webRequestServices;
		private readonly string _ebaySignInUrl;
		private readonly string _endPointBulkExhange;

		public int MaxThreadsCount => 5;
		private const int MaxRequestTimeoutMs = 30 * 60 * 1000;
		private int RequestTimeoutMs;

		public Func< string > AdditionalLogInfo { get; set; }

		public EbayServiceLowLevel( EbayUserCredentials credentials, EbayConfig ebayConfig, IWebRequestServices webRequestServices, int requestTimeoutMs = MaxRequestTimeoutMs )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();
			Condition.Requires( webRequestServices, "webRequestServices" ).IsNotNull();
			Condition.Requires( ebayConfig, "ebayDevCredentials" ).IsNotNull();

			this._userCredentials = credentials;
			this._webRequestServices = webRequestServices;
			this._endPoint = ebayConfig.EndPoint;
			this._endPointBulkExhange = ebayConfig.EndPointBulkExhange;
			this._ebaySignInUrl = ebayConfig.SignInUrl;
			this._itemsPerPage = 200;
			this._ebayConfig = ebayConfig;
			this.RequestTimeoutMs = requestTimeoutMs;
		}

		public EbayServiceLowLevel( EbayUserCredentials userCredentials, EbayConfig ebayConfig )
			: this( userCredentials, ebayConfig, new WebRequestServices() )
		{
		}

		#region EbayStandartRequest
		private async Task< WebRequest > CreateEbayStandardPostRequestAsync( string url, Dictionary< string, string > headers, string body, string mark, CancellationToken cts )
		{
			if( cts.IsCancellationRequested )
				throw new TaskCanceledException( $"Request was cancelled or timed out, Mark: {mark}" );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiCompatibilityLevel ) )
				headers.Add( EbayHeaders.XEbayApiCompatibilityLevel, EbayHeadersValues.XEbayApiCompatibilityLevel );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiDevName ) )
				headers.Add( EbayHeaders.XEbayApiDevName, this._ebayConfig.DevName );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiAppName ) )
				headers.Add( EbayHeaders.XEbayApiAppName, this._ebayConfig.AppName );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiSiteid ) )
				headers.Add( EbayHeaders.XEbayApiSiteid, this._userCredentials.SiteId.ToString() );

			return await this._webRequestServices.CreateServicePostRequestAsync( url, body, headers, cts, mark ).ConfigureAwait( false );
		}

		public WebRequest CreateEbayStandardPostRequest( string url, Dictionary< string, string > headers, string body, string mark )
		{
			var resultTask = this.CreateEbayStandardPostRequestAsync( url, headers, body, mark, CancellationToken.None );
			resultTask.Wait();
			return resultTask.Result;
		}

		public async Task< WebRequest > CreateEbayStandardPostRequestWithCertAsync( string url, Dictionary< string, string > headers, string body, string mark, CancellationToken cts )
		{
			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiCertName ) )
				headers.Add( EbayHeaders.XEbayApiCertName, this._ebayConfig.CertName );

	var ebayStandartPostRequestAsync = await this.CreateEbayStandardPostRequestAsync( url, headers, body, mark, cts ).ConfigureAwait( false );
			return ebayStandartPostRequestAsync;
		}

		public async Task< WebRequest > CreateEbayStandardPostRequestToBulkExchangeServerAsync( string url, Dictionary< string, string > headers, string body, string mark = "" )
		{
			return await this._webRequestServices.CreateServicePostRequestAsync( url, body, headers, CancellationToken.None, mark ).ConfigureAwait( false );
		}

		public WebRequest CreateEbayStandardPostRequestWithCert( string url, Dictionary< string, string > headers, string body, string mark )
		{
			var requestTask = this.CreateEbayStandardPostRequestWithCertAsync( url, headers, body, mark, CancellationToken.None );
			requestTask.Wait();
			return requestTask.Result;
		}
		#endregion

		#region GetOrders
		private string CreateGetOrdersRequestBody( DateTime timeRangeFrom, DateTime timeRangeTo, int recordsPerPage, int pageNumber, GetOrdersTimeRangeEnum getOrdersTimeRangeEnum )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><{5}From>{1}</{5}From><{5}To>{2}</{5}To><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination></GetOrdersRequest>​",
				this._userCredentials.Token,
				timeRangeFrom.ToStringUtcIso8601(),
				timeRangeTo.ToStringUtcIso8601(),
				recordsPerPage,
				pageNumber,
				getOrdersTimeRangeEnum );
		}

		private string CreateGetOrdersRequestBody( int recordsPerPage, int pageNumber, params string[] ordersIds )
		{
			var ordersIdsTags = ordersIds.Aggregate( "", ( ac, x ) => ac + string.Format( "<OrderID>{0}</OrderID>", x ) );
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><OrderIDArray>{1}</OrderIDArray><Pagination><EntriesPerPage>{2}</EntriesPerPage><PageNumber>{3}</PageNumber></Pagination></GetOrdersRequest>​",
				this._userCredentials.Token,
				ordersIdsTags,
				recordsPerPage,
				pageNumber );
		}

		private string CreateGetSellingManagerSoldListingsRequestBody( string orderSellingManagerRecordNumber )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellingManagerSoldListingsRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><Search><SearchType>SaleRecordID</SearchType><SearchValue>{1}</SearchValue></Search></GetSellingManagerSoldListingsRequest>​",
				this._userCredentials.Token,
				orderSellingManagerRecordNumber );
		}

		private string CreateGetSellingManagerSoldListingsRequestBody( DateTime timeRangeFrom, DateTime timeRangeTo, int recordsPerPage, int pageNumber )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellingManagerSoldListingsRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><Pagination><EntriesPerPage>{1}</EntriesPerPage><PageNumber>{2}</PageNumber></Pagination><SaleDateRange><TimeFrom>{3}</TimeFrom><TimeTo>{4}</TimeTo></SaleDateRange></GetSellingManagerSoldListingsRequest>​",
				this._userCredentials.Token,
				recordsPerPage,
				pageNumber,
				timeRangeFrom.ToStringUtcIso8601(),
				timeRangeTo.ToStringUtcIso8601() );
		}

		private static Dictionary< string, string > CreateEbayGetSellingManagerSoldListingsRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.GetSellingManagerSoldListings },
			};
		}

		private static Dictionary< string, string > CreateEbayGetOrdersRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.GetOrders },
				{  EbayHeaders.XEbayApiCompatibilityLevel, "1057"}
			};
		}

		public async Task< GetOrdersResponse > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo, GetOrdersTimeRangeEnum getOrdersTimeRangeEnum, CancellationToken cts, string mark = "" )
		{
			return await this.GetOrdersTemplateAsync( page => this.CreateGetOrdersRequestBody( createTimeFrom, createTimeTo, this._itemsPerPage, page, getOrdersTimeRangeEnum ), cts, mark ).ConfigureAwait( false );
		}

		public async Task< GetOrdersResponse > GetOrdersAsync( CancellationToken cts, string mark = "", params string[] ordersIds )
		{
			return await this.GetOrdersTemplateAsync( page => this.CreateGetOrdersRequestBody( this._itemsPerPage, page, ordersIds ), cts, mark ).ConfigureAwait( false );
		}

		private async Task< GetOrdersResponse > GetOrdersTemplateAsync( Func< int, string > getRequestBody, CancellationToken cts, string mark = "" )
		{
			return await this.GetEbayMultiPageRequestAsync(
				headers : CreateEbayGetOrdersRequestHeadersWithApiCallName(),
				getRequestBodyByPageNumber : getRequestBody,
				responseParser : x => new EbayGetOrdersResponseParser().Parse( x ),
				cts : cts,
				mark : mark,
				useCert: true
			).ConfigureAwait( false );
		}

		public async Task< GetSellingManagerSoldListingsResponse > GetSellingManagerOrderByRecordNumberAsync( string saleRecordNumber, string mark, CancellationToken cts )
		{
			return await this.GetEbaySingleRequestAsync(
				headers : CreateEbayGetSellingManagerSoldListingsRequestHeadersWithApiCallName(),
				body : this.CreateGetSellingManagerSoldListingsRequestBody( saleRecordNumber ),
				responseParser : x => new EbayGetSellingManagerSoldListingsResponseParser().Parse( x ),
				token : cts,
				mark : mark,
				useCert: true
			).ConfigureAwait( false );
		}

		public async Task< GetSellingManagerSoldListingsResponse > GetSellingManagerSoldListingsByPeriodAsync( DateTime timeFrom, DateTime timeTo, CancellationToken ct, int pageLimit = 0, string mark = "" )
		{
			if( ct.IsCancellationRequested )
				throw new WebException( "Task was canceled" );

			var recordsPerPage = this._itemsPerPage;
			const int pageNumber = 1;

			var sellingManagerSoldListings = await this.GetSellingManagerSoldListingsByPeriodSinglePageAsync( ct, timeFrom, timeTo, recordsPerPage, pageNumber, mark ).ConfigureAwait( false );

			if( sellingManagerSoldListings == null || sellingManagerSoldListings.Errors != null )
				return sellingManagerSoldListings;

			var totalPages = sellingManagerSoldListings.PaginationResult?.TotalNumberOfPages ?? 0;

			if( ( pageLimit > 0 ) && ( totalPages > pageLimit ) )
			{
				sellingManagerSoldListings.IsLimitedResponse = true;
				return sellingManagerSoldListings;
			}

			if( totalPages <= 1 )
				return sellingManagerSoldListings;

			if( ct.IsCancellationRequested )
				throw new WebException( "Task was canceled" );

			var errorList = new List< ResponseError >();
			var pages = new List< int >();
			for( var i = 2; i <= sellingManagerSoldListings.PaginationResult?.TotalNumberOfPages; i++ )
			{
				pages.Add( i );
			}

			var sellingManagerSoldListingsTempList = await pages.ProcessInBatchAsync( this.MaxThreadsCount, async x => await this.GetSellingManagerSoldListingsByPeriodSinglePageAsync( ct, timeFrom, timeTo, recordsPerPage, x, mark ).ConfigureAwait( false ) ).ConfigureAwait( false );

			sellingManagerSoldListingsTempList.ForEach( x =>
			{
				if( x.Errors != null )
				{
					errorList.AddRange( x.Errors );
				}
				else
				{
					sellingManagerSoldListings.Orders.AddRange( x.Orders );
				}
			} );

			sellingManagerSoldListings.Errors = errorList;

			return sellingManagerSoldListings;
		}

		private async Task< GetSellingManagerSoldListingsResponse > GetSellingManagerSoldListingsByPeriodSinglePageAsync( CancellationToken ct, DateTime timeFrom, DateTime timeTo, int recordsPerPage, int pageNumber, string mark = "" )
		{
			return await this.GetEbaySingleRequestAsync(
				headers : CreateEbayGetSellingManagerSoldListingsRequestHeadersWithApiCallName(),
				body : this.CreateGetSellingManagerSoldListingsRequestBody( timeFrom, timeTo, recordsPerPage, pageNumber ),
				responseParser : x => new EbayGetSellingManagerSoldListingsResponseParser().Parse( x ),
				token : ct,
				mark : mark,
				useCert : true
			).ConfigureAwait( false );
		}

		public string ToJson()
		{
			var aditionalLogInfo = this.AdditionalLogInfo != null ? this.AdditionalLogInfo() : PredefinedValues.NotAvailable;
			return string.Format( "{{AssemblyVer:{3},AccountName:'{0}',SiteId:{1},AdditionalInfo:{4},Token:'{2}'}}",
				( this._userCredentials == null || string.IsNullOrWhiteSpace( this._userCredentials.AccountName ) ) ? PredefinedValues.NotAvailable : this._userCredentials.AccountName,
				this._userCredentials == null ? PredefinedValues.NotAvailable : this._userCredentials.SiteId.ToString(),
				( this._userCredentials == null || string.IsNullOrWhiteSpace( this._userCredentials.Token ) ) ? PredefinedValues.NotAvailable : this._userCredentials.Token,
				Assembly.GetExecutingAssembly().FullName,
				aditionalLogInfo
				);
		}
		#endregion

		#region GetSellerList
		private static Dictionary< string, string > CreateGetSellerListRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.GetSellerList },
			};
		}

		private string CreateGetSellerListRequestBody( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, int recordsPerPage, int pageNumber )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><{5}From>{1}</{5}From><{5}To>{2}</{5}To><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><DetailLevel>ReturnAll</DetailLevel></GetSellerListRequest>​​",
				this._userCredentials.Token,
				timeFrom.ToStringUtcIso8601(),
				timeTo.ToStringUtcIso8601(),
				recordsPerPage,
				pageNumber,
				getSellerListTimeRangeEnum );
		}

		private string CreateGetSellerListCustomRequestBody( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, int recordsPerPage, int pageNumber )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><{5}From>{1}</{5}From><{5}To>{2}</{5}To><IncludeVariations>true</IncludeVariations><Pagination ComplexType=\"PaginationType\"><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination>  <DetailLevel>ReturnAll</DetailLevel><OutputSelector>ItemArray.Item.BuyItNowPrice,ItemArray.Item.OutOfStockControl,PaginationResult,HasMoreItems,ItemArray.Item.SKU,ItemArray.Item.Variations,ItemArray.Item.Quantity,ItemArray.Item.Title,ItemArray.Item.ItemID,ItemArray.Item.SellingStatus.CurrentPrice,ItemArray.Item.SellingStatus.QuantitySold,ItemArray.Item.Site,ItemArray.Item.ListingDuration,ItemArray.Item.ConditionID,ItemArray.Item.ConditionDescription</OutputSelector> </GetSellerListRequest>​​​",
				this._userCredentials.Token,
				timeFrom.ToStringUtcIso8601(),
				timeTo.ToStringUtcIso8601(),
				recordsPerPage,
				pageNumber,
				getSellerListTimeRangeEnum );
		}

		private string CreateGetSellerListCustomProductRequestBody( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, int recordsPerPage, int pageNumber )
		{
			return "<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials>" +
				$"<eBayAuthToken>{this._userCredentials.Token}</eBayAuthToken></RequesterCredentials><{getSellerListTimeRangeEnum}From>{timeFrom.ToStringUtcIso8601()}</{getSellerListTimeRangeEnum}From><{getSellerListTimeRangeEnum}To>{timeTo.ToStringUtcIso8601()}</{getSellerListTimeRangeEnum}To><IncludeVariations>true</IncludeVariations><Pagination ComplexType=\"PaginationType\"><EntriesPerPage>{recordsPerPage}</EntriesPerPage><PageNumber>{pageNumber}</PageNumber></Pagination>  " +
				"<DetailLevel>ItemReturnDescription</DetailLevel><OutputSelector>ItemArray.Item.ItemID,ItemArray.Item.SKU,ItemArray.Item.Title,ItemArray.Item.PrimaryCategory.CategoryName,ItemArray.Item.PictureDetails.PictureURL,ItemArray.Item.Description,ItemArray.Item.ShippingPackageDetails.WeightMajor,ItemArray.Item.ShippingPackageDetails.WeightMinor,ItemArray.Item.Variations,PaginationResult,HasMoreItems,ItemArray.Item.ListingDuration</OutputSelector> </GetSellerListRequest>​​​";
		}

		public async Task< GetSellerListResponse > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark )
		{
			return await this.GetEbayMultiPageRequestAsync(
				headers : CreateGetSellerListRequestHeadersWithApiCallName(),
				getRequestBodyByPageNumber : page => this.CreateGetSellerListRequestBody( timeFrom, timeTo, getSellerListTimeRangeEnum, this._itemsPerPage, page ),
				responseParser : x => new EbayGetSallerListResponseParser().Parse( x ),
				cts : CancellationToken.None,
				mark : mark
			).ConfigureAwait( false );
		}

		public async Task< GetSellerListCustomResponse > GetSellerListCustomAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark )
		{
			return await this.GetEbayMultiPageRequestAsync(
				headers : CreateGetSellerListRequestHeadersWithApiCallName(),
				getRequestBodyByPageNumber : page => this.CreateGetSellerListCustomRequestBody( timeFrom, timeTo, getSellerListTimeRangeEnum, this._itemsPerPage, page ),
				responseParser : x => new EbayGetSellerListCustomResponseParser().Parse( x ),
				cts : CancellationToken.None,
				mark : mark
			).ConfigureAwait( false );
		}

		public async Task< IEnumerable< GetSellerListCustomResponse > > GetSellerListCustomResponsesWithMaxThreadsRestrictionAsync( CancellationToken ct, DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark )
		{
			if( ct.IsCancellationRequested )
				throw new TaskCanceledException( $"Request was cancelled or timed out, Mark: {mark}" );

			var recordsPerPage = this._itemsPerPage;
			const int pageNumber = 1;

			var getSellerListResponse = await this.GetSellerListCustomResponseAsync( ct, timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, pageNumber, mark ).ConfigureAwait( false );

			var pages = new List< int >();

			var getSellerListCustomResponses = new List< GetSellerListCustomResponse > { getSellerListResponse };
			if( getSellerListResponse != null && getSellerListResponse.Errors == null )
			{
				if( getSellerListResponse.PaginationResult.TotalNumberOfPages > 1 )
				{
					for( var i = 2; i <= getSellerListResponse.PaginationResult.TotalNumberOfPages; i++ )
					{
						pages.Add( i );
					}
					var getSellerListCustomResponsesTemp = await pages.ProcessInBatchAsync( this.MaxThreadsCount, async x => await this.GetSellerListCustomResponseAsync( ct, timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, x, mark ).ConfigureAwait( false ) ).ConfigureAwait( false );
					getSellerListCustomResponses.AddRange( getSellerListCustomResponsesTemp );
				}
			}

			return getSellerListCustomResponses.Where( x => x != null ).ToList();
		}

		public async Task< IEnumerable< GetSellerListCustomProductResponse > > GetSellerListCustomProductResponsesWithMaxThreadsRestrictionAsync( CancellationToken ct, DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark )
		{
			if( ct.IsCancellationRequested )
				throw new TaskCanceledException( $"Request was cancelled or timed out, Mark: {mark}" );

			var recordsPerPage = this._itemsPerPage;
			const int pageNumber = 1;

			var getSellerListResponse = await this.GetSellerListCustomProductResponseAsync( ct, timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, pageNumber, mark ).ConfigureAwait( false );

			var pages = new List< int >();

			var getSellerListCustomResponses = new List< GetSellerListCustomProductResponse > { getSellerListResponse };
			if( getSellerListResponse != null && getSellerListResponse.Errors == null )
			{
				if( getSellerListResponse.PaginationResult.TotalNumberOfPages > 1 )
				{
					for( var i = 2; i <= getSellerListResponse.PaginationResult.TotalNumberOfPages; i++ )
					{
						pages.Add( i );
					}
					var getSellerListCustomResponsesTemp = await pages.ProcessInBatchAsync( this.MaxThreadsCount, async x => await this.GetSellerListCustomProductResponseAsync( ct, timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, x, mark ).ConfigureAwait( false ) ).ConfigureAwait( false );
					getSellerListCustomResponses.AddRange( getSellerListCustomResponsesTemp );
				}
			}

			return getSellerListCustomResponses.Where( x => x != null ).ToList();
		}

		private async Task< GetSellerListCustomResponse > GetSellerListCustomResponseAsync( CancellationToken ct, DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, int recordsPerPage, int pageNumber, string mark )
		{
			return await this.GetEbaySingleRequestAsync< GetSellerListCustomResponse >(
				headers : CreateGetSellerListRequestHeadersWithApiCallName(),
				body : this.CreateGetSellerListCustomRequestBody( timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, pageNumber ),
				responseParser : x => new EbayGetSellerListCustomResponseParser().Parse( x ),
				token : ct,
				mark : mark
			).ConfigureAwait( false );
		}

		private async Task< GetSellerListCustomProductResponse > GetSellerListCustomProductResponseAsync( CancellationToken ct, DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, int recordsPerPage, int pageNumber, string mark )
		{
			return await this.GetEbaySingleRequestAsync< GetSellerListCustomProductResponse >(
				headers : CreateGetSellerListRequestHeadersWithApiCallName(),
				body : this.CreateGetSellerListCustomProductRequestBody( timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, pageNumber ),
				responseParser : x => new EbayGetSellerListCustomProductResponseParser().Parse( x ),
				token : ct,
				mark : mark
			).ConfigureAwait( false );
		}
		#endregion

		#region GetItem
		private static Dictionary< string, string > CreateGetItemRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.GetItem },
			};
		}

		private string CreateGetItemByIdRequestBody( string id )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetItemRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><ItemID>{1}</ItemID></GetItemRequest>​",
				this._userCredentials.Token,
				id );
		}

		public async Task< Item > GetItemAsync( string id, string mark )
		{
			var response = await this.GetEbaySingleRequestAsync< Models.GetItemResponse.GetItemResponse >(
				headers : CreateGetItemRequestHeadersWithApiCallName(),
				body : this.CreateGetItemByIdRequestBody( id ),
				responseParser : x => new EbayGetItemResponseParser().Parse( x ),
				token : CancellationToken.None,
				mark : mark
			).ConfigureAwait( false );

			return response != null ? response.Item : new Item();
		}
		#endregion

		#region ReviseInventoryStatus
		private static Dictionary< string, string > CreateReviseInventoryStatusHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.ReviseInventoryStatus },
			};
		}

		private static string CreateInventoryStatusTag( long? itemIdMonad, long? quantityMonad, string sku )
		{
			var itemIdElement = itemIdMonad.HasValue ? string.Format( "<ItemID>{0}</ItemID>", itemIdMonad.Value ) : string.Empty;
			var quantityElement = quantityMonad.HasValue ? string.Format( "<Quantity>{0}</Quantity>", quantityMonad.Value ) : string.Empty;
			var skuElement = !string.IsNullOrWhiteSpace( sku ) ? string.Format( "<SKU>{0}</SKU>", SecurityElement.Escape( sku ) ) : string.Empty;
			var inventoryStatus = string.Format( "<InventoryStatus ComplexType=\"InventoryStatusType\">{0}{1}{2}</InventoryStatus>", itemIdElement, quantityElement, skuElement );
			return inventoryStatus;
		}

		private string CreateReviseInventoryStatusRequestBody( InventoryStatusRequest inventoryStatusReq1, InventoryStatusRequest inventoryStatusReq2 = null, InventoryStatusRequest inventoryStatusReq3 = null, InventoryStatusRequest inventoryStatusReq4 = null )
		{
			var inv1 = inventoryStatusReq1 != null ? CreateInventoryStatusTag( inventoryStatusReq1.ItemId, inventoryStatusReq1.Quantity, inventoryStatusReq1.Sku ) : string.Empty;
			var inv2 = inventoryStatusReq2 != null ? CreateInventoryStatusTag( inventoryStatusReq2.ItemId, inventoryStatusReq2.Quantity, inventoryStatusReq2.Sku ) : string.Empty;
			var inv3 = inventoryStatusReq3 != null ? CreateInventoryStatusTag( inventoryStatusReq3.ItemId, inventoryStatusReq3.Quantity, inventoryStatusReq3.Sku ) : string.Empty;
			var inv4 = inventoryStatusReq4 != null ? CreateInventoryStatusTag( inventoryStatusReq4.ItemId, inventoryStatusReq4.Quantity, inventoryStatusReq4.Sku ) : string.Empty;

			var body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials>{1}{2}{3}{4}</ReviseInventoryStatusRequest>",
				this._userCredentials.Token,
				inv1,
				inv2,
				inv3,
				inv4
				);
			return body;
		}

		public async Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusReq, InventoryStatusRequest inventoryStatusReq2 = null, InventoryStatusRequest inventoryStatusReq3 = null, InventoryStatusRequest inventoryStatusReq4 = null, string mark = "" )
		{
			var response = await this.GetEbaySingleRequestAsync(
				headers : CreateReviseInventoryStatusHeadersWithApiCallName(),
				body : this.CreateReviseInventoryStatusRequestBody( inventoryStatusReq, inventoryStatusReq2, inventoryStatusReq3, inventoryStatusReq4 ),
				responseParser : x => new EbayReviseInventoryStatusResponseParser().Parse( x ),
				token : CancellationToken.None,
				mark : mark,
				useCert: true
			).ConfigureAwait( false );

			response.RequestedItems = new List< InventoryStatusRequest >() { inventoryStatusReq, inventoryStatusReq2, inventoryStatusReq3, inventoryStatusReq4 }.Where( x => x != null ).ConvertTo< InventoryStatusRequest, Models.ReviseInventoryStatusResponse.Item >().ToList();
			return response;
		}

		public async Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > inventoryStatuses, string mark )
		{
			const int maxItemsPerCall = 4;

			var inventoryStatusRequests = inventoryStatuses.ToList();

			var resultResponses = new List< InventoryStatusResponse >();

			var chunks = new List< Tuple< InventoryStatusRequest, InventoryStatusRequest, InventoryStatusRequest, InventoryStatusRequest > >();

			var inventoriyStatusesCount = inventoryStatuses.Count();
			for( var i = 0; i < inventoriyStatusesCount; i += maxItemsPerCall )
			{
				var statusReq = i < inventoriyStatusesCount ? inventoryStatusRequests[ i ] : null;

				var secondInCortege = i + 1;
				var statusReq2 = secondInCortege < inventoriyStatusesCount ? inventoryStatusRequests[ secondInCortege ] : null;

				var thirdInCortege = i + 2;
				var statusReq3 = thirdInCortege < inventoriyStatusesCount ? inventoryStatusRequests[ thirdInCortege ] : null;

				var fourthInCortege = i + 3;
				var statusReq4 = fourthInCortege < inventoriyStatusesCount ? inventoryStatusRequests[ fourthInCortege ] : null;

				chunks.Add( new Tuple< InventoryStatusRequest, InventoryStatusRequest, InventoryStatusRequest, InventoryStatusRequest >( statusReq, statusReq2, statusReq3, statusReq4 ) );
			}

			var reviseInventoryStatusResponses = await chunks.ProcessInBatchAsync( this.MaxThreadsCount, x => this.ReviseInventoryStatusAsync( x.Item1, x.Item2, x.Item3, x.Item4, mark ) ).ConfigureAwait( false );
			resultResponses.AddRange( reviseInventoryStatusResponses.ToList() );

			return resultResponses;
		}
		#endregion

		#region ReviseFixedPriceItem
		private string CreateReviseFixedPriceItemRequestBody( ReviseFixedPriceItemRequest inventoryStatusReq )
		{
			var header = string.Format( "<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseFixedPriceItemRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials>", this._userCredentials.Token );
			var sku = SecurityElement.Escape( inventoryStatusReq.Sku );
			var condition = inventoryStatusReq.ConditionID > 0 ? string.Format("<ConditionID>{0}</ConditionID>", inventoryStatusReq.ConditionID) : string.Empty;
			var variationsBody = string.Empty;

			if ( inventoryStatusReq.HasVariations )
			{
				foreach( var variation in inventoryStatusReq.Variations )
				{
					variationsBody += string.Format( "<Variation><SKU>{0}</SKU><Quantity>{1}</Quantity></Variation>", SecurityElement.Escape( variation.Sku ), variation.Quantity );
				}
			}

			var body = inventoryStatusReq.HasVariations ? string.Format(
				"{0}<Item ComplexType=\"ItemType\"><ItemID>{1}</ItemID><Variations>{2}</Variations><OutOfStockControl>{3}</OutOfStockControl>{4}</Item></ReviseFixedPriceItemRequest>",
				header,
				inventoryStatusReq.ItemId,
				variationsBody,
				true,
				condition
				) :
				string.Format(
					"{0}<Item ComplexType=\"ItemType\"><ItemID>{1}</ItemID><Quantity>{2}</Quantity><SKU>{3}</SKU><OutOfStockControl>{4}</OutOfStockControl>{5}</Item></ReviseFixedPriceItemRequest>",
					header,
					inventoryStatusReq.ItemId,
					inventoryStatusReq.Quantity,
					sku,
					true,
					condition
					);
			return body;
		}

		private static Dictionary< string, string > CreateReviseFixedPriceItemHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.ReviseFixedPriceItem },
			};
		}

		public async Task< ReviseFixedPriceItemResponse > ReviseFixedPriceItemAsync( ReviseFixedPriceItemRequest fixedPriceItem, string mark )
		{
			return await this.GetEbaySingleRequestAsync(
				headers : CreateReviseFixedPriceItemHeadersWithApiCallName(),
				body : this.CreateReviseFixedPriceItemRequestBody( fixedPriceItem ),
				responseParser : x => new EbayReviseFixedPriceItemResponseParser().Parse( x ),
				token : CancellationToken.None,
				mark : mark,
				useCert: true
			).ConfigureAwait( false );
		}
		#endregion

		#region Authorization
		private static Dictionary< string, string > CreateGetSessionIDRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.GetSessionID },
			};
		}

		private string CreateGetSessionIdRequestBody( string ruName )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSessionIDRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RuName>{0}</RuName></GetSessionIDRequest>​​",
				ruName );
		}

		private static Dictionary< string, string > CreateFetchTokenRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.FetchToken },
			};
		}

		private string CreateFetchTokenRequestBody( string sessionId )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><FetchTokenRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><SessionID>{0}</SessionID></FetchTokenRequest>",
				sessionId );
		}

		public string GetSessionId( string mark )
		{
			string result = null;

			var body = this.CreateGetSessionIdRequestBody( this._ebayConfig.RuName );

			var headers = CreateGetSessionIDRequestHeadersWithApiCallName();

			ActionPolicies.Get.Do( () =>
			{
				var webRequest = this.CreateEbayStandardPostRequestWithCert( this._endPoint, headers, body, mark );

				using( var memStream = this._webRequestServices.GetResponseStream( webRequest, mark ) )
				{
					var tempSessionId = new EbayGetSessionIdResponseParser().Parse( memStream );
					if( tempSessionId != null )
						result = tempSessionId.SessionId;
				}
			} );

			return result;
		}

		public Uri GetAuthenticationUri( string sessionId )
		{
			var uri = new Uri( string.Format( "{0}?SignIn&RuName={1}&SessID={2}", this._ebaySignInUrl, this._ebayConfig.RuName, sessionId ) );
			return uri;
		}

		public void AuthenticateUser( string sessionId )
		{
			var uri = this.GetAuthenticationUri( sessionId );
			Process.Start( uri.AbsoluteUri );
		}

		public string FetchToken( string sessionId, string mark )
		{
			string result = null;

			var body = this.CreateFetchTokenRequestBody( sessionId );
			var headers = CreateFetchTokenRequestHeadersWithApiCallName();
			var webRequest = this.CreateEbayStandardPostRequestWithCert( this._endPoint, headers, body, mark );

			using( var memStream = this._webRequestServices.GetResponseStream( webRequest, mark ) )
			{
				var tempResponse = new EbayFetchTokenResponseParser().Parse( memStream );
				if( tempResponse != null && tempResponse.Errors == null )
					result = tempResponse.EbayAuthToken;
			}

			return result;
		}
		#endregion

		#region JobService
		public async Task< CreateJobResponse > CreateUploadJobAsync( Guid guid, string mark )
		{
			var result = new CreateJobResponse();

			var body = this.CreateCreateUploadJobRequestBody( guid, UploadJobTypeEnum.ReviseInventoryStatus );

			var headers = this.CreateCreateUploadJobRequestHeaders();

			await ActionPolicies.GetAsync.Do( async () =>
			{
				var webRequest = await this.CreateEbayStandardPostRequestToBulkExchangeServerAsync( this._endPointBulkExhange, headers, body ).ConfigureAwait( false );

				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, CancellationToken.None ).ConfigureAwait( false ) )
				{
					var createJobResponseParsed = new EbayBulkCreateJobParser().Parse( memStream );
					if( createJobResponseParsed != null )
					{
						if( createJobResponseParsed.Errors != null )
						{
							result.Errors = createJobResponseParsed.Errors;
							return;
						}

						result.JobId = createJobResponseParsed.JobId;
					}
				}
			} );

			return result;
		}

		public async Task< AbortJobResponse > AbortJobAsync( string jobId, string mark )
		{
			var result = new AbortJobResponse();

			var body = this.CreateAbortJobRequestBody( jobId );

			var headers = this.CreateAbortJobRequestHeaders();

			await ActionPolicies.GetAsync.Do( async () =>
			{
				var webRequest = await this.CreateEbayStandardPostRequestToBulkExchangeServerAsync( this._endPointBulkExhange, headers, body ).ConfigureAwait( false );

				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, CancellationToken.None ).ConfigureAwait( false ) )
				{
					var abortJobResponse = new EbayBulkAbortJobParser().Parse( memStream );
					if( abortJobResponse != null )
					{
						if( abortJobResponse.Errors != null )
							result.Errors = abortJobResponse.Errors;
					}
				}
			} );

			return result;
		}

		private Dictionary< string, string > CreateAbortJobRequestHeaders()
		{
			return new Dictionary< string, string >
			{
				{ "X-EBAY-SOA-OPERATION-NAME", "abortJob" },
				{ "X-EBAY-SOA-SECURITY-TOKEN", "TOKEN" },
			};
		}

		private string CreateAbortJobRequestBody( string jobId )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><abortJobRequest xmlns=\"http://www.ebay.com/marketplace/services\"><!-- Call-specific Input Fields --><jobId>{0}</jobId></abortJobRequest>",
				jobId
				);
		}

		private Dictionary< string, string > CreateCreateUploadJobRequestHeaders()
		{
			return new Dictionary< string, string >
			{
				{ "X-EBAY-SOA-OPERATION-NAME", "createUploadJob" },
				{ "X-EBAY-SOA-SECURITY-TOKEN", "TOKEN" },
			};
		}

		private string CreateCreateUploadJobRequestBody( Guid guid, UploadJobTypeEnum uploadJobType )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><createUploadJobRequest xmlns=\"http://www.ebay.com/marketplace/services\"><fileType>XML</fileType><uploadJobType>{0}</uploadJobType><UUID>{1}</UUID></createUploadJobRequest>",
				uploadJobType.ToString(),
				guid.ToString()
				);
		}
		#endregion

		#region Generic requests
		public async Task< TResponse > GetEbaySingleRequestAsync< TResponse >( Dictionary< string, string > headers, string body, Func< Stream, TResponse > responseParser, CancellationToken token, string mark = "", bool useCert = false, int pageNumber = -1 ) where TResponse : EbayBaseResponse, new()
		{
			if( token.IsCancellationRequested )
				throw new WebException( "Task was canceled" );

			var response = new TResponse();

			var repeatsByTheReasonOfInternalError = 0;

			await ActionPolicies.GetAsyncShort.Do( async () =>
			{
				var webRequest = await ( useCert ? this.CreateEbayStandardPostRequestWithCertAsync( this._endPoint, headers, body, mark, token ) : this.CreateEbayStandardPostRequestAsync( this._endPoint, headers, body, mark, token ) ).ConfigureAwait( false );

				TResponse parsedResponse;
				var timeoutToken = CreateTimeoutLinkedToken( token, this.RequestTimeoutMs );
				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, timeoutToken ).ConfigureAwait( false ) )
				{
					if ( memStream != null ) 
					{ 
						parsedResponse = responseParser( memStream );
					} 
					else
					{
						var pageNumberLog = pageNumber != -1 ? $" for page{pageNumber}" : "";
						throw new EbayCommonException( $"Blank memory stream is returned{pageNumberLog}; Mark:{mark}" );
					}
				}

				if( parsedResponse != null )
				{
					parsedResponse.IfThereAreEbayInvalidTokenErrorsDo( x =>
					{
						throw new EbayCommonException( $"eBay invalid token error occurred {repeatsByTheReasonOfInternalError} times;Mark:{mark};Errors:{x.Errors.ToJson()};Headers:{headers.ToJson()};Body:{body}" );
					} );

					if( parsedResponse.Errors != null )
					{
						parsedResponse.IfThereAreEbayInternalErrorsDo( x =>
						{
							if( repeatsByTheReasonOfInternalError++ < MaxRetryCountOnEbayInternalError )
								throw new EbayCommonException( $"eBay internal error occurred {repeatsByTheReasonOfInternalError} times;Mark:{mark};Errors:{x.Errors.ToJson()};Headers:{headers.ToJson()};Body:{body}" );
						} );

						var message = $"Error occurred;Mark:{mark};Headers:{headers.ToJson()};Body:{body}";
						parsedResponse.Errors.ForEach( x =>
						{
							x.UserDisplayHint = string.IsNullOrWhiteSpace( x.UserDisplayHint ) ? message : x.UserDisplayHint + ";" + message;
						} );
					}

					response = parsedResponse;
				}
			} ).ConfigureAwait( false );

			return response;
		}

		private static CancellationToken CreateTimeoutLinkedToken( CancellationToken token, int timeoutMs = MaxRequestTimeoutMs )
		{
			var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource( token );
			linkedTokenSource.CancelAfter( timeoutMs );
			return linkedTokenSource.Token;
		}

		private async Task< TResponse > GetEbayMultiPageRequestAsync< TResponse >( Dictionary< string, string > headers, Func< int, string > getRequestBodyByPageNumber, Func< Stream, TResponse > responseParser, CancellationToken cts, string mark = "", bool useCert = false ) where TResponse : EbayBaseResponse, IPaginationResponse< TResponse >, new()
		{
			var response = new TResponse();
			var errorList = new List< ResponseError >();

			var pageNumber = 1;
			var hasMorePages = false;

			do
			{
				var page = await this.GetEbaySingleRequestAsync(
					headers : headers,
					body : getRequestBodyByPageNumber( pageNumber ),
					responseParser : responseParser,
					token : cts,
					mark : mark,
					useCert : useCert,
					pageNumber: pageNumber
				).ConfigureAwait( false );

				if( page.Errors != null )
				{
					errorList.AddRange( page.Errors );
				}
				else
				{
					hasMorePages = page.HasMorePages;
					response.AddObjectsFromPage( page );
				}

				pageNumber++;
			} while( hasMorePages );

			if( errorList.Count > 0 )
			{
				response.Errors = errorList;
			}

			return response;
		}
		#endregion
	}

	internal enum UploadJobTypeEnum
	{
		ReviseInventoryStatus
	}

	internal enum GetSellerListTimeRangeEnum
	{
		StartTime,
		EndTime
	}

	internal enum GetOrdersTimeRangeEnum
	{
		CreateTime,
		ModTime
	}
}