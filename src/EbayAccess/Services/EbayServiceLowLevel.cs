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

		public EbayServiceLowLevel( EbayUserCredentials credentials, EbayConfig ebayConfig, IWebRequestServices webRequestServices )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();
			Condition.Requires( webRequestServices, "webRequestServices" ).IsNotNull();
			Condition.Requires( ebayConfig, "ebayDevCredentials" ).IsNotNull();

			this._userCredentials = credentials;
			this._webRequestServices = webRequestServices;
			this._endPoint = ebayConfig.EndPoint;
			this._itemsPerPage = 50;
			this._ebayConfig = ebayConfig;
		}

		public EbayServiceLowLevel( EbayUserCredentials userCredentials, EbayConfig ebayConfig )
			: this( userCredentials, ebayConfig, new WebRequestServices() )
		{
		}

		#region EbayStandartRequest
		public async Task< WebRequest > CreateEbayStandartPostRequestAsync( string url, Dictionary< string, string > headers, string body )
		{
			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiCompatibilityLevel ) )
				headers.Add( EbayHeaders.XEbayApiCompatibilityLevel, EbayHeadersValues.XEbayApiCompatibilityLevel );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiDevName ) )
				headers.Add( EbayHeaders.XEbayApiDevName, this._ebayConfig.DevName );

			if( !headers.Exists( keyValuePair => keyValuePair.Key == EbayHeaders.XEbayApiAppName ) )
				headers.Add(EbayHeaders.XEbayApiAppName, this._ebayConfig.AppName);

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
				{ EbayHeaders.XEbayApiCallName, "GetOrders" },
			};
		}

		public IEnumerable< Order > GetOrders( DateTime createTimeFrom, DateTime createTimeTo )
		{
			var orders = new List< Order >();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;

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

						var tempOrders = new EbayGetOrdersResponseParser().Parse( memStream );
						if( tempOrders != null && tempOrders.Orders != null )
							orders.AddRange( tempOrders.Orders );
					}
				} );

				pageNumber++;
			} while( orders.Count < totalRecords );

			return orders;
		}

		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime createTimeFrom, DateTime createTimeTo )
		{
			var orders = new List< Order >();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;

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

						var tempOrders = new EbayGetOrdersResponseParser().Parse( memStream );
						if( tempOrders != null && tempOrders.Orders != null )
							orders.AddRange( tempOrders.Orders );
					}
				} ).ConfigureAwait( false );

				pageNumber++;
				//} while( ( pagination != null ) && pagination.TotalNumberOfPages > pageNumber );
			} while( totalRecords > orders.Count() );

			return orders;
		}
		#endregion

		#region GetSellerList
		private static Dictionary< string, string > CreateGetSellerListRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, "GetSellerList" },
			};
		}

		private string CreateGetSellerListRequestBody( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum, int recordsPerPage, int pageNumber )
		{
			return string.Format(
				//"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><{5}From>{1}</{5}From><{5}To>{2}</{5}To><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><GranularityLevel>Fine</GranularityLevel></GetSellerListRequest>​​",
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><{5}From>{1}</{5}From><{5}To>{2}</{5}To><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><DetailLevel>ReturnAll</DetailLevel></GetSellerListRequest>​​",
				this._userCredentials.Token,
				timeFrom.ToStringUtcIso8601(),
				timeTo.ToStringUtcIso8601(),
				recordsPerPage,
				pageNumber,
				timeRangeEnum );
		}

		public IEnumerable< Item > GetSellerList( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum )
		{
			var orders = new List< Item >();

			var totalRecords = 0;
			var alreadyReadRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			do
			{
				var body = this.CreateGetSellerListRequestBody( timeFrom, timeTo, timeRangeEnum, recordsPerPage, pageNumber );

				var headers = CreateGetSellerListRequestHeadersWithApiCallName();

				ActionPolicies.Get.Do( () =>
				{
					var webRequest = this.CreateEbayStandartPostRequest( this._endPoint, headers, body );

					using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var getSellerListResponse = new EbayGetSallerListResponseParser().Parse( memStream );
						if( getSellerListResponse != null && getSellerListResponse.Items != null )
						{
							orders.AddRange( getSellerListResponse.Items );
							alreadyReadRecords += getSellerListResponse.Items.Count;
						}
					}
				} );

				pageNumber++;
			} while( alreadyReadRecords < totalRecords );

			return orders;
		}

		public async Task< IEnumerable< Item > > GetSellerListAsync( DateTime timeFrom, DateTime timeTo, TimeRangeEnum timeRangeEnum )
		{
			var items = new List< Item >();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			do
			{
				var body = this.CreateGetSellerListRequestBody( timeFrom, timeTo, timeRangeEnum, recordsPerPage, pageNumber );

				var headers = CreateGetSellerListRequestHeadersWithApiCallName();

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var getSellerListResponse = new EbayGetSallerListResponseParser().Parse( memStream );
						if( getSellerListResponse != null && getSellerListResponse.Items != null )
							items.AddRange( getSellerListResponse.Items );
					}
				} ).ConfigureAwait( false );

				pageNumber++;
			} while( items.Count < totalRecords );

			return items;
		}
		#endregion

		#region GetItem
		private static Dictionary< string, string > CreateGetItemRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ EbayHeaders.XEbayApiCallName, "GetItem" },
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
				{ EbayHeaders.XEbayApiCallName, "ReviseInventoryStatus" },
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
		}
		#endregion

		#region Authorization
		private static Dictionary<string, string> CreateGetSessionIDRequestHeadersWithApiCallName()
		{
			return new Dictionary<string, string>
			{
				//todo: rename to 'headersNames'
				//todo: add enum MethodsNames
				{ EbayHeaders.XEbayApiCallName, "GetSessionID" },
			};
		}

		private string CreateGetSessionIdRequestBody( string ruName )
		{
			return string.Format(
				//"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSessionIDRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><RuName>{1}</RuName></GetSessionIDRequest>​​",
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSessionIDRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RuName>{0}</RuName></GetSessionIDRequest>​​",
				//this._userCredentials.Token,
				ruName );
		}

		private static Dictionary<string, string> CreateFetchTokenRequestHeadersWithApiCallName()
		{
			return new Dictionary<string, string>
			{
				{ EbayHeaders.XEbayApiCallName, "FetchToken" },
			};
		}

		private string CreateFetchTokenRequestBody(string sessionId)
		{
			return string.Format(
				//"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSessionIDRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><RuName>{1}</RuName></GetSessionIDRequest>​​",
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><FetchTokenRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><SessionID>{0}</SessionID></FetchTokenRequest>",
				//this._userCredentials.Token,
				sessionId);
		}

		public async Task<string> GetSessionIdAsync(string ruName)
		{
			string result = null;

			var body = this.CreateGetSessionIdRequestBody(ruName);

			var headers = CreateGetSessionIDRequestHeadersWithApiCallName();

			await ActionPolicies.GetAsync.Do(async () =>
			{
				var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync(this._endPoint, headers, body).ConfigureAwait(false);

				using (var memStream = await this._webRequestServices.GetResponseStreamAsync(webRequest).ConfigureAwait(false))
				{
					var tempSessionId = new EbayGetSessionIdResponseParser().Parse(memStream);
					if (tempSessionId != null)
						result = tempSessionId.SessionId;
				}
			});

			return result;
		}

		public void AutentificateUser(string ruName, string sessionId)
		{
			//todo: move to const
			//var uri = new Uri( string.Format( "https://signin.sandbox.ebay.com/ws/eBayISAPI.dll?SignIn&RuName={0}&SessID={1}", ruName, sessionId ) );
			var uri = new Uri( string.Format( "https://signin.ebay.com/ws/eBayISAPI.dll?SignIn&RuName={0}&SessID={1}", ruName, sessionId ) );
			Process.Start(uri.AbsoluteUri);


		}

		public async Task< string> FetchTokenAsync(string sessionId)
		{
			string result = null;

			var body = this.CreateFetchTokenRequestBody(sessionId);

			var headers = CreateFetchTokenRequestHeadersWithApiCallName();

			await ActionPolicies.GetAsync.Do(async () =>
			{
				var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync(this._endPoint, headers, body).ConfigureAwait(false);

				using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
				{
					var tempResponse = new EbayFetchTokenResponseParser().Parse( memStream );
					if( tempResponse != null && tempResponse.Error == null )
						result = tempResponse.EbayAuthToken;
					else
						throw new Exception( "Can't fetch token" );
				}
			});

			return result;
		}

		#endregion
	}

	public enum TimeRangeEnum
	{
		StartTime,
		EndTime
	}
}