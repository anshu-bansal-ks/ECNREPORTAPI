SELECT 
                    invoice_date, invoice_no, po_no,
                    ISNULL([Curr], 0) AS Curr,
                    ISNULL([Over30], 0) AS Over30,
                    ISNULL([Over60], 0) AS Over60,
                    ISNULL([Over90], 0) AS Over90,
                    ISNULL([Curr], 0) + ISNULL([Over30], 0) + ISNULL([Over60], 0) + ISNULL([Over90], 0) AS Total
                FROM (
                    SELECT AGEGROUP, SUM(open_amount) AS AMT, invoice_date, invoice_no, po_no
                    FROM (
                        SELECT p.invoice_date, p.invoice_no, p.open_amount, p.po_no,
                               CASE  
                                    WHEN DATEDIFF(dd, p.invoice_date, GETDATE()) > 90 THEN 'Over90'
                                    WHEN DATEDIFF(dd, p.invoice_date, GETDATE()) > 60 THEN 'Over60'
                                    WHEN DATEDIFF(dd, p.invoice_date, GETDATE()) > 30 THEN 'Over30'
                                    ELSE 'Curr'
                               END AS AGEGROUP
                        FROM p21_view_apinv_hdr p WITH (NOLOCK)
                        WHERE p.vendor_id = @vendorId AND p.paid_in_full = 'N'
                    ) t1
                    GROUP BY AGEGROUP, invoice_date, invoice_no, po_no
                ) t2
                PIVOT (SUM(AMT) FOR AGEGROUP IN ([Curr], [Over30], [Over60], [Over90])) AS PivotTable
                ORDER BY invoice_date DESC, invoice_no