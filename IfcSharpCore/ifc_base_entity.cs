// ifc_base_entity.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)


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

public int Id=0;
public string IfcId() => (this.Id==0)?"*":"#"+this.Id;
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
protected void AddNext(){Id=NextGlobalId++;Repository.CurrentModel.EntityList.Add(this);}
static public int NextGlobalCommentId=0;
protected void AddNextCommentLine(){Id=NextGlobalCommentId--;Repository.CurrentModel.EntityList.Add(this);}
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




}// ifc=======================================