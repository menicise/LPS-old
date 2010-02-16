-- select primary key and FK contrainsts

SELECT tc.constraint_name,
          tc.constraint_type,
          tc.table_name,
          kcu.column_name,
--	  tc.is_deferrable,
--          tc.initially_deferred,
--          rc.match_option AS match_type,
--          rc.update_rule AS on_update,
--          rc.delete_rule AS on_delete,
          ccu.table_name AS references_table,
          ccu.column_name AS references_field
     FROM information_schema.table_constraints tc
LEFT JOIN information_schema.key_column_usage kcu
       ON tc.constraint_catalog = kcu.constraint_catalog
      AND tc.constraint_schema = kcu.constraint_schema
      AND tc.constraint_name = kcu.constraint_name
LEFT JOIN information_schema.referential_constraints rc
       ON tc.constraint_catalog = rc.constraint_catalog
      AND tc.constraint_schema = rc.constraint_schema
      AND tc.constraint_name = rc.constraint_name
LEFT JOIN information_schema.constraint_column_usage ccu
       ON rc.unique_constraint_catalog = ccu.constraint_catalog
      AND rc.unique_constraint_schema = ccu.constraint_schema
      AND rc.unique_constraint_name = ccu.constraint_name
WHERE tc.constraint_type in ('FOREIGN KEY', 'PRIMARY KEY')
ORDER BY tc.constraint_name




-- select whoole table and constraints
SELECT 
  cols.ordinal_position, cols.column_name, cols.udt_name, cols.is_nullable, 
  cols.character_maximum_length, cols.numeric_precision, cols.numeric_precision_radix, cols.numeric_scale,
  tc.constraint_type, tc.constraint_name, tc.table_name, kcu.column_name,
--  tc.is_deferrable, tc.initially_deferred, rc.match_option AS match_type, rc.update_rule AS on_update, rc.delete_rule AS on_delete,
  ccu.table_name AS references_table,
  ccu.column_name AS references_field
from information_schema.columns cols
LEFT JOIN information_schema.key_column_usage kcu
       ON cols.column_name = kcu.column_name
      AND cols.table_schema = kcu.table_schema
      AND cols.table_name = kcu.table_name
LEFT JOIN information_schema.table_constraints tc
       ON tc.constraint_catalog = kcu.constraint_catalog
      AND tc.constraint_schema = kcu.constraint_schema
      AND tc.constraint_name = kcu.constraint_name
LEFT JOIN information_schema.referential_constraints rc
       ON tc.constraint_catalog = rc.constraint_catalog
      AND tc.constraint_schema = rc.constraint_schema
      AND tc.constraint_name = rc.constraint_name
LEFT JOIN information_schema.constraint_column_usage ccu
       ON rc.unique_constraint_catalog = ccu.constraint_catalog
      AND rc.unique_constraint_schema = ccu.constraint_schema
      AND rc.unique_constraint_name = ccu.constraint_name
where cols.table_schema = 'public' 
and cols.table_name = 'c_dph'
order by cols.ordinal_position

