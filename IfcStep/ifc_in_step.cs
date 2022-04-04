// ifc_in_step.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ifc
{//==============================


    public partial class ENTITY
    {//==========================================================================================

        public static string ReplaceCharAt(string s, int i, char c) { char[] array = s.ToCharArray(); array[i] = c; s = new string(array); return s; }

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static object Parse2TYPE(string value, Type FieldType) {
            Type BaseType = FieldType.BaseType.GetGenericArguments()[0];
            object NewType = null;
            object[] TypeCtorArgs = new object[1];
            if ((value == "$") || (value == "*")) NewType = Activator.CreateInstance(FieldType);
            else {//=====================================================================
                if (BaseType == typeof(String)) { if (value == "$") TypeCtorArgs[0] = ""; else TypeCtorArgs[0] = ifc.IfcString.Decode(value); NewType = Activator.CreateInstance(FieldType, TypeCtorArgs); }
                else if (BaseType == typeof(int)) { TypeCtorArgs[0] = int.Parse(value); NewType = Activator.CreateInstance(FieldType, TypeCtorArgs); }
                else if (BaseType == typeof(double)) try { TypeCtorArgs[0] = double.Parse(value, CultureInfo.InvariantCulture); NewType = Activator.CreateInstance(FieldType, TypeCtorArgs); } catch { Console.WriteLine("Error on Parse2TYPE: " + CurrentLine); }
                else if (BaseType == typeof(bool)) { TypeCtorArgs[0] = (value == ".T."); NewType = Activator.CreateInstance(FieldType, TypeCtorArgs); }
                else if (BaseType.IsSubclassOf(typeof(TypeBase))) { NewType = Activator.CreateInstance(FieldType, Parse2TYPE(value, BaseType)); }
                else Console.WriteLine("UNKNOWN TYPE for expected Type " + FieldType.Name + ": Base=" + BaseType.Name + " value=" + value + "\r\n" + CurrentLine);
            } //=====================================================================
            return NewType;
        }
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


        /*
        Example: List1to3_LengthMeasure
        --------
        Example 1:  CartesianPoint.List1to3_LengthMeasure ( List1to3_LengthMeasure:List1to3<LengthMeasure is TYPE:TYPE<Double>>
        FieldType: List1to3_LengthMeasure
        FieldType.BaseType: List1to3<LengthMeasure>
        FieldType.BaseType.GetGenericArguments()[0]: LengthMeasure
        FieldType.BaseType.GetGenericArguments()[0].BaseType: TYPE<Double>
        FieldType.BaseType.GetGenericArguments()[0].GetGenericArguments()[0]: Double
         * Function target: NewInstance=new List1to3_LengthMeasure(new LengthMeasure(0.1),new LengthMeasure(1.2),new LengthMeasure(2.3));
         Example 2:
         Polyline.List2toUnbounded_CartesianPoint ( List2toUnbounded_CartesianPoint:List2toUnbounded<CartesianPoint: is ENTITY, not TYPE<ENTITY> !!
          [ifcSql(TypeId:  21)] public partial class List2toUnbounded_CartesianPoint:List2toUnbounded<CartesianPoint>{public List2toUnbounded_CartesianPoint(List2toUnbounded<CartesianPoint> value):base(value){} public List2toUnbounded_CartesianPoint(){} public List2toUnbounded_CartesianPoint(params CartesianPoint[] items):base(){foreach (CartesianPoint e in items)  this.Add(e);} new bool IsNull{get{return (this.Count==0);}set{if (value) this.Clear();}} }
        */

        public static Type GetGenericType(Type FieldType) {
            if (FieldType.BaseType.GetGenericArguments().Length > 0) return FieldType.BaseType.GetGenericArguments()[0]; //LengthMeasure or CartesianPoint
            else return FieldType.BaseType.BaseType.GetGenericArguments()[0]; //CompoundPlaneAngleMeasure
        }

        // 2022-04-04 (ef): now supports lists of list
        // TODO 2022-04-04 (ef) : refactor, de-spaghettify, look at use of 'GetFieldCtorArgs' vs. 'Parse2LIST'
        public static object[] GetFieldCtorArgs(Type GenericType, string[] ListElements, Type ParentType=null) {
            object[] FieldCtorArgs = new object[ListElements.Length];

            for (int ListPos = 0; ListPos < ListElements.Length; ListPos++) {//.....................................................
                string ListElement = ListElements[ListPos];
                if (GenericType == typeof(Int32)) {
                    //object[] GenericCtorArgs = new object[1];
                    //GenericCtorArgs[0] = Activator.CreateInstance(GenericType);  // ereugen
                    //GenericCtorArgs[0] = Int32.Parse(ListElement);               // Wert zuweisen                      ---> wetere Verwendng?
                    FieldCtorArgs[ListPos] = Int32.Parse(ListElement);//Activator.CreateInstance(GenericType,GenericCtorArgs);
                }
                else if (GenericType == typeof(double)) {
                    //object[] GenericCtorArgs = new object[1];
                    //GenericCtorArgs[0] = Activator.CreateInstance(GenericType);  // ereugen
                    //GenericCtorArgs[0] = double.Parse(ListElement);               // Wert zuweisen                      ---> wetere Verwendng?
                    FieldCtorArgs[ListPos] = double.Parse(ListElement, CultureInfo.InvariantCulture);
                }
                else if (GenericType.IsSubclassOf(typeof(TypeBase))) {
                    object[] GenericCtorArgs = new object[1];
                    GenericCtorArgs[0] = Activator.CreateInstance(GenericType); //LengthMeasure or CartesianPoint
                    Type GenericBaseType = GenericType.BaseType.GetGenericArguments()[0];    //Double from LengthMeasure -> TYPE<double> -> double
                    if (GenericBaseType == typeof(String)) { if (ListElement == "$") GenericCtorArgs[0] = ""; else GenericCtorArgs[0] = ifc.IfcString.Decode(ListElement); }
                    else if (GenericBaseType == typeof(int)) { GenericCtorArgs[0] = int.Parse(ListElement); }
                    else if (GenericBaseType == typeof(Int32)) { GenericCtorArgs[0] = Int32.Parse(ListElement); }
                    else if (GenericBaseType == typeof(double)) { GenericCtorArgs[0] = double.Parse(ListElement, CultureInfo.InvariantCulture); }
                    else {
                        if (typeof(ifcListInterface).IsAssignableFrom(GenericBaseType)) {
                            GenericCtorArgs[0] = Parse2LIST(GenericType, ListElement);
                        }
                    }
                    FieldCtorArgs[ListPos] = Activator.CreateInstance(GenericType, GenericCtorArgs);
                }
                else if (GenericType.IsSubclassOf(typeof(ENTITY))) {
                    object o = Activator.CreateInstance(GenericType);
                    ((ENTITY)o).LocalId = int.Parse(ListElement.Trim(' ').Substring(1));
                    FieldCtorArgs[ListPos] = o;
                }
                else if (GenericType.IsSubclassOf(typeof(SELECT))) {
                    object o = Activator.CreateInstance(GenericType);
                    ListElement = ListElement.Trim(' ');
                    if ((ListElement.Length > 0) && ListElement[0] == '#') { ((SELECT)o).Id = int.Parse(ListElement.Trim(' ').Substring(1)); } // hat SELECT id ? //  Console.WriteLine("C:"+ ((SELECT)o).Id +" "+( ((SELECT)o).IsNull).ToString() );
                    else {
                        ListElement = ListElement.Replace("IFC", "ifc.");
                        int posLpar = ListElement.IndexOf('(');
                        int posRpar = ListElement.Length - 1;//.LastIndexOf(')');
                        string body = ListElement.Substring(posLpar + 1, posRpar - posLpar - 1); // Argumenkörper extrahieren
                        string ElementName = ListElement.Substring(0, posLpar);
                        try {
                            Type t = Type.GetType(ElementName, true, true);
                            if (t.IsSubclassOf(typeof(TypeBase))) {
                                object[] GenericCtorArgs = new object[1];
                                if (t.IsSubclassOf(typeof(TYPE<string>))) { if (ListElement == "$") GenericCtorArgs[0] = ""; else GenericCtorArgs[0] = ifc.IfcString.Decode(body); }
                                else if (t.IsSubclassOf(typeof(TYPE<int>))) { GenericCtorArgs[0] = int.Parse(body); }
                                else if (t.IsSubclassOf(typeof(TYPE<Int32>))) { GenericCtorArgs[0] = Int32.Parse(body); }
                                else if (t.IsSubclassOf(typeof(TYPE<double>))) { GenericCtorArgs[0] = double.Parse(body, CultureInfo.InvariantCulture); }
                                else {
                                    // not found
                                }
                                o = Activator.CreateInstance(t, GenericCtorArgs);
                            }
                            
                            Type genericType = GetGenericType(t);
                            if (typeof(ifcListInterface).IsAssignableFrom(genericType)) {
                                int posL = ListElement.IndexOf('(') + 1;
                                string innerBody = ListElement.Substring(posL, ListElement.Substring(posL).LastIndexOf(')'));
                                object ctorArgs = Parse2LIST(genericType, innerBody, t);
                                object selectInstance = Activator.CreateInstance(t, ctorArgs);
                                o = Activator.CreateInstance(GenericType, selectInstance);
                            }
                        }
                        catch (Exception) { }
                    }
                    FieldCtorArgs[ListPos] = o;
                }
                else if (typeof(ifcListInterface).IsAssignableFrom(GenericType)) {
                    FieldCtorArgs[ListPos] = Parse2LIST(GenericType, ListElement);
                    //Console.WriteLine("TODO List++TYPE: Base=" + GenericType.Name + " not supportet. in \r\n" + CurrentLine);
                }
                else {
                    Console.WriteLine("Base=" + GenericType.Name + " not supportet. in \r\n" + CurrentLine);
                }
            }//.....................................................
            return FieldCtorArgs;
        }
        // 2022-04-04 (ef): now supports higher order lists, as well as lists of not TypeBase-Elements (e.g.: SELECT)
        // TODO 2022-04-04 (ef) : refactor, de-spaghettify, look at use of 'GetFieldCtorArgs' vs. 'Parse2LIST'
        public static object Parse2LIST(Type FieldType, string body, Type ParentType=null) {
            if (!typeof(IEnumerable).IsAssignableFrom(FieldType)) Console.WriteLine("Parse2LIST: " + FieldType + " is not IEnumerable");
            if ((body == "$") || (body == "*")) return Activator.CreateInstance(FieldType); // warum  nicht null ?
            Type GenericType = GetGenericType(FieldType);

            object o = null;
            string[] ListElements;
            bool isListOfList = body.Contains("),");

            int posL = body.IndexOf('(')+1;
            string innerBody = body.Substring(posL, body.Substring(posL).LastIndexOf(')'));
            if (isListOfList && innerBody.StartsWith("(")) {
                o = Parse2LIST(GenericType, innerBody, FieldType);
                return o;
            }
            else if (isListOfList && innerBody.StartsWith("IFC")) {
                ListElements = innerBody.Split(new string[] { ")," }, StringSplitOptions.None);
                for (int i = 0; i < ListElements.Length-1; i++) ListElements[i] += ")";
            }
            else if (!isListOfList && body.StartsWith("IFC")) {
                ListElements = new string[] { body };
            }
            else if (isListOfList && !innerBody.StartsWith("(")) {
                ListElements = body.Split(new string[] { ")," }, StringSplitOptions.None);
                for (int i = 0; i < ListElements.Length; i++) if(!ListElements[i].EndsWith(")")) ListElements[i] += ")";
            }
            else {
                ListElements = innerBody.Split(',');
            }
            if (ListElements.Length == 0) Console.WriteLine("ListElements.Length=0");

            if (ListElements[0] == "") try { o = Activator.CreateInstance(FieldType); } catch { Console.WriteLine("ERROR on Parse2LIST.1:" + CurrentLine); }
            else {
                try {
                    object[] ctorArgs = new object[ListElements.Length];
                    if (isListOfList) {
                        if (ParentType == null) {
                            Type GenericBaseType = FieldType.BaseType.GetGenericArguments()[0];
                            ctorArgs = GetFieldCtorArgs(GenericBaseType, ListElements);
                            o = Activator.CreateInstance(FieldType, ctorArgs);
                        }
                        else {
                            ctorArgs = GetFieldCtorArgs(FieldType, ListElements);
                            o = Activator.CreateInstance(ParentType, ctorArgs);
                        }
                    }
                    else {
                        ctorArgs = GetFieldCtorArgs(GenericType, ListElements, ParentType);
                        o = Activator.CreateInstance(FieldType, ctorArgs);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine($"ERROR on Parse2LIST.2:{CurrentLine}\n{e}");
                }
            }
            return o;
        } //++++++++++++++++++++++++++++++++++++++++++++++++++++


        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //public static string[] BaseTypes=new string[]{"INTEGER","BINARY","LOGICAL","REAL","BOOLEAN"};

        public static object ParseSelect(string Element, object o) {
            //Console.WriteLine("Parse SELECT: "+Element);
            string ElementName = Element.Replace("IFC", "");


            int posLpar = ElementName.IndexOf('(');
            int posRpar = ElementName.LastIndexOf(')');
            string body = ElementName.Substring(posLpar + 1, posRpar - posLpar - 1); // Argumenkörper extrahieren
            ElementName = ElementName.Substring(0, posLpar);


            bool ignoreCase = true;
            //foreach (string s in BaseTypes ) if (s==ElementName) {ElementName=ElementName.Substring(0,1)+ElementName.Substring(1).ToLower();ignoreCase=false;}
            ElementName = "ifc." + ElementName;

            try {
                Type t = Type.GetType(typeName: ElementName, throwOnError: true, ignoreCase: ignoreCase); // Console.WriteLine("A");

                if (t.IsSubclassOf(typeof(TypeBase))) {
                    object[] TypeCtorArgs = new object[1];// Console.WriteLine("B");
                    Type SelectType = o.GetType(); // Console.WriteLine("C");
                    TypeCtorArgs[0] = Parse2TYPE(body, t); // Console.WriteLine("D");
                                                           //Console.WriteLine(Element);
                                                           //Console.WriteLine(ElementName+": "+body+ " o: "+o.GetType().Name+" [0]= "+TypeCtorArgs[0].GetType().Name);
                    if (TypeCtorArgs[0] == null) Console.WriteLine("SELECT-type is null" + "\r\n" + CurrentLine);
                    else try { o = Activator.CreateInstance(SelectType, TypeCtorArgs); } catch (Exception e) { Console.WriteLine(e.Message + "\r\n" + CurrentLine); }
                }
            }
            catch (Exception e) { Console.WriteLine(ElementName + " body=" + body + " ERROR SELECT: " + posLpar + " " + e.Message + "\r\n" + CurrentLine); }// 2. true: ignoreCase
            return o;
        }



        public static string CurrentLine = "";
        public static string CurrentEntityComment = "";
        // 2022-04-02 (ef): added CurrentElementName, CurrentArgs and CurrentEntity as a static property
        //                  this helps with debugging and readability
        public static string CurrentElementName = "";
        public static string[] CurrentArgs;
        public static object CurrentEntity = null;
        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // 2022-04-02 (ef): formatting 
        public static void ParseIfcLine(Model CurrentModel, string line) {
            //Console.WriteLine("parse="+line);
            CurrentLine = line;
            int CommentOpenPos = line.IndexOf("/*"); if (CommentOpenPos >= 0) {
                CurrentEntityComment = line.Substring(CommentOpenPos + 2).Replace("*/", ""); line = line.Substring(0, CommentOpenPos);
                //Console.WriteLine("CommentOpenPos="+CommentOpenPos+" "+CurrentEntityComment);  
            }
            // if (EndOfLineComment!=null) s+="/* "+EndOfLineComment+" */";
            if (CommentOpenPos != 0) {//====================================================================================================

                int posA = line.IndexOf('=');
                int posLpar = line.IndexOf('(');
                int posRpar = line.LastIndexOf(')');
                CurrentElementName = line.Substring(posA + 1, posLpar - posA - 1).TrimStart(' ').Substring(3);
                string body = line.Substring(posLpar + 1, posRpar - posLpar - 1); // Argumentkörper extrahieren
                bool TxtOpen = false;
                int ParOpen = 0;
                for (int i = 0; i < body.Length; i++) {
                    if ((!TxtOpen) && (body[i] == '\'')) { TxtOpen = true; }//body=ReplaceCharAt(body,i,'[');}
                    else if ((!TxtOpen) && (body[i] == '(')) { ParOpen++; }//body=ReplaceCharAt(body,i,'<');}
                    else if ((!TxtOpen) && (body[i] == ')')) { ParOpen--; }//body=ReplaceCharAt(body,i,'>');}
                    else if ((TxtOpen) && (body[i] == '\'')) { TxtOpen = false; }//body=ReplaceCharAt(body,i,']');}
                    if ((TxtOpen) || (ParOpen > 0)) if (body[i] == ',') { body = ReplaceCharAt(body, i, (char)9); }
                }
                CurrentArgs = body.Split(',');
                for (int i = 0; i < CurrentArgs.Length; i++) CurrentArgs[i] = CurrentArgs[i].Replace((char)9, ',');

                try {
                    Type t = Type.GetType("ifc." + CurrentElementName, true, true);// 2. true: ignoreCase
                    CurrentEntity = Activator.CreateInstance(t); if (CommentOpenPos > 0) ((ENTITY)CurrentEntity).EndOfLineComment = CurrentEntityComment;
                    ((ENTITY)CurrentEntity).LocalId = int.Parse(line.Substring(1, posA - 1));

                    Dictionary<int, FieldInfo> orderedAttributeDictionary = GetOrderedAttributeDictionaryForCurrentEntity();
                    for (int i = 1; i <= orderedAttributeDictionary.Count; i++) {
                        FieldInfo field = orderedAttributeDictionary[i];
                        string value = "$";
                        if (i <= CurrentArgs.GetLength(0)) value = CurrentArgs[i - 1].Trim(' ').Trim('\'');
                        if (field.FieldType == typeof(String)) { if (value == "$") field.SetValue(CurrentEntity, ""/*null*/); else field.SetValue(CurrentEntity, ifc.IfcString.Decode(value)); }
                        else if (field.FieldType == typeof(int)) { if (value == "$") field.SetValue(CurrentEntity, 0); else field.SetValue(CurrentEntity, int.Parse(value)); }
                        else if (field.FieldType.IsSubclassOf(typeof(TypeBase))) {
                            try { field.SetValue(CurrentEntity, Parse2TYPE(value, field.FieldType)); } catch (Exception e) { throw new Exception("Parse.Type: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message); }
                        } //tb.GetBaseType()
                        else if (field.FieldType.IsSubclassOf(typeof(SELECT))) {
                            //Console.WriteLine("begin SELECT: "+line);
                            try {
                                object o = Activator.CreateInstance(field.FieldType);
                                if ((value.Length > 0) && (value[0] == '$')) { }
                                else if ((value.Length > 0) && (value[0] == '*')) { }
                                else if ((value.Length > 0) && (value[0] == '#')) { ((SELECT)o).Id = int.Parse(value.Substring(1)); }
                                else o = ParseSelect(value, o);
                                field.SetValue(CurrentEntity, o);
                            }
                            catch (Exception e) { throw new Exception("xx Parse.SELECT: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message); }
                        }
                        else if (field.FieldType.IsSubclassOf(typeof(ENTITY))) {
                            try {
                                object o = null; //falls $
                                if (value.Length > 0) if (value[0] == '*') o = Activator.CreateInstance(field.FieldType);
                                if (value.Length > 0) if (value[0] == '#') { o = Activator.CreateInstance(field.FieldType); ((ENTITY)o).LocalId = int.Parse(value.Substring(1)); }
                                field.SetValue(CurrentEntity, o);
                            }
                            catch (Exception e) { throw new Exception("Parse.ENTITY: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message); }
                        }
                        else if (field.FieldType.IsSubclassOf(typeof(Enum))) {
                            try {
                                object FieldInstance = Activator.CreateInstance(field.FieldType);
                                if ((value.Length > 0) && (value[0] == '$')) FieldInstance = 0;
                                else try { FieldInstance = Enum.Parse(field.FieldType, value.Substring(1, value.Length - 2)); } catch { Console.WriteLine("enum " + field.FieldType + "." + value + " not recognized"); }
                            }
                            catch (Exception e) { throw new Exception("Parse.Enum: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message); }
                        }
                        else if ((Nullable.GetUnderlyingType(field.FieldType) != null) && (Nullable.GetUnderlyingType(field.FieldType).IsSubclassOf(typeof(Enum)))) {
                            try {
                                object FieldInstance = null;
                                if ((value.Length > 0) && (value[0] != '$')) {
                                    FieldInstance = Activator.CreateInstance(field.FieldType);
                                    try { FieldInstance = Enum.Parse(Nullable.GetUnderlyingType(field.FieldType), value.Substring(1, value.Length - 2)); } catch { Console.WriteLine("enum " + field.FieldType + "." + value + " not recognized"); }
                                }
                                field.SetValue(CurrentEntity, FieldInstance);
                            }
                            catch (Exception e) { throw new Exception("Parse.Enum: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message); }
                        }
                        //else if( typeof(IEnumerable).IsAssignableFrom(field.FieldType)){
                        else if (typeof(ifcListInterface).IsAssignableFrom(field.FieldType)) {
                            try {
                                if (value == "$") field.SetValue(CurrentEntity, null);
                                else field.SetValue(CurrentEntity, Parse2LIST(field.FieldType, value));
                            }
                            catch (Exception e) {
                                Console.WriteLine("Parse.LIST:" + line);
                                throw new Exception("Parse.LIST: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message);
                            }
                        }
                        else Console.WriteLine(i + ": is " + field.FieldType.Name + "???????x? " + field.FieldType.Name + "  " + line); //ifc.WallStandardCase
                    }

                    CurrentModel.EntityList.Add((ENTITY)CurrentEntity); //Console.WriteLine();Console.WriteLine("AAA "+((ENTITY)CurrentEntity).ToString());
                }
                catch (Exception e) { Console.WriteLine("ERROR on ParseIfcLine:" + e.Message); Console.WriteLine(line); }//Console.ReadLine();}
            }//====================================================================================================
            else { EntityComment ec = new EntityComment(); ec.CommentLine = CurrentEntityComment; ec.LocalId = NextGlobalCommentId--; CurrentModel.EntityList.Add(ec); }
        }

        // 2022-04-02 (ef): new method 'GetOrderedAttributeDictionaryForCurrentEntity'
        private static Dictionary<int, FieldInfo> GetOrderedAttributeDictionaryForCurrentEntity() {
            Dictionary<int, FieldInfo> orderedAttributeDictionary = new Dictionary<int, FieldInfo>();
            foreach (FieldInfo field in CurrentEntity.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)) {
                foreach (Attribute attr in field.GetCustomAttributes(true)) if (attr is ifcAttribute) {
                        orderedAttributeDictionary.Add(((ifcAttribute)attr).OrdinalPosition, field);
                    }
            }
            return orderedAttributeDictionary;
        }
    }//ENTITY=====================================================================================================================


    public partial class Model
    {//==========================================================================================

        // 2022-04-02 (ef): new method 'ParseFileHeader'
        private static Dictionary<string, string> ParseFileHeader(string filePath) {
            StreamReader sr = new StreamReader(filePath);
            Dictionary<string, string> headerInfo = new Dictionary<string, string>();
            string line;
            string currentKey = string.Empty;
            string values = "";
            while ((line = sr.ReadLine()) != null && (line.Length > 1) && ((line + ' ')[0] != '#') && (!line.StartsWith("DATA"))) {
                if (line.StartsWith("FILE_DESCRIPTION")) {
                    currentKey = "FILE_DESCRIPTION";
                }
                if (line.StartsWith("FILE_NAME")) {
                    currentKey = "FILE_NAME";
                }
                if (line.StartsWith("FILE_SCHEMA")) {
                    currentKey = "FILE_SCHEMA";
                }

                if (!string.IsNullOrEmpty(currentKey) && !headerInfo.ContainsKey(currentKey)) {
                    headerInfo.Add(currentKey, values);
                    values = "";
                }

                if (headerInfo.ContainsKey(currentKey)) {
                    headerInfo[currentKey] += line;
                }
            };
            return headerInfo;
        }
        public static Model FromStepFile(string FileName) {
            //ifc.Repository.CurrentModel.
            string FileSchema = "-";
            Model CurrentModel = new ifc.Model(FileName.Replace(".ifc", ""));
            Dictionary<string, string> headerInfo = ParseFileHeader(FileName);
            if (headerInfo.ContainsKey("FILE_DESCRIPTION")) CurrentModel.Header.description = headerInfo["FILE_DESCRIPTION"].Split('\'')[1 * 2 - 1];
            if (headerInfo.ContainsKey("FILE_NAME")) {
                string[] HeaderLine = headerInfo["FILE_NAME"].Split('\'');
                CurrentModel.Header.name = HeaderLine[1 * 2 - 1];
                CurrentModel.Header.time_stamp = HeaderLine[2 * 2 - 1];
                CurrentModel.Header.author = HeaderLine[3 * 2 - 1];
                CurrentModel.Header.organization = HeaderLine[4 * 2 - 1];
                CurrentModel.Header.preprocessor_version = HeaderLine[5 * 2 - 1];
                CurrentModel.Header.originating_system = HeaderLine[6 * 2 - 1];
                CurrentModel.Header.authorization = HeaderLine[7 * 2 - 1];
            }
            if (headerInfo.ContainsKey("FILE_SCHEMA")) FileSchema = headerInfo["FILE_SCHEMA"].Split('\'')[1 * 2 - 1];
            if (FileSchema != Specification.SchemaName) Console.WriteLine("WARNING! Expected schema is '" + Specification.SchemaName + "', detected schema is '" + FileSchema + "'");

            StreamReader sr = new StreamReader(FileName);
            string line = "";
            while ((line = sr.ReadLine()) != null) if (line.Length > 3) {
                    line = line.TrimStart(' '); //Console.WriteLine(line);
                    if (line.Length > 3) if (line[0] == '#' || (line[0] == '/' && line[1] == '*')) ENTITY.ParseIfcLine(CurrentModel, line);
                }
            sr.Close();
            CurrentModel.AssignEntities();
            return CurrentModel;
        }// of FromStepFile
    }// of Model ==========================================================================================================

}// ifc==============================




