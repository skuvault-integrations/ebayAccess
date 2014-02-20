using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Misc;
using EbayAccess.Models;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess
{
	public sealed class EbayService: IEbayService
	{
		private readonly EbayUserCredentials _userCredentials;
		private EbayDevCredentials _ebayDevCredentials;
		private readonly string _endPoint;
		private readonly int _itemsPerPage;
		private readonly IWebRequestServices _webRequestServices;

		public EbayService(EbayUserCredentials credentials, string endPouint, IWebRequestServices webRequestServices,
			int itemsPerPage = 50 )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();
			Condition.Ensures( endPouint, "endPoint" ).IsNotNullOrEmpty();
			Condition.Requires( webRequestServices, "webRequestServices" ).IsNotNull();

			_userCredentials = credentials;
			_webRequestServices = webRequestServices;
			_endPoint = endPouint;
			_itemsPerPage = itemsPerPage;
		}

		public EbayService(EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials, string endPouint, int itemsPerPage = 50)
			: this( userCredentials, endPouint, new WebRequestServices( userCredentials,ebayDevCredentials ), itemsPerPage )
		{
			Condition.Requires(ebayDevCredentials, "ebayDevCredentials").IsNotNull();
			_ebayDevCredentials = ebayDevCredentials;
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

		public IEnumerable<Order> GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var orders = new List<Order>();
			PaginationResult pagination;

			int totalRecords = 0;
			int alreadyReadRecords = 0;
			int recordsPerPage = _itemsPerPage;
			int pageNumber = 1;

			do
			{
				var headers = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("X-EBAY-API-CERT-NAME", _ebayDevCredentials.CertName),
					new Tuple<string, string>("X-EBAY-API-CALL-NAME", "GetOrders"),
				};

				string body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination></GetOrdersRequest>​",
						_userCredentials.Token,
						dateFrom.ToString( "O" ).Substring( 0, 23 ) + "Z",
						dateTo.ToString( "O" ).Substring( 0, 23 ) + "Z",
						recordsPerPage,
						pageNumber );

				ActionPolicies.Get.Do( () =>
				{
					WebRequest webRequest = _webRequestServices.CreateEbayStandartPostRequest( _endPoint, headers, body );

					using( Stream memStream = _webRequestServices.GetResponseStream( webRequest ) )
					{
						pagination = new EbayPagesParser().ParsePaginationResultResponse( memStream );
						if( pagination != null )
						{
							totalRecords = pagination.TotalNumberOfEntries;
						}

						List<Order> tempOrders = new EbayOrdersParser().ParseOrdersResponse( memStream );
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

		public async Task<IEnumerable<Order>> GetOrdersAsync( DateTime dateFrom, DateTime dateTo )
		{
			var orders = new List<Order>();
			PaginationResult pagination = null;

			int totalRecords = 0;
			int alreadyReadRecords = 0;
			int recordsPerPage = _itemsPerPage;
			int pageNumber = 1;

			do
			{
				var headers = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("X-EBAY-API-CERT-NAME", _ebayDevCredentials.CertName),
					new Tuple<string, string>("X-EBAY-API-CALL-NAME", "GetOrders"),
				};

				string body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetOrdersRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><CreateTimeFrom>{1}</CreateTimeFrom><CreateTimeTo>{2}</CreateTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination></GetOrdersRequest>​",
						_userCredentials.Token,
						dateFrom.ToString( "O" ).Substring( 0, 23 ) + "Z",
						dateTo.ToString( "O" ).Substring( 0, 23 ) + "Z",
						recordsPerPage,
						pageNumber );

				await ActionPolicies.GetAsync.Do( async () =>
				{
					WebRequest webRequest = await _webRequestServices.CreateEbayStandartPostRequestAsync( _endPoint, headers, body );

					using( Stream memStream = await _webRequestServices.GetResponseStreamAsync( webRequest ) )
					{
						pagination = new EbayPagesParser().ParsePaginationResultResponse( memStream );
						if( pagination != null )
						{
							totalRecords = pagination.TotalNumberOfEntries;
						}

						List<Order> tempOrders = new EbayOrdersParser().ParseOrdersResponse( memStream );
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
		public IEnumerable<Item> GetItems( DateTime startTimeFrom, DateTime startTimeTo )
		{
			var orders = new List<Item>();
			PaginationResult pagination;

			int totalRecords = 0;
			int alreadyReadRecords = 0;
			int recordsPerPage = _itemsPerPage;
			int pageNumber = 1;
			do
			{
				string body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><StartTimeFrom>{1}</StartTimeFrom><StartTimeTo>{2}</StartTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><GranularityLevel>Fine</GranularityLevel></GetSellerListRequest>​​",
						_userCredentials.Token,
						startTimeFrom.ToString( "O" ).Substring( 0, 23 ) + "Z",
						startTimeTo.ToString( "O" ).Substring( 0, 23 ) + "Z",
						recordsPerPage,
						pageNumber );

				var headers = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("X-EBAY-API-CALL-NAME", "GetSellerList"),
				};

				ActionPolicies.Get.Do( () =>
				{
					WebRequest webRequest = _webRequestServices.CreateEbayStandartPostRequest( _endPoint, headers, body );

					using( Stream memStream = _webRequestServices.GetResponseStream( webRequest ) )
					{
						pagination = new EbayPagesParser().ParsePaginationResultResponse( memStream );
						if( pagination != null )
						{
							totalRecords = pagination.TotalNumberOfEntries;
						}

						List<Item> tempOrders = new EbayItemsParser().ParseGetSallerListResponse( memStream );
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

		public async Task<IEnumerable<Item>> GetItemsAsync( DateTime startTimeFrom, DateTime startTimeTo )
		{
			var orders = new List<Item>();
			PaginationResult pagination = null;

			int totalRecords = 0;
			int alreadyReadRecords = 0;
			int recordsPerPage = _itemsPerPage;
			int pageNumber = 1;
			do
			{
				string body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><StartTimeFrom>{1}</StartTimeFrom><StartTimeTo>{2}</StartTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><GranularityLevel>Fine</GranularityLevel></GetSellerListRequest>​​",
						_userCredentials.Token,
						startTimeFrom.ToString( "O" ).Substring( 0, 23 ) + "Z",
						startTimeTo.ToString( "O" ).Substring( 0, 23 ) + "Z",
						recordsPerPage,
						pageNumber );

				var headers = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("X-EBAY-API-CALL-NAME", "GetSellerList"),
				};

				await ActionPolicies.GetAsync.Do( async () =>
				{
					WebRequest webRequest = await _webRequestServices.CreateEbayStandartPostRequestAsync( _endPoint, headers, body );

					using( Stream memStream = await _webRequestServices.GetResponseStreamAsync( webRequest ) )
					{
						pagination = new EbayPagesParser().ParsePaginationResultResponse( memStream );
						if( pagination != null )
						{
							totalRecords = pagination.TotalNumberOfEntries;
						}

						List<Item> tempOrders = new EbayItemsParser().ParseGetSallerListResponse( memStream );
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
			var headers = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("X-EBAY-API-CALL-NAME", "ReviseInventoryStatus"),
				new Tuple<string, string>("X-EBAY-API-CERT-NAME", _ebayDevCredentials.CertName),
			};

			string body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><InventoryStatus ComplexType=\"InventoryStatusType\">{1}{2}{3}</InventoryStatus></ReviseInventoryStatusRequest>",
				_userCredentials.Token,
				inventoryStatus.ItemId.HasValue ? string.Format( "<ItemID>{0}</ItemID>", inventoryStatus.ItemId.Value ) : string.Empty,
				inventoryStatus.Quantity.HasValue
					? string.Format( "<Quantity>{0}</Quantity>", inventoryStatus.Quantity.Value )
					: string.Empty,
				string.IsNullOrWhiteSpace( inventoryStatus.Sku ) ? string.Format( "<SKU>{0}</SKU>", inventoryStatus.Sku ) : string.Empty
				);

			WebRequest request = _webRequestServices.CreateEbayStandartPostRequest( _endPoint, headers, body );

			using( Stream memStream = _webRequestServices.GetResponseStream( request ) )
			{
				InventoryStatus inventoryStatusResponse =
					new EbayInventoryStatusParser().ParseReviseInventoryStatusResponse( memStream );
				return inventoryStatusResponse;
			}
		}

		public async Task<InventoryStatus> ReviseInventoryStatusAsync( InventoryStatus inventoryStatus )
		{
			var headers = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("X-EBAY-API-CALL-NAME", "ReviseInventoryStatus"),
				new Tuple<string, string>("X-EBAY-API-CERT-NAME", _ebayDevCredentials.CertName),
			};

			string body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><InventoryStatus ComplexType=\"InventoryStatusType\">{1}{2}{3}</InventoryStatus></ReviseInventoryStatusRequest>",
				_userCredentials.Token,
				inventoryStatus.ItemId.HasValue ? string.Format( "<ItemID>{0}</ItemID>", inventoryStatus.ItemId.Value ) : string.Empty,
				inventoryStatus.Quantity.HasValue ? string.Format( "<Quantity>{0}</Quantity>", inventoryStatus.Quantity.Value ) : string.Empty,
				string.IsNullOrWhiteSpace( inventoryStatus.Sku ) ? string.Format( "<SKU>{0}</SKU>", inventoryStatus.Sku ) : string.Empty
				);

			WebRequest request = await _webRequestServices.CreateEbayStandartPostRequestAsync( _endPoint, headers, body );

			using( Stream memStream = await _webRequestServices.GetResponseStreamAsync( request ) )
			{
				InventoryStatus inventoryStatusResponse =
					new EbayInventoryStatusParser().ParseReviseInventoryStatusResponse( memStream );
				return inventoryStatusResponse;
			}
		}

		public IEnumerable<InventoryStatus> ReviseInventoriesStatus( IEnumerable<InventoryStatus> inventoryStatuses )
		{
			return
				inventoryStatuses.Select(
					productToUpdate => ActionPolicies.Submit.Get( () => ReviseInventoryStatus( productToUpdate ) ) )
					.Where( productUpdated => productUpdated != null )
					.ToList();
		}

		public async Task<IEnumerable<InventoryStatus>> ReviseInventoriesStatusAsync(
			IEnumerable<InventoryStatus> inventoryStatuses )
		{
			//foreach (var inventoryStatus in inventoryStatuses)
			//{
			//	var productToUpdate = inventoryStatus;
			//	await ActionPolicies.SubmitAsync.Do(async () => await this.ReviseInventoryStatusAsync(productToUpdate));
			//}

			return
				await new TaskFactory<IEnumerable<InventoryStatus>>().StartNew( () => ReviseInventoriesStatus( inventoryStatuses ) );

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