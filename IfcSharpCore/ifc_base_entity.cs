// ifc_base_entity.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using NetSystem=System;

public class ifcAttribute:System.Attribute{public ifcAttribute(int OrdinalPosition,bool optional=false,bool derived=false){this.OrdinalPosition=OrdinalPosition;this.optional=optional;this.derived=derived;}
                                           public int OrdinalPosition=0;
                                           public bool optional=false; //      ---> $
                                           public bool derived=false;  // ToDo ---> *
                                          }

public class ifcInverseAttribute:System.Attribute{public ifcInverseAttribute(string For){this.For=For;}
                                           public string For="";
                                          }


namespace ifc{//==============================


public partial class ENTITY:ifcSqlType{//==========================================================================================
public               ENTITY(){} 

public int LocalId=0;
public string IfcId() => (this.LocalId==0)?"*":"#"+this.LocalId;
public override string ToString() => IfcId();
public string ShortTypeName() => this.GetType().ToString().Replace("IFC4","ifc").Replace("ifc.","");
//public int SqlTypeId() => ((ifc.ifcSqlAttribute)this.GetType().GetCustomAttributes(true)[0]).SqlTypeId;
public string EndOfLineComment=null;
public long ifcSqlGlobalId=0;
public virtual void AssignInverseElements(){}
}//=====================================================================================================================


// static:
public partial class ENTITY:ifcSqlType{//==========================================================================================
static public int NextGlobalId=1;
protected void AddNext(){LocalId=NextGlobalId++;/*NetSystem.Console.WriteLine(this.ToStepLine());*/  Repository.CurrentModel.EntityList.Add(this);}
protected virtual void CheckValues(){}
protected virtual void SetDefaultValues(){}
static public int NextGlobalCommentId=0;
protected void AddNextCommentLine(){LocalId=NextGlobalCommentId--;Repository.CurrentModel.EntityList.Add(this);}
}//=====================================================================================================================


[ifcSql(TypeGroupId:5,TypeId:-1)] public partial class EntityComment:ENTITY{//==========================================================================================
public               EntityComment(){} 
public               EntityComment(string CommentLine){AddNextCommentLine();this.CommentLine=CommentLine;if (this.CommentLine.Length<74) this.CommentLine+=new string(' ',74-this.CommentLine.Length);}
public               EntityComment(string CommentLine,char FrameChar){this.CommentLine=CommentLine;if (this.CommentLine.Length<74) this.CommentLine+=new string(' ',74-this.CommentLine.Length);
                                                                      new EntityComment(new string(' ',74));
                                                                      new EntityComment(new string(FrameChar,74));
                                                                      AddNextCommentLine();
                                                                      new EntityComment(new string(FrameChar,74));
                                                                      new EntityComment(new string(' ',74));
                                                                     }
public string CommentLine="no comment";
public override string ToString(){return CommentLine;}
public static int HtmlCnt=0;
}//=====================================================================================================================

 #if !IFC2X3
public partial class CartesianTransformationOperator3DnonUniform:CartesianTransformationOperator3D{//===================
protected override void CheckValues(){} // Check >0
protected override void SetDefaultValues(){

if (Scale==null) Scale=new ifc.Real(1);
if (Scale2==null) Scale2=Scale;
if (Scale3==null) Scale3=Scale;
if (Axis1==null) Axis1=new ifc.Direction(x:1,y:0,z:0);
if (Axis2==null) Axis2=new ifc.Direction(x:0,y:1,z:0);
if (Axis3==null) Axis3=new ifc.Direction(x:0,y:0,z:1);

}
}//=====================================================================================================================
#endif

}// ifc=======================================