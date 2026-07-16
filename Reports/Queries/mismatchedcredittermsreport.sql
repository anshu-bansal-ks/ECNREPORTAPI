SELECT c.customer_id
, s.ship_to_id
, c.customer_name
, a.name ship2_name
, t.terms_desc bill_to_terms
, shipt.terms_desc ship2_terms
FROM p21_view_customer c
JOIN dbo.p21_view_ship_to s ON s.customer_id = c.customer_id
JOIN p21_view_terms t ON t.terms_id = c.terms_id
JOIN p21_view_terms shipt ON shipt.terms_id = s.terms_id
JOIN dbo.p21_view_address a ON a.id = s.ship_to_id
WHERE c.terms_id <> s.terms_id
AND c.delete_flag = 'N'
AND s.delete_flag = 'N'
ORDER BY c.customer_id
, s.ship_to_id