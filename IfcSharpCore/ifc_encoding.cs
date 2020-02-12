// ifc_encoding.cs, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)
// ported from
/// https://github.com/jmirtsch/GeometryGymIFC/blob/master/Core/IFC/ParserIFC.cs
/// http://www.buildingsmart-tech.org/implementation/get-started/string-encoding/string-encoding-decoding-summary

namespace ifc{//==============================

	public static class IfcString
	{

public static char[] SpecialCharacters={'ä','Ä','ö','Ö','ü','Ü','å','Å','ß','\n'};
public static bool ContainsSpecialCharacters(string str)
{
foreach (char c in str) foreach (char sc in SpecialCharacters) if (c==sc) return true;
return false;
}


		public static string Encode(string str) 
		{
         string result = "";
         for (int pos=0;pos<str.Length  ;pos++) {int i=str[pos];if (i < 32 || i > 126 || i==(int)'\\' || i==(int)'\'') result += "\\X2\\" + string.Format("{0:x4}", i).ToUpper() + "\\X0\\"; else result+=(char)i;}
         return result;
		}

		public static string Decode(string str)
		{string result=str;
         result=result.Replace("\\X2\\00E4\\X0\\","ä");
         result=result.Replace("\\X2\\00C4\\X0\\","Ä");
         result=result.Replace("\\X2\\00F6\\X0\\","ö");
         result=result.Replace("\\X2\\00D6\\X0\\","Ö");
         result=result.Replace("\\X2\\00FC\\X0\\","ü");
         result=result.Replace("\\X2\\00DC\\X0\\","Ü");
         result=result.Replace("\\X2\\00E5\\X0\\","å");
         result=result.Replace("\\X2\\00C5\\X0\\","Å");
         result=result.Replace("\\X2\\00DF\\X0\\","ß");
         result=result.Replace("\\X2\\000A\\X0\\","\n");
         result=result.Replace("\\X2\\005c\\X0\\","\\");
         result=result.Replace("\\X2\\0027\\X0\\","'");
		 return result;	
		}
 }// of IfcString

/*
"\S\d",'ä
"\S\D",'Ä
"\S\v",'ö
"\S\V",'Ö
"\S\|",'ü
"\S\\",'Ü


"\X\E4",'ä'
"\X\C4",'Ä
"\X\F6",'ö
"\X\D6",'Ö
"\X\FC",'ü
"\X\DC",'Ü
"\X\E5",'å
"\X\C5",'Å
"\X\DF",'ß
"\X\0A",'\n'

"\X2\00E4\X0\",'ä'
"\X2\00C4\X0\",'Ä
"\X2\00F6\X0\",'ö
"\X2\00D6\X0\",'Ö
"\X2\00FC\X0\",'ü
"\X2\00DC\X0\",'Ü
"\X2\00E5\X0\",'å
"\X2\00C5\X0\",'Å
"\X2\00DF\X0\",'ß
"\X2\000A\X0\",'\n'
*/

}// IFC4==============================
