using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.ReviseInventoryStatusResponse;

namespace EbayAccess.Services.Parsers
{
	public class EbayBulkAbortJobParser : EbayXmlParser< AbortJobResponse >
	{
		public override AbortJobResponse Parse( Stream stream, bool keepStreamPosition = true )
		{
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				var streamStartPos = stream.Position;

				var root = XElement.Load( stream );

				var erros = this.ResponseContainsErrors( root, ns );
				if( erros != null )
					return new AbortJobResponse { Errors = erros };

				var res = new AbortJobResponse();

				if( keepStreamPosition )
					stream.Position = streamStartPos;

				return res;
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

		protected override IEnumerable< ResponseError > ResponseContainsErrors( XElement root, XNamespace ns )
		{
			var isSuccess = root.Element( ns + "ack" );
			if( isSuccess != null && ( isSuccess.Value == "Failure" || isSuccess.Value == "PartialFailure" ) )
			{
				var ResponseError = new ResponseError();
				string temp = null;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "errorMessage", "error", "message" ) ) )
					ResponseError.ShortMessage = temp;

				if( !string.IsNullOrWhiteSpace( temp = EbayXmlParser< InventoryStatusResponse >.GetElementValue( root, ns, "errorMessage", "error", "errorId" ) ) )
					ResponseError.ErrorCode = temp;

				return new List< ResponseError > { ResponseError };
			}
			return null;
		}
	}

	public class AbortJobResponse : EbayBaseResponse
	{
	}
}