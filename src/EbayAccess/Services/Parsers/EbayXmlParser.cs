using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using EbayAccess.Misc;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.ReviseInventoryStatusResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayXmlParser< TParseResult >
	{
		public static string GetElementValue( XElement x, XNamespace ns, params string[] elementName )
		{
			var parsedElement = string.Empty;

			if( elementName.Length <= 0 )
				return parsedElement;

			var element = x.Element( ns + elementName[ 0 ] );
			if( element == null )
				return parsedElement;

			return elementName.Length > 1 ? GetElementValue( element, ns, elementName.Skip( 1 ).ToArray() ) : element.Value;
		}

		protected string GetElementAttribute( string attributeName, XElement xElement, XNamespace ns, params string[] elementName )
		{
			var elementAttribute = string.Empty;

			if( elementName.Length <= 0 )
				return xElement.Attribute( attributeName ).Value;

			var element = xElement.Element( ns + elementName[ 0 ] );
			if( element == null )
				return elementAttribute;

			return elementName.Length > 1 ? this.GetElementAttribute( attributeName, element, ns, elementName.Skip( 1 ).ToArray() ) : element.Attribute( attributeName ).Value;
		}

		public TParseResult Parse( WebResponse response )
		{
			var result = default( TParseResult );
			using( var responseStream = response.GetResponseStream() )
			{
				if( responseStream != null )
				{
					using( var memStream = new MemoryStream() )
					{
						responseStream.CopyTo( memStream, 0x100 );
						result = this.Parse( memStream );
					}
				}
			}

			return result;
		}

		public TParseResult Parse( String str )
		{
			var stream = new MemoryStream();
			var streamWriter = new StreamWriter( stream );
			streamWriter.Write( str );
			streamWriter.Flush();
			stream.Position = 0;

			using( stream )
				return Parse( stream );
		}

		public virtual TParseResult Parse( Stream stream, bool keepStremPosition = true )
		{
			return default( TParseResult );
		}

		protected virtual ResponseError ResponseContainsErrors( XElement root, XNamespace ns )
		{
			var isSuccess = root.Element( ns + "Ack" );
			if( isSuccess != null && isSuccess.Value == "Failure" )
			{
				var ResponseError = new ResponseError();
				string temp = null;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "Errors", "ShortMessage" ) ) )
					ResponseError.ShortMessage = temp;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "Errors", "LongMessage" ) ) )
					ResponseError.LongMessage = temp;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "Errors", "ErrorCode" ) ) )
					ResponseError.ErrorCode = temp;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "Errors", "UserDisplayHint" ) ) )
					ResponseError.UserDisplayHint = temp;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "Errors", "ServerityCode" ) ) )
					ResponseError.ServerityCode = temp;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "Errors", "ErrorClassification" ) ) )
					ResponseError.ErrorClassification = temp;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "Errors", "ErrorParameters" ) ) )
					ResponseError.ErrorParameters = temp;

				return ResponseError;
			}
			return null;
		}

		protected virtual PaginationResult GetPagination( XElement root, XNamespace ns )
		{
			try
			{
				var isSuccess = root.Element( ns + "PaginationResult" );
				if( isSuccess != null )
				{
					var pagination = new PaginationResult();

					pagination.TotalNumberOfPages = EbayXmlParser<InventoryStatusResponse>.GetElementValue(isSuccess, ns, "TotalNumberOfPages").ToIntOrDefault();

					pagination.TotalNumberOfEntries = EbayXmlParser<InventoryStatusResponse>.GetElementValue(isSuccess, ns, "TotalNumberOfEntries").ToIntOrDefault();

					return pagination;
				}
				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}