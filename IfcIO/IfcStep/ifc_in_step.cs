// ifc_in_step.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

// TODO (ef): refactor and cleanup
// TODO (ef): every 'Console.WriteLine' needs to be logged
// TODO (ef): make tests for each IFC Version
// TODO (ef): write build version of IfcSharp to IFC file

using System;
using NetSystem = System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ifc {


    public partial class ENTITY {

        public static string ReplaceCharAt(string s, int i, char c) { char[] array = s.ToCharArray(); array[i] = c; s = new string(array); return s; }

        /// <summary>
        /// get the type of a the value of given <paramref name="fieldType"/>.
        /// SELECT and LIST-types sometimes contain more complex objects.
        /// This helper-method retrieves the type of such objects, so it can be instatiated.
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns>type of generic constructor arguments</returns>
        public static Type GetValueType(Type fieldType) {
            if (fieldType.BaseType.GetGenericArguments().Length > 0) return fieldType.BaseType.GetGenericArguments()[0]; //i.e. LengthMeasure or CartesianPoint
            return fieldType.BaseType.BaseType.GetGenericArguments()[0]; //i.e. CompoundPlaneAngleMeasure
        }

        /// <summary>parses given <param name="value">value</param> and tries to instantiate an object of given <param name="valueType">valueType</param></summary>
        /// <returns>null on error; instance of <param>valueType</param> on success</returns>

        public static object Parse2TYPE(string value, Type valueType) { //TODO: catch more specific exceptions and throw custom IfcSharpExceptions  i.e.: infer argument mismatches and the like

            if (value == "$") return null;
            if (value == "*") return Activator.CreateInstance(valueType);

            Type valueBaseType = GetValueType(valueType); //valueType.BaseType.GetGenericArguments()[0];
            object instance = null;
            object[] ctorArgs = new object[1];

            if (valueBaseType == typeof(string)) {ctorArgs[0]=(value == "$")?"":ifc.IfcString.Decode(value.Replace("'","")); // 2024-04-01 (bb) remove string-characters
                                                  instance = Activator.CreateInstance(valueType, ctorArgs);
                                                 }
       else if (valueBaseType == typeof(int)   ) {ctorArgs[0] = int.Parse(value);
                                                  instance = Activator.CreateInstance(valueType, ctorArgs);
                                                 }
       else if (valueBaseType == typeof(double)) {try {ctorArgs[0] = double.Parse(value, CultureInfo.InvariantCulture);}catch (Exception e){Log.Add("Error on Parse2TYPE: valueBaseType="+valueBaseType+" valueType="+valueType+" value="+value+"\n" + CurrentLine+"\n"+e.Message, Log.Level.Exception);}
                                                  instance = Activator.CreateInstance(valueType, ctorArgs);
                                                 }
       else if (valueBaseType == typeof(bool))   {ctorArgs[0] = (value == ".T.");
                                                  instance = Activator.CreateInstance(valueType, ctorArgs);
                                                 }
       else if (valueBaseType.IsSubclassOf(typeof(TypeBase))) instance = Activator.CreateInstance(valueType, Parse2TYPE(value, valueBaseType)); // 2022-06-10 (ef): for lists which are defined inline (i.e.: (49,6,1,566000)) we need to also check if the baseclass of the given 'FieldType' can be represented as a subclass of LIST. If so, the given args are parsed to the type of LIST and the the FieldType instance is created.
       else if (typeof(ifcListInterface).IsAssignableFrom(valueBaseType)) 
                                                 {value=value.Trim('(').Trim(')');
                                                  try {ctorArgs[0] = Parse2LIST(valueBaseType, value);} catch (Exception e) {Log.Add($"Parse2TYPE (parsing list): {value}\nException: {e.Message}", Log.Level.Exception);}
                                                  instance = Activator.CreateInstance(valueType, ctorArgs[0]);
                                                 }
       else    {Log.Add($"UNKNOWN TYPE for expected Type {valueType.Name}: Base={valueBaseType.Name} value={value}\r\n{CurrentLine}", Log.Level.Error);}

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

            if (!typeof(IEnumerable).IsAssignableFrom(listType)) Log.Add($"ERROR on Parse2LIST: " + listType + " is not IEnumerable\n{CurrentLine}\n{e}", Log.Level.Exception);
            if ((body == "$") || (body == "*")) return Activator.CreateInstance(listType);

            Type valueType = GetValueType(listType);
            object instance = null; 
            string[] values;

            body=ExtractInnerCommas(body);
            values = body.Split(',');
            for (int i = 0; i < values.Length; i++) values[i] = values[i].Replace((char)9, ',');

            if (values.Length == 0) Log.Add($"ERROR on Parse2LIST ListElements.Length=0:{CurrentLine}", Log.Level.Error);

            if (values[0] == "" || values[0] == "$") try { instance = Activator.CreateInstance(listType); } catch(Exception e) { Log.Add($"ERROR on Parse2LIST.3:{CurrentLine}\n{e}", Log.Level.Exception);}
            else {try {
                       for (int i=0;i<values.Length;i++) values[i]=values[i].Trim(' ').Replace("'",""); // 2024-03-30 (bb) remove string-characters, 
                       for (int i=0;i<values.Length;i++) if (!values[i].StartsWith("IFC")) values[i]=values[i].Replace("(","").Replace(")",""); //2024-05-10 (bb) remove ) and (, 2024-05-20 (bb) only if not SELECT
                       instance = Activator.CreateInstance(listType, GetListCtorArgs(valueType, values));
                      }
                  catch (Exception e) {Log.Add($"ERROR on Parse2LIST.2:{CurrentLine}\nlistType={listType}\nvalueType={valueType}\n{e}", Log.Level.Exception);}
            }
            return instance;
        }


        // 2022-04-04 (ef): now supports lists of list
        // TODO 2022-04-04 (ef) : refactor, de-spaghettify, look at use of 'GetFieldCtorArgs' vs. 'Parse2LIST'
        private static object[] GetListCtorArgs(Type valueType, string[] values) {
            object[] valueInstances = new object[values.Length];
            for (int i = 0; i < values.Length; i++) {
                string value = values[i];
                if (valueType ==           typeof(int))       { valueInstances[i] = int.Parse(value);}
           else if (valueType ==           typeof(double))    { valueInstances[i] = double.Parse(value, CultureInfo.InvariantCulture); }
           else if (valueType.IsSubclassOf(typeof(TypeBase))) { valueInstances[i] = Parse2TYPE(value, valueType); }
           else if (valueType.IsSubclassOf(typeof(ENTITY)))   { object o = Activator.CreateInstance(valueType);
                                                                ((ENTITY)o).LocalId = int.Parse(value.Trim(' ').Substring(1));
                                                                valueInstances[i] = o;
                                                              }
           else if (valueType.IsSubclassOf(typeof(SELECT)))   {object o = Activator.CreateInstance(valueType);
                                                               if   (value.Length > 0 && value.Trim(' ')[0] == '#') {((SELECT)o).Id = int.Parse(value.Trim(' ').Substring(1));}  // if SELECT refers to another line, we only need the Id
                                                               else {o = ParseSELECT(value, valueType); } // otherwise we parse the entire SELECT
                                                               valueInstances[i] = o;
                                                              }
           else if (typeof(ifcListInterface).IsAssignableFrom(valueType)) {valueInstances[i] = Parse2LIST(valueType, value);}
           else    {Log.Add("Base=" + valueType.Name + " not supported. in \r\n" + CurrentLine, Log.Level.Error);}
            }
            return valueInstances;
        }
        

        
        private static readonly string[] BaseTypeNames = new string[] { "INTEGER", "BINARY", "LOGICAL", "REAL", "BOOLEAN" };

        public static object ParseSELECT(string value, Type selectType) {
            object instance = null;
            string selectedTypeName = value.Replace("IFC", "");

            int posLpar = selectedTypeName.IndexOf('(');
            int posRpar = selectedTypeName.LastIndexOf(')');
            string body = selectedTypeName.Substring(posLpar + 1, posRpar - posLpar - 1); // get body of STEP expression
            selectedTypeName = selectedTypeName.Substring(0, posLpar);

            bool ignoreCase = true;
            // 2022-06-10 (ef): there is currently some ambivalence with the naming scheme which is case-senstive. BASETYPES (which are representing the types available in the database) are capitalized,
            //                  whereas the actual IFC-Types are not. This can lead to confusion when parsing a STEP-file. Therefore, we need to check if the current 'ElementName' is one of the BaseTypes, if so we change the name to first letter captitalized and remaining lower.
            foreach (string name in BaseTypeNames) if (name == selectedTypeName) { selectedTypeName = selectedTypeName.Substring(0, 1) + selectedTypeName.Substring(1).ToLower(); ignoreCase = false; }
            selectedTypeName = "ifc." + selectedTypeName.Trim(' '); // bb 2024-05-05 added trim

            try {Type selectValueType = Type.GetType(typeName: selectedTypeName, throwOnError: true, ignoreCase: ignoreCase);
                 if (selectValueType.IsSubclassOf(typeof(TypeBase))) {
                    object[] typeCtorArgs = new object[1];
                    typeCtorArgs[0] = Parse2TYPE(body, selectValueType);
                    if   (typeCtorArgs[0] == null) Log.Add("SELECT-type is null\r\n" + CurrentLine, Log.Level.Exception);
                    else try {instance = Activator.CreateInstance(selectType, typeCtorArgs);} catch (Exception e) {Log.Add(e.Message + "\r\n" + CurrentLine, Log.Level.Exception);}  
                 }
            }   catch (Exception e) { Log.Add(selectedTypeName + " body=" + body + " ERROR SELECT: " + posLpar + " " + e.Message + "\r\n" + CurrentLine, Log.Level.Exception); }
            return instance;
        }
        
        private static string CurrentLine= "";
        private static string CurrentEntityComment= "";
        private static string CurrentTypeName  = ""; // 2022-04-02 (ef): added CurrentElementName, CurrentArgs and CurrentEntity as a static property, this helps with debugging and readability
        private static string[] CurrentArgs;
        private static string[] ListElements;
        private static object CurrentInstance=null;

private static string ExtractInnerCommas(string body) // 2024-05-12 (bb): from inline to function
{                bool txtOpen = false;
                int parOpen = 0;

                for (int i = 0; i < body.Length; i++) {
                         if ((!txtOpen) && (body[i] =='\''))  txtOpen = true; 
                    else if ((!txtOpen) && (body[i] == '('))  parOpen++; 
                    else if ((!txtOpen) && (body[i] == ')'))  parOpen--; 
                    else if (( txtOpen) && (body[i] == '\'')) txtOpen = false; 
                    if ((txtOpen) || (parOpen > 0)) if (body[i] == ',') { body = ReplaceCharAt(body, i, (char)9); } // replace inner comma with tab
                }
return body;
}
private static void RefillInnerCommasOfCurrentArgs () {for (int i = 0; i < CurrentArgs .Length; i++) CurrentArgs [i] = CurrentArgs [i].Replace((char)9, ',');} // 2024-05-12 (bb): put comma back from tab
private static void RefillInnerCommasOfListElements() {for (int i = 0; i < ListElements.Length; i++) ListElements[i] = ListElements[i].Replace((char)9, ',');} // 2024-05-12 (bb): put comma back from tab
 

        /// <summary> Parses the given <paramref name="ifcLine"/> and tries to add the resulting <code>ENTITY</code> to the given <paramref name="targetModel"/> </summary> <exception cref="IfcSharpException"></exception>
        public static void ParseIfcLine(Model targetModel, string ifcLine) {
            CurrentLine = ifcLine;
            int commentOpenPos = ifcLine.IndexOf("/*"); 
            int commentClosePos = ifcLine.IndexOf("*/"); 
            bool InnerComment= (commentClosePos >= 0) && (commentClosePos+2 < ifcLine.Length);// 2024-05-20 (bb) 
            if (InnerComment) ifcLine=ifcLine.Substring(0, commentOpenPos)+ifcLine.Substring(commentClosePos+2); // 2024-05-20 (bb) remove inner comment, ToDo extract muliple inner comments and outer comments in addition
            else if (commentOpenPos >= 0) { CurrentEntityComment = ifcLine.Substring(commentOpenPos + 3).Replace(" */", ""); ifcLine = ifcLine.Substring(0, commentOpenPos);}

            if (commentOpenPos != 0) { // no comment (-1) or comment after ifc line
                int posA = ifcLine.IndexOf('=');
                int posLpar = ifcLine.IndexOf('(');
                int posRpar = ifcLine.LastIndexOf(')');
                CurrentTypeName = ifcLine.Substring(posA + 1, posLpar - posA - 1).TrimStart(' ').TrimEnd(' ').Substring(3);
                string body = ifcLine.Substring(posLpar + 1, posRpar - posLpar - 1); // extract argument body
                body=ExtractInnerCommas(body);CurrentArgs = body.Split(',');RefillInnerCommasOfCurrentArgs();// split entity attributes

                try {
                    Type t = Type.GetType("ifc." + CurrentTypeName, throwOnError: true, ignoreCase: true);
                    CurrentInstance = Activator.CreateInstance(t);
                    if (commentOpenPos > 0) ((ENTITY) CurrentInstance).EndOfLineComment = CurrentEntityComment;
                    ((ENTITY) CurrentInstance).LocalId = int.Parse(ifcLine.Substring(1, posA - 1));
                    Dictionary<int, FieldInfo> attributes = GetAttributesOfObject(CurrentInstance);

                    for (int i = 1; i <= attributes.Count; i++) {
                        FieldInfo field = attributes[i];// we access a dictionary via key (which starts at 1), not an array
                        string value = "$";
                        if (i <= CurrentArgs.GetLength(0)) value = CurrentArgs[i - 1].Trim(' ').Trim('\'');
                        if (field.FieldType == typeof(string)) {if (value == "$") field.SetValue(CurrentInstance, "" /*null*/);else field.SetValue(CurrentInstance, IfcString.Decode(value));}
                   else if (field.FieldType == typeof(int   )) {if (value == "$") field.SetValue(CurrentInstance, 0);          else field.SetValue(CurrentInstance, int.Parse(value));}
                   else if (field.FieldType.IsSubclassOf(typeof(TypeBase))) {try {field.SetValue(CurrentInstance, Parse2TYPE(value, field.FieldType));}
                                                                             catch (Exception e) {Log.Add("Parse.Type: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message, Log.Level.ThrowException);}
                                                                            } 
                   else if (field.FieldType.IsSubclassOf(typeof(SELECT)))   {try {object selectTypeInstance = Activator.CreateInstance(field.FieldType);
                                                                                  if (value.StartsWith("#")) {((SELECT) selectTypeInstance).Id = int.Parse(value.Substring(1));}
                                                                                  else if (!value.StartsWith("$") && !value.StartsWith("*")) {selectTypeInstance = ParseSELECT(value, field.FieldType);}
                                                                                  field.SetValue(CurrentInstance, selectTypeInstance);
                                                                                 }
                                                                             catch (Exception e){Log.Add($"Parse.SELECT: Field {i}: {field.FieldType}: {e.Message}", Log.Level.ThrowException);}
                                                                            }
                   else if (field.FieldType.IsSubclassOf(typeof(ENTITY))) {try {object entityInstance = null; // case of $
                                                                                if (value.StartsWith("*")) entityInstance = Activator.CreateInstance(field.FieldType);
                                                                           else if (value.StartsWith("#")) {entityInstance = Activator.CreateInstance(field.FieldType);
                                                                                                            ((ENTITY) entityInstance).LocalId = int.Parse(value.Substring(1));
                                                                                                           }
                                                                                field.SetValue(CurrentInstance, entityInstance);
                                                                               }
                                                                           catch (Exception e) {Log.Add($"Parse.ENTITY: Field {i}: {field.FieldType}: {e.Message}", Log.Level.ThrowException);}
                                                                          }
                   else if (field.FieldType.IsSubclassOf(typeof(Enum))) {try {object enumInstance = Activator.CreateInstance(field.FieldType);
                                                                              if   ((value.Length > 0) && (value[0] == '$')) enumInstance = 0;
                                                                              else {try {enumInstance = Enum.Parse(field.FieldType,value.Substring(1, value.Length - 2));
                                                                                         field.SetValue(CurrentInstance, enumInstance);
                                                                                        }
                                                                                    catch {Log.Add($"enum {field.FieldType}.{value} not recognized", Log.Level.Exception);}
                                                                                   }
                                                                             }
                                                                         catch (Exception e) {Log.Add($"Parse.ENTITY: Field {i}: {field.FieldType}: {e.Message}", Log.Level.ThrowException);}
                                                                        }
                   else if ((Nullable.GetUnderlyingType(field.FieldType) != null) &&
                            (Nullable.GetUnderlyingType(field.FieldType).IsSubclassOf(typeof(Enum)))) {try {object enumInstance = null;
                                                                                                            if ((value.Length > 0) && (value[0] != '$')) 
                                                                                                               {enumInstance = Activator.CreateInstance(field.FieldType);
                                                                                                                try {enumInstance = Enum.Parse(Nullable.GetUnderlyingType(field.FieldType),
                                                                                                                     value.Substring(1, value.Length - 2));
                                                                                                                    } 
                                                                                                                catch {Log.Add($"enum {field.FieldType}.{value} not recognized", Log.Level.ThrowException);}
                                                                                                               }
                                                                                                            field.SetValue(CurrentInstance, enumInstance);
                                                                                                           }
                                                                                                       catch (Exception e) {Log.Add($"Parse.ENTITY: Field {i}: {field.FieldType}: {e.Message}", Log.Level.ThrowException);}
                                                                                                      }
                   else if (typeof(ifcListInterface).IsAssignableFrom(field.FieldType)) {if (value == "$") field.SetValue(CurrentInstance,null); else
                                                                                         try {int posL = value.IndexOf('(') + 1;
                                                                                              value = value.Substring(posL,value.Substring(posL).LastIndexOf(')'));
                                                                                              if (value.StartsWith("$")) field.SetValue(CurrentInstance, null); // maybe this will not work (bb 2024-05-17)
                                                                                              else {if (ifcSqlType.SqlTypeGroupId(field.FieldType)==(int)ifc.TypeGroup.LISTTYPE2D)  // isListOfList
                                                                                                       {value=ExtractInnerCommas(value);ListElements = value.Split(',');RefillInnerCommasOfListElements(); // split list elements
                                                                                                        object[] valueInstances = new object[ListElements.Length];
                                                                                                        for (int i2 = 0; i2 < ListElements.Length; i2++) 
                                                                                                            {ListElements[i2] = ListElements[i2].Replace((char)9,','); // put comma back from tab
                                                                                                             int posL2 = ListElements[i2].IndexOf('(') + 1;
                                                                                                             ListElements[i2] = ListElements[i2].Substring(posL2, ListElements[i2].Substring(posL2).LastIndexOf(')')); 
                                                                                                             string[] ListElement=ListElements[i2].Split(','); 
                                                                                                             valueInstances[i2]=Activator.CreateInstance(GetValueType(field.FieldType), GetListCtorArgs(GetValueType(GetValueType(field.FieldType)),ListElement));
                                                                                                            }
                                                                                                         field.SetValue(CurrentInstance,Activator.CreateInstance(field.FieldType,valueInstances));
                                                                                                       } 
                                                                                                   else{field.SetValue(CurrentInstance, Parse2LIST(field.FieldType, value));
                                                                                                       }
                                                                                           
                                                                                                   }
                                                                                             } catch (Exception e) {Log.Add($"Parse.LIST: Field {i}: {field.FieldType}: {e.Message}\n{value}\n{CurrentLine}\n", Log.Level.ThrowException);}
                                                                                        }
                   else     Log.Add($"Field {i}: unknown type {field.FieldType.Name} at {field.FieldType.Name}\n{ifcLine}" , Log.Level.Error);
                    }

                    targetModel.EntityList.Add((ENTITY) CurrentInstance);
                }
                catch (Exception e) {Log.Add($"ERROR on ParseIfcLine: {e.Message}\nline: {ifcLine}", Log.Level.Exception);}
            }
            else { EntityComment ec = new EntityComment(); ec.CommentLine = CurrentEntityComment; ec.LocalId = NextGlobalCommentId--; targetModel.EntityList.Add(ec); }
        }


     // 2022-04-02 (ef): new method 'GetOrderedAttributeDictionaryForCurrentEntity'
    private static Dictionary<int, FieldInfo> GetAttributesOfObject(object obj) {
            Dictionary<int, FieldInfo> attributeDictionary = new Dictionary<int, FieldInfo>();
            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)) {
                foreach (Attribute attr in field.GetCustomAttributes(true))
                    if (attr is ifcAttribute) attributeDictionary.Add(((ifcAttribute) attr).OrdinalPosition, field);
            }
            return attributeDictionary;
        }// of GetAttributesOfObject
    }// of class ENTITY


    public partial class Model {
        
        /// <summary>
        /// Parses the IFC Header Data of the given <paramref name="filePath"/> and returns a <see cref="HeaderData"/> instance.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Instance of <see cref="HeaderData"/>; null if an error occurs</returns>
        private static HeaderData ParseHeaderData(string filePath) {
            string fileDescription = string.Empty;
            string fileName = string.Empty;
            string fileSchema = string.Empty;
            try {
                HeaderData headerData = new HeaderData();

                //first get the entire header string from start to ENDSEC;
                string headerString = "";
                using (StreamReader sr = new StreamReader(filePath)) {
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        headerString += line;
                        if (line.Contains("ENDSEC")) break;
                    }
                }

                string[] keywords = new string[] {"FILE_DESCRIPTION", "FILE_NAME", "FILE_SCHEMA", "ENDSEC" };
                Dictionary<string, string> headerInfo = new Dictionary<string, string>();
                for (int i = 0;  i < keywords.Length - 1; i++) {
                    string keyword = keywords[i];
                    string nextKeyword = keywords[i+1];
                    int pos = headerString.IndexOf(keyword);
                    int nextPos = headerString.IndexOf(nextKeyword);
                    headerInfo.Add(keyword, headerString.Substring(pos, nextPos - pos));
                } 

                fileDescription = headerInfo["FILE_DESCRIPTION"].Replace(" ", "");
                fileName = headerInfo["FILE_NAME"].Replace(" ", "");
                fileSchema = headerInfo["FILE_SCHEMA"].Replace(" ", "");

                // now, that the data is cleaned up, we can parse the data
                headerData.ViewDefinition = fileDescription.Contains('$') ? "ViewDefinition[undefinded]" : fileDescription.Split('\'')[1 * 2 - 1];
                headerData.ImplementationLevel = fileDescription.Contains('$') ? "2;1" : fileDescription.Split('\'')[1 * 4 - 1];
                                        fileName=fileName.Replace("()","''");
                                        fileName=fileName.Replace("$","''");
                string[] fileNameArgs = fileName.Split('\'');
                headerData.Name = fileNameArgs[1 * 2 - 1];
                headerData.TimeStamp = fileNameArgs[2 * 2 - 1];
                headerData.Author = fileNameArgs[3 * 2 - 1];
                headerData.Organization = fileNameArgs[4 * 2 - 1];

                headerData.PreprocessorVersion = fileNameArgs[5 * 2 - 1];
                headerData.OriginatingSystem = fileNameArgs[6 * 2 - 1];
                headerData.Authorization = fileNameArgs[7 * 2 - 1];
                headerData.FileSchema = fileSchema.Split('\'')[1 * 2 - 1];

                if (headerData.FileSchema != Specification.SchemaName) Log.Add($"Expected schema '{Specification.SchemaName}' != '{fileSchema}'", Log.Level.Warning);

                return headerData;
            }
            catch (Exception e) {
                Log.Add($"Exception parsing Header: {fileDescription};{fileName};{fileSchema}->{e.Message}",Log.Level.Exception);
                return null;
            }
        }

        public static Model FromStepFile(string filePath,bool assignInverseElements=true) { // 2022-12-03 (bb) AssignInverseElements
            Log.Add($"Reading STEP file '{filePath}'", Log.Level.Info);
            Model model = new ifc.Model(filePath.Replace(".ifc", ""));
            try{model.Header = ParseHeaderData(filePath);}catch(Exception e){Log.Add(e.Message,Log.Level.Exception);} // 2022-10-16 (bb) added try/catch
            if (model.Header == null) {
                model.Header = new HeaderData(); 
                model.Header.Init(NetSystem.IO.Path.GetFileNameWithoutExtension(filePath), "N/A", "N/A", "N/A");
            }
            
            StreamReader sr = new StreamReader(filePath);
            string line = "";
            while ((line = sr.ReadLine()) != null) if (line.Length > 3) {
                    line = line.TrimStart(' ');
                    // 2022-10-16 (bb) evaluate multiline-header (alpha):
                    line = line.TrimEnd(' ');
                    if (line.Length > 3) if (line[line.Length-1]!=';' && !(line[line.Length-2]=='*' && line[line.Length-1]=='/')) line+=sr.ReadLine();
                    if (line.Length > 3) if (line[0] == '#' || (line[0] == '/' && line[1] == '*')) try {ENTITY.ParseIfcLine(model, line);}catch (Exception e) { Log.Add(e.Message+ " in:" +line, Log.Level.Exception);}
            }
            sr.Close();
            model.AssignEntities();
            if (assignInverseElements) foreach (ifc.ENTITY e in  model.EntityList) e.AssignInverseElements();  // 2022-12-03 (bb) AssignInverseElements
            Log.Add($"Finished reading STEP file '{filePath}'.", Log.Level.Info);
            Log.Add($"Created {model.EntityList.Count} IFC Entities.", Log.Level.Info);
            return model;
        }
    }

}




