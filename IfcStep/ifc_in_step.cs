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
using System.Text.RegularExpressions;

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

        /// <summary>
        /// parses given <param name="value">value</param> and tries to instantiate an object of given <param name="valueType">valueType</param>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <returns>null on error; instance of <param>valueType</param> on success</returns>
        public static object Parse2TYPE(string value, Type valueType) {
            //TODO: catch more specific exceptions and throw custom IfcSharpExceptions
            //      i.e.: infer argument mismatches and the like
            if (value == "$") return null;
            
            Type valueBaseType = GetValueType(valueType); //valueType.BaseType.GetGenericArguments()[0];
            object instance = null;
            object[] ctorArgs = new object[1];
            //if ((value == "$") || (value == "*")) instance = Activator.CreateInstance(valueType);
            if (value == "*") instance = Activator.CreateInstance(valueType);
            else {
                if (valueBaseType == typeof(string)) {
                    if (value == "$") ctorArgs[0] = "";
                    else ctorArgs[0] = ifc.IfcString.Decode(value);
                    instance = Activator.CreateInstance(valueType, ctorArgs);
                }
                else if (valueBaseType == typeof(int)) {
                    ctorArgs[0] = int.Parse(value);
                    instance = Activator.CreateInstance(valueType, ctorArgs);
                }
                else if (valueBaseType == typeof(double))
                    try {
                        ctorArgs[0] = double.Parse(value, CultureInfo.InvariantCulture);
                        instance = Activator.CreateInstance(valueType, ctorArgs);
                    }
                    catch (Exception e){
                        Log.Add($"Parse2TYPE: Exception parsing to double\n{CurrentLine}{e.Message}", Log.Level.Exception);
                    }
                else if (valueBaseType == typeof(bool)) {
                    ctorArgs[0] = (value == ".T.");
                    instance = Activator.CreateInstance(valueType, ctorArgs);
                }
                else if (valueBaseType.IsSubclassOf(typeof(TypeBase))) {
                    instance = Activator.CreateInstance(valueType, Parse2TYPE(value, valueBaseType));
                }

                // 2022-06-10 (ef): for lists which are defined inline (i.e.: (49,6,1,566000))
                //                  we need to also check if the baseclass of the given 'FieldType' can be represented as a subclass of LIST.
                //                  if so, the given args are parsed to the type of LIST and the the FieldType instance is created.
                else if (typeof(ifcListInterface).IsAssignableFrom(valueBaseType)) {
                    try {
                        ctorArgs[0] = Parse2LIST(valueBaseType, value);
                        instance = Activator.CreateInstance(valueType, ctorArgs[0]);
                    }
                    catch (Exception e) {
                        Log.Add($"Parse2TYPE (parsing list): {value}\nException: {e.Message}", Log.Level.Exception);
                    }
                }
                else {
                    Log.Add($"UNKNOWN TYPE for expected Type {valueType.Name}: Base={valueBaseType.Name} value={value}\r\n{CurrentLine}", Log.Level.Error);
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
        
        /// <summary>
        /// Parses a given <paramref name="body"/> and tries to instantiate a type <paramref name="listType"/>.
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        /// <exception cref="IfcSharpException"></exception>
        public static object Parse2LIST(Type listType, string body) {
            if (!typeof(IEnumerable).IsAssignableFrom(listType)) Console.WriteLine("Parse2LIST: " + listType + " is not IEnumerable");
            if ((body == "$") || (body == "*")) return Activator.CreateInstance(listType);
            Type valueType = GetValueType(listType);
            object instance = null;
            string valuesString = body.Substring(1, body.Length - 2);
            string delimiter = valuesString.Contains(")") ? ")," : ",";
            string[] values = valuesString.Split(new string[] { delimiter }, StringSplitOptions.None);
            if (delimiter == "),") {
                // when the valueString was split by '),' we need to add the closing braces again at the end of each 'value'
                for (int i = 0; i < values.Length - 1; i++) {
                    values[i] = values[i] += ")";
                }
            }

            if (values.Length == 0) {
                string msg = $"No values were parsed in {listType} of #{CurrentId}";
                Log.Add(msg, Log.Level.Error);
                throw new IfcSharpException(msg);
            }

            if (values[0] == "") {
                try { instance = Activator.CreateInstance(listType); }
                catch { Log.Add($"ERROR on Parse2LIST.1: #{CurrentId}", Log.Level.Exception); }
            }
            else {
                try { instance = Activator.CreateInstance(listType, GetListCtorArgs(valueType, values)); }
                catch (Exception e) { Log.Add($"ERROR on Parse2LIST.2: #{CurrentId}\n{e}", Log.Level.Exception); }
            }
            return instance;
        }


        /// <summary>
        /// Helper Method which creates instances of given <paramref name="valueType"/> by the given array of <paramref name="values"/>
        /// The <paramref name="values"/> are passed as <see cref="string"/> and are being parsed at runtime.
        /// </summary>
        /// <param name="valueType"><see cref="Type"/> of values stored in the list</param>
        /// <param name="values">Array of values as <see cref="string"/></param>
        /// <returns>Array of <see cref="object"/> instances of the given <see cref="Type"/> <paramref name="valueType"/></returns>
        private static object[] GetListCtorArgs(Type valueType, string[] values) {
            object[] valueInstances = new object[values.Length];

            for (int i = 0; i < values.Length; i++) {
                string value = values[i];
                if (valueType == typeof(int)) {
                    valueInstances[i] = int.Parse(value);
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
                    if (value.Length > 0 && value.Trim(' ')[0] == '#') {
                        // if SELECT refers to another line, we only need the Id
                        ((SELECT)o).Id = int.Parse(value.Trim(' ').Substring(1)); 
                    }
                    else {
                        // otherwise we parse the entire SELECT
                        o = ParseSELECT(value, valueType);
                    }
                    valueInstances[i] = o;
                }
                else if (typeof(ifcListInterface).IsAssignableFrom(valueType)) {
                    valueInstances[i] = Parse2LIST(valueType, value);
                }
                else {
                    Console.WriteLine("Base=" + valueType.Name + " not supported. in \r\n" + CurrentLine);
                }
            }
            return valueInstances;
        }
        

        
        private static string[] BaseTypeNames { get; } = new string[] { "INTEGER", "BINARY", "LOGICAL", "REAL", "BOOLEAN" };

        private static object ParseSELECT(string value, Type selectType) {
            object instance = null;
            string selectedTypeName = value.Replace("IFC", "");

            int posLpar = selectedTypeName.IndexOf('(');
            int posRpar = selectedTypeName.LastIndexOf(')');
            string body = selectedTypeName.Substring(posLpar + 1, posRpar - posLpar - 1); // get body of STEP expression
            selectedTypeName = selectedTypeName.Substring(0, posLpar);

            bool ignoreCase = true;
            // 2022-06-10 (ef): there is currently some ambivalence with the naming scheme which is case-senstive.
            //                  BASETYPES (which are representing the types available in the database) are capitalized,
            //                  whereas the actual IFC-Types are not. This can lead to confusion when parsing a STEP-file.
            //                  Therefore, we need to check if the current 'ElementName' is one of the BaseTypes, if so we change the name to first letter captitalized and remaining lower.
            foreach (string name in BaseTypeNames) {
                if (name != selectedTypeName) continue;
                selectedTypeName = selectedTypeName.Substring(0, 1) + selectedTypeName.Substring(1).ToLower();
                ignoreCase = false;
            }
            selectedTypeName = "ifc." + selectedTypeName;
            
            try {
                Type selectValueType = Type.GetType(typeName: selectedTypeName, throwOnError: true, ignoreCase: ignoreCase);
                if (selectValueType.IsSubclassOf(typeof(TypeBase))) {
                    object[] typeCtorArgs = new object[1];
                    typeCtorArgs[0] = Parse2TYPE(body, selectValueType);
                    if (typeCtorArgs[0] == null) Log.Add($"SELECT-type #{CurrentId} is null", Log.Level.Error);
                    else {
                        try { instance = Activator.CreateInstance(selectType, typeCtorArgs); }
                        catch (Exception e) { Log.Add($"Exception in ParseSELECT: #{CurrentId} | {e.Message}", Log.Level.Exception); }
                    }
                }
            }
            catch (Exception e) { Log.Add($"Exception in ParseSELECT: #{CurrentId} | {e.Message}", Log.Level.Exception); }
            return instance;
        }
        
        private static string CurrentLine { get; set; } = "";

        private static string CurrentEntityComment { get; set; } = "";
        
        // 2022-04-02 (ef): added CurrentElementName, CurrentArgs and CurrentEntity as a static property
        //                  this helps with debugging and readability
        private static string CurrentTypeName { get; set; } = "";
        private static string[] CurrentArgs { get; set; }
        private static object CurrentInstance { get; set; }
        private static int CurrentId {
            get { return CurrentInstance is ENTITY e ? e.LocalId : -1; }
        }
        
        /// <summary>
        /// Parses the given <paramref name="ifcLine"/> and tries to add the resulting <code>ENTITY</code> to the given <paramref name="targetModel"/> 
        /// </summary>
        /// <param name="targetModel"></param>
        /// <param name="ifcLine"></param>
        /// <exception cref="IfcSharpException"></exception>
        public static void ParseIfcLine(Model targetModel, string ifcLine) {
            //Console.WriteLine("parse="+line);
            CurrentLine = ifcLine;
            int commentOpenPos = ifcLine.IndexOf("/*"); 
            if (commentOpenPos >= 0) {
                CurrentEntityComment = ifcLine.Substring(commentOpenPos + 3).Replace(" */", ""); ifcLine = ifcLine.Substring(0, commentOpenPos);
            }
            if (commentOpenPos != 0) {
                int posA = ifcLine.IndexOf('=');
                int posLpar = ifcLine.IndexOf('(');
                int posRpar = ifcLine.LastIndexOf(')');
                CurrentTypeName = ifcLine.Substring(posA + 1, posLpar - posA - 1).TrimStart(' ').TrimEnd(' ').Substring(3);
                string body = ifcLine.Substring(posLpar + 1, posRpar - posLpar - 1); // extract argument-body
                bool txtOpen = false;
                int parOpen = 0;
                for (int i = 0; i < body.Length; i++) {
                    if ((!txtOpen) && (body[i] == '\'')) { txtOpen = true; }
                    else if ((!txtOpen) && (body[i] == '(')) { parOpen++; }
                    else if ((!txtOpen) && (body[i] == ')')) { parOpen--; }
                    else if ((txtOpen) && (body[i] == '\'')) { txtOpen = false; }
                    if ((txtOpen) || (parOpen > 0)) if (body[i] == ',') { body = ReplaceCharAt(body, i, (char)9); }
                }
                CurrentArgs = body.Split(',');
                for (int i = 0; i < CurrentArgs.Length; i++) CurrentArgs[i] = CurrentArgs[i].Replace((char)9, ',');

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
                        if (field.FieldType == typeof(string)) {
                            if (value == "$") field.SetValue(CurrentInstance, string.Empty /*null*/);
                            else field.SetValue(CurrentInstance, IfcString.Decode(value));
                        }
                        else if (field.FieldType == typeof(int)) {
                            if (value == "$") field.SetValue(CurrentInstance, 0);
                            else field.SetValue(CurrentInstance, int.Parse(value));
                        }
                        else if (field.FieldType.IsSubclassOf(typeof(TypeBase))) {
                            try {
                                field.SetValue(CurrentInstance, Parse2TYPE(value, field.FieldType));
                            }
                            catch (Exception e) {
                                string msg = "Parse.Type: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message;
                                Log.Add(msg, Log.Level.Exception);
                                throw new IfcSharpException(msg);
                            }
                        } 
                        else if (field.FieldType.IsSubclassOf(typeof(SELECT))) {
                            try {
                                object selectTypeInstance = Activator.CreateInstance(field.FieldType);
                                if (value.StartsWith("#")) {
                                    ((SELECT) selectTypeInstance).Id = int.Parse(value.Substring(1));
                                }
                                else if (!value.StartsWith("$") && !value.StartsWith("*")) {
                                    selectTypeInstance = ParseSELECT(value, field.FieldType);
                                }
                                field.SetValue(CurrentInstance, selectTypeInstance);
                            }
                            catch (Exception e) {
                                //TODO: logging
                                string msg = "xx Parse.SELECT: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message;
                                Log.Add(msg, Log.Level.Exception);
                                throw new IfcSharpException(msg);
                            }
                        }
                        else if (field.FieldType.IsSubclassOf(typeof(ENTITY))) {
                            try {
                                object entityInstance = null; //falls $
                                if (value.StartsWith("*")) entityInstance = Activator.CreateInstance(field.FieldType);
                                else if (value.StartsWith("#")) {
                                    entityInstance = Activator.CreateInstance(field.FieldType);
                                    ((ENTITY) entityInstance).LocalId = int.Parse(value.Substring(1));
                                }

                                field.SetValue(CurrentInstance, entityInstance);
                            }
                            catch (Exception e) {
                                string msg = "Parse.ENTITY: Field " + i + ": " + field.FieldType.ToString() + ": " + e.Message;
                                Log.Add(msg, Log.Level.Exception);
                                throw new IfcSharpException(msg);
                            }
                        }
                        else if (field.FieldType.IsSubclassOf(typeof(Enum))) {
                            Log.Add($"Enum in {CurrentLine}", Log.Level.Debug);
                            try {
                                object enumInstance = Activator.CreateInstance(field.FieldType);
                                if ((value.Length > 0) && (value[0] == '$')) enumInstance = 0;
                                else {
                                    try {
                                        enumInstance = Enum.Parse(field.FieldType, value.Substring(1, value.Length - 2));
                                        field.SetValue(CurrentInstance, enumInstance);
                                    }
                                    catch { Log.Add($"enum {field.FieldType}.{value} not recognized", Log.Level.Exception); }
                                }
                            }
                            catch (Exception e) {
                                string msg = $"Parse.Enum: Field {i}: {field.FieldType}: {e.Message}";
                                Log.Add(msg, Log.Level.Exception);
                                throw new IfcSharpException(msg);
                            }
                        }
                        else if ((Nullable.GetUnderlyingType(field.FieldType) != null) && (Nullable.GetUnderlyingType(field.FieldType).IsSubclassOf(typeof(Enum)))) {
                            try {
                                object enumInstance = null;
                                if ((value.Length > 0) && (value[0] != '$')) {
                                    enumInstance = Activator.CreateInstance(field.FieldType);
                                    try {
                                        enumInstance = Enum.Parse(Nullable.GetUnderlyingType(field.FieldType), value.Substring(1, value.Length - 2));
                                    }
                                    catch {
                                        Log.Add($"enum {field.FieldType}.{value} not recognized", Log.Level.Exception);
                                    }
                                }

                                field.SetValue(CurrentInstance, enumInstance);
                            }
                            catch (Exception e) {
                                string msg = $"Parse.Enum: Field {i}: {field.FieldType}: {e.Message}";
                                Log.Add(msg, Log.Level.Exception);
                                throw new IfcSharpException(msg);
                            }
                        }
                        else if (typeof(ifcListInterface).IsAssignableFrom(field.FieldType)) {
                            try {
                                if (value.StartsWith("$")) field.SetValue(CurrentInstance, null);
                                else field.SetValue(CurrentInstance, Parse2LIST(field.FieldType, value.Replace(" ","")));
                            }
                            catch (Exception e) {
                                string msg = $"Parse.LIST: Field {i}: {field.FieldType}: {e.Message}";
                                Log.Add(msg, Log.Level.Exception);
                                throw new IfcSharpException(msg);
                            }
                        }
                        else {
                            Log.Add($"{i}: is {field.FieldType.Name} ???????x? {field.FieldType.Name} {ifcLine}", Log.Level.Error);
                        }
                    }

                    targetModel.EntityList.Add((ENTITY) CurrentInstance);
                }
                catch (Exception e) {
                    Log.Add($"ERROR on ParseIfcLine: {e.Message}\n{ifcLine}", Log.Level.Exception);
                }
            }
            else { EntityComment ec = new EntityComment(); ec.CommentLine = CurrentEntityComment; ec.LocalId = NextGlobalCommentId--; targetModel.EntityList.Add(ec); }
        }

        // 2022-07-05 (ef): renamed method 'GetAttributesOfObject'
        // TODO: move to reflection_helper
        private static Dictionary<int, FieldInfo> GetAttributesOfObject(object obj) {
            Dictionary<int, FieldInfo> attributeDictionary = new Dictionary<int, FieldInfo>();
            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)) {
                foreach (Attribute attr in field.GetCustomAttributes(true)) {
                    if (attr is ifcAttribute ifcAttribute) attributeDictionary.Add(ifcAttribute.OrdinalPosition, field);
                }
            }
            return attributeDictionary;
        }
    }

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
                using (StreamReader sr = new StreamReader(filePath)) {
                    string line;
                    string currentSection = string.Empty;
                    while ((line = sr.ReadLine()) != null) {
                        if (line.StartsWith("ENDSEC")) break;
                        if (line.StartsWith("FILE_DESCRIPTION")) currentSection = "FILE_DESCRIPTION";
                        else if (line.StartsWith("FILE_NAME")) currentSection = "FILE_NAME";
                        else if (line.StartsWith("FILE_SCHEMA")) currentSection = "FILE_SCHEMA";
                        if (currentSection == "FILE_DESCRIPTION") fileDescription += line;
                        if (currentSection == "FILE_NAME") fileName += line;
                        if (currentSection == "FILE_SCHEMA") fileSchema += line;
                    }
                }

                fileDescription = fileDescription.Replace(" ", "");
                fileName = fileName.Replace(" ", "");
                fileSchema = fileSchema.Replace(" ", "");

                // now, that the data is cleaned up, we can parse the data
                headerData.ViewDefinition = fileDescription.Contains('$') ? "ViewDefinition[undefinded]" : fileDescription.Split('\'')[1 * 2 - 1];
                headerData.ImplementationLevel = fileDescription.Contains('$') ? "2;1" : fileDescription.Split('\'')[1 * 4 - 1];
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

        public static Model FromStepFile(string filePath) {
            Log.Add($"Reading STEP file '{filePath}'", Log.Level.Info);
            Model model = new ifc.Model(filePath.Replace(".ifc", ""));
            model.Header = ParseHeaderData(filePath);
            if (model.Header == null) {
                model.Header = new HeaderData(); 
                model.Header.Init(NetSystem.IO.Path.GetFileNameWithoutExtension(filePath), "N/A", "N/A", "N/A");
            }
            
            StreamReader sr = new StreamReader(filePath);
            string line = "";
            while ((line = sr.ReadLine()) != null) if (line.Length > 3) {
                    line = line.TrimStart(' ');
                    if (line.Length > 3) if (line[0] == '#' || (line[0] == '/' && line[1] == '*')) ENTITY.ParseIfcLine(model, line);
            }
            sr.Close();
            model.AssignEntities();
            Log.Add($"Finished reading STEP file '{filePath}'.", Log.Level.Info);
            Log.Add($"Created {model.EntityList.Count} IFC Entities.", Log.Level.Info);
            return model;
        }
    }

}




