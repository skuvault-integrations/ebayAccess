using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EbayAccess.Models;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;

namespace EbayAccess
{
	public interface IEbayService
	{
		//todo: declare and realise it
		IEnumerable<EbayInventoryUploadResponse> InventoryUpload(EbayUploadConfig config, Stream stream);

		Task<IEnumerable<EbayInventoryUploadResponse>> InventoryUploadAsync(EbayUploadConfig config, Stream stream);

		IEnumerable<Order> GetOrders(DateTime dateFrom, DateTime dateTo);

		InventoryStatus ReviseInventoryStatus(InventoryStatus inventoryStatus);
	}

	public class EbayUploadConfig
	{
	}
}