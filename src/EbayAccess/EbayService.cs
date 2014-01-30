using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
	public sealed class EbayService : IEbayService
	{
		private readonly IWebRequestServices _webRequestServices;
		private readonly EbayCredentials _credentials;
		private readonly string _endPoint;
		private readonly int _itemsPerPage;

		public EbayService(EbayCredentials credentials, string endPouint, IWebRequestServices webRequestServices, int itemsPerPage = 50)
		{
			Condition.Requires(credentials, "credentials").IsNotNull();
			Condition.Ensures(endPouint, "endPoint").IsNotNullOrEmpty();
			Condition.Requires(webRequestServices, "webRequestServices").IsNotNull();

			this._credentials = credentials;
			this._webRequestServices = webRequestServices;
			this._endPoint = endPouint;

			_itemsPerPage = itemsPerPage;
		}

		public EbayService(EbayCredentials credentials, string endPouint, int itemsPerPage = 50)
			: this(credentials, endPouint, new WebRequestServices(credentials), itemsPerPage)
		{
		}

		#region Upload

		public IEnumerable<EbayInventoryUploadResponse> InventoryUpload(EbayUploadConfig config, Stream file)
		{
			//todo: add logic
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<EbayInventoryUploadResponse>> InventoryUploadAsync(EbayUploadConfig config,
			Stream stream)
		{
			//todo: add logic
			throw new NotImplementedException();
		}

		#endregion

		#region Logging

		public void LogReportResponseError()
		{
			throw new NotImplementedException();
			//this.Log().Error( "Failed to get file for account '{0}'", this._credentials.AccountName );
		}

		public void LogUploadItemResponseError(EbayInventoryUploadResponse response)
		{
			throw new NotImplementedException();
			//this.Log().Error( "Failed to upload item with SKU '{0}'. Status code:'{1}', message: {2}", response.Sku, response.Status, response.Message );
		}

		#endregion

		public IEnumerable<Order> GetOrders(DateTime dateFrom, DateTime dateTo)
		{
			var orders = new List<Order>();

			ActionPolicies.Get.Do(() =>
			{
				orders = this._webRequestServices.GetOrders(_endPoint, dateFrom, dateTo);
			});

			return orders;
		}

		//for get only actual lists, specivy startTimeFrom = curentDate,startTimeTo = curentDate+3 month
		public IEnumerable<Item> GetItems(DateTime startTimeFrom, DateTime startTimeTo)
		{
			List<Item> orders = new List<Item>();
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
						this._credentials.Token,
						startTimeFrom.ToString("O").Substring(0, 23) + "Z",
						startTimeTo.ToString("O").Substring(0, 23) + "Z",
						recordsPerPage,
						pageNumber);

				var headers = new List<Tuple<string, string>>
				{
					new Tuple<string, string>("X-EBAY-API-CALL-NAME", "GetSellerList"),
				};

				ActionPolicies.Get.Do(() =>
				{
					var webRequest = this._webRequestServices.GetItemsSmart(_endPoint, headers, body);

					using (var memStream = _webRequestServices.GetResponseStream(webRequest))
					{
						pagination = new EbayPagesParser().ParsePaginationResultResponse(memStream);
						if (pagination != null)
						{
							totalRecords = pagination.TotalNumberOfEntries;
						}

						var tempOrders = new EbayItemsParser().ParseGetSallerListResponse(memStream);
						if (tempOrders != null)
						{
							orders.AddRange(tempOrders);
							alreadyReadRecords += tempOrders.Count;
						}
					}
				});

				pageNumber++;
			} while (alreadyReadRecords < totalRecords);

			return orders;
		}

		public InventoryStatus ReviseInventoryStatus(InventoryStatus inventoryStatus)
		{
			var headers = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("X-EBAY-API-CALL-NAME", "ReviseInventoryStatus"),
				new Tuple<string, string>("X-EBAY-API-CERT-NAME", "d1ee4c9c-0425-43d0-857a-a9fc36e6e6b3"),
			};

			string body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><InventoryStatus ComplexType=\"InventoryStatusType\">{1}{2}{3}</InventoryStatus></ReviseInventoryStatusRequest>",
				this._credentials.Token,
				inventoryStatus.ItemID.HasValue? string.Format( "<ItemID>{0}</ItemID>",inventoryStatus.ItemID.Value):string.Empty,
				inventoryStatus.Quantity.HasValue? string.Format( "<Quantity>{0}</Quantity>",inventoryStatus.Quantity.Value):string.Empty,
				string.IsNullOrWhiteSpace(inventoryStatus.SKU)? string.Format( "<SKU>{0}</SKU>",inventoryStatus.SKU):string.Empty
				);

			var request = _webRequestServices.GetItemsSmart(_endPoint, headers, body);

			using (var memStream = _webRequestServices.GetResponseStream(request))
			{
				var inventoryStatusResponse = new EbayInventoryStatusParser().ParseReviseInventoryStatusResponse(memStream);
				return inventoryStatusResponse;
			}
		}

		public async Task<InventoryStatus> ReviseInventoryStatusAsync(InventoryStatus inventoryStatus)
		{
			var headers = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("X-EBAY-API-CALL-NAME", "ReviseInventoryStatus"),
				new Tuple<string, string>("X-EBAY-API-CERT-NAME", "d1ee4c9c-0425-43d0-857a-a9fc36e6e6b3"),
			};

			string body = string.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><ReviseInventoryStatusRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><InventoryStatus ComplexType=\"InventoryStatusType\">{1}{2}{3}</InventoryStatus></ReviseInventoryStatusRequest>",
				this._credentials.Token,
				inventoryStatus.ItemID.HasValue ? string.Format("<ItemID>{0}</ItemID>", inventoryStatus.ItemID.Value) : string.Empty,
				inventoryStatus.Quantity.HasValue ? string.Format("<Quantity>{0}</Quantity>", inventoryStatus.Quantity.Value) : string.Empty,
				string.IsNullOrWhiteSpace(inventoryStatus.SKU) ? string.Format("<SKU>{0}</SKU>", inventoryStatus.SKU) : string.Empty
				);

			var request = await _webRequestServices.GetItemsSmartAsync(_endPoint, headers, body);

			using (var memStream = await _webRequestServices.GetResponseStream(request))
			{
				var inventoryStatusResponse = new EbayInventoryStatusParser().ParseReviseInventoryStatusResponse(memStream);
				return inventoryStatusResponse;
			}
		}

		private Stream CopyToMemoryStream(Stream source)
		{
			var memoryStrem = new MemoryStream();
			source.CopyTo(memoryStrem, 0x100);
			memoryStrem.Position = 0;
			return memoryStrem;
		}
	}
}