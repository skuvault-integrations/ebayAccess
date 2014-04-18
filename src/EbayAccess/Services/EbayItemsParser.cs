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
					string temp;
					var res = new Item();

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "AutoPay" ) ) )
						res.AutoPay = bool.Parse( temp );

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "BuyItNowPrice" ) ) )
						res.BuyItNowPrice = decimal.Parse( temp.Replace( '.', ',' ) );

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "Country" ) ) )
						res.Country = temp;

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "Currency" ) ) )
						res.Currency = temp;

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "HideFromSearch" ) ) )
						res.HideFromSearch = bool.Parse( temp );

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ItemID" ) ) )
						res.ItemId = long.Parse( temp );

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingType" ) ) )
						res.ListingType = ( ListingType )Enum.Parse( typeof( ListingType ), temp );

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "Quantity" ) ) )
						res.Quantity = long.Parse( temp );

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ReservePrice" ) ) )
						//todo: replace "replace" to parser parameter in whol sol
						res.ReservePrice = decimal.Parse( temp.Replace( '.', ',' ) );

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "Site" ) ) )
						res.Site = temp;

					if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "Title" ) ) )
						res.Title = temp;

					var listingDetails = x.Element( ns + "ListingDetails" );
					if( listingDetails != null )
					{
						res.ListingDetails = new ListingDetails();

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "Adult" ) ) )
							res.ListingDetails.Adult = bool.Parse( temp );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "BindingAuction" ) ) )
							res.ListingDetails.BindingAuction = bool.Parse( temp );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "CheckoutEnabled" ) ) )
							res.ListingDetails.CheckoutEnabled = bool.Parse( temp );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "ConvertedBuyItNowPrice" ) ) )
							res.ListingDetails.ConvertedBuyItNowPrice = decimal.Parse( temp.Replace( '.', ',' ) );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "ConvertedReservePrice" ) ) )
							res.ListingDetails.ConvertedReservePrice = decimal.Parse( temp.Replace( '.', ',' ) );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "ConvertedStartPrice" ) ) )
							res.ListingDetails.ConvertedStartPrice = decimal.Parse( temp.Replace( '.', ',' ) );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "EndTime" ) ) )
							res.ListingDetails.EndTime = ( DateTime.Parse( temp ) );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "HasPublicMessages" ) ) )
							res.ListingDetails.HasPublicMessages = bool.Parse( temp );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "HasReservePrice" ) ) )
							res.ListingDetails.HasReservePrice = bool.Parse( temp );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "HasUnansweredQuestions" ) ) )
							res.ListingDetails.HasUnansweredQuestions = bool.Parse( temp );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "StartTime" ) ) )
							res.ListingDetails.StartTime = ( DateTime.Parse( temp ) );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "ViewItemURL" ) ) )
							res.ListingDetails.ViewItemUrl = temp;

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "ListingDetails", "ViewItemURLForNaturalSearch" ) ) )
							res.ListingDetails.ViewItemUrlForNaturalSearch = temp;
					}

					var primaryCategory = x.Element( ns + "PrimaryCategory" );
					if( primaryCategory != null )
					{
						res.PrimaryCategory = new Category();

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "PrimaryCategory", "CategoryID" ) ) )
							res.PrimaryCategory.CategoryId = long.Parse( temp );

						if( !string.IsNullOrWhiteSpace( temp = this.GetElementValue( x, ns, "PrimaryCategory", "CategoryName" ) ) )
							res.PrimaryCategory.CategoryName = temp;
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

		private string GetElementValue( XElement x, XNamespace ns, params string[] elementName )
		{
			var parsedElement = string.Empty;

			if( elementName.Length <= 0 )
				return parsedElement;

			var element = x.Element( ns + elementName[ 0 ] );
			if( element == null )
				return parsedElement;

			return elementName.Length > 1 ? this.GetElementValue( element, ns, elementName.Skip( 1 ).ToArray() ) : element.Value;
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