SELECT inv_mast.item_id
, inv_mast.item_desc
, inventory_supplier_x_loc.location_id
, inventory_supplier_x_loc.average_lead_time as lead_time
, inventory_supplier.supplier_id
, supplier.supplier_name
, inventory_supplier.division_id
, division.division_name
FROM inventory_supplier_x_loc
INNER JOIN inventory_supplier ON inventory_supplier_x_loc.inventory_supplier_uid = inventory_supplier.inventory_supplier_uid
INNER JOIN supplier ON inventory_supplier.supplier_id = supplier.supplier_id
INNER JOIN division ON inventory_supplier.division_id = division.division_id
AND inventory_supplier.supplier_id = division.supplier_id
INNER JOIN inv_loc ON inventory_supplier_x_loc.location_id = inv_loc.location_id
AND inventory_supplier.inv_mast_uid = inv_loc.inv_mast_uid
INNER JOIN location ON location.location_id = inv_loc.location_id
INNER JOIN inv_mast ON inventory_supplier.inv_mast_uid = inv_mast.inv_mast_uid
LEFT OUTER JOIN vendor_supplier ON supplier.supplier_id = vendor_supplier.supplier_id
AND inv_loc.company_id = vendor_supplier.company_id
AND vendor_supplier.primary_vendor = 'Y'
AND vendor_supplier.delete_flag <> 'Y'
WHERE inventory_supplier.supplier_id = @supplierId
AND inv_loc.company_id = @compId
AND inventory_supplier_x_loc.location_id IN (
      SELECT value 
      FROM {dashboard}.dbo.fn_CommaSeparatedStringToTable(
          CASE 
              WHEN @locationId = 'ALL' THEN @LocationList 
              ELSE @locationId 
          END, 
          ','
      )
  )
AND inv_mast.delete_flag = 'N'
ORDER BY inv_mast.item_id
, inventory_supplier_x_loc.location_id;