using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Infrastructure;
using EbayAccess.Misc;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services;
using EbayAccess.Services.Parsers;
using Netco.Extensions;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess
{
	public sealed class EbayService : IEbayService
	{
		private readonly EbayUserCredentials _userCredentials;
		private readonly EbayDevCredentials _ebayDevCredentials;
		private readonly string _endPoint;
		private readonly int _itemsPerPage;
		private readonly IWebRequestServices _webRequestServices;
		private const string XEbayApiCallName = "X-EBAY-API-CALL-NAME";

		public EbayService( EbayUserCredentials credentials, EbayDevCredentials ebayDevCredentials, IWebRequestServices webRequestServices, string endPouint = "https://api.ebay.com/ws/api.dll", int itemsPerPage = 50 )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();
			Condition.Ensures( endPouint, "endPoint" ).IsNotNullOrEmpty();
			Condition.Requires( webRequestServices, "webRequestServices" ).IsNotNull();
			Condition.Requires( ebayDevCredentials, "ebayDevCredentials" ).IsNotNull();

			this._userCredentials = credentials;
			this._webRequestServices = webRequestServices;
			this._endPoint = endPouint;
			this._itemsPerPage = itemsPerPage;
			this._ebayDevCredentials = ebayDevCredentials;
		}

		public EbayService( EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials, string endPouint = "https://api.ebay.com/ws/api.dll", int itemsPerPage = 50 )
			: this( userCredentials, ebayDevCredentials, new WebRequestServices( userCredentials, ebayDevCredentials ), endPouint, itemsPerPage )
		{
		}

		#region Logging
		public void LogReportResponseError()
		{
			throw new NotImplementedException();
			//this.Log().Error( "Failed to get file for account '{0}'", this._credentials.AccountName );
		}

		public void LogUploadItemResponseError( InventoryStatusResponse response )
		{
			throw new NotImplementedException();
			//this.Log().Error( "Failed to upload item with SKU '{0}'. Status code:'{1}', message: {2}", response.Sku, response.Status, response.Message );
		}
		#endregion

		#region EbayStandartRequest
		public async Task< WebRequest > CreateEbayStandartPostRequestAsync( string url, Dictionary< string, string > headers, string body )
		{
			try
			{
				const string xEbayApiCompatibilityLevel = "X-EBAY-API-COMPATIBILITY-LEVEL";
				if( !headers.Exists( keyValuePair => keyValuePair.Key == xEbayApiCompatibilityLevel ) )
					headers.Add( xEbayApiCompatibilityLevel, "853" );

				const string xEbayApiDevName = "X-EBAY-API-DEV-NAME";
				if( !headers.Exists( keyValuePair => keyValuePair.Key == xEbayApiDevName ) )
					headers.Add( xEbayApiDevName, this._ebayDevCredentials.DevName );

				const string xEbayApiAppName = "X-EBAY-API-APP-NAME";
				if( !headers.Exists( keyValuePair => keyValuePair.Key == xEbayApiAppName ) )
					headers.Add( xEbayApiAppName, this._ebayDevCredentials.AppName );

				const string xEbayApiSiteid = "X-EBAY-API-SITEID";
				if( !headers.Exists( keyValuePair => keyValuePair.Key == xEbayApiSiteid ) )
					headers.Add( xEbayApiSiteid, "0" );

				return await this._webRequestServices.CreateServicePostRequestAsync( url, body, headers ).ConfigureAwait( false );
			}
			catch( WebException exc )
			{
				// todo: log some exceptions
				throw;
			}
		}

		public WebRequest CreateEbayStandartPostRequest( string url, Dictionary< string, string > headers, string body )
		{
			var resultTask = this.CreateEbayStandartPostRequestAsync( url, headers, body );
			resultTask.Wait();
			return resultTask.Result;
		}

		public async Task< WebRequest > CreateEbayStandartPostRequestWithCertAsync( string url, Dictionary< string, string > headers, string body )
		{
			try
			{
				const string xEbayApiCertName = "X-EBAY-API-CERT-NAME";
				if( !headers.Exists( keyValuePair => keyValuePair.Key == xEbayApiCertName ) )
					headers.Add( xEbayApiCertName, this._ebayDevCredentials.CertName );

				return await this.CreateEbayStandartPostRequestAsync( url, headers, body ).ConfigureAwait( false );
			}
			catch( WebException exc )
			{
				// todo: log some exceptions
				throw;
			}
		}

		public WebRequest CreateEbayStandartPostRequestWithCert( string url, Dictionary< string, string > headers, string body )
		{
			var requestTask = this.CreateEbayStandartPostRequestWithCertAsync( url, headers, body );
			requestTask.Wait();
			return requestTask.Result;
		}
		#endregion

		#region GetOrders
		private string CreateGetOrdersRequestBody( DateTime dateFrom, DateTime dateTo, int recordsPerPage, int pageNumber )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination></GetOrdersRequest>​",
				this._userCredentials.Token,
				dateFrom.ToStringUtcIso8601(),
				dateTo.ToStringUtcIso8601(),
				recordsPerPage,
				pageNumber );
		}

		private void PopulateOrdersItemsDetails( IEnumerable< Order > orders )
		{
			foreach( var order in orders )
			{
				foreach( var transaction in order.TransactionArray )
				{
					transaction.Item.ItemDetails = this.GetItem( transaction.Item.ItemId );
				}
			}
		}

		private static Dictionary< string, string > CreateEbayGetOrdersRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ XEbayApiCallName, "GetOrders" },
			};
		}

		public IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo, bool includeDetails = false )
		{
			var orders = new List< Order >();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;

			do
			{
				var headers = CreateEbayGetOrdersRequestHeadersWithApiCallName();

				var body = this.CreateGetOrdersRequestBody( dateFrom, dateTo, recordsPerPage, pageNumber );

				ActionPolicies.Get.Do( () =>
				{
					var webRequest = this.CreateEbayStandartPostRequestWithCert( this._endPoint, headers, body );

					using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = new EbayGetOrdersResponseParser().Parse( memStream );
						if( tempOrders != null )
							orders.AddRange( tempOrders.Orders );
					}
				} );

				pageNumber++;
			} while( orders.Count < totalRecords );

			if( includeDetails )
				this.PopulateOrdersItemsDetails( orders );
			return orders;
		}

		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo, bool includeDetails = false )
		{
			var orders = new List< Order >();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;

			do
			{
				var headers = CreateEbayGetOrdersRequestHeadersWithApiCallName();

				var body = this.CreateGetOrdersRequestBody( dateFrom, dateTo, recordsPerPage, pageNumber );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = new EbayGetOrdersResponseParser().Parse( memStream );
						if( tempOrders != null )
							orders.AddRange( tempOrders.Orders );
					}
				} ).ConfigureAwait( false );

				pageNumber++;
				//} while( ( pagination != null ) && pagination.TotalNumberOfPages > pageNumber );
			} while( totalRecords > orders.Count() );

			if( includeDetails )
				this.PopulateOrdersItemsDetails( orders );
			return orders;
		}
		#endregion

		#region GetItems
		private static Dictionary< string, string > CreateGetItemsRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ XEbayApiCallName, "GetSellerList" },
			};
		}

		private string CreateGetItemsRequestBody( DateTime startTimeFrom, DateTime startTimeTo, int recordsPerPage, int pageNumber )
		{
			return string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><StartTimeFrom>{1}</StartTimeFrom><StartTimeTo>{2}</StartTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><GranularityLevel>Fine</GranularityLevel></GetSellerListRequest>​​",
				this._userCredentials.Token,
				startTimeFrom.ToStringUtcIso8601(),
				startTimeTo.ToStringUtcIso8601(),
				recordsPerPage,
				pageNumber );
		}

		public IEnumerable< Item > GetItems( DateTime startTimeFrom, DateTime startTimeTo )
		{
			var orders = new List< Item >();

			var totalRecords = 0;
			var alreadyReadRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			do
			{
				var body = this.CreateGetItemsRequestBody( startTimeFrom, startTimeTo, recordsPerPage, pageNumber );

				var headers = CreateGetItemsRequestHeadersWithApiCallName();

				ActionPolicies.Get.Do( () =>
				{
					var webRequest = this.CreateEbayStandartPostRequest( this._endPoint, headers, body );

					using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = new EbayGetSallerListResponseParser().Parse( memStream );
						if( tempOrders != null )
						{
							orders.AddRange( tempOrders.Items );
							alreadyReadRecords += tempOrders.Items.Count;
						}
					}
				} );

				pageNumber++;
			} while( alreadyReadRecords < totalRecords );

			return orders;
		}

		public async Task< IEnumerable< Item > > GetItemsAsync( DateTime startTimeFrom, DateTime startTimeTo )
		{
			var orders = new List< Item >();

			var totalRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			do
			{
				var body = this.CreateGetItemsRequestBody( startTimeFrom, startTimeTo, recordsPerPage, pageNumber );

				var headers = CreateGetItemsRequestHeadersWithApiCallName();

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
					{
						var pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = new EbayGetSallerListResponseParser().Parse( memStream );
						if( tempOrders != null )
							orders.AddRange( tempOrders.Items );
					}
				} ).ConfigureAwait( false );

				pageNumber++;
			} while( orders.Count < totalRecords );

			return orders;
		}
		#endregion

		#region GetItem
		private static Dictionary< string, string > CreateGetItemRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ XEbayApiCallName, "GetItem" },
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
		#endregion

		#region Upload
		private string CreateReviseInventoryStatusRequestBody( long? itemIdMonad, long? quantityMonad, string skuMonad )
		{
			var itemIdElement = itemIdMonad.HasValue ? string.Format( "<ItemID>{0}</ItemID>", itemIdMonad.Value ) : string.Empty;
			var quantityElement = quantityMonad.HasValue
				? string.Format( "<Quantity>{0}</Quantity>", quantityMonad.Value )
				: string.Empty;

			var skuElement = string.IsNullOrWhiteSpace( skuMonad ) ? string.Format( "<SKU>{0}</SKU>", skuMonad ) : string.Empty;

			var body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><InventoryStatus ComplexType=\"InventoryStatusType\">{1}{2}{3}</InventoryStatus></ReviseInventoryStatusRequest>",
				this._userCredentials.Token,
				itemIdElement,
				quantityElement,
				skuElement
				);
			return body;
		}

		private static Dictionary< string, string > CreateReviseInventoryStatusHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ XEbayApiCallName, "ReviseInventoryStatus" },
			};
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
	}
}