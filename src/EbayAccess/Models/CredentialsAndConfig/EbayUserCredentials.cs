using CuttingEdge.Conditions;

namespace EbayAccess.Models.Credentials
{
	public class EbayUserCredentials
	{
		public string AccountName { get; set; }
		public string Token { get; set; }
		public int SiteId { get; set; }

		public EbayUserCredentials( string accountName, string token, int siteId = ( int )ebaySites.US )
		{
			Condition.Requires( accountName, "accountName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( token, "token" ).IsNotNullOrWhiteSpace();

			this.AccountName = accountName;
			this.Token = token;
			this.SiteId = siteId;
		}
	}
}