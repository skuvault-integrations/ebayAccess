namespace EbayAccessTests.TestEnvironment.TestResponses
{
	public class ReviseFixedPriceItemResponse
	{
		public const string ServerResponseContainsPictureError = @"<ReviseFixedPriceItemResponse xmlns=""urn:ebay:apis:eBLBaseComponents"">
												<Timestamp>2014-08-26T00:00:54.956Z</Timestamp>
												<Ack>Failure</Ack>
												<Errors>
													<ShortMessage>Buyers love large photos that clearly show the item, so please upload high-resolution photos that are at least 500 pixels on the longest side.</ShortMessage>
													<LongMessage>Buyers love large photos that clearly show the item, so please upload high-resolution photos that are at least 500 pixels on the longest side.</LongMessage>
													<ErrorCode>21919137</ErrorCode>
													<SeverityCode>Error</SeverityCode>
													<ErrorParameters ParamID=""0"">
														<Value>http://i.ebayimg.com/</Value>
													</ErrorParameters>
													<ErrorClassification>RequestError</ErrorClassification>
												</Errors>
												<Errors>
													<ShortMessage>Funds from your sales will be unavailable and show as pending in your PayPal account for a period of time.</ShortMessage>
													<LongMessage>Funds from your sales will be unavailable and show as pending in your PayPal account for a period of time. Learn more: http://cgi6.ebay.com/ws/eBayISAPI.dll?UserPolicyMessaging</LongMessage>
													<ErrorCode>21917236</ErrorCode>
													<SeverityCode>Warning</SeverityCode>
													<ErrorParameters ParamID=""0"">
														<Value>http://cgi6.ebay.com/ws/eBayISAPI.dll?UserPolicyMessaging</Value>
													</ErrorParameters>
													<ErrorClassification>RequestError</ErrorClassification>
												</Errors>
												<Version>885</Version>
												<Build>E885_UNI_API5_16967625_R1</Build>
											</ReviseFixedPriceItemResponse>";
	}
}