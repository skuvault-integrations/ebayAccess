using CuttingEdge.Conditions;

namespace EbayAccess.Models.Credentials
{
	public class EbayUserCredentials
	{
		public object AccountName { get; set; }
		public string Token { get; set; }

		public EbayUserCredentials( string accountName, string token )
		{
			Condition.Requires( accountName, "accountName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( token, "token" ).IsNotNullOrWhiteSpace();

			this.AccountName = accountName;
			this.Token = token;
		}
	}
}