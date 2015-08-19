using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Misc;
using EbayAccess.Models;
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
using Order = EbayAccess.Models.GetSellingManagerSoldListingsResponse.Order;

namespace EbayAccess.Services
{
	internal sealed class EbayServiceLowLevel : IEbayServiceLowLevel
	{
		private readonly EbayUserCredentials _userCredentials;
		private readonly EbayConfig _ebayConfig;
		private readonly string _endPoint;
		private readonly int _itemsPerPage;
		private readonly IWebRequestServices _webRequestServices;
		private readonly string _ebaySignInUrl;
		private readonly string _endPointBulkExhange;

		public int MaxThreadsCount
		{
			get { return 18; }
		}

		public Func< string > AdditionalLogInfo { get; set; }

		public EbayServiceLowLevel( EbayUserCredentials credentials, EbayConfig ebayConfig, IWebRequestServices webRequestServices )
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
		}

		public EbayServiceLowLevel( EbayUserCredentials userCredentials, EbayConfig ebayConfig )
			: this( userCredentials, ebayConfig, new WebRequestServices() )
		{
		}

		#region EbayStandartRequest
		private async Task< WebRequest > CreateEbayStandartPostRequestAsync( string url, Dictionary< string, string > headers, string body, string mark, CancellationToken cts )
		{
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

		public WebRequest CreateEbayStandartPostRequest( string url, Dictionary< string, string > headers, string body, string mark )
		{
			var resultTask = this.CreateEbayStandartPostRequestAsync( url, headers, body, mark, CancellationToken.None );
			resultTask.Wait();
			return resultTask.Result;
		}

		public async Task<WebRequest> CreateEbayStandartPostRequestWithCertAsync(string url, Dictionary<string, string> headers, string body, string mark, CancellationToken cts)
		{
			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiCertName ) )
				headers.Add( EbayHeaders.XEbayApiCertName, this._ebayConfig.CertName );

