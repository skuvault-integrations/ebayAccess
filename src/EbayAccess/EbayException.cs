using System;
using System.Runtime.CompilerServices;

namespace EbayAccess
{
	public class EbayException : Exception
	{
		protected EbayException( string message, Exception exception )
			: base( message, exception )
		{
		}

		protected EbayException( string message )
			: base( message )
		{
		}
	}

	public class EbayAuthException : EbayException
	{
		public EbayAuthException( string message, Exception exception, [ CallerMemberName ] string memberName = "" )
			: base( string.Format( "{0}:{1}", memberName, message ), exception )
		{
		}
	}

	public class EbayCommonException : EbayException
	{
		public EbayCommonException( string message, Exception exception, [ CallerMemberName ] string memberName = "" )
			: base( string.Format( "{0}:{1}", memberName, message ), exception )
		{
		}

		public EbayCommonException( string message, [ CallerMemberName ] string memberName = "" )
			: base( string.Format( "{0}:{1}", memberName, message ) )
		{
		}
	}
}