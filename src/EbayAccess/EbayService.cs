using System;
using System.Collections.Generic;
using System.Data;
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
		private string Boundary;
		private byte[] BoundaryBytes;
		private byte[] Trailer;
		private byte[] FormItemBytes;
		private byte[] HeaderBytes;

		public EbayService(EbayCredentials credentials, string endPouint)
		{
			Condition.Requires(credentials, "credentials").IsNotNull();
			Condition.Ensures(endPouint, "endPoint").IsNotNullOrEmpty();

			this._webRequestServices = new WebRequestServices( credentials );
			this._credentials = credentials;
			this._endPoint = endPouint;
		}

		#region Upload
		//public IEnumerable< EbayInventoryUploadResponse > InventoryUpload( TeapplixUploadConfig config, Stream file )
		//{
		//	IEnumerable< EbayInventoryUploadResponse > result = null;
		//	var request = this._services.CreateServicePostRequest( config.GetServiceUrl( this._credentials ), this.Boundary );

		//	using( var requestStream = request.GetRequestStream() )
		//	{
		//		requestStream.Write( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

		//		requestStream.Write( this.FormItemBytes, 0, this.FormItemBytes.Length );

		//		requestStream.Write( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

		//		requestStream.Write( this.HeaderBytes, 0, this.HeaderBytes.Length );

		//		file.CopyTo( requestStream );

		//		requestStream.Write( this.Trailer, 0, this.Trailer.Length );
		//	}

		//	ActionPolicies.TeapplixSubmitPolicy.Do( () =>
		//		{
		//			result = this._services.GetUploadResult( request );
		//		} );
		//	this.CheckTeapplixUploadSuccess( result );

		//	return result;
		//}

		//public async Task< IEnumerable< EbayInventoryUploadResponse > > InventoryUploadAsync( TeapplixUploadConfig config, Stream stream )
		//{
		//	var request = this._services.CreateServicePostRequest( config.GetServiceUrl( this._credentials ), this.Boundary );

		//	using( var requestStream = await request.GetRequestStreamAsync() )
		//	{
		//		await requestStream.WriteAsync( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

		//		await requestStream.WriteAsync( this.FormItemBytes, 0, this.FormItemBytes.Length );

		//		await requestStream.WriteAsync( this.BoundaryBytes, 0, this.BoundaryBytes.Length );

		//		await requestStream.WriteAsync( this.HeaderBytes, 0, this.HeaderBytes.Length );

		//		await stream.CopyToAsync( requestStream );

		//		await requestStream.WriteAsync( this.Trailer, 0, this.Trailer.Length );
		//	}

		//	var result = await this._services.GetUploadResultAsync( request );
		//	this.CheckTeapplixUploadSuccess( result );

		//	return result;
		//}
		
		#endregion

		#region Report

		//public IEnumerable< TeapplixOrder > GetCustomerReport( TeapplixReportConfig config )
		//{
		//	var reequest = this._services.CreateServiceGetRequest( config.GetServiceUrl( this._credentials ) );

		//	using( var response = reequest.GetResponse() )
		//	using( var responseStream = response.GetResponseStream() )
		//	{
		//		if( responseStream == null )
		//		{
		//			this.LogReportResponseError();
		//			return Enumerable.Empty< TeapplixOrder >();
		//		}

		//		var memStream = new MemoryStream();
		//		responseStream.CopyTo( memStream, 0x1000 );

		//		var orders = ActionPolicies.TeapplixGetPolicy.Get( () => this._services.GetParsedOrders( memStream ) );
		//		return orders;
		//	}
		//}

		//public async Task< IEnumerable< TeapplixOrder > > GetCustomerReportAsync( TeapplixReportConfig config )
		//{
		//	var tokenSource = new CancellationTokenSource();
		//	var token = tokenSource.Token;
		//	var reequest = this._services.CreateServiceGetRequest( config.GetServiceUrl( this._credentials ) );

		//	using( var response = await reequest.GetResponseAsync() )
		//	using( var responseStream = response.GetResponseStream() )
		//	{
		//		if( responseStream == null )
		//		{
		//			this.LogReportResponseError();
		//			return Enumerable.Empty< TeapplixOrder >();
		//		}

		//		var memStream = new MemoryStream();
		//		await responseStream.CopyToAsync( memStream, 0x1000, token );

		//		return this._services.GetParsedOrders( memStream );
		//	}
		//}
		
		#endregion

		//private void CheckTeapplixUploadSuccess( IEnumerable< EbayInventoryUploadResponse > uploadResponse )
		//{
		//	foreach( var item in uploadResponse )
		//	{
		//		if( item.Status != InventoryUploadStatusEnum.Success )
		//			this.LogUploadItemResponseError( item );
		//	}
		//}

		//private void InitUploadElements()
		//{
		//	this.Boundary = DateTime.Now.Ticks.ToString("x");
		//	this.BoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + this.Boundary + "\r\n");
		//	this.Trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + this.Boundary + "--\r\n");
		//	this.FormItemBytes = TeapplixUploadTemplates.GetFormDataTemplate();
		//	this.HeaderBytes = TeapplixUploadTemplates.GetHeaderTemplate();
		//}

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

		public IEnumerable<EbayOrder> GetOrders(DateTime dateFrom, DateTime dateTo)
		{
			var orders = new List<EbayOrder>();

			ActionPolicies.Get.Do(() =>
			{
				orders = this._webRequestServices.GetOrders(_endPoint,dateFrom,dateTo);
			});


			return orders;
		}
	}
}