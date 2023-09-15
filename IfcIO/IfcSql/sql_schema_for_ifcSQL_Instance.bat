sqlcmd -S%SqlServer%  -dSchemaEvaluation -Q"EXECUTE [SchemaEvaluation].[dbo].[print_CS] 'ifcSQL',9,'ifc_in_out_sql','_ifcSQL_for_ifcSQL_instance','ifcSQL'" > ifcSQL_for_ifcSQL_Instance_db_generated.cs
pause

