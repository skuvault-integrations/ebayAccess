using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListResponse;
using Item = EbayAccess.Models.GetSellerListResponse.Item;

namespace EbayAccess.Services
{
	public class EbayItemsParser
	{
		public List< Order > ParseGetSallerListResponse( String str )
		{
			//todo: make parser
			throw new NotImplementedException();
		}

		public List< Item > ParseGetSallerListResponse( Stream stream )
		{
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var root = XElement.Load( stream );

				var xmlItems = root.Descendants( ns + "Item" );

				var orders = xmlItems.Select( x =>
				{
					object temp = null;
					var res = new Item();

					if( this.GetElementValue( x, ref temp, ns, "AutoPay" ) )
						res.AutoPay = bool.Parse( ( string )temp );

					if( this.GetElementValue( x, ref temp, ns, "BuyItNowPrice" ) )
						res.BuyItNowPrice = decimal.Parse( ( ( string )temp ).Replace( '.', ',' ) );

					if( this.GetElementValue( x, ref temp, ns, "Country" ) )
						res.Country = ( string )temp;

					if( this.GetElementValue( x, ref temp, ns, "Currency" ) )
						res.Currency = ( string )temp;

					if( this.GetElementValue( x, ref temp, ns, "HideFromSearch" ) )
						res.HideFromSearch = bool.Parse( ( string )temp );

					if( this.GetElementValue( x, ref temp, ns, "ItemID" ) )
						res.ItemId = long.Parse( ( string )temp );

					if( this.GetElementValue( x, ref temp, ns, "ListingType" ) )
					{
						ListingType tempListingType;
						if( Enum.TryParse( ( string )temp, out tempListingType ) )
							res.ListingType = tempListingType;
					}

					if( this.GetElementValue( x, ref temp, ns, "Quantity" ) )
						res.Quantity = long.Parse( ( string )temp );

					if( this.GetElementValue( x, ref temp, ns, "ReservePrice" ) )
						res.ReservePrice = decimal.Parse( ( ( string )temp ).Replace( '.', ',' ) );

					if( this.GetElementValue( x, ref temp, ns, "Site" ) )
						res.Site = ( string )temp;

					if( this.GetElementValue( x, ref temp, ns, "Title" ) )
						res.Title = ( string )temp;

					var listingDetails = x.Element( ns + "ListingDetails" );
					if( listingDetails != null )
					{
						res.ListingDetails = new ListingDetails();

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "Adult" ) )
							res.ListingDetails.Adult = bool.Parse( ( string )temp );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "BindingAuction" ) )
							res.ListingDetails.BindingAuction = bool.Parse( ( string )temp );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "CheckoutEnabled" ) )
							res.ListingDetails.CheckoutEnabled = bool.Parse( ( string )temp );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "ConvertedBuyItNowPrice" ) )
							res.ListingDetails.ConvertedBuyItNowPrice = decimal.Parse( ( ( string )temp ).Replace( '.', ',' ) );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "ConvertedReservePrice" ) )
							res.ListingDetails.ConvertedReservePrice = decimal.Parse( ( ( string )temp ).Replace( '.', ',' ) );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "ConvertedStartPrice" ) )
							res.ListingDetails.ConvertedStartPrice = decimal.Parse( ( ( string )temp ).Replace( '.', ',' ) );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "EndTime" ) )
							res.ListingDetails.EndTime = ( DateTime.Parse( ( string )temp ) );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "HasPublicMessages" ) )
							res.ListingDetails.HasPublicMessages = bool.Parse( ( string )temp );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "HasReservePrice" ) )
							res.ListingDetails.HasReservePrice = bool.Parse( ( string )temp );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "HasUnansweredQuestions" ) )
							res.ListingDetails.HasUnansweredQuestions = bool.Parse( ( string )temp );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "StartTime" ) )
							res.ListingDetails.StartTime = ( DateTime.Parse( ( string )temp ) );

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "ViewItemURL" ) )
							res.ListingDetails.ViewItemUrl = ( string )temp;

						if( this.GetElementValue( x, ref temp, ns, "ListingDetails", "ViewItemURLForNaturalSearch" ) )
							res.ListingDetails.ViewItemUrlForNaturalSearch = ( string )temp;
					}

					var primaryCategory = x.Element( ns + "PrimaryCategory" );
					if( primaryCategory != null )
					{
						res.PrimaryCategory = new Category();

						if( this.GetElementValue( x, ref temp, ns, "PrimaryCategory", "CategoryID" ) )
							res.PrimaryCategory.CategoryId = long.Parse( ( string )temp );

						if( this.GetElementValue( x, ref temp, ns, "PrimaryCategory", "CategoryName" ) )
							res.PrimaryCategory.CategoryName = ( string )temp;
					}

					return res;
				} ).ToList();

				return orders;
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

		public List< Item > ParseGetSallerListResponse( WebResponse response )
		{
			List< Item > result = null;
			using( var responseStream = response.GetResponseStream() )
			{
				if( responseStream != null )
				{
					using( var memStream = new MemoryStream() )
					{
						responseStream.CopyTo( memStream, 0x100 );
						result = ParseGetSallerListResponse( memStream );
					}
				}
			}

			return result;
		}
	}
}