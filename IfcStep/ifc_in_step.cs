// ifc_in_step.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

// TODO (ef): refactor and cleanup
//            every 'Console.WriteLine' needs to be logged

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Linq;

namespace ifc {


    public partial class ENTITY {

        public static string ReplaceCharAt(string s, int i, char c) { char[] array = s.ToCharArray(); array[i] = c; s = new string(array); return s; }

        /// <summary>
        /// get the type of a the value of given <param name="FieldType"></param>.
        /// SELECT and LIST-types sometimes contain more complex objects.
        /// This helper-method retrieves the type of such objects, so it can be instatiated.
        /// </summary>
        /// <param name="FieldType"></param>
        /// <returns>type of generic constructor arguments</returns>
        public static Type GetValueType(Type fieldType) {
            if (fieldType.BaseType.GetGenericArguments().Length > 0) return fieldType.BaseType.GetGenericArguments()[0]; //i.e. LengthMeasure or CartesianPoint
            else return fieldType.BaseType.BaseType.GetGenericArguments()[0]; //i.e. CompoundPlaneAngleMeasure
        }

        /// <summary>
        /// parses given <param name="value"></param> and tries to instantiate an object of given <param name="valueType"></param>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <returns>null on error; instance of <param name="valueType"></returns>
        public static object Parse2TYPE(string value, Type valueType) {
            Type valueBaseType = GetValueType(valueType);//valueType.BaseType.GetGenericArguments()[0];
            object instance = null;
            object[] ctorArgs = new object[1];
            
            //if ((value == "$") || (value == "*")) instance = Activator.CreateInstance(valueType);
            if (value == "$") return null;
            else if (value == "*") instance = Activator.CreateInstance(valueType);
            else {
                if (valueBaseType == typeof(string)) { if (value == "$") ctorArgs[0] = ""; else ctorArgs[0] = ifc.IfcString.Decode(value); instance = Activator.CreateInstance(valueType, ctorArgs); }
                else if (valueBaseType == typeof(int)) { ctorArgs[0] = int.Parse(value); instance = Activator.CreateInstance(valueType, ctorArgs); }
                else if (valueBaseType == typeof(double)) try { ctorArgs[0] = double.Parse(value, CultureInfo.InvariantCulture); instance = Activator.CreateInstance(valueType, ctorArgs); } catch { Console.WriteLine("Error on Parse2TYPE: " + CurrentLine); }
                else if (valueBaseType == typeof(bool)) { ctorArgs[0] = (value == ".T."); instance = Activator.CreateInstance(valueType, ctorArgs); }
                else if (valueBaseType.IsSubclassOf(typeof(TypeBase))) { instance = Activator.CreateInstance(valueType, Parse2TYPE(value, valueBaseType)); }

                // 2022-06-10 (ef): for lists which are defined inline (i.e.: (49,6,1,566000))
                //                  we need to also check if the baseclass of the given 'FieldType' can be represented as a subclass of LIST.
                //                  if so, the given args are parsed to the type of LIST and the the FieldType instance is created.
                else if (typeof(ifcListInterface).IsAssignableFrom(valueBaseType)) {
                    try {
                        ctorArgs[0] = Parse2LIST(valueBaseType, value);
                        instance = Activator.CreateInstance(valueType, ctorArgs[0]);
                    }
                    catch (Exception e) {
                        // TODO (ef): implement logging
                        Console.WriteLine("Parse2TYPE (parsing list):" + value + "\nException: " + e.Message);
                    }
                }
                else {
                    // TODO (ef): implement logging
                    Console.WriteLine("UNKNOWN TYPE for expected Type " + valueType.Name + ": Base=" + valueBaseType.Name + " value=" + value + "\r\n" + CurrentLine);
                }
            } 
            return instance;
        }


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

        // 




        // 2022-04-04 (ef): now supports higher order lists, as well as lists of not TypeBase-Elements (e.g.: SELECT)
        // TODO 2022-04-04 (ef) : refactor, de-spaghettify, look at use of 'GetFieldCtorArgs' vs. 'Parse2LIST'
        public static object Parse2LIST(Type listType, string body, Type parentType = null) {
            if (!typeof(IEnumerable).IsAssignableFrom(listType)) Console.WriteLine("Parse2LIST: " + listType + " is not IEnumerable");
            if ((body == "$") || (body == "*")) return Activator.CreateInstance(listType);
            Type valueType = GetValueType(listType);

