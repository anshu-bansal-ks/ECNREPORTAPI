SELECT customer.customer_id
,customer.customer_name
,da_cust_stats_static.lastsl as last_sale_date
,right(payment_account.acct_no,4) as card_ending
,payment_types.payment_type_desc as payment_type
,payment_account.payment_acct_desc as card_name
,payment_account.acct_expiration_date as expiration_date
,da_rep.rep
FROM customer WITH ( NOLOCK )
LEFT OUTER JOIN da_rep (NOLOCK) ON dbo.customer.customer_id = dbo.DA_Rep.customer_id
LEFT OUTER JOIN payment_account_x_customer(NOLOCK) ON payment_account_x_customer.customer_id = customer.customer_id
LEFT OUTER JOIN payment_account(NOLOCK) ON payment_account_x_customer.payment_account_uid = payment_account.payment_account_uid
LEFT OUTER JOIN payment_types WITH ( NOLOCK ) ON payment_types.payment_type_id = payment_account_x_customer.payment_type_id
LEFT OUTER JOIN dbo.DA_Cust_Stats_Static ON dbo.DA_Cust_Stats_Static.customer_id = customer.customer_id
WHERE ( @repId = 'ALL' OR da_rep.salesrep_id = @repId ) 
And DATEDIFF(dd, GETDATE(), acct_expiration_date) < 60
AND DATEDIFF(dd, GETDATE(), acct_expiration_date) >= -31
AND payment_account.row_status_flag = 704
AND customer.delete_flag = 'N'
ORDER BY rep, customer_name