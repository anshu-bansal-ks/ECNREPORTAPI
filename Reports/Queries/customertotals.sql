IF OBJECT_ID('tempdb..#vstable') IS NOT NULL
DROP TABLE #vstable;
SELECT invoice_no
, invoice_date
, po_no
, customer_id
, customer_name
 , supplier_id
 , supplier_name
 , amt
 INTO #vstable
 FROM ( SELECT ih.invoice_no
 , ih.invoice_date
 , ih.po_no
 , ih.customer_id
 , c.customer_name
 , il.supplier_id
 , s.supplier_name
 , SUM(il.extended_price) amt
 FROM ccecn.dbo.p21_view_invoice_hdr ih
 JOIN ccecn.dbo.p21_view_customer c ON c.customer_id = ih.customer_id
 JOIN ccecn.dbo.p21_view_invoice_line il ON il.invoice_no = ih.invoice_no
 JOIN ccecn.dbo.p21_view_supplier s ON s.supplier_id = il.supplier_id
 WHERE ih.invoice_date > '8/3/21 00:00:00'
 AND (ih.po_no LIKE 'ECN21%')
 AND il.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND delete_flag = 0
 )
 GROUP BY ih.invoice_no
 , ih.invoice_date
 , ih.po_no
 , ih.customer_id
 , c.customer_name
 , il.supplier_id
 , s.supplier_name
 ) vsresults;
 SELECT customer_id
 , customer_name
 , SUM(amt) showtot
 FROM #vstable
 WHERE (
 po_no LIKE 'ECN21-BO%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND delete_flag = 0
 ))
 OR
 (
 po_no LIKE 'ECN21-W1%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND [week] = 1
 AND delete_flag = 0
 ))
 OR
 (
 po_no LIKE 'ECN21-W2%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND [week] = 2
 AND delete_flag = 0
 ))
 OR
 (
 po_no LIKE 'ECN21-W3%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND [week] = 3
 AND delete_flag = 0
 ))
 OR
 (
 po_no LIKE 'ECN21-W4%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND [week] = 4
 AND delete_flag = 0
 ))
 OR
 (
 po_no LIKE 'ECN21-W5%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND [week] = 5
 AND delete_flag = 0
 ))
 OR
 (
 po_no LIKE 'ECN21-W6%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND [week] = 6
 AND delete_flag = 0
 ))
 OR
 (
 po_no LIKE 'ECN21-W7%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND [week] = 7
 AND delete_flag = 0
 ))
 OR
 (
 po_no LIKE 'ECN21-W8%'
 AND #vstable.supplier_id IN (
 SELECT DISTINCT
 supplier_id
 FROM {dashboard}.dbo.[vshowdisc]
 WHERE [year] = 2021
 AND [week] = 8
 AND delete_flag = 0
 ))
 GROUP BY customer_id
 , customer_name
 ORDER BY customer_name