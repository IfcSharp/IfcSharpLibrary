// ifc_header.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)


namespace ifc{//==============================
public class HeaderData{//==============================================================================================
public string description=ifc.Specification.SchemaName;// e.g. ViewDefinition[CoordinationView], only STEP, not ifcXML
public string name="";// FileName
public string time_stamp="";
public string author="Bernhard Simon Bock, Friedrich Eder";
public string organization=@"https://github.com/IfcSharp";
public string preprocessor_version=@"https://github.com/IfcSharp";
public string originating_system="";
public string authorization="";
public string documentation="";
public void Init(string name,string description, string author,string preprocessor_version)
{this.name=name;this.description=description;this.author=author;this.preprocessor_version=preprocessor_version;}
public void Reset(){description=ifc.Specification.SchemaName;name="IfcSharp";time_stamp="";author="Bernhard Simon Bock, Friedrich Eder";organization=@"https://github.com/IfcSharp";preprocessor_version="";originating_system="";authorization=@"https://github.com/IfcSharp";documentation=""; }
}//====================================================================================================================

}// ifc=======================================