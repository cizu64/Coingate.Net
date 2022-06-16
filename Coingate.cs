using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Coingate.Net
{
    public class Coingate
    {
        public string ApiToken;
        private readonly HttpClient _client;
        private readonly string _baseUri;

        /// <summary>
        /// Pass in the  coingate api key,api secret and appid found on your dashboard.
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        /// <param name="appId">The app id</param>
        /// <param name="useSandbox">True:use sandbox</param>
        public Coingate(string apiToken, bool useSandbox = true)
        {
            ApiToken = apiToken;
            _baseUri = useSandbox ? "https://api-sandbox.coingate.com/" : "https://api.coingate.com/";
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", "Token " + ApiToken);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
      
        /// <summary>
        /// Get all orders https://developer.coingate.com/docs/list-orders
        /// </summary>
        /// <param name="resourcePath">/v2/orders or defined</param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public async Task<string> GetOrders(string resourcePath = "/v2/orders/", int pageNo = 1, int pageSize = 10, string sort = "created_at_desc")
        {
            _client.BaseAddress = new Uri(_baseUri);
            string url = resourcePath + "?per_page=" + pageNo + "&page=" + pageSize + "&sort=" + sort;
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return "";
            var orders = await response.Content.ReadAsStringAsync();
            return orders;
        }

        /// <summary>
        /// Get order by id https://developer.coingate.com/docs/get-order
        /// </summary>
        /// <param name="orderId">The order id</param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public async Task<string> GetOrder(int orderId, string resourcePath = "/v2/orders/")
        {
            _client.BaseAddress = new Uri(_baseUri);
            var response = await _client.GetAsync(resourcePath + orderId);
            if (!response.IsSuccessStatusCode) return "";
            var order = await response.Content.ReadAsStringAsync();
            return order;  
        }

        /// <summary>
        /// Create an order https://developer.coingate.com/docs/create-order
        /// </summary>
        /// <param name="dto">The order object</param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public async Task<dynamic> CreateOrder(Order dto, string resourcePath = "/v2/orders/")
        {
            _client.BaseAddress = new Uri(_baseUri);
            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("order_id", dto.OrderId.ToString()),
                new KeyValuePair<string, string>("price_amount", dto.Price.ToString()),
                new KeyValuePair<string, string>("price_currency", dto.Currency),
                new KeyValuePair<string, string>("receive_currency", dto.ReceiveCurrency),
                new KeyValuePair<string, string>("title", dto.Title),
                new KeyValuePair<string, string>("description", dto.Description),
                new KeyValuePair<string, string>("callback_url", dto.CallbackUrl),
                new KeyValuePair<string, string>("cancel_url", dto.CancelUrl),
                new KeyValuePair<string, string>("success_url", dto.SuccessUrl)
            });
            var response = await _client.PostAsync(resourcePath, body);
            if (!response.IsSuccessStatusCode) return HttpStatusCode.BadRequest;
            var order = await response.Content.ReadAsStringAsync();
            return order;
        }

        /// <summary>
        /// Checkout https://developer.coingate.com/docs/checkout
        /// </summary>
        /// <param name="id">The order id</param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public async Task<string> Checkout(int orderId, string paycurrency)
        {
            _client.BaseAddress = new Uri(_baseUri);
            var resourcePath = $"/v2/orders/{orderId}/checkout";
            var content = new Dictionary<string, string>
            {
                { "pay_currency", paycurrency }
            };
            var serilaize = JsonConvert.SerializeObject(content);
            var body = new StringContent(serilaize);
            var response = await _client.PostAsync(resourcePath, body);
            if (!response.IsSuccessStatusCode) return "";
            var order = await response.Content.ReadAsStringAsync();
            return order;  
        }
    }
}
