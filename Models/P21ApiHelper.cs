using Dapper;
using Microsoft.Data.SqlClient;
using RestSharp;
using System.Xml.Linq;

namespace ECNREPORTAPI.Models
{
    public class P21TokenInfo
    {
        public string ApiUrl  { get; set; } = "";
        public string ApiUser { get; set; } = "";
        public string ApiPw   { get; set; } = "";
        public string Token   { get; set; } = "";
    }

    public class P21ApiHelper
    {
        private readonly IConfiguration _config;
        private readonly string _dashboard;

        public P21ApiHelper(IConfiguration config)
        {
            _config = config;
            _dashboard = _config["DASHBOARD"] ?? "";
        }

        public async Task<P21TokenInfo?> GetTokenAsync(string compId)
        {
            var common = new Common(_config);
            using var con = new SqlConnection(common.ConStr);

            var row = await con.QueryFirstOrDefaultAsync<dynamic>(
                $@"SELECT p21ApiURL, p21ApiUser, p21ApiPW 
                   FROM {_dashboard}.dbo.datasources 
                   WHERE dsource = @code OR code = @code",
                new { code = compId });

            if (row == null || string.IsNullOrWhiteSpace((string?)row.p21ApiURL))
                return null;

            string apiUrl  = ((string)row.p21ApiURL).TrimEnd('/');
            string apiUser = (string)(row.p21ApiUser ?? "");
            string apiPw   = (string)(row.p21ApiPW ?? "");

            var client = new RestClient($"{apiUrl}/api/security/token");
            var req = new RestRequest();
            req.AddQueryParameter("username", apiUser);
            req.AddQueryParameter("password", apiPw);

            var res = await client.ExecutePostAsync(req);
            if (!res.IsSuccessful || string.IsNullOrWhiteSpace(res.Content))
                return null;

            return new P21TokenInfo
            {
                ApiUrl  = apiUrl,
                ApiUser = apiUser,
                ApiPw   = apiPw,
                Token   = res.Content.Trim().Trim('"')
            };
        }

        public async Task<string> ValidateOrderAsync(
            string compId, P21TokenInfo info, string customerId, string orderNo)
        {
            try
            {
                var client = new RestClient($"{info.ApiUrl}/uiserver0/api/v2/transaction");
                var req = new RestRequest();
                req.AddHeader("Content-Type", "application/xml");
                req.AddHeader("Accept", "text/xml");
                req.AddHeader("Authorization", info.Token);
                req.AddQueryParameter("token", info.Token);

                string xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<TransactionSet>
  <Transactions>
    <Transaction>
      <Name>Order</Name>
      <Operations>
        <Operation>
          <Name>Validate</Name>
          <Keys>
            <Key><Name>OrderNumber</Name><Value>{orderNo}</Value></Key>
            <Key><Name>CustomerId</Name><Value>{customerId}</Value></Key>
          </Keys>
        </Operation>
      </Operations>
    </Transaction>
  </Transactions>
</TransactionSet>";

                req.AddStringBody(xml, DataFormat.Xml);
                var res = await client.ExecutePostAsync(req);

                if (res.IsSuccessful && !string.IsNullOrWhiteSpace(res.Content))
                {
                    string status = "Unknown";
                    try
                    {
                        var xdoc = XDocument.Parse(res.Content);
                        status = xdoc.Descendants("Status").FirstOrDefault()?.Value ?? "Unknown";
                    }
                    catch { }

                    return $"{compId}: Order # {orderNo} Validation Result: {status}";
                }

                return $"{compId}: Order # {orderNo} Error in Validation (HTTP {(int)res.StatusCode})";
            }
            catch (Exception ex)
            {
                return $"{compId}: Order # {orderNo} Error in Validation – {ex.Message}";
            }
        }
    }
}