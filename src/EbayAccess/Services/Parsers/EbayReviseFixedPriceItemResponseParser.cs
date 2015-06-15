using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Misc;
using EbayAccess.Models.ReviseFixedPriceItemResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayReviseFixedPriceItemResponseParser : EbayXmlParser< ReviseFixedPriceItemResponse >
	{
		public ReviseFixedPriceItemResponse Parse( Stream stream )
		{
			ReviseFixedPriceItemResponse inventoryStatusResponse = null;
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var root = XElement.Load( stream );

				var error = this.ResponseContainsErrors( root, ns );
				if( error != null )
					return new ReviseFixedPriceItemResponse() { Errors = error };

				inventoryStatusResponse = new ReviseFixedPriceItemResponse();

				inventoryStatusResponse.Item.ItemId = GetElementValue( root, ns, "ItemID" ).ToLong();
				inventoryStatusResponse.Item.Sku = GetElementValue( root, ns, "SKU" );

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