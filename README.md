# IfcSharpLibrary

The C#-Library of IfcSharp contains interfaces to the IFC-model of buildingSMART, which is described [here](https://technical.buildingsmart.org/standards/ifc/ifc-schema-specifications/).

## Introduction

### Create a simple IFC file

The creation of an IFC file requires only a few lines of code. 
The underlying class model has the same structure as the IFC data model and therby enforcing the definitions of the corresponding IFC Schema (2X3, 4.0 etc.)
The advantage of this early-binding approach is that errors in an IFC model can be handled at compile time rather than at runtime (late-binding).<br>

```csharp
// create a new ifc.Model which stores all ifc.ENTITY data inside the ifc.Repository.CurrentModel
ifc.Repository.CurrentModel = new ifc.Model(Name: "hello_project_output"); 

// create a new ifc.Project, this implicitly appends the ifc.ENTITY to ifc.Repository.CurrentModel 
// NOTE: every ifc.ENTITY has an EndOfLineComment which can be used as a comment for STEP files
ifc.Project project = new ifc.Project(Name: new ifc.Label("my first ifc-project"), EndOfLineComment: "creating the project");

// IfcSharpLibrary also takes care of creating the GlobalId if you simple pass 'null'
ifc.Building building = new ifc.Building(GlobalId: null, Name: new ifc.Label("my first ifc-building"));

// due to reflection ifc.ENTITY objects which have no parent dont need to be assigned it to a variable
new ifc.RelAggregates(RelatingObject: project, RelatedObjects: new ifc.Set1toUnbounded_ObjectDefinition(building));

// write the current state of the ifc.Repository.CurrentModel to a STEP file
// the output path defaults to ifc.Model.Name (in this case 'hello_project_output.ifc')
ifc.Repository.CurrentModel.ToStepFile(); 

```

Several IFC versions are supported (see [Supported IFC Schemas](#Supported-IFC-Schemas)).
All IFC versions use the same read and write routines.
The read and write routines use the [Reflection](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/) mechanism.
This avoids that for each class a separate read or write routine must be kept.
The [IfcStep](IfcIO/IfcStep/README.md) export therefore comprises only a few lines of code, since it evaluates the IFC schema mapped in the C# class library in a general valid manner via [Reflection](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/).<br>
This mechanism is also used to keep track of all `ifc.ENTITY` instances in memory and write them to a file or database. This means that once you assigned a `ifc.Repository.CurrentModel` at runtime you can just instantiate objects from anywhere in your project. This makes *IfcSharpLibrary* a very sleek solution as it is reduces the overhead of assigning every single `ifc.ENTITY` to its parent.
### Export capabilities

With the IfcSharp Library you can write IFC-models by C#-code or read and write to different formats, like this:

```csharp
ifc.Repository.CurrentModel.ToStepFile();  // creates hello_project_output.ifc (step-format)
ifc.Repository.CurrentModel.ToHtmlFile();  // creates hello_project_output.html in step-format with syntax highlighting
ifc.Repository.CurrentModel.ToCsFile();    // creates hello_project_output.cs with c# code (useful for creating code from existing files)
ifc.Repository.CurrentModel.ToSqliteFile();// creates hello_project_output.sqlite3 with the default option exportCompleteSchema=false 
ifc.Repository.CurrentModel.ToXmlFile();   // creates hello_project_output.ifcXml
ifc.Repository.CurrentModel.ToSqlFile();   // creates SQL for ifcSQL without server-connection
ifc.Repository.CurrentModel.ToSql(ServerName: System.Environment.GetEnvironmentVariable("SqlServer"), 
                                  DatabaseName: "ifcSQL",
                                  ProjectId: 3,
                                  WriteMode: ifc.Model.eWriteMode.OnlyIfEmpty); // SQL server connection required
```

One of these Formats is ifcSQL, that ist not intended to transport Models form A to B. It is instead intended to store and query models as a collection of digital twins (see [IfcSql documentation](IfcIO/IfcSql/README.md)).

### Further examples

Please refer to examples listed in [IfcSharpApps](https://github.com/IfcSharp/IfcSharpApps)

## Implementation details

### General structure

IfcSharpLibrary can be broken down into three different components:

  1. [IfcSharpCore](IfcSharpCore): Implements general functionality (i.e. types, units, logging etc.)
  2. [IfcSchema](IfcSchema): Implementations for the different IFC schemas as C# classes (see [IfcSchema documentation](/IfcSchema/README.md))
  3. [IfcIO](IfcIO): Currently we support STEP, SQL, SQLite, CS, HTML and XML (some implementations are still in need for further development and testing)

This makes this library highly customizable and enables you to choose specifically which implementations you need.

### Supported IFC Schemas

We currently support these IFC schemas:
 - [IFC2X3](https://standards.buildingsmart.org/IFC/RELEASE/IFC2x3/TC1/HTML/)
 - [IFC4](https://standards.buildingsmart.org/IFC/RELEASE/IFC4/ADD2_TC1/HTML/)
 - [IFC4.1](https://standards.buildingsmart.org/IFC/RELEASE/IFC4_1/FINAL/HTML/)
 - [IFC4.3](https://github.com/buildingSMART/IFC4.3-html/releases/tag/sep-13-release)
 - [IFC4.3.2.0 - Coming soon](https://ifc43-docs.standards.buildingsmart.org/)

> **_NOTE:_** Due to the class model you can only include the `IfcSchema` for **one** IFC version. This means for multiple IFC versions you need to create multiple build-projects.

## How to Install and Run the Project

Simply run `git clone https://github.com/IfcSharp/IfcSharpLibrary.git`.

To start off you can directly add a reference to the included `IfcSharpLibrary.csproj` file, which is configured to use the IFC4 bindings.

> **_NOTE:_** The default `IfcSharpLibrary.csproj` project does not include the optional *[SQLite](https://www.sqlite.org/index.html)* support. In order to use it you have to install the additional dependencies and include the files from `IfcIO/IfcSqlite` in your project (see also [IfcSqlite README](IfcIO/IfcSqlite/README.md)).

After you included/created the project the `ifc` namespace should be available and you can start developing with *IfcSharpLibrary*.

### *Optional: IfcSqlite*

See [IfcSqlite documentation](IfcIO/IfcSqlite/README.md)

## Credits

Bernhard Simon Bock [@bsbock](https://www.github.com/bsbock)<br>
Friedrich Eder [@friedrichEder](https://www.github.com/friedrichEder)

## Licence

This project is licensed under the terms of [this](LICENSE.md) MIT license.


