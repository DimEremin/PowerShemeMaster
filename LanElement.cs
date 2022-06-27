using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

namespace PowerShemeMaster
{
    public class LanElement
    {
        #region Свойства
        public Document Document { get; set; }
        public FamilyInstance ShemeElement;
        public ElectricalSystem Circuit { get; set; }
        public int Index { get; set; }
        public string Label { get; set; }
        public string Phase { get; set; }
        public int NumberOfPhases { get; set; }
        public int CircuitBreakerNominal { get; set; }
        public string CircuitBreakerType { get; set; }
        public string DiffSet { get; set; }
        public string DiffType { get; set; }
        public string TimeCurrentCharacteristic { get; set; }
        public string UltimateBreakingCapacity { get; set; }
        public string Panel { get; set; }
        public double CellWidth { get; set; }
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public string ThirdLine { get; set; }
        public ElementId LoadType { get; set; }
        public string GroupNumber { get; set; }
        public string Power { get; set; }
        public double Current { get; set; }
        public double VoltageDrop { get; set; }
        public string Room { get; set; }
        public string LoadName { get; set; }
        public int FullLength { get; set; }
        public static ElementId DefaultBreakerTypeId = null;
        public static ElementId DefaultLoadType= null;
        #endregion Свойства

        public LanElement(ElectricalSystem circuit, int i)
        {
            Document = circuit.Document;
            Circuit = circuit;
            Index = i;


                switch (circuit.LookupParameter("Тип аппарата защиты").AsValueString())
                {
                    case "ВН":
                        Label = "QS";
                        break;

                    case "УЗО":
                        Label = "FI";
                        break;

                    default:
                        Label = "QF";
                        break;
                }


            if ((Phase = circuit.LookupParameter("Метка фазы").AsString()) == null)
            {
                Phase = "";
            }

            NumberOfPhases = circuit.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_WIRE_NUM_HOTS_PARAM).AsInteger();

           if ((CircuitBreakerNominal = circuit.LookupParameter("ЛАН_Номинал аппарата защиты").AsInteger())== 0)
           {
                CircuitBreakerNominal = 16;
           }

           if ((CircuitBreakerType = "ЛАН_" + circuit.LookupParameter("Тип аппарата защиты").AsValueString())== null)

           {
                CircuitBreakerType = "ЛАН_АВ";
           }
           if (CircuitBreakerType == "ЛАН_(нет)")
           {
               CircuitBreakerType = "ЛАН_АВ";
           }

            if ((DiffType = circuit.LookupParameter("ЛАН_Тип дифф. защиты (для АВДТ и УЗО)").AsValueString())==null)
            {
                DiffType = "";
            }

            if ((DiffSet = circuit.LookupParameter("ЛАН_Уставка дифф. защиты(для АВДТ и УЗО)").AsValueString())==null)
            {
                DiffSet = "";
            }

            if ((TimeCurrentCharacteristic = circuit.LookupParameter("ЛАН_Времятоковая характеристика автомата").AsValueString()) == null)
            {
                DiffSet = "";
            }

            if ((UltimateBreakingCapacity = circuit.LookupParameter("ЛАН_Предельная коммутационная способность автомата").AsValueString()) == null)
            {
                DiffSet = "";
            }

            Panel = circuit.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM).AsValueString();

            CellWidth = 0.02;
            FullLength = GetLength();
             
            if ((FirstLine = circuit.LookupParameter("Выбор проводника").AsValueString()) == null)
            {
                FirstLine = "";
            }

            SecondLine = BuildSecondLine();

            LoadType = circuit.LookupParameter("УГО Нагрузки").AsElementId();

