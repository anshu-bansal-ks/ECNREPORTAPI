-- SECTION 1: BASIC DETAILS
select customer.customer_id,address.name,address.mail_address1,address.mail_address2,
                address.mail_city,address.mail_state,address.mail_postal_code,address.phys_address1,
                address.phys_address2,address.phys_city,address.phys_state,address.phys_postal_code,
                address.central_watts_number,address.central_phone_number,address.central_fax_number,
                address.email_address,address.url,terms.terms_desc,customer.credit_status,
                customer.salesrep_id,contacts.first_name,contacts.last_name,customer.credit_limit,
                customer.date_acct_opened,address.corp_address_id,da_arnote.note ,sf_account_id
                from customer with(nolock)
                join address with(nolock) on customer.customer_id = address.id
                join terms with(nolock) on customer.terms_id = terms.terms_id
                join contacts with(nolock) on customer.salesrep_id = contacts.id
                left outer join da_arnote with(nolock) on da_arnote.customer_id = customer.customer_id
                Left Join {dashboard}.dbo.p21_customer_sf_account_map sf (nolock)
                on sf.customer_id = customer.customer_id
                where customer.customer_id = @custId;


-- SECTION 2: GROUP CODE
SELECT p_groupcodeID,groupcodes.groupcode
FROM {dashboard}.dbo.groupcodes (nolock)
JOIN {dashboard}.dbo.groupdesc ON groupdesc.groupcode = groupcodes.groupcode
WHERE (groupcodes.delete_flag = 0 OR groupcodes.delete_flag IS NULL)
AND company = @compId 
AND customer_id = @custId ;


-- SECTION 3: TOTAL DUE (SAFE)
 SELECT  [CURRENT], [OVER30], [OVER60], [OVER90],
                ISNULL([CURRENT],0)+ISNULL([OVER30],0)+ISNULL([OVER60],0)+ISNULL([OVER90],0) AS TOTALDUE
                FROM ( SELECT AGEGROUP, SUM(amt_remaining_frominv) as AMT FROM
                ( SELECT invoice_hdr.invoice_date, p21_invoice_amt_remaining_view.amt_remaining_frominv,
                datediff(dd, invoice_hdr.invoice_date, getdate()) as [AGE],
                CASE WHEN datediff(dd, invoice_hdr.invoice_date, getdate()) > 90 THEN 'OVER90'
                WHEN datediff(dd, invoice_hdr.invoice_date, getdate()) > 60 THEN 'OVER60'
                WHEN datediff(dd, invoice_hdr.invoice_date, getdate()) > 30 THEN 'OVER30'
                ELSE 'CURRENT' END as AGEGROUP
                FROM invoice_hdr with(nolock), p21_invoice_amt_remaining_view with(nolock)
                WHERE invoice_hdr.invoice_no = p21_invoice_amt_remaining_view.invoice_no
                AND invoice_hdr.customer_id = @custId
                AND p21_invoice_amt_remaining_view.paid_in_full_flag = 'N'
                AND invoice_hdr.consolidated = 'N' )d
                GROUP BY AGEGROUP )s
                PIVOT ( SUM(AMT) FOR AGEGROUP IN([CURRENT],[OVER30],[OVER60],[OVER90]) ) AS PivotTable


-- SECTION 4: SALES SUMMARY (SAFE)
 select DA_CUST_STATS_STATIC.FirstSl, DA_CUST_STATS_STATIC.LastSl,
                DA_CUST_STATS_STATIC.AvgSl, DA_CUST_STATS_STATIC.CountSls,
                DA_CUST_STATS_STATIC.LifeSls, da_ytd_static.ytd_sales, da_ytd_static.ly_sales
                from DA_CUST_STATS_STATIC with(NOLOCK)
                left outer join da_ytd_static with(NOLOCK)
                on DA_CUST_STATS_STATIC.customer_id = da_ytd_static.customer_id
                where DA_CUST_STATS_STATIC.customer_id = @custId
                group by DA_CUST_STATS_STATIC.FirstSl, DA_CUST_STATS_STATIC.LastSl,
                DA_CUST_STATS_STATIC.AvgSl, DA_CUST_STATS_STATIC.CountSls,
                DA_CUST_STATS_STATIC.LifeSls, da_ytd_static.ytd_sales, da_ytd_static.ly_sales



-- SECTION 5: SALES DETAILS (Last 12 Months from Current Month)
Select customer_credit_history.customer_id, customer.customer_name,
            CASE customer_credit_history.month_invoiced
            When 1 then 'JAN' When 2 then 'FEB' When 3 then 'MAR'
            When 4 then 'APR' When 5 then 'MAY' When 6 then 'JUN'
            When 7 then 'JUL' When 8 then 'AUG' When 9 then 'SEP'
            When 10 then 'OCT' When 11 then 'NOV' When 12 then 'DEC'
            END as month_invoicedname,
            customer_credit_history.year_invoiced,
            (customer_credit_history.invoiced_sales + invoiced_other_charges) as invoiced_sales,
            customer_credit_history.amount_paid,
            customer_credit_history.amount_paid - customer_credit_history.invoiced_sales as PmtHist
            from customer_credit_history with (nolock)
            join customer with (nolock) on customer.customer_id = customer_credit_history.customer_id
            where customer_credit_history.customer_id = @custId and
            ( customer_credit_history.year_invoiced = year(getdate()) or
            (customer_credit_history.year_invoiced = year(dateadd(yy, -1, getdate())) and
            customer_credit_history.month_invoiced > month(getdate())) )
            order by customer_credit_history.year_invoiced desc,
            customer_credit_history.month_invoiced desc;