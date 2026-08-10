SELECT DISTINCT c.customer_id
, c.customer_name
, ISNULL(ytd.lytd_sales, 0) as lytd_sales
, ISNULL(ytd.ytd_sales, 0) as ytd_sales
, rep
FROM p21_view_customer c
LEFT OUTER JOIN dbo.da_ytd_static ytd ON ytd.customer_id = c.customer_id
JOIN da_rep rep ON rep.customer_id = c.customer_id
WHERE c.customer_id IN (
SELECT customer_id
FROM {dashboard}.dbo.groupcodes
WHERE [groupcodes].groupcode = @group_code
AND groupcodes.company = @CompId
AND ISNULL(groupcodes.delete_flag, 0) = 0)
ORDER BY c.customer_id