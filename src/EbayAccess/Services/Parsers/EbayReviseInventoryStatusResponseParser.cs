using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.ReviseInventoryStatusResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayReviseInventoryStatusResponseParser : EbayXmlParser< InventoryStatusResponse >
	{
		public InventoryStatusResponse Parse( Stream stream )
		{
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var root = XElement.Load( stream );

				InventoryStatusResponse inventoryStatusResponse = null;

				var error = this.ResponseContainsErrors( root, ns );
				if( error != null )
					return new InventoryStatusResponse() { Error = error };

				var elInventoryStatus = root.Element( ns + "InventoryStatus" );
				if( elInventoryStatus != null )
				{
					string temp = null;

					inventoryStatusResponse = new InventoryStatusResponse();

					inventoryStatusResponse.Sku = GetElementValue( elInventoryStatus, ns, "SKU" );

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "ItemID" ) ) )
					{
						long tempRes;
						if( long.TryParse( temp, out tempRes ) )
							inventoryStatusResponse.ItemId = tempRes;
					}

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "Quantity" ) ) )
					{
						long tempRes;
						if( long.TryParse( temp, out tempRes ) )
							inventoryStatusResponse.Quantity = tempRes;
					}

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "StartPrice" ) ) )
					{
						double tempRes;
						if( double.TryParse( temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempRes ) )
							inventoryStatusResponse.StartPrice = tempRes;
					}
				}

				return inventoryStatusResponse;
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
	}
}