			return await this.CreateEbayStandartPostRequestAsync( url, headers, body, mark, cts ).ConfigureAwait( false );
		}

		public async Task< WebRequest > CreateEbayStandartPostRequestToBulkExchangeServerAsync( string url, Dictionary< string, string > headers, string body, string mark = "" )
		{
			return await this._webRequestServices.CreateServicePostRequestAsync( url, body, headers, CancellationToken.None, mark ).ConfigureAwait( false );
		}

		public WebRequest CreateEbayStandartPostRequestWithCert( string url, Dictionary< string, string > headers, string body, string mark )
		{
			var requestTask = this.CreateEbayStandartPostRequestWithCertAsync( url, headers, body, mark, CancellationToken.None );
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
			};
		}

		public async Task< GetOrdersResponse > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo, GetOrdersTimeRangeEnum getOrdersTimeRangeEnum, string mark = "" )
		{
			var orders = new GetOrdersResponse();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			var hasMoreOrders = false;

			do
			{
				var headers = CreateEbayGetOrdersRequestHeadersWithApiCallName();

				var body = this.CreateGetOrdersRequestBody( createTimeFrom, createTimeTo, recordsPerPage, pageNumber, getOrdersTimeRangeEnum );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body, mark, CancellationToken.None ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, CancellationToken.None ).ConfigureAwait( false ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var getOrdersResponseParsed = new EbayGetOrdersResponseParser().Parse( memStream );
						if( getOrdersResponseParsed != null )
						{
							if( getOrdersResponseParsed.Errors != null )
							{
								orders.Errors = getOrdersResponseParsed.Errors;
								return;
							}
							hasMoreOrders = getOrdersResponseParsed.HasMoreOrders;
							if( getOrdersResponseParsed.Orders != null )
								orders.Orders.AddRange( getOrdersResponseParsed.Orders );
						}
					}
				} ).ConfigureAwait( false );

				pageNumber++;
			} while( hasMoreOrders );

			return orders;
		}

		public async Task< GetOrdersResponse > GetOrdersAsync( string mark = "", params string[] ordersIds )
		{
			var orders = new GetOrdersResponse();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			var hasMoreOrders = false;

			do
			{
				var headers = CreateEbayGetOrdersRequestHeadersWithApiCallName();

				var body = this.CreateGetOrdersRequestBody( recordsPerPage, pageNumber, ordersIds );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body, mark, CancellationToken.None ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, CancellationToken.None ).ConfigureAwait( false ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var getOrdersResponseParsed = new EbayGetOrdersResponseParser().Parse( memStream );
						if( getOrdersResponseParsed != null )
						{
							if( getOrdersResponseParsed.Errors != null )
							{
								orders.Errors = getOrdersResponseParsed.Errors;
								return;
							}
							hasMoreOrders = getOrdersResponseParsed.HasMoreOrders;
							if( getOrdersResponseParsed.Orders != null )
								orders.Orders.AddRange( getOrdersResponseParsed.Orders );
						}
					}
				} ).ConfigureAwait( false );

				pageNumber++;
			} while( hasMoreOrders );

			return orders;
		}

		public async Task< GetSellingManagerSoldListingsResponse > GetSellngManagerOrderByRecordNumberAsync( string salerecordNumber, string mark, CancellationToken cts )
		{
			var orders = new GetSellingManagerSoldListingsResponse();

			orders.Orders = new List< Order >();

			var headers = CreateEbayGetSellingManagerSoldListingsRequestHeadersWithApiCallName();

			var body = this.CreateGetSellingManagerSoldListingsRequestBody( salerecordNumber );

			var repeatsByTheReasonOfInternalError = 0;

			await ActionPolicies.GetAsyncShort.Do( async () =>
			{
				var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body, mark, cts ).ConfigureAwait(false);
				
				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, cts ).ConfigureAwait( false ) )
				{
					var getOrdersResponseParsed = new EbayGetSellingManagerSoldListingsResponseParser().Parse( memStream );
					if( getOrdersResponseParsed != null )
					{
						if( getOrdersResponseParsed.Errors != null )
						{
							var internalErrors = getOrdersResponseParsed.Errors.Where( x => x.SeverityCode == "Error" && x.ErrorCode == "10007" ).ToList();
							var otherErrors = getOrdersResponseParsed.Errors.Where( x => x.SeverityCode == "Error" && x.ErrorCode != "10007" ).ToList();

							var containsOnlyInternalErrors = internalErrors.Count > 0 && otherErrors.Count == 0;

							if( repeatsByTheReasonOfInternalError++ < 3 && containsOnlyInternalErrors )
								throw new EbayCommonException( string.Format( "Occudred when getting:{0};Mark:{1};Errors:{2}", salerecordNumber, mark, internalErrors.ToJson() ) );

							var message = string.Format( "Occudred when getting:{0};Mark:{1}", salerecordNumber, mark );
							getOrdersResponseParsed.Errors.ForEach( x =>
							{
								x.UserDisplayHint = string.IsNullOrWhiteSpace( x.UserDisplayHint ) ? message : x.UserDisplayHint + ";" + message;
							} );

							orders.Errors = getOrdersResponseParsed.Errors;

							return;
						}

						if( getOrdersResponseParsed.Orders != null )
							orders.Orders.AddRange( getOrdersResponseParsed.Orders );
					}
				}
			} ).ConfigureAwait( false );

			return orders;
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

		public async Task< GetSellerListResponse > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark )
		{
			var items = new GetSellerListResponse();

			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			var hasMoreItems = false;
			do
			{
				var body = this.CreateGetSellerListRequestBody( timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, pageNumber );

				var headers = CreateGetSellerListRequestHeadersWithApiCallName();

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body, mark, CancellationToken.None ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, CancellationToken.None ).ConfigureAwait( false ) )
					{
						var getSellerListResponse = new EbayGetSallerListResponseParser().Parse( memStream );
						if( getSellerListResponse != null )
						{
							if( getSellerListResponse.Errors != null )
							{
								items.Errors = getSellerListResponse.Errors;
								return;
							}
							hasMoreItems = getSellerListResponse.HasMoreItems;
							if( getSellerListResponse.Items != null )
								items.Items.AddRange( getSellerListResponse.Items );
						}
					}
				} ).ConfigureAwait( false );

				pageNumber++;
			} while( hasMoreItems );

			return items;
		}

		public async Task< GetSellerListCustomResponse > GetSellerListCustomAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark )
		{
			var items = new GetSellerListCustomResponse();

			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			var hasMoreItems = false;
			do
			{
				var body = this.CreateGetSellerListCustomRequestBody( timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, pageNumber );

				var headers = CreateGetSellerListRequestHeadersWithApiCallName();

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body, mark, CancellationToken.None ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, CancellationToken.None ).ConfigureAwait( false ) )
					{
						var getSellerListResponse = new EbayGetSallerListCustomResponseParser().Parse( memStream );
						if( getSellerListResponse != null )
						{
							if( getSellerListResponse.Errors != null )
							{
								items.Errors = getSellerListResponse.Errors;
								return;
							}
							hasMoreItems = getSellerListResponse.HasMoreItems;
							if( getSellerListResponse.Items != null )
								items.Items.AddRange( getSellerListResponse.Items );
						}
					}
				} ).ConfigureAwait( false );

				pageNumber++;
			} while( hasMoreItems );

			return items;
		}

		public async Task< IEnumerable< GetSellerListCustomResponse > > GetSellerListCustomResponsesWithMaxThreadsRestrictionAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, string mark )
		{
			var recordsPerPage = this._itemsPerPage;
			const int pageNumber = 1;

			var getSellerListResponse = await this.GetSellerListCustomResponseAsync( timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, pageNumber, mark ).ConfigureAwait( false );

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
					var getSellerListCustomResponsesTemp = await pages.ProcessInBatchAsync( MaxThreadsCount, async x => await this.GetSellerListCustomResponseAsync( timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, x, mark ).ConfigureAwait( false ) ).ConfigureAwait( false );
					getSellerListCustomResponses.AddRange( getSellerListCustomResponsesTemp );
				}
			}

			return getSellerListCustomResponses.Where( x => x != null ).ToList();
		}

		private async Task< GetSellerListCustomResponse > GetSellerListCustomResponseAsync( DateTime timeFrom, DateTime timeTo, GetSellerListTimeRangeEnum getSellerListTimeRangeEnum, int recordsPerPage, int pageNumber, string mark )
		{
			var body = this.CreateGetSellerListCustomRequestBody( timeFrom, timeTo, getSellerListTimeRangeEnum, recordsPerPage, pageNumber );

			var headers = CreateGetSellerListRequestHeadersWithApiCallName();

			GetSellerListCustomResponse getSellerListResponse = null;

			await ActionPolicies.SubmitAsync.Do( async () =>
			{
				var webRequest = await this.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body, mark, CancellationToken.None ).ConfigureAwait( false );

				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, CancellationToken.None ).ConfigureAwait( false ) )
					getSellerListResponse = new EbayGetSallerListCustomResponseParser().Parse( memStream );
			} ).ConfigureAwait( false );

			return getSellerListResponse;
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
			var order = new Item();

			var body = this.CreateGetItemByIdRequestBody( id );

			var headers = CreateGetItemRequestHeadersWithApiCallName();

			await ActionPolicies.GetAsync.Do( async () =>
			{
				var webRequest = await this.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body, mark, CancellationToken.None ).ConfigureAwait( false );

				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest, mark, CancellationToken.None ).ConfigureAwait( false ) )
				{
					var tempOrders = new EbayGetItemResponseParser().Parse( memStream );
					if( tempOrders != null )
						order = tempOrders.Item;
				}
			} );

			return order;
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

		private string CreateReviseInventoryStatusRequestBody( long? itemIdMonad, long? quantityMonad, string sku )
		{
			var inventoryStatus = CreateInventoryStatusTag( itemIdMonad, quantityMonad, sku );

			var body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials>{1}</ReviseInventoryStatusRequest>",
				this._userCredentials.Token,
				inventoryStatus
				);
			return body;
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

		public InventoryStatusResponse ReviseInventoryStatus( InventoryStatusRequest inventoryStatusReq, string mark )
		{
			var headers = CreateReviseInventoryStatusHeadersWithApiCallName();

			var body = this.CreateReviseInventoryStatusRequestBody( inventoryStatusReq.ItemId, inventoryStatusReq.Quantity, inventoryStatusReq.Sku );

			var request = this.CreateEbayStandartPostRequestWithCert( this._endPoint, headers, body, mark );

			using( var memStream = this._webRequestServices.GetResponseStream( request, mark ) )
			{
				var inventoryStatusResponse =
					new EbayReviseInventoryStatusResponseParser().Parse( memStream );
				return inventoryStatusResponse;
			}
		}

		public async Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusReq, InventoryStatusRequest inventoryStatusReq2 = null, InventoryStatusRequest inventoryStatusReq3 = null, InventoryStatusRequest inventoryStatusReq4 = null, string mark = "" )
		{
			var headers = CreateReviseInventoryStatusHeadersWithApiCallName();

			var body = this.CreateReviseInventoryStatusRequestBody( inventoryStatusReq, inventoryStatusReq2, inventoryStatusReq3, inventoryStatusReq4 );

			var request = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body, mark, CancellationToken.None ).ConfigureAwait( false );

			using( var memStream = await this._webRequestServices.GetResponseStreamAsync( request, mark, CancellationToken.None ).ConfigureAwait( false ) )
			{
				var inventoryStatusResponse =
					new EbayReviseInventoryStatusResponseParser().Parse( memStream );
				return inventoryStatusResponse;
			}
		}

		public IEnumerable< InventoryStatusResponse > ReviseInventoriesStatus( IEnumerable< InventoryStatusRequest > inventoryStatuses, string mark )
		{
			return
				inventoryStatuses.Select(
					productToUpdate => ActionPolicies.Submit.Get( () => this.ReviseInventoryStatus( productToUpdate, mark ) ) )
					.Where( productUpdated => productUpdated != null )
					.ToList();
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
		private string CreateReviseFixedPriceItemRequestBody( ReviseFixedPriceItemRequest inventoryStatusReq, bool isVariation )
		{
			var sku = string.Format( "<SKU>{0}</SKU>", SecurityElement.Escape( inventoryStatusReq.Sku ) );
			var condition = inventoryStatusReq.ConditionID > 0 ? string.Format("<ConditionID>{0}</ConditionID>", inventoryStatusReq.ConditionID) : string.Empty;
			var body = isVariation ? string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseFixedPriceItemRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><Item ComplexType=\"ItemType\"><ItemID>{1}</ItemID><Variations><Variation>{2}<Quantity>{3}</Quantity></Variation></Variations><OutOfStockControl>{4}</OutOfStockControl>{5}</Item></ReviseFixedPriceItemRequest>",
				this._userCredentials.Token,
				inventoryStatusReq.ItemId,
				sku,
				inventoryStatusReq.Quantity,
				true,
				condition
				) :
				string.Format(
					"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseFixedPriceItemRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><Item ComplexType=\"ItemType\"><ItemID>{1}</ItemID><Quantity>{2}</Quantity>{3}<OutOfStockControl>{4}</OutOfStockControl>{5}</Item></ReviseFixedPriceItemRequest>",
					this._userCredentials.Token,
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

		public async Task< ReviseFixedPriceItemResponse > ReviseFixedPriceItemAsync( ReviseFixedPriceItemRequest fixedPriceItem, string mark, bool isVariation )
		{
			var headers = CreateReviseFixedPriceItemHeadersWithApiCallName();

			var body = this.CreateReviseFixedPriceItemRequestBody( fixedPriceItem, isVariation );

			var request = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body, mark, CancellationToken.None ).ConfigureAwait( false );

			using( var memStream = await this._webRequestServices.GetResponseStreamAsync( request, mark, CancellationToken.None ).ConfigureAwait( false ) )
			{
				var inventoryStatusResponse =
					new EbayReviseFixedPriceItemResponseParser().Parse( memStream );
				return inventoryStatusResponse;
			}
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
				var webRequest = this.CreateEbayStandartPostRequestWithCert( this._endPoint, headers, body, mark );

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
			var webRequest = this.CreateEbayStandartPostRequestWithCert( this._endPoint, headers, body, mark );

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
				var webRequest = await this.CreateEbayStandartPostRequestToBulkExchangeServerAsync( this._endPointBulkExhange, headers, body ).ConfigureAwait( false );

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
				var webRequest = await this.CreateEbayStandartPostRequestToBulkExchangeServerAsync( this._endPointBulkExhange, headers, body ).ConfigureAwait( false );

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
				{ "X-EBAY-SOA-SECURITY-TOKEN", "AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R" },
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
				{ "X-EBAY-SOA-SECURITY-TOKEN", "AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R" },
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