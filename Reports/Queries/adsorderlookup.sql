SELECT 'IVD' AS company,
oe_hdr.ship2_name,
oe_hdr.order_no,
oe_hdr.cancel_flag,
oe_hdr.po_no,
oe_pick_ticket.pick_ticket_no,
oe_pick_ticket.delete_flag,
oe_pick_ticket.print_date,
p21_view_invoice_hdr.invoice_no,
p21_view_invoice_hdr.date_created,
oe_pick_ticket.last_maintained_by
FROM ccivd.dbo.oe_hdr WITH (NOLOCK)
LEFT OUTER JOIN ccivd.dbo.oe_pick_ticket WITH (NOLOCK) ON ccivd.dbo.oe_hdr.order_no = ccivd.dbo.oe_pick_ticket.order_no
LEFT OUTER JOIN ccivd.dbo.p21_view_invoice_hdr WITH (NOLOCK) ON ccivd.dbo.oe_pick_ticket.invoice_no = ccivd.dbo.p21_view_invoice_hdr.invoice_no
WHERE oe_hdr.po_no LIKE @pono
UNION
SELECT 'ECN' AS company,
oe_hdr.ship2_name,
oe_hdr.order_no,
oe_hdr.cancel_flag,
oe_hdr.po_no,
oe_pick_ticket.pick_ticket_no,
oe_pick_ticket.delete_flag,
oe_pick_ticket.print_date,
p21_view_invoice_hdr.invoice_no,
p21_view_invoice_hdr.date_created,
oe_pick_ticket.last_maintained_by
FROM ccecn.dbo.oe_hdr WITH (NOLOCK)
LEFT OUTER JOIN ccecn.dbo.oe_pick_ticket WITH (NOLOCK) ON ccecn.dbo.oe_hdr.order_no = ccecn.dbo.oe_pick_ticket.order_no
LEFT OUTER JOIN ccecn.dbo.p21_view_invoice_hdr WITH (NOLOCK) ON ccecn.dbo.oe_pick_ticket.invoice_no = ccecn.dbo.p21_view_invoice_hdr.invoice_no
WHERE oe_hdr.po_no LIKE @pono
UNION
SELECT 'XGEN' AS company,
oe_hdr.ship2_name,
oe_hdr.order_no,
oe_hdr.cancel_flag,
oe_hdr.po_no,
oe_pick_ticket.pick_ticket_no,
oe_pick_ticket.delete_flag,
oe_pick_ticket.print_date,
p21_view_invoice_hdr.invoice_no,
p21_view_invoice_hdr.date_created,
oe_pick_ticket.last_maintained_by
FROM ccxg.dbo.oe_hdr WITH (NOLOCK)
LEFT OUTER JOIN ccxg.dbo.oe_pick_ticket WITH (NOLOCK) ON ccxg.dbo.oe_hdr.order_no = ccxg.dbo.oe_pick_ticket.order_no
LEFT OUTER JOIN ccxg.dbo.p21_view_invoice_hdr WITH (NOLOCK) ON ccxg.dbo.oe_pick_ticket.invoice_no = ccxg.dbo.p21_view_invoice_hdr.invoice_no
WHERE oe_hdr.po_no LIKE @pono