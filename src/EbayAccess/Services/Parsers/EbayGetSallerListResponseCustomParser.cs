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
	public class EbayGetSallerListCustomResponseParser : EbayXmlParser< GetSellerListCustomResponse >
	{
		public override GetSellerListCustomResponse Parse( Stream stream, bool keepStremPosition = true )
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

				var pagination = this.GetPagination(root, ns);
				if( pagination != null )
					getSellerListResponse.PaginationResult = pagination;

				var xmlItems = root.Descendants( ns + "Item" );

				getSellerListResponse.HasMoreItems = GetElementValue( root, ns, "HasMoreItems" ).ToBool( false );

				var orders = xmlItems.Select( x =>
				{
					var res = new Item();

					if( !string.IsNullOrWhiteSpace( temp = GetElementValue( x, ns, "BuyItNowPrice" ) ) )
					{
						res.BuyItNowPrice = temp.ToDecimalDotOrComaSeparated( false );
						res.BuyItNowPriceCurrencyId = this.GetElementAttribute( "currencyID", x, ns, "BuyItNowPrice" );
					}

					res.ItemId = GetElementValue( x, ns, "ItemID" );

					res.Quantity = GetElementValue( x, ns, "Quantity" ).ToIntOrDefault( false );

					res.Title = GetElementValue( x, ns, "Title" );

					res.Sku = GetElementValue( x, ns, "SKU" );

					var sellingStatus = x.Element( ns + "SellingStatus" );
					if( sellingStatus != null )
					{
						res.SellingStatus = new SellingStatus();
						res.SellingStatus.CurrentPrice = GetElementValue( x, ns, "SellingStatus", "CurrentPrice" ).ToDecimalDotOrComaSeparated( false );
						res.SellingStatus.CurrentPriceCurrencyId = this.GetElementAttribute( "currencyID", x, ns, "SellingStatus", "CurrentPrice" );
						res.SellingStatus.QuantitySold = GetElementValue( x, ns, "SellingStatus", "QuantitySold" ).ToIntOrDefault( false );
					}

					var variations = x.Element( ns + "Variations" );
					if( variations != null )
					{
						res.Variations = new List< Variation >();

						var variationsElem = variations.Descendants( ns + "Variation" ).ToList();

						var variationsObj = variationsElem.Select( variat =>
						{
							var tempVariation = new Variation();

							tempVariation.Sku = GetElementValue( variat, ns, "SKU" );
							tempVariation.StartPrice = GetElementValue( variat, ns, "StartPrice" ).ToDecimalDotOrComaSeparated( false );
							tempVariation.StartPriceCurrencyId = this.GetElementAttribute( "currencyID", x, ns, "SellingStatus", "StartPrice" );
							tempVariation.Quantity = GetElementValue( variat, ns, "Quantity" ).ToIntOrDefault( false );
							tempVariation.SellingStatus = new SellingStatus { QuantitySold = GetElementValue( variat, ns, "SellingStatus", "QuantitySold" ).ToIntOrDefault( false ) };

							return tempVariation;
						} ).ToList();

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