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
    public class SalesByBrandForVendorModel
    {
        public string supplier_id { get; set; } = string.Empty;
        public string supplier_name { get; set; } = string.Empty;
        public string parker_product_cd { get; set; } = string.Empty;
        public decimal UNITS { get; set; }
        public decimal SALES { get; set; }
        public decimal COST { get; set; }
        public decimal gross_profit { get; set; }
        public decimal profit_percent { get; set; }
        public string str_brandName { get; set; } = string.Empty;
    }

    public class RefBrandModel
    {
        public string str_brandCode { get; set; } = string.Empty;
        public string str_brandName { get; set; } = string.Empty;
    }

    public class SalesByBrandForVendorService
    {
        private readonly IConfiguration _configuration;
        private readonly string _dashboard;

        public SalesByBrandForVendorService(IConfiguration configuration)
        {
            _configuration = configuration;
            _dashboard = _configuration["DASHBOARD"] ?? "DashboardDB";
        }

        public async Task<List<SalesByBrandForVendorModel>> GetDataAsync(string compId, string supplierId, string from_date, string till_date, string t_period)
        {
            var rlist = new List<SalesByBrandForVendorModel>();

            try
            {
                if (!string.IsNullOrEmpty(t_period) && t_period != "Time Period")
                {
                    var pd = Common.getPeriod(t_period);
                    from_date = pd.from_date;
                    till_date = pd.till_date;
                }

                string subquery = "1=1";
                if (!string.IsNullOrEmpty(from_date) && !string.IsNullOrEmpty(till_date))
                {
                    subquery = "ih.invoice_date BETWEEN @fromDate AND @tillDate";
                }

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Queries", "salesbybrandforvendor.sql");
                if (!File.Exists(filePath)) return rlist;

                string query = await File.ReadAllTextAsync(filePath);
                query = query.Replace("{dashboard}", _dashboard)
                             .Replace("{subquery}", subquery);

                Common obj_gd = new Common(_configuration);
                string conStr = string.IsNullOrWhiteSpace(compId) 
                    ? obj_gd.ConStr 
                    : obj_gd.GetDataBaseConnectionStringHardCoded(compId);

                int commandTimeout = int.TryParse(_configuration["SqlCommandTimeOut"], out int timeout) ? timeout : 30;

                IEnumerable<SalesByBrandForVendorModel> salesData;
                using (var con = new SqlConnection(conStr))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@supplierid", supplierId);
                    parameters.Add("@fromDate", !string.IsNullOrEmpty(from_date) ? $"{from_date} 00:00:00" : "1900-01-01");
                    parameters.Add("@tillDate", !string.IsNullOrEmpty(till_date) ? $"{till_date} 23:59:59" : "2099-12-31");

                    salesData = await con.QueryAsync<SalesByBrandForVendorModel>(query, parameters, commandTimeout: commandTimeout);
                }

                string ecnConStr = obj_gd.ConEcnStr;
                IEnumerable<RefBrandModel> brandData;
                string brandQuery = @"SELECT TOP (1000) [str_brandName], UPPER([str_brandCode]) as str_brandCode 
                                      FROM [ecn].[dbo].[refBrands] WHERE bln_valid = 1 ORDER BY dte_updated DESC";

                using (var conEcn = new SqlConnection(ecnConStr))
                {
                    brandData = await conEcn.QueryAsync<RefBrandModel>(brandQuery, commandTimeout: commandTimeout);
                }

                var joinResult = from s in salesData
                                 join b in brandData on s.parker_product_cd equals b.str_brandCode into brandGroup
                                 from bg in brandGroup.DefaultIfEmpty()
                                 select new SalesByBrandForVendorModel
                                 {
                                     supplier_id = s.supplier_id,
                                     supplier_name = s.supplier_name,
                                     parker_product_cd = s.parker_product_cd,
                                     UNITS = s.UNITS,
                                     SALES = s.SALES,
                                     COST = s.COST,
                                     gross_profit = s.gross_profit,
                                     profit_percent = s.profit_percent,
                                     str_brandName = bg?.str_brandName ?? ""
                                 };

                rlist = joinResult.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception occured (SalesByBrandForVendor): " + ex.ToString());
            }

            return rlist;
        }
    }
}