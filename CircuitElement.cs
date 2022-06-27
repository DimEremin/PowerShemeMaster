using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB.Electrical;

namespace PowerShemeMaster
{
    public class CircuitElement
    {
        public static List<XYZ> CommonPath;
        public XYZ Location;
        public int Length;
        public double Power;
        public FamilyInstance Element;
        public int Index;
        double minDistance;

        public CircuitElement(FamilyInstance e)
            {
            Element = e;
            Document doc = e.Document;
            Element element;
            String powerParam="";

            var parameterCircuitPower = e.LookupParameter("ADSK_Номинальная мощность");
            if (parameterCircuitPower != null)
            {
                 powerParam = Work.GetParameterValue(parameterCircuitPower);
            }
            else 
            {
                element = doc.GetElement(e.GetTypeId());
                parameterCircuitPower = element.LookupParameter("ADSK_Номинальная мощность");
                if (parameterCircuitPower != null)
                {
                     powerParam = Work.GetParameterValue(parameterCircuitPower);
                }
                else
                {
                    powerParam = "0 Вт";
                }
            }

                if (powerParam == "")
                {
                    powerParam = "0 Вт";
                }
            

            powerParam = powerParam.Substring(0, powerParam.Length - 3);
            Power = Math.Round(Convert.ToDouble(powerParam)/1000.00,2);

            //MessageBox.Show(e.LookupParameter("ADSK_Номинальная мощность").AsString());

            Location = (e.Location as LocationPoint).Point;
            //index = 0;
            //MessageBox.Show("Location " + Location.ToString());
            minDistance = 1000000.00;
            foreach (XYZ xyz in CommonPath)
            {
                if (minDistance > xyz.DistanceTo(Location))
                {
                    minDistance = xyz.DistanceTo(Location);
                    Index = CommonPath.IndexOf(xyz);
                }
            }
            //MessageBox.Show("Index " + Index.ToString() + "Номер цепи:" + e.LookupParameter("Номер цепи"));
            double iLength = 0;
            int i = 0;
            while (i < Index)
            {
                //MessageBox.Show("XYZ " + i.ToString() + " " + CommonPath[i].ToString() + "\n" +
                //    "XYZ " + (i + 1).ToString() + " " + CommonPath[i + 1].ToString() + "\n" +
                //    "Distance " + (Convert.ToInt32(CommonPath[i].DistanceTo(CommonPath[i + 1]))).ToString());

                iLength += Work.ConvertFeetToMeters(CommonPath[i].DistanceTo(CommonPath[i+1]));
                i++;
            }
            Length = Convert.ToInt32(iLength);

            //MessageBox.Show("Length " + Length.ToString());

        }
    }


}
