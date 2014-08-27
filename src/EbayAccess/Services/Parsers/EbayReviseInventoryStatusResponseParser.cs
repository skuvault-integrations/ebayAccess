using System;
using System.Globalization;
using System.IO;
using System.Linq;
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
					return new InventoryStatusResponse() { Errors = error };

				inventoryStatusResponse = new InventoryStatusResponse();

				var inventoryStatuses = root.Descendants( ns + "InventoryStatus" );

				if( inventoryStatuses != null )
				{
					var res = inventoryStatuses.Select( elInventoryStatus =>
					{
						if( elInventoryStatus != null )
						{
							string temp = null;

							var Item = new Item();

							Item.Sku = GetElementValue( elInventoryStatus, ns, "SKU" );

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "ItemID" ) ) )
							{
								long tempRes;
								if( long.TryParse( temp, out tempRes ) )
									Item.ItemId = tempRes;
							}

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "Quantity" ) ) )
							{
								long tempRes;
								if( long.TryParse( temp, out tempRes ) )
									Item.Quantity = tempRes;
							}

							if( !string.IsNullOrWhiteSpace( temp = GetElementValue( elInventoryStatus, ns, "StartPrice" ) ) )
							{
								double tempRes;
								if( double.TryParse( temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempRes ) )
									Item.StartPrice = tempRes;
							}
							return Item;
						}
						return null;
					} );
					inventoryStatusResponse.Items = res.Where( x => x != null ).ToList();
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