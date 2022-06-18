// ifc_base_geometry.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

namespace ifc{//==============================


public partial class CartesianPoint:Point{//============================================================================
public               CartesianPoint(double x, double y, double z,string EndOfLineComment=null):base(){AddNext();this.Coordinates=new List1to3_LengthMeasure((LengthMeasure)  x,(LengthMeasure)  y,(LengthMeasure)  z);this.EndOfLineComment=  EndOfLineComment;}
public               CartesianPoint(double x, double y          ,string EndOfLineComment=null):base(){AddNext();this.Coordinates=new List1to3_LengthMeasure((LengthMeasure)  x,(LengthMeasure)  y);                   this.EndOfLineComment=  EndOfLineComment;}
public               CartesianPoint(CartesianPoint p                                         ):base(){AddNext();this.Coordinates=new List1to3_LengthMeasure((LengthMeasure)p.x,(LengthMeasure)p.y,(LengthMeasure)p.z);this.EndOfLineComment=p.EndOfLineComment;} // issue: need for distinction 2D/3D here
public double x {get{return this.Coordinates[0].TypeValue;}}
public double y {get{return this.Coordinates[1].TypeValue;}}
public double z {get{return this.Coordinates[2].TypeValue;}}
public static CartesianPoint operator - (CartesianPoint p) {return new CartesianPoint(      -p.x,      -p.y,      -p.z);}// issue: need for distinction 2D/3D here
public        CartesianPoint Add        (double x, double y, double z) {return new CartesianPoint(this.x+x,this.y+y,this.z+z);}// issue: need for distinction 2D/3D here
}//=====================================================================================================================

 #if !IFC2X3
public partial class Direction:GeometricRepresentationItem{//===========================================================
public               Direction(double x, double y, double z,string EndOfLineComment=null):base(){AddNext();this.DirectionRatios=new List2to3_Real((Real)  x,(Real)  y,(Real)  z);this.EndOfLineComment=  EndOfLineComment;}
public               Direction(double x, double y          ,string EndOfLineComment=null):base(){AddNext();this.DirectionRatios=new List2to3_Real((Real)  x,(Real)  y);          this.EndOfLineComment=  EndOfLineComment;}
public               Direction(Direction d                                              ):base(){AddNext();this.DirectionRatios=new List2to3_Real((Real)d.x,(Real)d.y,(Real)d.z);this.EndOfLineComment=d.EndOfLineComment;} // issue: need for distinction 2D/3D here
public double x {get{return this.DirectionRatios[0].TypeValue;}}
public double y {get{return this.DirectionRatios[1].TypeValue;}}
public double z {get{return this.DirectionRatios[2].TypeValue;}}
public static Direction operator - (Direction d              ) {return new Direction(-d.x,-d.y,-d.z);               }// issue: need for distinction 2D/3D here
public static Direction operator + (Direction d1,Direction d2) {return new Direction(d1.x+d2.x,d1.y+d2.y,d1.z+d2.z);}// issue: need for distinction 2D/3D here
public static Direction operator - (Direction d1,Direction d2) {return new Direction(d1.x-d2.x,d1.y-d2.y,d1.z-d2.z);}// issue: need for distinction 2D/3D here
public static Direction operator * (Direction d, double Scale) {return new Direction(d.x*Scale,d.y*Scale,d.z*Scale);}// issue: need for distinction 2D/3D here

}//=====================================================================================================================
#else
public partial class Direction:GeometricRepresentationItem{//===========================================================
public               Direction(double x, double y, double z,string EndOfLineComment=null):base(){AddNext();this.DirectionRatios=new List2to3_double((REAL)  x,(REAL)  y,(REAL)  z);this.EndOfLineComment=  EndOfLineComment;}
public               Direction(double x, double y          ,string EndOfLineComment=null):base(){AddNext();this.DirectionRatios=new List2to3_double((REAL)  x,(REAL)  y);            this.EndOfLineComment=  EndOfLineComment;}
public               Direction(Direction d                                              ):base(){AddNext();this.DirectionRatios=new List2to3_double((REAL)d.x,(REAL)d.y,(REAL)d.z);this.EndOfLineComment=d.EndOfLineComment;} // issue: need for distinction 2D/3D here
public double x {get{return this.DirectionRatios[0];}}
public double y {get{return this.DirectionRatios[1];}}
public double z {get{return this.DirectionRatios[2];}}
public static Direction operator - (Direction d              ) {return new Direction(-d.x,-d.y,-d.z);               }// issue: need for distinction 2D/3D here
public static Direction operator + (Direction d1,Direction d2) {return new Direction(d1.x+d2.x,d1.y+d2.y,d1.z+d2.z);}// issue: need for distinction 2D/3D here
public static Direction operator - (Direction d1,Direction d2) {return new Direction(d1.x-d2.x,d1.y-d2.y,d1.z-d2.z);}// issue: need for distinction 2D/3D here
public static Direction operator * (Direction d, double Scale) {return new Direction(d.x*Scale,d.y*Scale,d.z*Scale);}// issue: need for distinction 2D/3D here

}//=====================================================================================================================
#endif


public partial class Axis2Placement3D:Placement{//======================================================================
public               Axis2Placement3D(Axis2Placement3D template,string EndOfLineComment=null):base(){AddNext();this.Location=template.Location;this.Axis=template.Axis;this.RefDirection=template.RefDirection;this.EndOfLineComment=EndOfLineComment;}
public               Axis2Placement3D Clone(CartesianPoint p,string EndOfLineComment=null) {return new Axis2Placement3D(Location:p,Axis:this.Axis,RefDirection:this.RefDirection,EndOfLineComment:EndOfLineComment);}

}//=====================================================================================================================



/*
public partial class CompoundPlaneAngleMeasure :List3to4<int>{
public CompoundPlaneAngleMeasure (params int[] items):base() {foreach (int e in items)  this.Add((int)e);}  // INTEGER
}
*/

}// ifc==============================