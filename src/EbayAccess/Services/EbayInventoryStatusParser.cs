using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.ReviseInventoryStatusRequest;

namespace EbayAccess.Services
{
	public class EbayInventoryStatusParser
	{
		public InventoryStatus ParseReviseInventoryStatusResponse( string str )
		{
			//todo: make parser
			throw new NotImplementedException();
		}

		public InventoryStatus ParseReviseInventoryStatusResponse( Stream stream )
		{
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var root = XElement.Load( stream );

				InventoryStatus inventoryStatus = null;

				if( this.ResponseContainsErrors( root, ns ) )
					return inventoryStatus = new InventoryStatus { Error = this.ResponseError };

				var elInventoryStatus = root.Element( ns + "InventoryStatus" );
				if( elInventoryStatus != null )
				{
					object temp = null;

					inventoryStatus = new InventoryStatus();

					if( this.GetElementValue( elInventoryStatus, ref temp, ns, "SKU" ) )
						inventoryStatus.Sku = ( string )temp;

					if( this.GetElementValue( elInventoryStatus, ref temp, ns, "ItemID" ) )
					{
						long tempRes;
						if( long.TryParse( ( string )temp, out tempRes ) )
							inventoryStatus.ItemId = tempRes;
					}

					if( this.GetElementValue( elInventoryStatus, ref temp, ns, "Quantity" ) )
					{
						long tempRes;
						if( long.TryParse( ( string )temp, out tempRes ) )
							inventoryStatus.Quantity = tempRes;
					}

					if( this.GetElementValue( elInventoryStatus, ref temp, ns, "StartPrice" ) )
					{
						double tempRes;
						if( double.TryParse( ( string )temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempRes ) )
							inventoryStatus.StartPrice = tempRes;
					}
				}

				return inventoryStatus;
			}
			catch( Exception ex )
			{
				var buffer = new byte[ stream.Length ];
				stream.Read( buffer, 0, ( int )stream.Length );
				var utf8Encoding = new UTF8Encoding();
				var bufferStr = utf8Encoding.GetString( buffer );
				throw new Exception( "Can't parse: " + bufferStr, ex );
			}
		}

		private bool ResponseContainsErrors( XElement root, XNamespace ns )
		{
			var isSuccess = root.Element( ns + "Ack" );
			if( isSuccess != null && isSuccess.Value == "Failure" )
			{
				this.ResponseError = new ResponseError();
				object temp = null;

				if( this.GetElementValue( root, ref temp, ns, "Errors", "ShortMessage" ) )
					this.ResponseError.ShortMessage = ( string )temp;

				if( this.GetElementValue( root, ref temp, ns, "Errors", "LongMessage" ) )
					this.ResponseError.LongMessage = ( string )temp;

				if( this.GetElementValue( root, ref temp, ns, "Errors", "ErrorCode" ) )
					this.ResponseError.ErrorCode = ( string )temp;

				if( this.GetElementValue( root, ref temp, ns, "Errors", "UserDisplayHint" ) )
					this.ResponseError.UserDisplayHint = ( string )temp;

				if( this.GetElementValue( root, ref temp, ns, "Errors", "ServerityCode" ) )
					this.ResponseError.ServerityCode = ( string )temp;

				if( this.GetElementValue( root, ref temp, ns, "Errors", "ErrorClassification" ) )
					this.ResponseError.ErrorClassification = ( string )temp;

				if( this.GetElementValue( root, ref temp, ns, "Errors", "ErrorParameters" ) )
					this.ResponseError.ErrorParameters = ( string )temp;

				return true;
			}
			return false;
		}

		public ResponseError ResponseError { get; protected set; }

		private bool GetElementValue( XElement x, ref object parsedElement, XNamespace ns, params string[] elementName )
		{
			if( elementName.Length > 0 )
			{
				var element = x.Element( ns + elementName[ 0 ] );
				if( element != null )
				{
					if( elementName.Length > 1 )
						return this.GetElementValue( element, ref parsedElement, ns, elementName.Skip( 1 ).ToArray() );
					parsedElement = element.Value;
					return true;
				}
			}

			return false;
		}

		private object GetElementValue( XElement x, XNamespace ns, params string[] elementName )
		{
			object parsedElement = null;
			this.GetElementValue( x, ref parsedElement, ns, elementName );
			return parsedElement;
		}

		public InventoryStatus ParseReviseInventoryStatusResponse( WebResponse response )
		{
			InventoryStatus result = null;
			using( var responseStream = response.GetResponseStream() )
			{
				if( responseStream != null )
				{
					using( var memStream = new MemoryStream() )
					{
						responseStream.CopyTo( memStream, 0x100 );
						result = ParseReviseInventoryStatusResponse( memStream );
					}
				}
			}

			return result;
		}
	}

	public class ResponseError
	{
		public string ShortMessage { get; set; }
		public string LongMessage { get; set; }
		public string ErrorCode { get; set; }
		public string UserDisplayHint { get; set; }
		public string ServerityCode { get; set; }
		public string ErrorClassification { get; set; }
		public string ErrorParameters { get; set; }
	}
}