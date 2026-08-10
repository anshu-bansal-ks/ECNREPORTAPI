SELECT ytd.ship_to_id
, a.name as ship_name
, ytd.customer_id
, c.customer_name
, a.phys_address1 as address1
, ISNULL(a.phys_address2, '') as address2
, a.phys_city as city
, a.phys_state as 'state'
, a.phys_postal_code as zip
, ISNULL(a.phys_country,'') as country
, a.central_phone_number as phone_number
, a.url as contact
, (repname.first_name + ' ' + repname.last_name) as rep
, ytd.ly_sales
, ytd.ytd_sales
FROM dbo.V_YTD_SHIPTO ytd
JOIN dbo.ship_to (NOLOCK) ship ON ship.ship_to_id = ytd.ship_to_id
JOIN dbo.ship_to_salesrep (NOLOCK) shiprep ON (shiprep.ship_to_id = ytd.ship_to_id AND shiprep.primary_salesrep = 'Y')
JOIN dbo.customer (NOLOCK) c ON c.customer_id = ship.customer_id
JOIN address (NOLOCK) a ON a.id = ship.ship_to_id
JOIN contacts (NOLOCK) repname ON repname.id = shiprep.salesrep_id
WHERE a.phys_state =@state
ORDER BY name