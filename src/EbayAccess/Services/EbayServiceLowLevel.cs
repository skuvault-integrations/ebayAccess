using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Misc;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services.Parsers;
using Netco.Extensions;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	public sealed class EbayServiceLowLevel : IEbayServiceLowLevel
	{
		private readonly EbayUserCredentials _userCredentials;
		private readonly EbayConfig _ebayConfig;
		private readonly string _endPoint;
		private readonly int _itemsPerPage;
		private readonly IWebRequestServices _webRequestServices;
		private string _ebaySignInUrl;

		public EbayServiceLowLevel( EbayUserCredentials credentials, EbayConfig ebayConfig, IWebRequestServices webRequestServices )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();
			Condition.Requires( webRequestServices, "webRequestServices" ).IsNotNull();
			Condition.Requires( ebayConfig, "ebayDevCredentials" ).IsNotNull();

			this._userCredentials = credentials;
			this._webRequestServices = webRequestServices;
			this._endPoint = ebayConfig.EndPoint;
			this._ebaySignInUrl = ebayConfig.SignInUrl;
			this._itemsPerPage = 50;
			this._ebayConfig = ebayConfig;
		}

		public EbayServiceLowLevel( EbayUserCredentials userCredentials, EbayConfig ebayConfig )
			: this( userCredentials, ebayConfig, new WebRequestServices() )
		{
		}

		#region EbayStandartRequest
		private async Task< WebRequest > CreateEbayStandartPostRequestAsync( string url, Dictionary< string, string > headers, string body )
		{
			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiCompatibilityLevel ) )
				headers.Add( EbayHeaders.XEbayApiCompatibilityLevel, EbayHeadersValues.XEbayApiCompatibilityLevel );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiDevName ) )
				headers.Add( EbayHeaders.XEbayApiDevName, this._ebayConfig.DevName );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiAppName ) )
				headers.Add( EbayHeaders.XEbayApiAppName, this._ebayConfig.AppName );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiSiteid ) )
				headers.Add( EbayHeaders.XEbayApiSiteid, EbayHeadersValues.XEbayApiSiteid );

			return await this._webRequestServices.CreateServicePostRequestAsync( url, body, headers ).ConfigureAwait( false );
		}

		public WebRequest CreateEbayStandartPostRequest( string url, Dictionary< string, string > headers, string body )
		{
			var resultTask = this.CreateEbayStandartPostRequestAsync( url, headers, body );
			resultTask.Wait();
			return resultTask.Result;
		}

		public async Task< WebRequest > CreateEbayStandartPostRequestWithCertAsync( string url, Dictionary< string, string > headers, string body )
		{
			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiCertName ) )
				headers.Add( EbayHeaders.XEbayApiCertName, this._ebayConfig.CertName );

			return await this.CreateEbayStandartPostRequestAsync( url, headers, body ).ConfigureAwait( false );
		}

		public WebRequest CreateEbayStandartPostRequestWithCert( string url, Dictionary< string, string > headers, string body )
		{
			var requestTask = this.CreateEbayStandartPostRequestWithCertAsync( url, headers, body );
			requestTask.Wait();
			return requestTask.Result;
		}
		#endregion

		#region GetOrders
		private string CreateGetOrdersRequestBody( DateTime createTimeFrom, DateTime createTimeTo, int recordsPerPage, int pageNumber )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination></GetOrdersRequest>​",
				this._userCredentials.Token,
				createTimeFrom.ToStringUtcIso8601(),
				createTimeTo.ToStringUtcIso8601(),
				recordsPerPage,
				pageNumber );
		}

		private static Dictionary< string, string > CreateEbayGetOrdersRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.GetOrders },
			};
		}

		public GetOrdersResponse GetOrders( DateTime createTimeFrom, DateTime createTimeTo )
		{
			var orders = new GetOrdersResponse();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			var hasMoreOrders = false;

			do
			{
				var headers = CreateEbayGetOrdersRequestHeadersWithApiCallName();

				var body = this.CreateGetOrdersRequestBody( createTimeFrom, createTimeTo, recordsPerPage, pageNumber );

				ActionPolicies.Get.Do( () =>
				{
					var webRequest = this.CreateEbayStandartPostRequestWithCert( this._endPoint, headers, body );

					using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var getOrdersResponseParsed = new EbayGetOrdersResponseParser().Parse( memStream );

						if( getOrdersResponseParsed != null )
						{
							if( getOrdersResponseParsed.Error != null )
							{
								orders.Error = getOrdersResponseParsed.Error;
								return;
							}

							hasMoreOrders = getOrdersResponseParsed.HasMoreOrders;
							if( getOrdersResponseParsed.Orders != null )
								orders.Orders.AddRange( getOrdersResponseParsed.Orders );
						}
					}
				} );

				pageNumber++;
			} while( hasMoreOrders );

			return orders;
		}

		public async Task< GetOrdersResponse > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo )
		{
			var orders = new GetOrdersResponse();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			var hasMoreOrders = false;

			do
			{
				var headers = CreateEbayGetOrdersRequestHeadersWithApiCallName();

				var body = this.CreateGetOrdersRequestBody( createTimeFrom, createTimeTo, recordsPerPage, pageNumber );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var getOrdersResponseParsed = new EbayGetOrdersResponseParser().Parse( memStream );
						if( getOrdersResponseParsed != null )
						{
							if( getOrdersResponseParsed.Error != null )
							{
								orders.Error = getOrdersResponseParsed.Error;
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
		#endregion

		#region GetSellerList
		private static Dictionary< string, string > CreateGetSellerListRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, EbayHeadersMethodnames.GetSellerList },
			};
		}

		private string CreateGetSellerListRequestBody( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum, int recordsPerPage, int pageNumber, GetSellerListDetailsLevelEnum detailsLevel )
		{
			switch( detailsLevel )
			{
				case GetSellerListDetailsLevelEnum.IdQtyPriceTitleSkuVariations:
					return string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><{5}From>{1}</{5}From><{5}To>{2}</{5}To><IncludeVariations>true</IncludeVariations><Pagination ComplexType=\"PaginationType\"><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination>  <DetailLevel>ReturnAll</DetailLevel><OutputSelector>PaginationResult,HasMoreItems,ItemArray.Item.SKU,ItemArray.Item.Variations,ItemArray.Item.Quantity,ItemArray.Item.Title,ItemArray.Item.ItemID,ItemArray.Item.SellingStatus.CurrentPrice</OutputSelector> </GetSellerListRequest>​​​",
						this._userCredentials.Token,
						timeFrom.ToStringUtcIso8601(),
						timeTo.ToStringUtcIso8601(),
						recordsPerPage,
						pageNumber,
						timeRangeEnum );
				default:
					return string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><{5}From>{1}</{5}From><{5}To>{2}</{5}To><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><DetailLevel>ReturnAll</DetailLevel></GetSellerListRequest>​​",
						this._userCredentials.Token,
						timeFrom.ToStringUtcIso8601(),
						timeTo.ToStringUtcIso8601(),
						recordsPerPage,
						pageNumber,
						timeRangeEnum );
			}
		}

		public GetSellerListResponse GetSellerList( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum, GetSellerListDetailsLevelEnum detailsLevel )
		{
			var items = new GetSellerListResponse();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			var hasMoreItems = false;
			do
			{
				var body = this.CreateGetSellerListRequestBody( timeFrom, timeTo, timeRangeEnum, recordsPerPage, pageNumber, detailsLevel );

				var headers = CreateGetSellerListRequestHeadersWithApiCallName();

				ActionPolicies.Get.Do( () =>
				{
					var webRequest = this.CreateEbayStandartPostRequest( this._endPoint, headers, body );

					using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var getSellerListResponse = new EbayGetSallerListResponseParser( detailsLevel ).Parse( memStream );
						if( getSellerListResponse != null )
						{
							if( getSellerListResponse.Error != null )
							{
								items.Error = getSellerListResponse.Error;
								return;
							}
							hasMoreItems = getSellerListResponse.HasMoreItems;
							if( getSellerListResponse.Items != null )
								items.Items.AddRange( getSellerListResponse.Items );
						}
					}
				} );
				pageNumber++;
			} while( hasMoreItems );

			return items;
		}

		public async Task< GetSellerListResponse > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum, GetSellerListDetailsLevelEnum detailsLevel )
		{
			var items = new GetSellerListResponse();

			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			var hasMoreItems = false;
			do
			{
				var body = this.CreateGetSellerListRequestBody( timeFrom, timeTo, timeRangeEnum, recordsPerPage, pageNumber, detailsLevel );

				var headers = CreateGetSellerListRequestHeadersWithApiCallName();

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
					{
						var getSellerListResponse = new EbayGetSallerListResponseParser( detailsLevel ).Parse( memStream );
						if( getSellerListResponse != null )
						{
							if( getSellerListResponse.Error != null )
							{
								items.Error = getSellerListResponse.Error;
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

		public Item GetItem( string id )
		{
			var order = new Item();

			var body = this.CreateGetItemByIdRequestBody( id );

			var headers = CreateGetItemRequestHeadersWithApiCallName();

			ActionPolicies.Get.Do( () =>
			{
				var webRequest = this.CreateEbayStandartPostRequest( this._endPoint, headers, body );

				using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
				{
					var tempOrders = new EbayGetItemResponseParser().Parse( memStream );
					if( tempOrders != null )
						order = tempOrders.Item;
				}
			} );

			return order;
		}

		public async Task< Item > GetItemAsync( string id )
		{
			var order = new Item();

			var body = this.CreateGetItemByIdRequestBody( id );

			var headers = CreateGetItemRequestHeadersWithApiCallName();

			await ActionPolicies.GetAsync.Do( async () =>
			{
				var webRequest = await this.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body ).ConfigureAwait( false );

				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
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
			var itemIdElement = itemIdMonad.HasValue ? string.Format( "<ItemID>{0}</ItemID>", itemIdMonad.Value ) : string.Empty;
			var quantityElement = quantityMonad.HasValue ? string.Format( "<Quantity>{0}</Quantity>", quantityMonad.Value ) : string.Empty;
			var skuElement = !string.IsNullOrWhiteSpace( sku ) ? string.Format( "<SKU>{0}</SKU>", sku ) : string.Empty;

			var body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><InventoryStatus ComplexType=\"InventoryStatusType\">{1}{2}{3}</InventoryStatus></ReviseInventoryStatusRequest>",
				this._userCredentials.Token,
				itemIdElement,
				quantityElement,
				skuElement
				);
			return body;
		}

		public InventoryStatusResponse ReviseInventoryStatus( InventoryStatusRequest inventoryStatusReq )
		{
			var headers = CreateReviseInventoryStatusHeadersWithApiCallName();

			var body = this.CreateReviseInventoryStatusRequestBody( inventoryStatusReq.ItemId, inventoryStatusReq.Quantity, inventoryStatusReq.Sku );

			var request = this.CreateEbayStandartPostRequestWithCert( this._endPoint, headers, body );

			using( var memStream = this._webRequestServices.GetResponseStream( request ) )
			{
				var inventoryStatusResponse =
					new EbayReviseInventoryStatusResponseParser().Parse( memStream );
				return inventoryStatusResponse;
			}
		}

		public async Task< InventoryStatusResponse > ReviseInventoryStatusAsync( InventoryStatusRequest inventoryStatusReq )
		{
			var headers = CreateReviseInventoryStatusHeadersWithApiCallName();

			var body = this.CreateReviseInventoryStatusRequestBody( inventoryStatusReq.ItemId, inventoryStatusReq.Quantity, inventoryStatusReq.Sku );

			var request = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body ).ConfigureAwait( false );

			using( var memStream = await this._webRequestServices.GetResponseStreamAsync( request ).ConfigureAwait( false ) )
			{
				var inventoryStatusResponse =
					new EbayReviseInventoryStatusResponseParser().Parse( memStream );
				return inventoryStatusResponse;
			}
		}

		public IEnumerable< InventoryStatusResponse > ReviseInventoriesStatus( IEnumerable< InventoryStatusRequest > inventoryStatuses )
		{
			return
				inventoryStatuses.Select(
					productToUpdate => ActionPolicies.Submit.Get( () => this.ReviseInventoryStatus( productToUpdate ) ) )
					.Where( productUpdated => productUpdated != null )
					.ToList();
		}

		public async Task< IEnumerable< InventoryStatusResponse > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatusRequest > inventoryStatuses )
		{
			var reviseInventoryTasks =
				inventoryStatuses.Select(
					productToUpdate => ActionPolicies.Submit.Get( async () => await this.ReviseInventoryStatusAsync( productToUpdate ).ConfigureAwait( false ) ) )
					.Where( productUpdated => productUpdated != null )
					.ToList();
			return await Task.WhenAll( reviseInventoryTasks ).ConfigureAwait( false );

			//var reviseInventoryStatussesTask = new List< Task<InventoryStatusResponse> >();
			//foreach (var inventoryStatus in inventoryStatuses)
			//{
			//	var tesk = ActionPolicies.SubmitAsync.Get( async () => await this.ReviseInventoryStatusAsync( inventoryStatus ).ConfigureAwait( false ) );
			//	reviseInventoryStatussesTask.Add( tesk );
			//}

			//return await Task.WhenAll(reviseInventoryStatussesTask).ConfigureAwait(false);
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

		public async Task< string > GetSessionIdAsync()
		{
			string result = null;

			var body = this.CreateGetSessionIdRequestBody( this._ebayConfig.RuName );

			var headers = CreateGetSessionIDRequestHeadersWithApiCallName();

			await ActionPolicies.GetAsync.Do( async () =>
			{
				var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body ).ConfigureAwait( false );

				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
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

		public async Task< string > FetchTokenAsync( string sessionId )
		{
			string result = null;

			var body = this.CreateFetchTokenRequestBody( sessionId );

			var headers = CreateFetchTokenRequestHeadersWithApiCallName();

			await ActionPolicies.GetAsync.Do( async () =>
			{
				var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body ).ConfigureAwait( false );

				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
				{
					var tempResponse = new EbayFetchTokenResponseParser().Parse( memStream );
					if( tempResponse != null && tempResponse.Error == null )
						result = tempResponse.EbayAuthToken;
					else
						throw new Exception( "Can't fetch token" );
				}
			} );

			return result;
		}
		#endregion
	}

	public enum TimeRangeEnum
	{
		StartTime,
		EndTime
	}

	public enum GetSellerListDetailsLevelEnum
	{
		Default,
		IdQtyPriceTitleSkuVariations
	}
}