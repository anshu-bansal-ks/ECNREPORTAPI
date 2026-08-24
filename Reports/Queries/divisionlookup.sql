SELECT p21_view_division.division_id,
p21_view_division.division_name,
p21_view_division.supplier_id,
p21_view_supplier.supplier_name
FROM p21_view_division p21_view_division
JOIN p21_view_supplier p21_view_supplier ON p21_view_division.supplier_id = p21_view_supplier.supplier_id
WHERE p21_view_division.delete_flag = 'N'
AND p21_view_supplier.delete_flag = 'N'
AND p21_view_division.division_name LIKE '%' + @strfilter + '%'
ORDER BY p21_view_division.division_name,
p21_view_supplier.supplier_name