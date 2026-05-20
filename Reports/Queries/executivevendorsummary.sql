select sl.supplier_id,
 sl.supplier_name,
 sl.NJ_Sales,
 COALESCE(inv.NJ_Invty,0) as NJ_Inv_value,
 COALESCE(openpo.NJ_OpenPo,0) as NJ_Open_Po
,sl.FL_Sales,
COALESCE(inv.FL_Invty,0) As FL_Inv_value,
COALESCE(openpo.FL_OpenPo,0) As FL_Open_Po,
sl.CA_Sales
,COALESCE(inv.CA_Invty,0) As CA_Inv_value,
COALESCE(openpo.CA_OpenPo,0) As CA_Open_Po,
(NJ_Sales+FL_Sales+CA_Sales) as Total_Sales
,((sl.NJ_Sales-sl.NJ_COGS)+(sl.FL_Sales-sl.FL_COGS)+(sl.CA_Sales-sl.CA_COGS))/CASE WHEN (NJ_Sales+FL_Sales+CA_Sales)=0 Then null else (NJ_Sales+FL_Sales+CA_Sales) End * 100 as profit
,(COALESCE(inv.NJ_Invty,0)+COALESCE(inv.FL_Invty,0)+COALESCE(inv.CA_Invty,0) ) as Total_Inv_value
,(COALESCE(openpo.NJ_OpenPo,0)+COALESCE(openpo.FL_OpenPo,0)+COALESCE(openpo.CA_OpenPo,0)) As Total_Open_Po
from ( SELECT il.supplier_id, s.supplier_name
, SUM (CASE WHEN l.location_id=100035 THEN il.extended_price ELSE 0 END) AS NJ_Sales
, SUM (CASE WHEN l.location_id=100035 THEN il.cogs_amount ELSE 0 END) AS NJ_COGS
, SUM (CASE WHEN l.location_id=100038 THEN il.extended_price ELSE 0 END) AS FL_Sales
, SUM (CASE WHEN l.location_id=100038 THEN il.cogs_amount ELSE 0 END) AS FL_COGS
, SUM (CASE WHEN l.location_id=100105 THEN il.extended_price ELSE 0 END) AS CA_Sales 
, SUM (CASE WHEN l.location_id=100105 THEN il.cogs_amount ELSE 0 END) AS CA_COGS
FROM p21_view_invoice_hdr ih
INNER JOIN p21_view_invoice_line il ON ih.invoice_no=il.invoice_no
INNER JOIN p21_view_supplier s ON s.supplier_id=il.supplier_id
INNER JOIN dbo.p21_view_oe_pick_ticket pt ON pt.invoice_no=ih.invoice_no
INNER JOIN dbo.p21_view_location l ON l.location_id=pt.location_id
WHERE ih.invoice_date > DATEADD(DAY,-(@daysold),GETDATE()) 
AND pt.location_id IN ( 100035, 100038, 100105 )
GROUP BY il.supplier_id, s.supplier_name )sl
left Join ( SELECT supplier_id
, ISNULL ([100035], 0) NJ_Invty
, ISNULL ([100038], 0) FL_Invty
, ISNULL ([100105], 0) CA_Invty
, ISNULL ([100035], 0) +ISNULL ([100038], 0) +ISNULL ([100105], 0) TotalValue
FROM( SELECT s.supplier_id, invval.location_id
, ROUND (SUM (invval.qty_on_hand * invval.cost), 2) value
FROM p21_view_inventory_value_report invval
JOIN dbo.p21_view_supplier s ON s.supplier_id=invval.primary_supplier_id
WHERE  invval.location_id IN ( 100035, 100038, 100105 )
GROUP BY s.supplier_id, invval.location_id
) locvalue
PIVOT(SUM(value)
FOR [location_id] IN([100035], [100038], [100105])
) AS PvtInv
)inv on inv.supplier_id = sl.supplier_id
left join( SELECT s.supplier_id, s.supplier_name
, SUM (CASE WHEN p21_view_location.location_id=100035 THEN (unit_price * (qty_ordered-qty_received)) ELSE 0 END) AS NJ_OpenPo
, SUM (CASE WHEN p21_view_location.location_id=100038 THEN (unit_price * (qty_ordered-qty_received)) ELSE 0 END) AS FL_OpenPo
, SUM (CASE WHEN p21_view_location.location_id=100105 THEN (unit_price * (qty_ordered-qty_received)) ELSE 0 END) AS CA_OpenPo
FROM p21_view_po_hdr
JOIN p21_view_po_line ON p21_view_po_line.po_no = p21_view_po_hdr.po_no
JOIN p21_view_location ON p21_view_location.location_id = p21_view_po_hdr.location_id
JOIN p21_view_supplier s ON s.supplier_id = p21_view_po_hdr.supplier_id
JOIN p21_view_inventory_supplier invsup ON ( invsup.inv_mast_uid = p21_view_po_line.inv_mast_uid 
AND invsup.supplier_id = p21_view_po_hdr.supplier_id)
WHERE p21_view_po_line.cancel_flag = 'n'
AND (p21_view_po_hdr.cancel_flag = 'n' OR p21_view_po_hdr.cancel_flag IS NULL)
AND p21_view_po_line.complete = 'n'
AND p21_view_po_line.delete_flag = 'n'
AND p21_view_po_hdr.delete_flag = 'N'
AND po_type NOT IN ( 'Q', 'X' )
GROUP BY s.supplier_id, s.supplier_name
)openpo on openpo.supplier_id = sl.supplier_id
Order By sl.supplier_name