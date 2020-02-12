// ifc_base_geometry.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

namespace ifc{//==============================


public partial class CartesianPoint:Point{//============================================================================
public               CartesianPoint(double x, double y, double z,string EndOfLineComment=null):base(){AddNext();this.Coordinates=new List1to3_LengthMeasure(new LengthMeasure(x),(LengthMeasure)y,(LengthMeasure)z);this.EndOfLineComment=EndOfLineComment;}
public               CartesianPoint(double x, double y          ,string EndOfLineComment=null):base(){AddNext();this.Coordinates=new List1to3_LengthMeasure((LengthMeasure)x,(LengthMeasure)y);this.EndOfLineComment=EndOfLineComment;}
}//=====================================================================================================================

public partial class Direction:GeometricRepresentationItem{//===========================================================
public               Direction(double x, double y, double z,string EndOfLineComment=null):base(){AddNext();this.DirectionRatios=new List2to3_Real((Real)x,(Real)y,(Real)z);this.EndOfLineComment=EndOfLineComment;}
public               Direction(double x, double y          ,string EndOfLineComment=null):base(){AddNext();this.DirectionRatios=new List2to3_Real((Real)x,(Real)y);        this.EndOfLineComment=EndOfLineComment;}
}//=====================================================================================================================


public partial class CompoundPlaneAngleMeasure :List3to4<INTEGER>{
public CompoundPlaneAngleMeasure (params int[] items):base() {foreach (int e in items)  this.Add((INTEGER)e);} 
}

}// ifc==============================