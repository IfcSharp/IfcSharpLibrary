// ifc_base_unit.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;

public class units{
public static double mm2m=1.0/1000.0;
public static double m2mm=1000.0;
}

namespace ifc{//==============================

public class ifcUnitAtribute:Attribute{public ifcUnitAtribute(UnitEnum eUnitEnum=UnitEnum.USERDEFINED,DerivedUnitEnum eDerivedUnitEnum=DerivedUnitEnum.USERDEFINED){this.eUnitEnum=eUnitEnum;this.eDerivedUnitEnum=eDerivedUnitEnum;}
                                       public UnitEnum eUnitEnum=UnitEnum.USERDEFINED;
                                       public DerivedUnitEnum eDerivedUnitEnum=DerivedUnitEnum.USERDEFINED;
                                      }

public interface ifcUnitInterface{
bool IsUnitEnum();
bool IsDerivedUnitEnum();
UnitEnum eUnitEnum();
DerivedUnitEnum eDerivedUnitEnum();
}


public partial class DimensionalExponents:ENTITY{
public       DimensionalExponents(SIUnitName n, int LengthExponent,int MassExponent,int TimeExponent,int ElectricCurrentExponent,int ThermodynamicTemperatureExponent,int AmountOfSubstanceExponent,int LuminousIntensityExponent):base(){/*Id=NextGlobalId++;EntityList.Add(this);*/this.LengthExponent=LengthExponent;this.MassExponent=MassExponent;this.TimeExponent=TimeExponent;this.ElectricCurrentExponent=ElectricCurrentExponent;this.ThermodynamicTemperatureExponent=ThermodynamicTemperatureExponent;this.AmountOfSubstanceExponent=AmountOfSubstanceExponent;this.LuminousIntensityExponent=LuminousIntensityExponent;}
}


public partial class SIUnit:NamedUnit{
/* DERIVED */ public       SIUnit(UnitEnum UnitType,SIUnitName Name,SIPrefix? Prefix=null,string EndOfLineComment=null):base(){AddNext();this.Dimensions=DimensionsForSiUnit(Name);this.UnitType=UnitType;this.Prefix=Prefix;this.Name=Name;this.EndOfLineComment=EndOfLineComment;}





DimensionalExponents DimensionsForSiUnit(SIUnitName n)
       {
        switch (n) {case SIUnitName.METRE          :return new DimensionalExponents(n,1, 0, 0, 0, 0, 0, 0);
                    case SIUnitName.SQUARE_METRE   :return new DimensionalExponents(n,2, 0, 0, 0, 0, 0, 0);
                    case SIUnitName.CUBIC_METRE    :return new DimensionalExponents(n,3, 0, 0, 0, 0, 0, 0);
                    case SIUnitName.GRAM           :return new DimensionalExponents(n,0, 1, 0, 0, 0, 0, 0);
                    case SIUnitName.SECOND         :return new DimensionalExponents(n,0, 0, 1, 0, 0, 0, 0);
                    case SIUnitName.AMPERE         :return new DimensionalExponents(n,0, 0, 0, 1, 0, 0, 0);
                    case SIUnitName.KELVIN         :return new DimensionalExponents(n,0, 0, 0, 0, 1, 0, 0);
                    case SIUnitName.MOLE           :return new DimensionalExponents(n,0, 0, 0, 0, 0, 1, 0);
                    case SIUnitName.CANDELA        :return new DimensionalExponents(n,0, 0, 0, 0, 0, 0, 1);
                    case SIUnitName.RADIAN         :return new DimensionalExponents(n,0, 0, 0, 0, 0, 0, 0);
                    case SIUnitName.STERADIAN      :return new DimensionalExponents(n,0, 0, 0, 0, 0, 0, 0);
                    case SIUnitName.HERTZ          :return new DimensionalExponents(n,0, 0, -1, 0, 0, 0, 0);
                    case SIUnitName.NEWTON         :return new DimensionalExponents(n,1, 1, -2, 0, 0, 0, 0);
                    case SIUnitName.PASCAL         :return new DimensionalExponents(n,-1, 1, -2, 0, 0, 0, 0);
                    case SIUnitName.JOULE          :return new DimensionalExponents(n,2, 1, -2, 0, 0, 0, 0);
                    case SIUnitName.WATT           :return new DimensionalExponents(n,2, 1, -3, 0, 0, 0, 0);
                    case SIUnitName.COULOMB        :return new DimensionalExponents(n,0, 0, 1, 1, 0, 0, 0);
                    case SIUnitName.VOLT           :return new DimensionalExponents(n,2, 1, -3, -1, 0, 0, 0);
                    case SIUnitName.FARAD          :return new DimensionalExponents(n,-2, -1, 4, 2, 0, 0, 0);
                    case SIUnitName.OHM            :return new DimensionalExponents(n,2, 1, -3, -2, 0, 0, 0);
                    case SIUnitName.SIEMENS        :return new DimensionalExponents(n,-2, -1, 3, 2, 0, 0, 0);
                    case SIUnitName.WEBER          :return new DimensionalExponents(n,2, 1, -2, -1, 0, 0, 0);
                    case SIUnitName.TESLA          :return new DimensionalExponents(n,0, 1, -2, -1, 0, 0, 0);
                    case SIUnitName.HENRY          :return new DimensionalExponents(n,2, 1, -2, -2, 0, 0, 0);
                    case SIUnitName.DEGREE_CELSIUS :return new DimensionalExponents(n,0, 0, 0, 0, 1, 0, 0);
                    case SIUnitName.LUMEN          :return new DimensionalExponents(n,0, 0, 0, 0, 0, 0, 1);
                    case SIUnitName.LUX            :return new DimensionalExponents(n,-2, 0, 0, 0, 0, 0, 1);
                    case SIUnitName.BECQUEREL      :return new DimensionalExponents(n,0, 0, -1, 0, 0, 0, 0);
                    case SIUnitName.GRAY           :return new DimensionalExponents(n,2, 0, -2, 0, 0, 0, 0);
                    case SIUnitName.SIEVERT        :return new DimensionalExponents(n,2, 0, -2, 0, 0, 0, 0);
                    default                        :return new DimensionalExponents(n,0, 0, 0, 0, 0, 0, 0);
                   }
        }
}


}// ifc==============================
