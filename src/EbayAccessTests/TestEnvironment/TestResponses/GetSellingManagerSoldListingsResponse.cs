namespace EbayAccessTests.TestEnvironment.TestResponses
{
	public static class GetSellingManagerSoldListingsResponse
	{
		public const string ResponseWithInternalError = @"<?xml version=""1.0"" encoding=""UTF-8""?>
													<GetSellingManagerSoldListingsResponse xmlns=""urn:ebay:apis:eBLBaseComponents"">
														<Timestamp>2014-08-26T05:19:22.883Z</Timestamp>
														<Ack>Failure</Ack>
														<Errors>
															<ShortMessage>Internal error to the application.</ShortMessage>
															<LongMessage>Internal error to the application.</LongMessage>
															<ErrorCode>10007</ErrorCode>
															<SeverityCode>Error</SeverityCode>
															<ErrorParameters ParamID=""0"">
																<Value>Web Service framework internal error.</Value>
															</ErrorParameters>
															<ErrorClassification>RequestError</ErrorClassification>
														</Errors>
														<Version>887</Version>
														<Build>E887_CORE_APISELLING_17004199_R1</Build>
													</GetSellingManagerSoldListingsResponse>";
		public const string ResponseWithSoldRecord = @"<GetSellingManagerSoldListingsResponse xmlns=""urn:ebay:apis:eBLBaseComponents"">
													<Timestamp>2014-08-26T05:19:17.977Z</Timestamp>
													<Ack>Success</Ack>
													<Version>887</Version>
													<Build>E887_CORE_APISELLING_17004199_R1</Build>
													<SaleRecord>
														<SellingManagerSoldTransaction>
															<TransactionID>0000000000000</TransactionID>
															<SaleRecordID>00000</SaleRecordID>
															<ItemID>000000000000</ItemID>
															<QuantitySold>1</QuantitySold>
															<ItemTitle>GALAXY NOTE 2 I317 T889 LCD DIGITIZER TOUCH SCREEN ASSEMBLY FRAME WHITE - B</ItemTitle>
															<ListingType>StoresFixedPrice</ListingType>
															<Relisted>false</Relisted>
															<SecondChanceOfferSent>false</SecondChanceOfferSent>
															<CustomLabel>SRN-SAM-334</CustomLabel>
															<SoldOn>eBay</SoldOn>
															<ListedOn>eBay</ListedOn>
															<CharityListing>false</CharityListing>
															<OrderLineItemID>000000000000-0000000000000</OrderLineItemID>
														</SellingManagerSoldTransaction>
														<ShippingAddress>
															<Name>xxx</Name>
															<PostalCode>00000</PostalCode>
														</ShippingAddress>
														<ShippingDetails>
															<ShippingType>Flat</ShippingType>
														</ShippingDetails>
														<TotalAmount currencyID=""USD"">133.65</TotalAmount>
														<TotalQuantity>1</TotalQuantity>
														<ActualShippingCost currencyID=""USD"">0.0</ActualShippingCost>
														<OrderStatus>
															<CheckoutStatus>CheckoutComplete</CheckoutStatus>
															<PaidStatus>Paid</PaidStatus>
															<ShippedStatus>Shipped</ShippedStatus>
															<PaymentMethodUsed>PayPal</PaymentMethodUsed>
															<FeedbackSent>false</FeedbackSent>
															<TotalEmailsSent>3</TotalEmailsSent>
															<ShippedTime>2014-08-18T15:24:50.000Z</ShippedTime>
															<PaidTime>2014-08-18T03:14:38.000Z</PaidTime>
														</OrderStatus>
														<SalePrice currencyID=""USD"">133.65</SalePrice>
														<DaysSinceSale>8</DaysSinceSale>
														<BuyerID>eaglefind</BuyerID>
														<BuyerEmail>xxx@xxx.xxx</BuyerEmail>
														<SaleRecordID>40343</SaleRecordID>
														<CreationTime>2014-08-18T03:14:37.000Z</CreationTime>
													</SaleRecord>
													<PaginationResult>
														<TotalNumberOfPages>1</TotalNumberOfPages>
														<TotalNumberOfEntries>1</TotalNumberOfEntries>
													</PaginationResult>
												</GetSellingManagerSoldListingsResponse>";
	}
}