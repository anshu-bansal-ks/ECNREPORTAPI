SELECT p21_view_customer_notepad.note_id
, c.customer_id
, c.customer_name
, r.salesrep_id
, rep
, topic
, note
, v_cust_note_area.note_area
, mandatory
, p21_view_customer_notepad.date_created
, p21_view_customer_notepad.created_by
FROM dbo.p21_view_customer_notepad
JOIN p21_view_customer c ON c.customer_id = p21_view_customer_notepad.customer_id
JOIN dbo.p21_view_address a ON a.id=c.customer_id
JOIN DA_Rep r ON r.customer_id=c.customer_id
LEFT OUTER JOIN v_cust_note_area ON v_cust_note_area.note_id = p21_view_customer_notepad.note_id
WHERE 1=1
AND p21_view_customer_notepad.delete_flag='N'
AND c.delete_flag='N'
AND r.salesrep_id=@repId
ORDER BY c.customer_name
, p21_view_customer_notepad.date_created DESC

