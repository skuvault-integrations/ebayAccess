using System.Linq;
using EbayAccess.Models;
using EbayAccess.Models.Credentials;
using LINQtoCSV;

namespace EbayAccessTests
{
	public class TestCredentials
	{
		private readonly FlatCsvLine _flatCsvLine;
		private readonly FlatDevCredentialCsvLine _flatDevCredentialCsvLine;

		public TestCredentials( string credentialsFilePath, string devCredentialsFilePath )
		{
			var cc = new CsvContext();
			this._flatCsvLine = cc.Read< FlatCsvLine >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();
			this._flatDevCredentialCsvLine = cc.Read< FlatDevCredentialCsvLine >( devCredentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();
		}

		public EbayUserCredentials GetEbayUserCredentials()
		{
			return new EbayUserCredentials( _flatCsvLine.AccountName, _flatCsvLine.Token );
		}

		public EbayDevCredentials GetEbayDevCredentials()
		{
			return new EbayDevCredentials( _flatDevCredentialCsvLine.AppName, _flatDevCredentialCsvLine.DevName, _flatDevCredentialCsvLine.CertName );
		}

		public string GetEbayEndPoint()
		{
			return "https://api.sandbox.ebay.com/ws/api.dll";
		}

		internal class FlatCsvLine
		{
			public FlatCsvLine()
			{
			}

			[ CsvColumn( Name = "AccountName", FieldIndex = 1 ) ]
			public string AccountName { get; set; }

			[ CsvColumn( Name = "Token", FieldIndex = 2 ) ]
			public string Token { get; set; }
		}

		internal class FlatDevCredentialCsvLine
		{
			public FlatDevCredentialCsvLine()
			{
			}

			[ CsvColumn( Name = "AppName", FieldIndex = 1 ) ]
			public string AppName { get; set; }

			[ CsvColumn( Name = "DevName", FieldIndex = 2 ) ]
			public string DevName { get; set; }

			[ CsvColumn( Name = "CertName", FieldIndex = 3 ) ]
			public string CertName { get; set; }
		}
	}

}