            object instance = null;
            string[] values;
            bool isListOfList = body.Contains("),");

            int posL = body.IndexOf('(') + 1;
            string innerBody = body.Substring(posL, body.Substring(posL).LastIndexOf(')'));
            if (isListOfList && innerBody.StartsWith("(")) {
                instance = Parse2LIST(valueType, innerBody, listType);
                return instance;
            }
            else if (isListOfList && innerBody.StartsWith("IFC")) {
                values = innerBody.Split(new string[] { ")," }, StringSplitOptions.None);
                for (int i = 0; i < values.Length - 1; i++) values[i] += ")";
            }
            else if (!isListOfList && body.StartsWith("IFC")) {
                values = new string[] { body };
            }
            else if (isListOfList && !innerBody.StartsWith("(")) {
                values = body.Split(new string[] { ")," }, StringSplitOptions.None);
                for (int i = 0; i < values.Length; i++) if (!values[i].EndsWith(")")) values[i] += ")";
            }
            else {
                values = innerBody.Split(',');
            }
            if (values.Length == 0) Console.WriteLine("ListElements.Length=0");

            if (values[0] == "") try { instance = Activator.CreateInstance(listType); } catch { Console.WriteLine("ERROR on Parse2LIST.1:" + CurrentLine); }
            else {
                try {
                    object[] ctorArgs = new object[values.Length];
                    if (isListOfList && parentType != null) {
                        //Type GenericBaseType = fieldType.BaseType.GetGenericArguments()[0];
                        //ctorArgs = GetListCtorArgs(valueType, values);
                        //instance = Activator.CreateInstance(listType, ctorArgs);
                        ctorArgs = GetListCtorArgs(listType, values);
                        instance = Activator.CreateInstance(parentType, ctorArgs);
                    }
                    else {
                        ctorArgs = GetListCtorArgs(valueType, values);
                        instance = Activator.CreateInstance(listType, ctorArgs);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine($"ERROR on Parse2LIST.2:{CurrentLine}\n{e}");
                }
            }
            return instance;
        }


        // 2022-04-04 (ef): now supports lists of list
        // TODO 2022-04-04 (ef) : refactor, de-spaghettify, look at use of 'GetFieldCtorArgs' vs. 'Parse2LIST'

        // only used in Parse2LIST
        // itself uses Parse2LIST
        public static object[] GetListCtorArgs(Type valueType, string[] values) {
            object[] valueInstances = new object[values.Length];

            for (int i = 0; i < values.Length; i++) {
                string value = values[i];
                if (valueType == typeof(Int32)) {
                    valueInstances[i] = Int32.Parse(value);
                }
                else if (valueType == typeof(double)) {
                    valueInstances[i] = double.Parse(value, CultureInfo.InvariantCulture);
                }
                else if (valueType.IsSubclassOf(typeof(TypeBase))) {
                    valueInstances[i] = Parse2TYPE(value, valueType);
                }
                else if (valueType.IsSubclassOf(typeof(ENTITY))) {
                    object o = Activator.CreateInstance(valueType);
                    ((ENTITY)o).LocalId = int.Parse(value.Trim(' ').Substring(1));
                    valueInstances[i] = o;
                }
                else if (valueType.IsSubclassOf(typeof(SELECT))) {
                    object o = Activator.CreateInstance(valueType);
                    if ((value.Length > 0) && value[0] == '#') {
                        // if SELECT refers to another line, we only need the Id
                        ((SELECT)o).Id = int.Parse(value.Trim(' ').Substring(1)); 
                    }
                    else {
                        // otherwise we parse the entire SELECT
                        o = ParseSelect(value, valueType);
                    }
                    valueInstances[i] = o;
                }
                else if (typeof(ifcListInterface).IsAssignableFrom(valueType)) {
                    valueInstances[i] = Parse2LIST(valueType, value);
                }
                else {
                    Console.WriteLine("Base=" + valueType.Name + " not supportet. in \r\n" + CurrentLine);
                }
            }
            return valueInstances;
        }
        

        
        public static string[] BaseTypeNames = new string[] { "INTEGER", "BINARY", "LOGICAL", "REAL", "BOOLEAN" };

