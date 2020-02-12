# IfcSharpLibrary

The C#-Library of IfcSharp contains interfaces to the IFC-model of buildingSMART, which is described here:<BR/>
https://technical.buildingsmart.org/standards/ifc/ifc-schema-specifications/


With the IfcSharp Library you can write IFC-models by C#-code or read and write to different formats, like this:

```csharp
ifc.Repository.CurrentModel.ToStepFile();  // creates hello_project_output.ifc (step-format)
ifc.Repository.CurrentModel.ToHtmlFile();  // creates hello_project_output.html in step-format with syntax highlighting
ifc.Repository.CurrentModel.ToCsFile();    // creates hello_project_output.cs with c# code (useful for creating code from existing files)
ifc.Repository.CurrentModel.ToSqliteFile();// creates hello_project_output.sqlite3 with the default option exportCompleteSchema=false 
ifc.Repository.CurrentModel.ToXmlFile();   // creates hello_project_output.ifcXml
ifc.Repository.CurrentModel.ToSqlFile();   // creates SQL for ifcSQL without server-connection
ifc.Repository.CurrentModel.ToSql(ServerName: System.Environment.GetEnvironmentVariable("SqlServer"), DatabaseName:"ifcSQL",ProjectId:3,WriteMode:ifc.Model.eWriteMode.OnlyIfEmpty); // Sql server connection required
```

One of these Formats is ifcSQL, that ist not intended to transport Models form A to B. It is instead intended to store and query models as a collection of digital twins.
ifcSQL is described in the Repository-Folder 'ifcSQL' in IfcSharpLibrary.
