// ifc_encoding.cs, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)
// ported from
/// https://github.com/jmirtsch/GeometryGymIFC/blob/master/Core/IFC/ParserIFC.cs
/// http://www.buildingsmart-tech.org/implementation/get-started/string-encoding/string-encoding-decoding-summary

namespace ifc{//==============================

	public static class IfcString
	{

public static char[] SpecialCharacters={'�','�','�','�','�','�','�','�','�','\n'};
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
         result=result.Replace("\\X2\\00E4\\X0\\","�");
         result=result.Replace("\\X2\\00C4\\X0\\","�");
         result=result.Replace("\\X2\\00F6\\X0\\","�");
         result=result.Replace("\\X2\\00D6\\X0\\","�");
         result=result.Replace("\\X2\\00FC\\X0\\","�");
         result=result.Replace("\\X2\\00DC\\X0\\","�");
         result=result.Replace("\\X2\\00E5\\X0\\","�");
         result=result.Replace("\\X2\\00C5\\X0\\","�");
         result=result.Replace("\\X2\\00DF\\X0\\","�");
         result=result.Replace("\\X2\\000A\\X0\\","\n");
         result=result.Replace("\\X2\\005c\\X0\\","\\");
         result=result.Replace("\\X2\\0027\\X0\\","'");
		 return result;	
		}
 }// of IfcString

/*
"\S\d",'�
"\S\D",'�
"\S\v",'�
"\S\V",'�
"\S\|",'�
"\S\\",'�


"\X\E4",'�'
"\X\C4",'�
"\X\F6",'�
"\X\D6",'�
"\X\FC",'�
"\X\DC",'�
"\X\E5",'�
"\X\C5",'�
"\X\DF",'�
"\X\0A",'\n'

"\X2\00E4\X0\",'�'
"\X2\00C4\X0\",'�
"\X2\00F6\X0\",'�
"\X2\00D6\X0\",'�
"\X2\00FC\X0\",'�
"\X2\00DC\X0\",'�
"\X2\00E5\X0\",'�
"\X2\00C5\X0\",'�
"\X2\00DF\X0\",'�
"\X2\000A\X0\",'\n'
*/

}// IFC4==============================
