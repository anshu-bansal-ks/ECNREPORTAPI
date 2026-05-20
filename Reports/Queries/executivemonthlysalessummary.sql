 SELECT [yr] as year,CUSTTYPE as customer_type,
isnull([Jan], 0) as [Jan],
isnull([Feb], 0) as [Feb],
isnull([Mar], 0) as [Mar],
isnull([Apr], 0) as [Apr],
isnull([May], 0) as [May],
isnull([Jun], 0) as [Jun],
isnull([Jul], 0) as [Jul],
isnull([Aug], 0) as [Aug],
isnull([Sep], 0) as [Sep],
isnull([Oct], 0) as [Oct],
isnull([Nov], 0) as [Nov],
isnull([Dec], 0) as [Dec],
(
isnull([Jan], 0)+
isnull([Feb], 0) +
isnull([Mar], 0) +
isnull([Apr], 0) +
isnull([May], 0) +
isnull([Jun], 0) +
isnull([Jul], 0) +
isnull([Aug], 0) +
isnull([Sep], 0) +
isnull([Oct], 0) +
isnull([Nov], 0) +
isnull([Dec], 0) 
) AS [total]
FROM(
SELECT
YEAR(invoice_date) [Yr],
CASE 
WHEN ship2_name LIKE 'ADS - %' THEN 'ADS' 
ELSE 'B2B' 
END CUSTTYPE ,
left(datename(m, invoice_date),3) as [Mnth],
isnull((total_amount - freight),0) total_amount
FROM invoice_hdr(nolock)
WHERE year(invoice_date) > 2005 AND YEAR(invoice_date) <= YEAR(GETDATE())
) SalesSummary
PIVOT
(
SUM(total_amount)
FOR [Mnth] IN (
[Jan],[Feb],[Mar],[Apr],
[May],[Jun],[Jul],[Aug],
[Sep],[Oct],[Nov],[Dec]
)
) AS PivotTable
ORDER BY [Yr] desc,CUSTTYPE DESC