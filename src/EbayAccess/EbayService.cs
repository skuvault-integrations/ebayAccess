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
using EbayAccess.Services;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess
{
	public sealed class EbayService : IEbayService
	{
		private readonly WebRequestServices _webRequestServices;
		private readonly EbayCredentials _credentials;
		private readonly string _endPoint;

		public EbayService(EbayCredentials credentials, string endPouint)
		{
			Condition.Requires(credentials, "credentials").IsNotNull();
			Condition.Ensures(endPouint, "endPoint").IsNotNullOrEmpty();

			this._credentials = credentials;
			this._webRequestServices = new WebRequestServices(this._credentials);
			this._endPoint = endPouint;
		}

		#region Upload

		public IEnumerable<EbayInventoryUploadResponse> InventoryUpload(TeapplixUploadConfig config, Stream file)
		{
			//todo: add logic
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<EbayInventoryUploadResponse>> InventoryUploadAsync(TeapplixUploadConfig config,
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

		public IEnumerable<Item> GetItemsSmart(DateTime dateFrom, DateTime dateTo)
		{
			List<Item> orders = new List<Item>();
			PaginationResult pagination;

			int totalRecords = 0;
			int alreadyReadRecords = 0;
			int recordsPerPage = 1;
			int pageNumber = 1;
			do
			{
				string body =
					string.Format(
						"<?xml version=\"1.0\" encoding=\"utf-8\"?><GetSellerListRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\"><RequesterCredentials><eBayAuthToken>{0}</eBayAuthToken></RequesterCredentials><StartTimeFrom>{1}</StartTimeFrom><StartTimeTo>{2}</StartTimeTo><Pagination><EntriesPerPage>{3}</EntriesPerPage><PageNumber>{4}</PageNumber></Pagination><GranularityLevel>Fine</GranularityLevel></GetSellerListRequest>​​",
						this._credentials.Token,
						dateFrom.ToString("O").Substring(0, 23) + "Z",
						dateTo.ToString("O").Substring(0, 23) + "Z",
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

					//using (var response = (HttpWebResponse) webRequest.GetResponse())
					//{
					//	using (Stream dataStream = response.GetResponseStream())
					//	{
					//		using (var memoryStream = CopyToMemoryStream(dataStream))
					//		{
					//			pagination = new EbayPagesParser().ParsePaginationResultResponse(memoryStream);
					//			if (pagination != null)
					//			{
					//				totalRecords = pagination.TotalNumberOfEntries;
					//			}

					//			var tempOrders = new EbayItemsParser().ParseGetSallerListResponse(memoryStream);
					//			if (tempOrders != null)
					//			{
					//				orders.AddRange(tempOrders);
					//				alreadyReadRecords += tempOrders.Count;
					//			}
					//		}
					//	}
					//}
				});

				pageNumber++;
			} while (alreadyReadRecords < totalRecords);

			return orders;
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