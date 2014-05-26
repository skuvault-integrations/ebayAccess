using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Misc;
using EbayAccess.Models.GetSellerListCustomResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayGetSallerListCustomResponseParser : EbayXmlParser<GetSellerListCustomResponse>
	{
		public override GetSellerListCustomResponse Parse(Stream stream, bool keepStremPosition = true)
		{
			try
			{
				string temp;

				var getSellerListResponse = new GetSellerListCustomResponse();

				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var streamStartPos = stream.Position;

				var root = XElement.Load( stream );

				var erros = this.ResponseContainsErrors( root, ns );
				if( erros != null )
					return new GetSellerListCustomResponse { Error = erros };

				var xmlItems = root.Descendants( ns + "Item" );

				if( !string.IsNullOrWhiteSpace( temp = GetElementValue( root, ns, "HasMoreItems" ) ) )
					getSellerListResponse.HasMoreItems = ( Boolean.Parse( temp ) );

				var orders = xmlItems.Select( x =>
				{
					var res = new Item();

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "BuyItNowPrice" ) ) )
					{
						res.BuyItNowPrice = temp.ToDecimalDotOrComaSeparated();
						res.BuyItNowPriceCurrencyId = this.GetElementAttribute( "currencyID", x, ns, "BuyItNowPrice" );
					}

					res.ItemId = GetElementValue( x, ns, "ItemID" );

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "Quantity" ) ) )
						res.Quantity = long.Parse( temp );

					res.Title = GetElementValue( x, ns, "Title" );

					res.Sku = GetElementValue( x, ns, "SKU" );

					var sellingStatus = x.Element( ns + "SellingStatus" );
					if( sellingStatus != null )
					{
						res.SellingStatus = new SellingStatus();
						res.SellingStatus.CurrentPrice = GetElementValue(x, ns, "SellingStatus", "QuantitySold").ToDecimalDotOrComaSeparated();
						res.SellingStatus.CurrentPriceCurrencyId = this.GetElementAttribute( "currencyID", x, ns, "SellingStatus", "CurrentPrice" );
						res.SellingStatus.QuantitySold = GetElementValue( x, ns, "SellingStatus", "CurrentPrice" ).ToIntOrDefault();
					}

					var variations = x.Element( ns + "Variations" );
					if( variations != null )
					{
						res.Variations = new List< Variation >();

						var variationsElem = variations.Descendants( ns + "Variation" );

						var variationsObj = variationsElem.Select( variat =>
						{
							var tempVariation = new Variation();

							tempVariation.Sku = GetElementValue( variat, ns, "SKU" );
							tempVariation.StartPrice = GetElementValue(variat, ns, "StartPrice").ToDecimalDotOrComaSeparated();
							tempVariation.StartPriceCurrencyId = this.GetElementAttribute("currencyID", x, ns, "SellingStatus", "StartPrice");
							tempVariation.Quantity = GetElementValue( variat, ns, "Quantity" ).ToIntOrDefault();
							tempVariation.SellingStatus.QuantitySold = GetElementValue( variat, ns, "SellingStatus", "QuantitySold" ).ToIntOrDefault();

							return tempVariation;
						} );

						res.Variations.AddRange( variationsObj );
					}

					if( keepStremPosition )
						stream.Position = streamStartPos;

					return res;
				} ).ToList();

				getSellerListResponse.Items = orders;
				return getSellerListResponse;
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