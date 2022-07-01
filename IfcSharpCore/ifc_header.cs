// ifc_header.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)


namespace ifc{//==============================
public class HeaderData{//==============================================================================================
//see: https://standards.buildingsmart.org/documents/Implementation/ImplementationGuide_IFCHeaderData_Version_1.0.2.pdf
public string ViewDefinition="$";// e.g. ViewDefinition[CoordinationView], only STEP, not ifcXML
public string ImplementationLevel = "2;1";
public string Name="";// FileName
public string TimeStamp="";
public string Author="Bernhard Simon Bock, Friedrich Eder";
public string Organization=@"https://github.com/IfcSharp";
public string PreprocessorVersion=@"https://github.com/IfcSharp";
public string OriginatingSystem="";
public string Authorization="";
public string Documentation="";
public string FileSchema = ifc.Specification.SchemaName;
public void Init(string name,string viewDefinition, string author,string preprocessorVersion)
{this.Name=name;this.ViewDefinition=viewDefinition;this.Author=author;this.PreprocessorVersion=preprocessorVersion;}
public void Reset(){ViewDefinition=ifc.Specification.SchemaName;Name="IfcSharp";TimeStamp="";Author="Bernhard Simon Bock, Friedrich Eder";Organization=@"https://github.com/IfcSharp";PreprocessorVersion="";OriginatingSystem="";Authorization=@"https://github.com/IfcSharp";Documentation=""; }
}//====================================================================================================================

}// ifc=======================================