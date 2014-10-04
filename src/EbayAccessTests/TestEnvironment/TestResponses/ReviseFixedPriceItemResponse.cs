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

		public const string ServerResponseContainsLvsBlockError = @"<ReviseFixedPriceItemResponse xmlns=""urn:ebay:apis:eBLBaseComponents"">
												<Timestamp>2014-08-26T00:00:54.956Z</Timestamp>
												<Ack>Failure</Ack>
												<Errors>
													<ShortMessage>Lvis blocked.</ShortMessage>
													<LongMessage>Lvis validation blocked.</LongMessage>
													<ErrorCode>21916293</ErrorCode>
													<SeverityCode>Error</SeverityCode>
													<ErrorParameters ParamID=""0"">
														<Value>161331258512</Value>
													</ErrorParameters>
													<ErrorParameters ParamID=""1"">
														<Value>566148-3</Value>
													</ErrorParameters>
													<ErrorParameters ParamID=""2"">
														<Value>&lt;div&gt;
													&lt;div class=&quot;errPanel&quot;&gt;&lt;div class=&quot;panel-n&quot;&gt;&lt;div class=&quot;panel-e&quot;&gt;&lt;div class=&quot;panel-w&quot;&gt;&lt;/div&gt;&lt;/div&gt;&lt;/div&gt;&lt;div class=&quot;panel-head&quot;&gt;&lt;table border=&quot;0&quot; cellspacing=&quot;0&quot; cellpadding=&quot;0&quot;&gt;&lt;tr&gt;&lt;td class=&quot;panel-icon&quot;&gt;&lt;/td&gt;&lt;td class=&quot;err-msg&quot;&gt;&lt;div&gt;&lt;b class=&quot;error&quot;&gt;Attention&lt;/b&gt;&lt;/div&gt;&lt;div&gt;&lt;p&gt;It looks like you&apos;ve exceeded the number of items you can list. You can list up to 100 items this month. &lt;/p&gt;
															&lt;p&gt;&lt;a href=&quot;https://scgi.ebay.com/ws/eBayISAPI.dll?UpgradeLimits&quot; target=&quot;_blank&quot;&gt;To request higher selling limits, please call us&lt;/a&gt;.&lt;a href=&quot;https://scgi.ebay.com/ws/eBayISAPI.dll?UpgradeLimits&quot; target=&quot;_blank&quot;&gt;&lt;/a&gt;&lt;/p&gt;&lt;/div&gt;&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;/div&gt;&lt;div class=&quot;panel-s&quot;&gt;&lt;div class=&quot;panel-e&quot;&gt;&lt;div class=&quot;panel-w&quot;&gt;&lt;/div&gt;&lt;/div&gt;&lt;/div&gt;&lt;/div&gt;
												&lt;/div&gt;&lt;font color=&quot;#BEBEBE&quot; size=&quot;1&quot;&gt;{e45471-641920x}&lt;/font&gt;</Value>
													</ErrorParameters>
													<ErrorParameters ParamID=""SKU"">
														<Value>566148-3</Value>
													</ErrorParameters>
													<ErrorClassification>RequestError</ErrorClassification>
												</Errors>
												<Version>885</Version>
												<Build>E885_UNI_API5_16967625_R1</Build>
											</ReviseFixedPriceItemResponse>";

		public const string VirtualNotSkippedError = @"<ReviseFixedPriceItemResponse xmlns=""urn:ebay:apis:eBLBaseComponents"">
												<Timestamp>2014-08-26T00:00:54.956Z</Timestamp>
												<Ack>Failure</Ack>
												<Errors>
													<ShortMessage>VirtualError</ShortMessage>
													<LongMessage>VirtualError</LongMessage>
													<ErrorCode>00000000</ErrorCode>
													<SeverityCode>Error</SeverityCode>
													<ErrorParameters ParamID=""0"">
														<Value>161331258512</Value>
													</ErrorParameters>
													<ErrorParameters ParamID=""1"">
														<Value>566148-3</Value>
													</ErrorParameters>
													<ErrorParameters ParamID=""2"">
														<Value>&lt;div&gt;
													&lt;div class=&quot;errPanel&quot;&gt;&lt;div class=&quot;panel-n&quot;&gt;&lt;div class=&quot;panel-e&quot;&gt;&lt;div class=&quot;panel-w&quot;&gt;&lt;/div&gt;&lt;/div&gt;&lt;/div&gt;&lt;div class=&quot;panel-head&quot;&gt;&lt;table border=&quot;0&quot; cellspacing=&quot;0&quot; cellpadding=&quot;0&quot;&gt;&lt;tr&gt;&lt;td class=&quot;panel-icon&quot;&gt;&lt;/td&gt;&lt;td class=&quot;err-msg&quot;&gt;&lt;div&gt;&lt;b class=&quot;error&quot;&gt;Attention&lt;/b&gt;&lt;/div&gt;&lt;div&gt;&lt;p&gt;It looks like you&apos;ve exceeded the number of items you can list. You can list up to 100 items this month. &lt;/p&gt;
															&lt;p&gt;&lt;a href=&quot;https://scgi.ebay.com/ws/eBayISAPI.dll?UpgradeLimits&quot; target=&quot;_blank&quot;&gt;To request higher selling limits, please call us&lt;/a&gt;.&lt;a href=&quot;https://scgi.ebay.com/ws/eBayISAPI.dll?UpgradeLimits&quot; target=&quot;_blank&quot;&gt;&lt;/a&gt;&lt;/p&gt;&lt;/div&gt;&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;/div&gt;&lt;div class=&quot;panel-s&quot;&gt;&lt;div class=&quot;panel-e&quot;&gt;&lt;div class=&quot;panel-w&quot;&gt;&lt;/div&gt;&lt;/div&gt;&lt;/div&gt;&lt;/div&gt;
												&lt;/div&gt;&lt;font color=&quot;#BEBEBE&quot; size=&quot;1&quot;&gt;{e45471-641920x}&lt;/font&gt;</Value>
													</ErrorParameters>
													<ErrorParameters ParamID=""SKU"">
														<Value>566148-3</Value>
													</ErrorParameters>
													<ErrorClassification>RequestError</ErrorClassification>
												</Errors>
												<Version>885</Version>
												<Build>E885_UNI_API5_16967625_R1</Build>
											</ReviseFixedPriceItemResponse>";

		public const string Success = @"<ReviseFixedPriceItemResponse xmlns=""urn:ebay:apis:eBLBaseComponents"">
										  <Timestamp>2014-08-21T10:03:39.623Z</Timestamp>
										  <Ack>Success</Ack>
										  <Version>883</Version>
										  <Build>E883_UNI_API5_16949904_R1</Build>
										  <ItemID>110141989389</ItemID>
										  <StartTime>2014-04-28T16:06:40.000Z</StartTime>
										  <EndTime>2014-08-26T16:06:40.000Z</EndTime>
										  <Fees>
											<Fee>
											  <Name>AuctionLengthFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>BoldFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>BuyItNowFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>CategoryFeaturedFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>FeaturedFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>GalleryPlusFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>FeaturedGalleryFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>FixedPriceDurationFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>GalleryFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>GiftIconFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>HighLightFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>InsertionFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>InternationalInsertionFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>ListingDesignerFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>ListingFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>PhotoDisplayFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>PhotoFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>ReserveFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>SchedulingFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>SubtitleFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>BorderFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>ProPackBundleFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>BasicUpgradePackBundleFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>ValuePackBundleFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>PrivateListingFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>ProPackPlusBundleFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
											<Fee>
											  <Name>MotorsGermanySearchFee</Name>
											  <Fee currencyID=""USD"">0.0</Fee>
											</Fee>
										</Fees>
										</ReviseFixedPriceItemResponse>";
	}
}