using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EbayAccess.Models;
using EbayAccess.Services;

namespace EbayAccess
{
	public interface IEbayService
	{
		//todo: declare and realise it
		IEnumerable<EbayInventoryUploadResponse> InventoryUpload(TeapplixUploadConfig config, Stream stream);
		Task<IEnumerable<EbayInventoryUploadResponse>> InventoryUploadAsync(TeapplixUploadConfig config, Stream stream);
		IEnumerable<EbayOrder> GetOrders(DateTime dateFrom, DateTime dateTo);
	}

	public class TeapplixUploadConfig
	{
	}
}