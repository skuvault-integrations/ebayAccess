using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using EbayAccess.Misc;
using EbayAccess.Models;
using EbayAccess.Services;
using Netco.Logging;

namespace EbayAccess
{
	public sealed class EbayService : IEbayService
	{
		private readonly WebRequestServices _webRequestServices;
		private readonly EbayCredentials _credentials;
		private readonly string _endPoint;

		public EbayService( EbayCredentials credentials, string endPouint )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();
			Condition.Ensures( endPouint, "endPoint" ).IsNotNullOrEmpty();

			this._credentials = credentials;
			this._webRequestServices = new WebRequestServices( this._credentials );
			this._endPoint = endPouint;
		}

		#region Upload

		public IEnumerable< EbayInventoryUploadResponse > InventoryUpload( TeapplixUploadConfig config, Stream file )
		{
			//todo: add logic
			throw new NotImplementedException();
		}

		public async Task< IEnumerable< EbayInventoryUploadResponse > > InventoryUploadAsync( TeapplixUploadConfig config,
			Stream stream )
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

		public void LogUploadItemResponseError( EbayInventoryUploadResponse response )
		{
			throw new NotImplementedException();
			//this.Log().Error( "Failed to upload item with SKU '{0}'. Status code:'{1}', message: {2}", response.Sku, response.Status, response.Message );
		}

		#endregion

		public IEnumerable< EbayOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var orders = new List< EbayOrder >();

			ActionPolicies.Get.Do( () =>
			{
				orders = this._webRequestServices.GetOrders( _endPoint, dateFrom, dateTo );
			} );

			return orders;
		}
	}
}