        public static object ParseSelect(string value, Type selectType) {
            object instance = null;
            string selectedTypeName = value.Replace("IFC", "");


            int posLpar = selectedTypeName.IndexOf('(');
            int posRpar = selectedTypeName.LastIndexOf(')');
            string body = selectedTypeName.Substring(posLpar + 1, posRpar - posLpar - 1); // get body of STEP expression
            selectedTypeName = selectedTypeName.Substring(0, posLpar);

            bool ignoreCase = true;
            // 2022-06-10 (ef): there is currently some ambivalence with the naming scheme which is case-senstive.
            //                  BASETYPES (which are representing the types availble in the database) are capitalized,
            //                  whereas the actual IFC-Types are not. This can lead to confusion when parsing a STEP-file.
            //                  Therefore, we need to check if the current 'ElementName' is one of the BaseTypes, if so we change the name to first letter captitalized and remaining lower.
            foreach (string name in BaseTypeNames) if (name == selectedTypeName) { selectedTypeName = selectedTypeName.Substring(0, 1) + selectedTypeName.Substring(1).ToLower(); ignoreCase = false; }
            selectedTypeName = "ifc." + selectedTypeName;

            try {
                Type selectValueType = Type.GetType(typeName: selectedTypeName, throwOnError: true, ignoreCase: ignoreCase);

                if (selectValueType.IsSubclassOf(typeof(TypeBase))) {
                    object[] typeCtorArgs = new object[1];
                    typeCtorArgs[0] = Parse2TYPE(body, selectValueType);
                    if (typeCtorArgs[0] == null) Console.WriteLine("SELECT-type is null" + "\r\n" + CurrentLine);
                    else {
                        try {
                            instance = Activator.CreateInstance(selectType, typeCtorArgs);
                        }
                        catch (Exception e) {
                            Console.WriteLine(e.Message + "\r\n" + CurrentLine);
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(selectedTypeName + " body=" + body + " ERROR SELECT: " + posLpar + " " + e.Message + "\r\n" + CurrentLine); }
            return instance;
        }



        public static string CurrentLine = "";
        public static string CurrentEntityComment = "";
        // 2022-04-02 (ef): added CurrentElementName, CurrentArgs and CurrentEntity as a static property
        //                  this helps with debugging and readability
        public static string CurrentElementName = "";
        public static string[] CurrentArgs;
        public static object CurrentEntity = null;

        // 2022-04-02 (ef): formatting 
        public static void ParseIfcLine(Model CurrentModel, string line) {
            //Console.WriteLine("parse="+line);
            CurrentLine = line;
            int CommentOpenPos = line.IndexOf("/*"); if (CommentOpenPos >= 0) {
                CurrentEntityComment = line.Substring(CommentOpenPos + 2).Replace("*/", ""); line = line.Substring(0, CommentOpenPos);
                //Console.WriteLine("CommentOpenPos="+CommentOpenPos+" "+CurrentEntityComment);  
            }
            // if (EndOfLineComment!=null) s+="/* "+EndOfLineComment+" */";
            if (CommentOpenPos != 0) {

                int posA = line.IndexOf('=');
                int posLpar = line.IndexOf('(');
                int posRpar = line.LastIndexOf(')');
                CurrentElementName = line.Substring(posA + 1, posLpar - posA - 1).TrimStart(' ').Substring(3);
                string body = line.Substring(posLpar + 1, posRpar - posLpar - 1); // Argumentk�rper extrahieren
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
                                else o = ParseSelect(value, field.FieldType);
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
                                else{ 
                                    try {
                                        FieldInstance = Enum.Parse(field.FieldType, value.Substring(1, value.Length - 2));
                                        field.SetValue(CurrentEntity, FieldInstance);
                                    }
                                    catch {
                                        Console.WriteLine("enum " + field.FieldType + "." + value + " not recognized");
                                    }
                                }
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
            }
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
            return orderedAttributeDictionary;//.OrderBy(a => a.Key).ToDictionary(k=>k.Key, v=>v.Value);
        }
    }


    public partial class Model {

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
        }
    }

}




