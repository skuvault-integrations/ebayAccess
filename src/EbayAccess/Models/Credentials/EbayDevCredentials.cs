using CuttingEdge.Conditions;

namespace EbayAccess.Models.Credentials
{
	public class EbayDevCredentials
	{
		public string AppName { get; private set; }
		public string DevName { get; private set; }
		public string CertName { get; private set; }

		public EbayDevCredentials( string appName, string devName, string certName )
		{
			Condition.Requires( appName, "devName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( devName, "appName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( certName, "certName" ).IsNotNullOrWhiteSpace();

			this.AppName = appName;
			this.DevName = devName;
			this.CertName = certName;
		}
	}
}