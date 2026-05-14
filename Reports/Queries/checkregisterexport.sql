select payments.bank_no
, payments.check_no
, payments.check_date
, payments.vendor_id
, address.name vendor_name
, payments.check_amount
from payments
join company on company.company_id = payments.company_no
join address on address.id = payments.vendor_id
join bank_accounts on bank_accounts.company_no = payments.company_no and bank_accounts.bank_no = payments.bank_no
left join currency_hdr on currency_hdr.currency_id = bank_accounts.currency_id
where (payments.company_no = @compId)
and payments.bank_no = @bank_no
AND payments.check_date BETWEEN {dateRange}
AND payments.company_no IN (@compId)
and check_no like '0%' 
AND check_amount > 0 
order by payments.company_no asc
, bank_accounts.bank_name asc
, payments.check_no asc