SELECT customer_id
,customer_name
,salesrep_id, rep
,[year]
,ISNULL(Jan,0) as Jan
,ISNULL(Feb,0) as Feb
,ISNULL(Mar,0) as Mar
,ISNULL(APR,0) as Apr
,ISNULL(May,0) as May
,ISNULL(Jun,0) as Jun
,ISNULL(Jul,0) as Jul
,ISNULL(Aug,0) as Aug
,ISNULL(Sep,0) as Sep
,ISNULL(Oct,0) as Oct
,ISNULL(Nov,0) as Nov
,ISNULL(Dec,0) as Dec
,ISNULL(Jan, 0) + ISNULL(Feb, 0) + ISNULL(Mar, 0) + ISNULL(APR, 0)
+ ISNULL(May, 0) + ISNULL(Jun, 0) + ISNULL(Jul, 0) + ISNULL(Aug, 0)
+ ISNULL(Sep, 0) + ISNULL(Oct, 0) + ISNULL(Nov, 0) + ISNULL(Dec, 0) as Total
FROM ( SELECT * FROM tbl_MonthlySlsxSupplierxCust
WHERE [Year] >= @year
AND supplier_id = @supplierId
AND( @repId = 'ALL' OR salesrep_id = @repId ) 
) SalesSummary PIVOT (
SUM(total_amount)
FOR [Month] IN (
[Jan],[Feb],[Mar],[Apr],
[May],[Jun],[Jul],[Aug],
[Sep],[Oct],[Nov],[Dec])) AS PivotTable
order by customer_name, [year] desc
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;