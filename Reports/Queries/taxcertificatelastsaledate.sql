SELECT c.customer_id
, c.customer_name
, rep
, a.phys_state
, LastSl as last_sale_date
, ud.resale
FROM p21_view_customer c
JOIN dbo.DA_Cust_Stats_Static stat ON stat.customer_id = c.customer_id
JOIN p21_view_address a ON a.id = c.customer_id
JOIN dbo.da_ytd_static ON da_ytd_static.customer_id = c.customer_id
JOIN DA_Rep ON DA_Rep.customer_id = c.customer_id
LEFT OUTER JOIN customer_ud ud ON ud.customer_id = c.customer_id
WHERE stat.LastSlDays < 730
AND ((@resalecert = 'COMPLETED' AND resale = 'completed')
OR (@resalecert = 'NONE' AND resale IS NULL)
OR (@resalecert = 'PARTIAL' AND resale = 'Partial')
)