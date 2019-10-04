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
	public class EbayGetSellerListCustomProductResponseParser : EbayXmlParser< GetSellerListCustomProductResponse >
	{
		public override GetSellerListCustomProductResponse Parse( Stream stream, bool keepStreamPosition = true )
		{
			try
			{
				if( stream == null )
					return null;			

				var getSellerListResponse = new GetSellerListCustomProductResponse();

				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var streamStartPos = stream.Position;

				var root = XElement.Load( stream );

				var erros = this.ResponseContainsErrors( root, ns );
				if( erros != null )
					return new GetSellerListCustomProductResponse { Errors = erros };

				var pagination = this.GetPagination( root, ns );
				if( pagination != null )
					getSellerListResponse.PaginationResult = pagination;

				var xmlItems = root.Descendants( ns + "Item" );

				getSellerListResponse.HasMoreItems = GetElementValue( root, ns, "HasMoreItems" ).ToBool();

				var items = xmlItems.Select( x => this.ParseProduct( x, ns, keepStreamPosition, stream, ref streamStartPos ) ).ToList();

				getSellerListResponse.Items = items;
				return getSellerListResponse;
			}
			catch( Exception exception )
			{
				var buffer = new byte[ stream.Length ];
				stream.Read( buffer, 0, ( int )stream.Length );
				var utf8Encoding = new UTF8Encoding();
				var bufferStr = utf8Encoding.GetString( buffer );
				throw new Exception( "Can't parse: " + bufferStr, exception );
			}
		}

		private Product ParseProduct( XElement element, XNamespace ns, bool keepStreamPosition, Stream stream, ref long streamStartPos )
		{
			var product = new Product();

			//res.ItemId = GetElementValue( element, ns, "ItemID" );

			string buyItNowPrice;
			if( !string.IsNullOrWhiteSpace( buyItNowPrice = GetElementValue( element, ns, "BuyItNowPrice" ) ) )
			{
				product.SalePrice = new SalePrice 
				{
					Price = buyItNowPrice.ToDecimalDotOrComaSeparated(),
					CurrencyId = this.GetElementAttribute( "currencyID", element, ns, "BuyItNowPrice" )
				};
			}

			product.Title = GetElementValue( element, ns, "Title" );
			product.Sku = GetElementValue( element, ns, "SKU" );
			product.ClassificationName = GetElementValue( element, ns, "PrimaryCategory" );
			var pictureUrls = element.Element( ns + "PictureDetails" )?.Descendants( ns + "PictureURL" ).Select( p => p.Value ).ToList();
			if( pictureUrls != null && pictureUrls.Any() )
			{
				product.ImageUrl = pictureUrls.FirstOrDefault();
			}
			product.LongDescription = GetElementValue( element, ns, "Description" );

			var shippingPackage = element.Descendants( ns + "ShippingPackageDetails" ).FirstOrDefault();

			if( shippingPackage != null && !string.IsNullOrWhiteSpace( shippingPackage.Value ) )
			{
				var weightMajor = shippingPackage.Element( ns + "WeightMajor");
				var weightMinor = shippingPackage.Element( ns + "WeightMinor");
				if( weightMajor != null && weightMinor != null )
				{
					product.Weight = new Weight( weightMajor.Value, weightMajor.Attribute( "unit" )?.Value,
						weightMinor.Value, weightMinor.Attribute( "unit" )?.Value);
				}
			}

			product.Duration = GetElementValue( element, ns, "ListingDuration" );

			var variationsListElem = element.Element( ns + "Variations" );
			if( variationsListElem != null )
			{
				product.Variations = new List< ProductVariation >();

				var variationsElem = variationsListElem.Descendants( ns + "Variation" ).ToList();

				var productVariations = variationsElem.Select( variationElement =>
				{
					var variation = new ProductVariation 
					{ 
						Sku = GetElementValue( variationElement, ns, "SKU" ), 
						Title = product.Title 
					};

					var variationSpecifics = variationElement.Descendants( ns + "VariationSpecifics" ).Descendants( ns + "NameValueList" ).ToList();
					if( variationSpecifics.Any() )
					{
						var variationNameValues = variationSpecifics.Select( v => new { Name = GetElementValue( v, ns, "Name" ), Value = GetElementValue( v, ns, "Value" ) } );
						variation.Title += ": " + string.Join(", ", variationNameValues.Select( v => v.Name + " - " + v.Value ) );
					}

					return variation;
				} ).ToList();

				product.Variations.AddRange( productVariations );
			}

			if( keepStreamPosition )
				stream.Position = streamStartPos;

			return product;
		}
	}
}
