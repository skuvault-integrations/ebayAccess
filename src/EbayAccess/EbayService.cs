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
				var headers = new List< Tuple< string, string > >
				{
					new Tuple< string, string >( "X-EBAY-API-CERT-NAME", this._ebayDevCredentials.CertName ),
					new Tuple< string, string >( "X-EBAY-API-CALL-NAME", "GetOrders" ),
				};

				var body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination></GetOrdersRequest>​",
						this._userCredentials.Token,
						dateFrom.ToStringUtcIso8601(),
						dateTo.ToStringUtcIso8601(),
						recordsPerPage,
						pageNumber );

				ActionPolicies.Get.Do( () =>
				{
					var webRequest = this._webRequestServices.CreateEbayStandartPostRequest( this._endPoint, headers, body );

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
				var headers = new List< Tuple< string, string > >
				{
					new Tuple< string, string >( "X-EBAY-API-CERT-NAME", this._ebayDevCredentials.CertName ),
					new Tuple< string, string >( "X-EBAY-API-CALL-NAME", "GetOrders" ),
				};

				var body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination></GetOrdersRequest>​",
						this._userCredentials.Token,
						dateFrom.ToStringUtcIso8601(),
						dateTo.ToStringUtcIso8601(),
						recordsPerPage,
						pageNumber );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this._webRequestServices.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ) )
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
			} while( ( pagination != null ) && pagination.TotalNumberOfPages > pageNumber );

			return orders;
		}
		#endregion

		#region GetItems
		//for get only actual lists, specivy startTimeFrom = curentDate,startTimeTo = curentDate+3 month
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
				var body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><StartTimeFrom>{1}</StartTimeFrom><StartTimeTo>{2}</StartTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><GranularityLevel>Fine</GranularityLevel></GetSellerListRequest>​​",
						this._userCredentials.Token,
						startTimeFrom.ToStringUtcIso8601(),
						startTimeTo.ToStringUtcIso8601(),
						recordsPerPage,
						pageNumber );

				var headers = new List< Tuple< string, string > >
				{
					new Tuple< string, string >( "X-EBAY-API-CALL-NAME", "GetSellerList" ),
				};

				ActionPolicies.Get.Do( () =>
				{
					var webRequest = this._webRequestServices.CreateEbayStandartPostRequest( this._endPoint, headers, body );

					using( var memStream = this._webRequestServices.GetResponseStream( webRequest ) )
					{
						pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = EbayItemsParser.ParseGetSallerListResponse( memStream );
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
				var body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><StartTimeFrom>{1}</StartTimeFrom><StartTimeTo>{2}</StartTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><GranularityLevel>Fine</GranularityLevel></GetSellerListRequest>​​",
						this._userCredentials.Token,
						startTimeFrom.ToStringUtcIso8601(),
						startTimeTo.ToStringUtcIso8601(),
						recordsPerPage,
						pageNumber );

				var headers = new List< Tuple< string, string > >
				{
					new Tuple< string, string >( "X-EBAY-API-CALL-NAME", "GetSellerList" ),
				};

				await ActionPolicies.GetAsync.Do( async () =>
				{
					var webRequest = await this._webRequestServices.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body );

					using( var memStream = await this._webRequestServices.GetResponseStreamAsync( webRequest ) )
					{
						pagination = new EbayPaginationResultResponseParser().Parse( memStream );
						if( pagination != null )
							totalRecords = pagination.TotalNumberOfEntries;

						var tempOrders = EbayItemsParser.ParseGetSallerListResponse( memStream );
						if( tempOrders != null )
						{
							orders.AddRange( tempOrders );
							alreadyReadRecords += tempOrders.Count;
						}
					}
				} );

				pageNumber++;
			} while( ( pagination != null ) ? pagination.TotalNumberOfPages < pageNumber : false );

			return orders;
		}
		#endregion

		#region Upload
		public InventoryStatus ReviseInventoryStatus( InventoryStatus inventoryStatus )
		{
			var headers = new List< Tuple< string, string > >
			{
				new Tuple< string, string >( "X-EBAY-API-CALL-NAME", "ReviseInventoryStatus" ),
				new Tuple< string, string >( "X-EBAY-API-CERT-NAME", this._ebayDevCredentials.CertName ),
			};

			var body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><InventoryStatus ComplexType=\"InventoryStatusType\">{1}{2}{3}</InventoryStatus></ReviseInventoryStatusRequest>",
				this._userCredentials.Token,
				inventoryStatus.ItemId.HasValue ? string.Format( "<ItemID>{0}</ItemID>", inventoryStatus.ItemId.Value ) : string.Empty,
				inventoryStatus.Quantity.HasValue
					? string.Format( "<Quantity>{0}</Quantity>", inventoryStatus.Quantity.Value )
					: string.Empty,
				string.IsNullOrWhiteSpace( inventoryStatus.Sku ) ? string.Format( "<SKU>{0}</SKU>", inventoryStatus.Sku ) : string.Empty
				);

			var request = this._webRequestServices.CreateEbayStandartPostRequest( this._endPoint, headers, body );

			using( var memStream = this._webRequestServices.GetResponseStream( request ) )
			{
				var inventoryStatusResponse =
					new EbayReviseInventoryStatusResponseParser().Parse( memStream );
				return inventoryStatusResponse;
			}
		}

		public async Task< InventoryStatus > ReviseInventoryStatusAsync( InventoryStatus inventoryStatus )
		{
			var headers = new List< Tuple< string, string > >
			{
				new Tuple< string, string >( "X-EBAY-API-CALL-NAME", "ReviseInventoryStatus" ),
				new Tuple< string, string >( "X-EBAY-API-CERT-NAME", this._ebayDevCredentials.CertName ),
			};

			var body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><InventoryStatus ComplexType=\"InventoryStatusType\">{1}{2}{3}</InventoryStatus></ReviseInventoryStatusRequest>",
				this._userCredentials.Token,
				inventoryStatus.ItemId.HasValue ? string.Format( "<ItemID>{0}</ItemID>", inventoryStatus.ItemId.Value ) : string.Empty,
				inventoryStatus.Quantity.HasValue ? string.Format( "<Quantity>{0}</Quantity>", inventoryStatus.Quantity.Value ) : string.Empty,
				string.IsNullOrWhiteSpace( inventoryStatus.Sku ) ? string.Format( "<SKU>{0}</SKU>", inventoryStatus.Sku ) : string.Empty
				);

			var request = await this._webRequestServices.CreateEbayStandartPostRequestAsync( this._endPoint, headers, body );

			using( var memStream = await this._webRequestServices.GetResponseStreamAsync( request ) )
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

		public async Task< IEnumerable< InventoryStatus > > ReviseInventoriesStatusAsync(
			IEnumerable< InventoryStatus > inventoryStatuses )
		{
			//foreach (var inventoryStatus in inventoryStatuses)
			//{
			//	var productToUpdate = inventoryStatus;
			//	await ActionPolicies.SubmitAsync.Do(async () => await this.ReviseInventoryStatusAsync(productToUpdate));
			//}

			return
				await new TaskFactory< IEnumerable< InventoryStatus > >().StartNew( () => this.ReviseInventoriesStatus( inventoryStatuses ) );

			//{
			//	return inventoryStatuses.Select(
			//		productToUpdate => ActionPolicies.Submit.Get(() => this.ReviseInventoryStatus(productToUpdate)))
			//		.Where(productUpdated => productUpdated != null)
			//		.ToList();
			//});
		}
		#endregion
	}
}