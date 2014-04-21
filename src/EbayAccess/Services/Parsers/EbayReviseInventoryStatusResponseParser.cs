using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.ReviseInventoryStatusRequest;

namespace EbayAccess.Services.Parsers
{
	public class EbayReviseInventoryStatusResponseParser : EbayXmlParser< InventoryStatus >
	{
		public ResponseError ResponseError { get; protected set; }

		public InventoryStatus Parse( Stream stream )
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
					string temp = null;

					inventoryStatus = new InventoryStatus();

					inventoryStatus.Sku = GetElementValue( elInventoryStatus, ns, "SKU" );

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "ItemID" ) ) )
					{
						long tempRes;
						if( long.TryParse( temp, out tempRes ) )
							inventoryStatus.ItemId = tempRes;
					}

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "Quantity" ) ) )
					{
						long tempRes;
						if( long.TryParse( temp, out tempRes ) )
							inventoryStatus.Quantity = tempRes;
					}

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "StartPrice" ) ) )
					{
						double tempRes;
						if( double.TryParse( temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempRes ) )
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
				string temp = null;

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "Errors", "ShortMessage" ) ) )
					this.ResponseError.ShortMessage = temp;

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "Errors", "LongMessage" ) ) )
					this.ResponseError.LongMessage = temp;

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "Errors", "ErrorCode" ) ) )
					this.ResponseError.ErrorCode = temp;

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "Errors", "UserDisplayHint" ) ) )
					this.ResponseError.UserDisplayHint = temp;

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "Errors", "ServerityCode" ) ) )
					this.ResponseError.ServerityCode = temp;

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "Errors", "ErrorClassification" ) ) )
					this.ResponseError.ErrorClassification = temp;

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "Errors", "ErrorParameters" ) ) )
					this.ResponseError.ErrorParameters = temp;

				return true;
			}
			return false;
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