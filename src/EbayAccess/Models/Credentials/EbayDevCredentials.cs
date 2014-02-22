using CuttingEdge.Conditions;

namespace EbayAccess.Models.Credentials
{
	public class EbayDevCredentials
	{
		public string DevName { get; private set; }
		public string AppName { get; private set; }
		public string CertName { get; private set; }

		public EbayDevCredentials( string devName, string appName, string certName )
		{
			Condition.Requires( devName, "devName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( appName, "appName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( certName, "certName" ).IsNotNullOrWhiteSpace();

			this.DevName = devName;
			this.AppName = appName;
			this.CertName = certName;
		}
	}
}