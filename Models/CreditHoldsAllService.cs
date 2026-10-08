using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace ECNREPORTAPI.Models
{
    public class CreditHoldsAllModel
    {
        public string customer_id { get; set; } = string.Empty;
        public string customer_name { get; set; } = string.Empty;
        public string terms_desc { get; set; } = string.Empty;
        public string orderHeaderTerms { get; set; } = string.Empty;
        public decimal credit_limit { get; set; }
        public decimal credit_limit_used { get; set; }
        public string credit_status { get; set; } = string.Empty;
        public string carrier { get; set; } = string.Empty;
        public string order_no { get; set; } = string.Empty;
        public string order_date { get; set; } = string.Empty;
        public decimal order_total { get; set; }
        public string validation_status { get; set; } = string.Empty;
        public string job_name { get; set; } = string.Empty;
        public string po_no { get; set; } = string.Empty;
        public string salesrep { get; set; } = string.Empty;
        public long Time_In_Q { get; set; }
        public string comp_id { get; set; } = string.Empty;
    }

    public class CreditHoldsAllService
    {
        private readonly IConfiguration _configuration;

        public CreditHoldsAllService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Dictionary<string, List<CreditHoldsAllModel>>> GetDataAsync()
        {
            var groupedData = new Dictionary<string, List<CreditHoldsAllModel>>();

            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", "creditholdsall.sql");
                if (!File.Exists(filePath)) return groupedData;

                string query = await File.ReadAllTextAsync(filePath);
                string cmpListConfig = _configuration["CMPCODELIST"] ?? "";
                string[] cmplist = cmpListConfig.Split(',', StringSplitOptions.RemoveEmptyEntries);

                Common obj_gd = new Common(_configuration);
                int commandTimeout = int.TryParse(_configuration["SqlCommandTimeOut"], out int timeout) ? timeout : 30;

                foreach (var comp in cmplist)
                {
                    string trimmedComp = comp.Trim();
                    string conStr = obj_gd.GetDataBaseConnectionStringHardCoded(trimmedComp);

                    if (string.IsNullOrEmpty(conStr)) continue;

                    using (var con = new SqlConnection(conStr))
                    {
                        var result = (await con.QueryAsync<CreditHoldsAllModel>(query, commandTimeout: commandTimeout)).ToList();
                        foreach (var item in result)
                        {
                            item.comp_id = trimmedComp;
                        }
                        
                        if (result.Count > 0)
                        {
                            groupedData[trimmedComp] = result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in CreditHoldsAllService: " + ex.Message);
            }

            return groupedData;
        }
    }
}