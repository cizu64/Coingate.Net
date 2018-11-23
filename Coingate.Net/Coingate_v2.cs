using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace Coingate.Net
{
    public class Coingate
    {
        public string ApiKey, ApiSecret, AppId;
        private readonly HttpClient _client;
        private readonly int _nonce;
        private readonly string _baseUri;

        /// <summary>
        /// Pass in the  coingate api key,api secret and appid found on your dashboard.
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        /// <param name="appId">The app id</param>
        /// <param name="useSandbox">True:use sandbox</param>
        public Coingate(string apiKey, string apiSecret, string appId, bool useSandbox = true)
        {
            ApiKey = apiKey;
            ApiSecret = apiSecret;
            AppId = appId;
            _baseUri = useSandbox ? "https://api-sandbox.coingate.com/" : "https://api.coingate.com/";
            _nonce = DateTime.Now.Second;
            _client = new HttpClient();
        }

        /// <summary>
        /// Generate signature 
        /// </summary>
        /// <param name="key">The api secret key</param>
        /// <param name="message">The signature formed by adding the nonce,appKd and apiKey</param>
        /// <returns></returns>
        private string ConfigureHmac(string key, string message)
        {
            var encoding = Encoding.UTF8;
            var keyByte = encoding.GetBytes(key);
            var hmac = new HMACSHA256(keyByte);
            var hash = hmac.ComputeHash(encoding.GetBytes(message));
            var sb = new StringBuilder(hash.Length);
            foreach (var character in hash)
            {
                sb.Append(character.ToString("x2"));
            }
            return sb.ToString();
        }

        private string Signature()
        {
            var message = _nonce + AppId + ApiKey;
            var signature = ConfigureHmac(ApiSecret, message);
            return signature;
        }

        /// <summary>
        /// Add the required headers needed for coingate auth
        /// </summary>
        /// <param name="signature"></param>
        private void ConfigureHeaders(string signature)
        {
            _client.DefaultRequestHeaders.Add("Access-Key", ApiKey);
            _client.DefaultRequestHeaders.Add("Access-Nonce", _nonce.ToString());
            _client.DefaultRequestHeaders.Add("Access-Signature", signature);
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
        public async Task<dynamic> GetOrders(string resourcePath = "/v2/orders/", int pageNo = 1, int pageSize = 10, string sort = "created_at_desc")
        {
            _client.BaseAddress = new Uri(_baseUri);
             ConfigureHeaders(Signature());
            string url = resourcePath + "?per_page=" + pageNo + "&page=" + pageSize + "&sort=" + sort;
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return HttpStatusCode.BadRequest;
            var orders = await response.Content.ReadAsAsync<dynamic>();
            return orders;
        }

        /// <summary>
        /// Get order by id https://developer.coingate.com/docs/get-order
        /// </summary>
        /// <param name="orderId">The order id</param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public async Task<dynamic> GetOrder(int orderId, string resourcePath = "/v2/orders/")
        {
            _client.BaseAddress = new Uri(_baseUri);
            ConfigureHeaders(Signature());
            var response = await _client.GetAsync(resourcePath + orderId);
            if (!response.IsSuccessStatusCode) return HttpStatusCode.BadRequest;
            var order = await response.Content.ReadAsAsync<dynamic>();
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
            ConfigureHeaders(Signature());
            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("order_id", dto.OrderId.ToString()),
                new KeyValuePair<string, string>("price", dto.Price.ToString()),
                new KeyValuePair<string, string>("currency", dto.Currency),
                new KeyValuePair<string, string>("receive_currency", dto.ReceiveCurrency),
                new KeyValuePair<string, string>("title", dto.Title),
                new KeyValuePair<string, string>("description", dto.Description),
                new KeyValuePair<string, string>("callback_url", dto.CallbackUrl),
                new KeyValuePair<string, string>("cancel_url", dto.CancelUrl),
                new KeyValuePair<string, string>("success_url", dto.SuccessUrl)
            });
            var response = await _client.PostAsync(resourcePath, body);
            if (!response.IsSuccessStatusCode) return HttpStatusCode.BadRequest;
            var order = await response.Content.ReadAsAsync<dynamic>();
            return order;
        }
    }
}
