using System;

namespace EbayAccess.Models.Credentials
{
	public class EbayUserCredentials
	{
		public string AccountName { get; set; }
		public string Token { get; set; }
		public int SiteId { get; set; }

		public EbayUserCredentials( string accountName, string token, int siteId = ( int )ebaySites.US )
		{
			if( string.IsNullOrWhiteSpace( accountName ) )
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof( accountName ) );
			if( string.IsNullOrWhiteSpace( token ) )
				throw new ArgumentException( "Value cannot be null or whitespace.", nameof( token ) );

			this.AccountName = accountName;
			this.Token = token;
			this.SiteId = siteId;
		}
	}
}