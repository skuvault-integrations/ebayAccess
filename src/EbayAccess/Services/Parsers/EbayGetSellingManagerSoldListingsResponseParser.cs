using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Misc;
using EbayAccess.Models.GetSellingManagerSoldListingsResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayGetSellingManagerSoldListingsResponseParser : EbayXmlParser< GetSellingManagerSoldListingsResponse >
	{
		public override GetSellingManagerSoldListingsResponse Parse( Stream stream, bool keepStremPosition = true )
		{
			try
			{
				var getOrdersResponse = new GetSellingManagerSoldListingsResponse();

				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var streamStartPos = stream.Position;

				var root = XElement.Load( stream );

				var xmlOrders = root.Descendants( ns + "SaleRecord" );

				var error = this.ResponseContainsErrors( root, ns );
				if( error != null )
				{
					getOrdersResponse.Errors = error;
					return getOrdersResponse;
				}

				var orders = xmlOrders.Select( x =>
				{
					var resultOrder = new Order();

					resultOrder.SaleRecordID = GetElementValue( x, ns, "SaleRecordID" );
					resultOrder.CreationTime = GetElementValue( x, ns, "CreationTime" ).ToDateTime();

					if( keepStremPosition )
						stream.Position = streamStartPos;

					return resultOrder;
				} ).ToList();

				getOrdersResponse.Orders = orders;
				return getOrdersResponse;
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