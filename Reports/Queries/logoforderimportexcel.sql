Select ex.id
,ex.company
,ex.customer_id
,ex.salesrepid as sales_rep_id
,ex.po_number
,COUNT(eoi.item_id) as no_of_item
, ur.Name as user_name
, ex.dateCreated as date_Created
,CASE ex.Orderstatus WHEN  1 THEN 'No Complete' WHEN 3 THEN 'Complete' END AS status 
from {dashboard}.dbo.excelOrders as ex
Join {dashboard}.dbo.excelOrderItems eoi on eoi.excelorderid = ex.id
Left Join {dashboard}.dbo.users_unifiedportal ur on ur.Userid = ex.userID
where ex.dateCreated BETWEEN {dateRange}
group by  ex.id,ex.company,ex.customer_id
,ex.salesrepid,ex.po_number,
ex.dateCreated,ex.Orderstatus
,ur.Name