            GroupNumber = circuit.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_NUMBER).AsValueString();

            Power = circuit.get_Parameter(BuiltInParameter.RBS_ELEC_TRUE_LOAD).AsValueString();

            Current =  Math.Round(circuit.get_Parameter(BuiltInParameter.RBS_ELEC_APPARENT_CURRENT_PARAM).AsDouble(),2);

            VoltageDrop = 0;

            if ((Room = circuit.LookupParameter("Номер помещения").AsValueString()) == null)
            {
                Room = "";
            }

            LoadName = circuit.get_Parameter(BuiltInParameter.RBS_ELEC_CIRCUIT_NAME).AsValueString();
            
        }
        public void Build (View view, XYZ xyz)
        {
            ShemeElement = Work.PlaceFamily(view, CircuitBreakerType, xyz);

            if (NumberOfPhases == 1)
            {
                ShemeElement.LookupParameter("Фаза").Set(Phase);
                ShemeElement.LookupParameter("1 фаза").Set(1);
            }
            else
            {
                ShemeElement.LookupParameter("Фаза").Set("");
                ShemeElement.LookupParameter("1 фаза").Set(0);
            }

            ShemeElement.LookupParameter("Панель").Set(Circuit.PanelName);
            ShemeElement.LookupParameter("Позиционное обозначение элемента").Set(Label + Index);  
            ShemeElement.LookupParameter("Номинальный ток элемента").Set(CircuitBreakerNominal);

            if (Circuit.LookupParameter("Тип аппарата защиты").AsValueString() == "АВДТ" | 
                Circuit.LookupParameter("Тип аппарата защиты").AsValueString() == "УЗО")
            {
                ShemeElement.LookupParameter("Тип дифф. защиты (для АВДТ и УЗО)").Set(DiffType);
                ShemeElement.LookupParameter("Уставка дифф. защиты (для АВДТ и УЗО)").Set(DiffSet);
                ShemeElement.LookupParameter("Показать линию N без разрыва").Set(0);
                ShemeElement.LookupParameter("Показать линию N с разрывом").Set(1);
            }
            else
            {
                ShemeElement.LookupParameter("Тип дифф. защиты (для АВДТ и УЗО)").Set("");
                ShemeElement.LookupParameter("Уставка дифф. защиты (для АВДТ и УЗО)").Set("");
                ShemeElement.LookupParameter("Показать линию N без разрыва").Set(1);
                ShemeElement.LookupParameter("Показать линию N с разрывом").Set(0);
            }
            ShemeElement.LookupParameter("Характеристика аппарата защиты").Set(TimeCurrentCharacteristic);
            ShemeElement.LookupParameter("Предельная коммутационная способность").Set(UltimateBreakingCapacity);
            ShemeElement.LookupParameter("Идентификатор группы").Set(GroupNumber);
            ShemeElement.LookupParameter("Установленная мощность").Set(Power);
            ShemeElement.LookupParameter("Расчетный ток").Set(Current.ToString());
            ShemeElement.LookupParameter("Падение напряжения").Set(VoltageDrop.ToString());
            ShemeElement.LookupParameter("Наименование помещений").Set(Room);
            ShemeElement.LookupParameter("Наименование потребителя").Set(LoadName);
            ShemeElement.LookupParameter("Первая строка").Set(FirstLine);
            ShemeElement.LookupParameter("Вторая строка").Set(SecondLine);
            if (LoadType.IntegerValue != -1)
            {
                ShemeElement.LookupParameter("УГО потребителя").Set(LoadType);
            }
            else
            {
                Circuit.LookupParameter("ЛАН_Тип нагрузки").Set(DefaultLoadType);
                LoadType = Circuit.LookupParameter("УГО Нагрузки").AsElementId();
                ShemeElement.LookupParameter("УГО потребителя").Set(LoadType);
            }
        }

        public int GetLength ()
        {

            double length;
            double cuttingReserve;
            int lengthReservePercent;
            int count;

            length = Work.ConvertFeetToMeters
                (Circuit.get_Parameter
                (BuiltInParameter.RBS_ELEC_CIRCUIT_LENGTH_PARAM).
                AsDouble());
           

            try
            {
                lengthReservePercent = Circuit.LookupParameter("Запас проводника_Электрические цепи").AsInteger();
            }
            catch
            {
                lengthReservePercent = 0;
            }

            try
            {
                cuttingReserve = Work.ConvertFeetToMeters
                    (Circuit.LookupParameter("Запас на разделку проводника_Электрические цепи").
                    AsDouble());
            }
            catch
            {
                cuttingReserve = 0;
            }

            count = (Circuit.get_Parameter
                (BuiltInParameter.RBS_ELEC_CIRCUIT_NUMBER_OF_ELEMENTS_PARAM).
                AsInteger());

            //Длина + Длина * Запас проводника_Электрические цепи / 100 + Количество элементов* Запас на разделку проводника_Электрические цепи

            length += length* lengthReservePercent/100 + count * cuttingReserve;
            return Convert.ToInt32(Math.Round(length, 0, MidpointRounding.AwayFromZero));

        }

        public string BuildSecondLine ()
        {
            string secondLine;
            int lengthInPipe;
           
            string pipeIndex = "";
            string pipeDiameter;

            Dictionary<string, List<string>> pipeIndeces = new Dictionary<string, List<string>>()

            { 
                { "ПВХ", new List<string>() {"пвх" } },
                { "ПНД", new List<string>() {"пнд" } },
                { "Т", new List<string>() {"сталь","электросвар", "металлическая" } },
                { "МР", new List<string>() {"рукав" } },
                { "К", new List<string>() {"короб", "канал" } }
            };



            try
            {
                lengthInPipe =
                    Convert.ToInt32(
                    Math.Round(
                    Work.ConvertFeetToMeters(
                        Circuit.LookupParameter("Длина трубы_Электрические цепи").
                        AsDouble()),0));
            }
           
            catch
            {
                lengthInPipe = 0;
            }
            //MessageBox.Show(lengthInPipe.ToString());
                

            if (lengthInPipe > 0)
            {
                string pipeType;
                try
                {
                    pipeType = Circuit.LookupParameter("Выбор трубы").AsValueString();
                }
                catch
                {
                    pipeType = "ПВХ";
                }
                
                foreach (var pipe in pipeIndeces)
                {
                    foreach (string x in pipe.Value)
                    {
                        if (pipeType.ToLower().Contains(x))
                        {
                            pipeIndex = pipe.Key;
                            break;
                        }
                    }
                }
                try
                {
                    pipeDiameter = Circuit.LookupParameter("Диаметр по стандарту трубы_Электрические цепи").AsValueString();
                }
                catch
                {
                    pipeDiameter = "";
                }

                secondLine = "Л - " + (FullLength - lengthInPipe) + " м; " +  pipeIndex + pipeDiameter + " - " + lengthInPipe + " м";
                return secondLine;
            }



            secondLine = "Л - " + FullLength + " м" ;


            return secondLine;
        }

        //Метод задает номинал аппарата защиты для эл.цепи 
        public void SetNominal()
        {
            List<int> ListOfNominalsOther = new List<int>() { 16, 20, 25, 32, 40, 50, 63, 80, 100, 125, 160, 250, 320, 400, 630 };
            List<int> ListOfNominalsLightings = new List<int>() { 10, 16, 20, 25, 32, 40, 50, 63, 80, 100, 125, 160, 250, 320, 400, 630 };
            List<int> ListOfNominals = new List<int>();

            //Ток в цепи
            double current = Circuit.get_Parameter(BuiltInParameter.RBS_ELEC_APPARENT_CURRENT_PARAM).AsDouble();

            //Классификация нагрузок  
            var loadType = Circuit.get_Parameter(BuiltInParameter.CIRCUIT_LOAD_CLASSIFICATION_PARAM).AsString();

            if (loadType.Contains("освещение"))
            {
                ListOfNominals = ListOfNominalsLightings;
            }
            else
            {
                ListOfNominals = ListOfNominalsOther;
            }

            CircuitBreakerNominal = ListOfNominals.Find(p => p > current);

            var ParameterCircuitNominal = Circuit.LookupParameter("ЛАН_Номинал аппарата защиты");
            if (ParameterCircuitNominal != null)
            {
                ParameterCircuitNominal.Set(CircuitBreakerNominal);
            }

            if (DefaultBreakerTypeId != null)
            {
                var ParameterCircuitType = Circuit.LookupParameter("Тип аппарата защиты");
                if (ParameterCircuitType != null)
                {
                    ParameterCircuitType.Set(DefaultBreakerTypeId);
                }
            }

            TimeCurrentCharacteristic = "C";
            var ParameterTimeCurrentCharacteristic = Circuit.LookupParameter("ЛАН_Времятоковая характеристика автомата");
            if (ParameterTimeCurrentCharacteristic != null)
            {
                ParameterTimeCurrentCharacteristic.Set(TimeCurrentCharacteristic);
            }

            UltimateBreakingCapacity = "6 кА";
            var ParameterUltimateBreakingCapacity = Circuit.LookupParameter("ЛАН_Предельная коммутационная способность автомата");
            if (ParameterUltimateBreakingCapacity != null)
            {
                ParameterUltimateBreakingCapacity.Set(UltimateBreakingCapacity);
            }
        }

        //Метод расчитывает сечение кабеля и задает значение ключевого параметра "Выбор проводника"
        public void SetCable(Dictionary<ElementId, string> baseofcables)
        {
            string mountingType;
            try
            {
                mountingType = Circuit.LookupParameter("ЛАН_Способ монтажа").AsValueString();
            }
            catch
            {
                mountingType = "B2";
            }

            string index = mountingType + "-" + NumberOfPhases;

            string cableSize = "";

            if (!MountingTypeLibrary.Data.ContainsKey(index))
            {
                index = "B2" + "-" + NumberOfPhases;
            }

            foreach (var ddt in MountingTypeLibrary.Data[index])
            {
                if (CircuitBreakerNominal <= ddt.Key)
                {
                    cableSize = ddt.Value;
                    break;
                }
            }

            var parameterCable = Circuit.LookupParameter("Выбор проводника");

            if (parameterCable != null)
            {
                foreach (var cable in baseofcables)
                {
                    //MessageBox.Show(cable.Value);
                    if (cable.Value == (NumberOfPhases+2).ToString() + "х" + cableSize)
                    {
                        parameterCable.Set(cable.Key);
                        break;
                    }
                }
            }
        }

        //Метод расчитывает и задает значение падения напряжения
        public void CalcAndSetVoltageDrop()
        {
            double k;
            switch (NumberOfPhases)
            {
                case 1:
                    k = 12;
                    break;

                case 3:
                    k = 72;
                    break;

                default:
                    k = 12;
                    break;
            }
            double circuitCableSize;
            try
            {
                 circuitCableSize = Convert.ToDouble(Circuit.LookupParameter("Количество жил и сечение проводника_Электрические цепи").AsString().Substring(2));
            }
            catch
            {
                circuitCableSize = 1.5;
                Circuit.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set("Ошибка при расчете падения напряжения. Расчет выполнен для сечения 1.5 мм2");
            }



            ElectricalSystem elcircuit = Circuit as ElectricalSystem;

            List<XYZ> circuitPath = elcircuit.GetCircuitPath() as List<XYZ>;
            CircuitElement.CommonPath = circuitPath;

            var circuitElements = new List<CircuitElement>();

            foreach (FamilyInstance el in Circuit.Elements)
            {
                circuitElements.Add(new CircuitElement(el));
                break;
            }
            double power = 0;

            var sortedcircuitElements = circuitElements.OrderBy(key => key.Length).ToList();

            foreach (var el in sortedcircuitElements)
            {
                power += el.Power;
            }

            foreach (CircuitElement circuitElement in sortedcircuitElements)
            {
                VoltageDrop += Math.Round(((power * circuitElement.Length) / (circuitCableSize * k)), 2);
                power -= circuitElement.Power;
            }

            if (Circuit.LookupParameter("ЛАН_Потеря напряжения") != null)
            {
                Circuit.LookupParameter("ЛАН_Потеря напряжения").Set(VoltageDrop.ToString());
            }

        }

        //Метод расчитывает и задает значение диаметра трубы
        public void SetPipe(Dictionary<ElementId, double> baseofpipes, double loadCoefficient)
        {
            double cableDiameter;
            try
            {
                cableDiameter = Circuit.LookupParameter("Диаметр проводника_Электрические цепи").AsDouble();
            }
            catch
            {
                cableDiameter = 0;
            }

            if (cableDiameter > 0)
            {
                foreach (var pipe in baseofpipes)
                {

                    if (Math.PI * Math.Pow(cableDiameter / 2, 2) <= (loadCoefficient / 100) * Math.PI * Math.Pow(pipe.Value / 2, 2))
                    {
                        try
                        {
                            Circuit.LookupParameter("Выбор трубы").Set(pipe.Key);
                        }
                        catch { }
                        break;
                    }
                }
            } 
        }
    }
}
