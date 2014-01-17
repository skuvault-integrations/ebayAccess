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
		//IEnumerable<EbayInventoryUploadResponse> InventoryUpload(TeapplixUploadConfig config, Stream stream);
		//Task<IEnumerable<EbayInventoryUploadResponse>> InventoryUploadAsync(TeapplixUploadConfig config, Stream stream);
		//IEnumerable<EbayOrder> GetCustomerReport(TeapplixReportConfig config);
		//Task<IEnumerable<EbayOrder>> GetCustomerReportAsync(TeapplixReportConfig config);

		IEnumerable<EbayOrder> GetOrders(DateTime dateFrom, DateTime dateTo);
	}

	public class TeapplixReportConfig
	{
	}

	public class TeapplixUploadConfig
	{
	}
}