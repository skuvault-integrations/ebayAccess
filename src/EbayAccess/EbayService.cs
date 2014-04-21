using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Infrastructure;
using EbayAccess.Misc;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using EbayAccess.Services.Parsers;
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

		public void LogUploadItemResponseError( InventoryStatus response )
		{
			throw new NotImplementedException();
			//this.Log().Error( "Failed to upload item with SKU '{0}'. Status code:'{1}', message: {2}", response.Sku, response.Status, response.Message );
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

		private static Dictionary< string, string > CreateEbayGetOrdersRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ "X-EBAY-API-CALL-NAME", "GetOrders" },
			};
		}

		public IEnumerable< Order > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var orders = new List< Order >();
			PaginationResult pagination;

			var totalRecords = 0;
			var alreadyReadRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;

			do
			{
				var headers = CreateEbayGetOrdersRequestHeadersWithApiCallName();

				var body = this.CreateGetOrdersRequestBody( dateFrom, dateTo, recordsPerPage, pageNumber );

				ActionPolicies.Get.Do( () =>
				{
					var webRequest = this._webRequestServices.CreateEbayStandartPostRequestWithCert( this._endPoint, headers, body );

					using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
					{
						pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = new EbayGetOrdersResponseParser().Parse( memStream );
						if( tempOrders != null )
						{
							orders.AddRange( tempOrders );
							alreadyReadRecords += tempOrders.Count;
						}
					}
				} );

				pageNumber++;
			} while( orders.Count < totalRecords );

			return orders;
		}

		public async Task< IEnumerable< Order > > GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var orders = new List< Order >();
			PaginationResult pagination = null;

			var totalRecords = 0;
			var alreadyReadRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;

			do
			{
				var headers = CreateEbayGetOrdersRequestHeadersWithApiCallName();

				var body = this.CreateGetOrdersRequestBody(dateFrom, dateTo, recordsPerPage, pageNumber);

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this._webRequestServices.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
					{
						pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = new EbayGetOrdersResponseParser().Parse( memStream );
						if( tempOrders != null )
						{
							orders.AddRange( tempOrders );
							alreadyReadRecords += tempOrders.Count;
						}
					}
				} ).ConfigureAwait( false );

				pageNumber++;
			} while( ( pagination != null ) && pagination.TotalNumberOfPages > pageNumber );

			return orders;
		}
		#endregion

		#region GetItems
		//for get only actual lists, specivy startTimeFrom = curentDate,startTimeTo = curentDate+3 month
		private static Dictionary< string, string > CreateGetItemsRequestHeadersWithApiCallName()
		{
			return new Dictionary< string, string >
			{
				{ "X-EBAY-API-CALL-NAME", "GetSellerList" },
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
			PaginationResult pagination;

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
					var webRequest = this._webRequestServices.CreateEbayStandartPostRequest( this._endPoint, headers, body );

					using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
					{
						pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = new EbayGetSallerListResponseParser().Parse( memStream );
						if( tempOrders != null )
						{
							orders.AddRange( tempOrders );
							alreadyReadRecords += tempOrders.Count;
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
			PaginationResult pagination = null;

			var totalRecords = 0;
			var alreadyReadRecords = 0;
			var recordsPerPage = this._itemsPerPage;
			var pageNumber = 1;
			do
			{
				var body = this.CreateGetItemsRequestBody(startTimeFrom, startTimeTo, recordsPerPage, pageNumber);

				var headers = CreateGetItemsRequestHeadersWithApiCallName();

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this._webRequestServices.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body ).ConfigureAwait( false );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ).ConfigureAwait( false ) )
					{
						pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = new EbayGetSallerListResponseParser().Parse( memStream );
						if( tempOrders != null )
						{
							orders.AddRange( tempOrders );
							alreadyReadRecords += tempOrders.Count;
						}
					}
				} ).ConfigureAwait( false );

				pageNumber++;
			} while( ( pagination != null ) ? pagination.TotalNumberOfPages < pageNumber : false );

			return orders;
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
				{ "X-EBAY-API-CALL-NAME", "ReviseInventoryStatus" },
			};
		}

		public InventoryStatus ReviseInventoryStatus( InventoryStatus inventoryStatus )
		{
			var headers = CreateReviseInventoryStatusHeadersWithApiCallName();

			var body = this.CreateReviseInventoryStatusRequestBody( inventoryStatus.ItemId, inventoryStatus.Quantity, inventoryStatus.Sku );

			var request = this._webRequestServices.CreateEbayStandartPostRequestWithCert( this._endPoint, headers, body );

			using( var memStream = this._webRequestServices.GetResponseStream( request ) )
			{
				var inventoryStatusResponse =
					new EbayReviseInventoryStatusResponseParser().Parse( memStream );
				return inventoryStatusResponse;
			}
		}

		public async Task< InventoryStatus > ReviseInventoryStatusAsync( InventoryStatus inventoryStatus )
		{
			var headers = CreateReviseInventoryStatusHeadersWithApiCallName();

			var body = this.CreateReviseInventoryStatusRequestBody( inventoryStatus.ItemId, inventoryStatus.Quantity, inventoryStatus.Sku );

			var request = await this._webRequestServices.CreateEbayStandartPostRequestWithCertAsync( this._endPoint, headers, body ).ConfigureAwait( false );

			using (var memStream = await this._webRequestServices.GetResponseStreamAsync(request).ConfigureAwait(false))
			{
				var inventoryStatusResponse =
					new EbayReviseInventoryStatusResponseParser().Parse( memStream );
				return inventoryStatusResponse;
			}
		}

		public IEnumerable< InventoryStatus > ReviseInventoriesStatus( IEnumerable< InventoryStatus > inventoryStatuses )
		{
			return
				inventoryStatuses.Select(
					productToUpdate => ActionPolicies.Submit.Get( () => this.ReviseInventoryStatus( productToUpdate ) ) )
					.Where( productUpdated => productUpdated != null )
					.ToList();
		}

		public async Task< IEnumerable< InventoryStatus > > ReviseInventoriesStatusAsync( IEnumerable< InventoryStatus > inventoryStatuses )
		{
			var reviseInventoryTasks =
				inventoryStatuses.Select(
					productToUpdate => ActionPolicies.Submit.Get( async () => await this.ReviseInventoryStatusAsync( productToUpdate ).ConfigureAwait(false) ) )
					.Where( productUpdated => productUpdated != null )
					.ToList();

			return await Task.WhenAll( reviseInventoryTasks ).ConfigureAwait( false );
		}
		#endregion
	}
}