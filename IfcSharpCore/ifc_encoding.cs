// ifc_encoding.cs, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)
// ported from
/// https://github.com/jmirtsch/GeometryGymIFC/blob/master/Core/IFC/ParserIFC.cs
/// http://www.buildingsmart-tech.org/implementation/get-started/string-encoding/string-encoding-decoding-summary

namespace ifc{//==============================

	public static class IfcString
	{

public static char[] SpecialCharacters={'δ','Δ','φ','Φ','ό','ά','ε','Ε','ί','\n'};
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
         result=result.Replace("\\X2\\00E4\\X0\\","δ");
         result=result.Replace("\\X2\\00C4\\X0\\","Δ");
         result=result.Replace("\\X2\\00F6\\X0\\","φ");
         result=result.Replace("\\X2\\00F600DF\\X0\\","φί"); // 2024-04-02 (bb): added
         result=result.Replace("\\X2\\00D6\\X0\\","Φ");
         result=result.Replace("\\X2\\00FC\\X0\\","ό");
         result=result.Replace("\\X2\\00DC\\X0\\","ά");
         result=result.Replace("\\X2\\00E5\\X0\\","ε");
         result=result.Replace("\\X2\\00C5\\X0\\","Ε");
         result=result.Replace("\\X2\\00DF\\X0\\","ί");
         result=result.Replace("\\X2\\000A\\X0\\","\n");
         result=result.Replace("\\X2\\005c\\X0\\","\\");
         result=result.Replace("\\X2\\0027\\X0\\","'");
		 return result;	
		}
 }// of IfcString

/*
"\S\d",'δ
"\S\D",'Δ
"\S\v",'φ
"\S\V",'Φ
"\S\|",'ό
"\S\\",'ά


"\X\E4",'δ'
"\X\C4",'Δ
"\X\F6",'φ
"\X\D6",'Φ
"\X\FC",'ό
"\X\DC",'ά
"\X\E5",'ε
"\X\C5",'Ε
"\X\DF",'ί
"\X\0A",'\n'

"\X2\00E4\X0\",'δ'
"\X2\00C4\X0\",'Δ
"\X2\00F6\X0\",'φ
"\X2\00D6\X0\",'Φ
"\X2\00FC\X0\",'ό
"\X2\00DC\X0\",'ά
"\X2\00E5\X0\",'ε
"\X2\00C5\X0\",'Ε
"\X2\00DF\X0\",'ί
"\X2\000A\X0\",'\n'
*/

}// IFC4==============================
