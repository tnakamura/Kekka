using OrderTaking.PlaceOrder.Api;

var request = new HttpRequest(
    Action: "GET",
    Uri: "https://localhost:8081",
    Body: new JsonString(@"{
        ""OrderId"": ""ABCDE"",
        ""CustomerInfo"": {
            ""FirstName"": ""Takefusa"",
            ""LastName"": ""Kubo"",
            ""EmailAddress"": ""take.kubo@example.com""
        },
        ""ShippingAddress"": {
            ""AddressLine1"": ""Tenjin"",
            ""City"": ""Fukuoka"",
            ""ZipCode"": ""81000""
        },
        ""BillingAddress"": {
            ""AddressLine1"": ""Tenjin"",
            ""City"": ""Fukuoka"",
            ""ZipCode"": ""81000""
        },
        ""Lines"": [
            {
                ""OrderLineId"": ""FGHIJ"",
                ""ProductCode"": ""W1234"",
                ""Quantity"": 100
            }
        ]
    }"));

var response = await Workflow.PlaceOrderApi(request);

Console.WriteLine(response.Body.String);
Console.ReadLine();

