using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Architecture;


namespace PowerShemeMaster
{
    public class LanPanel
    {
        public string Name { get; set; }
        public Document Document { get; set; }
        public List<ElectricalSystem> Circuits { get; set; }
        public List<LanElement> Elements { get; set; }
        public ViewDrafting View { get; set; }
        public int QFCount { get; set; }
        public int QSCount { get; set; }
        public int FICount { get; set; }

        public LanPanel(List<ElectricalSystem> panelCircuits)
        {
            Name = panelCircuits.First().PanelName;
            Document = panelCircuits.First().Document;
            
            Circuits = panelCircuits;

            Circuits =
            (from с in panelCircuits // передаем каждый элемент из panelCircuits в переменную c
             orderby с.Name  // упорядочиваем по возрастанию
             select с).ToList(); // выбираем объект в создаваемую коллекцию

            QFCount = 0;
            QSCount = 0;
            FICount = 0;

            Elements = new List<LanElement>();

            foreach (ElectricalSystem circuit in Circuits)
            {
                switch (circuit.LookupParameter("Тип аппарата защиты").AsValueString())
                {
                    case "ВН":
                        QSCount++;
                        Elements.Add(new LanElement(circuit, QSCount));
                        break;

                    case "УЗО":
                        FICount++;
                        Elements.Add(new LanElement(circuit, FICount));
                        break;

                    default:
                        QFCount++;
                        Elements.Add(new LanElement(circuit, QFCount));
                        break;
                }
            }
        }

        public void Build()
        {
            FamilyInstance panel = Circuits.FirstOrDefault().BaseEquipment;
            View = Work.CreateViewDrafting(Document, Name);

            try
            {
                View.LookupParameter("ADSK_Назначение вида").Set("Схемы");
                View.LookupParameter("ADSK_Штамп Раздел проекта").Set("ЭМ_Однолинейные схемы");
            }
            catch
            {

            }

            double coord = 0;

            FamilyInstance ShemeStart = Work.PlaceFamily(View, "ЛАН_ГС_Боковик схемы", new XYZ(Work.ConvertMetersToFeet(coord-0.01), 0, 0));
            if (panel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NUMPHASES_PARAM).AsInteger() ==1)
            {
                ShemeStart.LookupParameter("1 фаза").Set(1);
            }
            else
            {
                ShemeStart.LookupParameter("1 фаза").Set(0);
            }
            ShemeStart.LookupParameter("N").Set(1);
            ShemeStart.LookupParameter("PE").Set(1);
            ShemeStart.LookupParameter("Фаза").Set(1);
            ShemeStart.LookupParameter("Показать вводную линию фазы").Set(1);
            ShemeStart.LookupParameter("Показать вводную линию N").Set(1);

            foreach (LanElement lanelement in Elements)
            {
                lanelement.Build(View, new XYZ(Work.ConvertMetersToFeet(coord), 0, 0));
                coord += lanelement.CellWidth;
            }

            Work.PlaceFamily(View, "Конец схемы", new XYZ(Work.ConvertMetersToFeet(coord-0.01), 0, 0));

            

            double current = Math.Round(panel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_TOTAL_DEMAND_CURRENT_PARAM).AsDouble(), 2);
            double demandFactor = Math.Round(panel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_TOTAL_DEMAND_FACTOR_PARAM).AsDouble(), 2);
            List<int> ListOfNominals = new List<int>() { 25, 32, 40, 50, 63, 80, 100, 125, 160, 250, 320, 400, 630 };
            int nominal = ListOfNominals.Find(x => x > current);

            FamilyInstance inputBreaker = Work.PlaceFamily(View, "Автоматический выключатель (ГС)", new XYZ(Work.ConvertMetersToFeet(0.01), Work.ConvertMetersToFeet(0.21), 0));

            inputBreaker.LookupParameter("Маркировка").Set("1QF");
            inputBreaker.LookupParameter("Панель").Set(Name);
            inputBreaker.LookupParameter("Номинальный ток  А").Set(nominal);
            inputBreaker.LookupParameter("Тип дифф. защиты (для АВДТ и УЗО)").Set("");
            inputBreaker.LookupParameter("Уставка дифф. защиты (для АВДТ и УЗО)").Set("");
            inputBreaker.LookupParameter("Времятоковая характеристика").Set("C");
            inputBreaker.LookupParameter("Предельная коммутационная способность").Set("6 кА");
            inputBreaker.LookupParameter("Количество полюсов").Set(panel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_NUMPHASES_PARAM).AsInteger());

            double cosf = 0;
            string trueload = panel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_TOTALESTLOAD_PARAM).AsValueString();
            string power = panel.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_TOTALLOAD_PARAM).AsValueString();
           

            FamilyInstance powers = Work.PlaceFamily(View, "ЛАН_ГС_Мощности щита", new XYZ(Work.ConvertMetersToFeet(0.03), Work.ConvertMetersToFeet(0.21), 0));
            powers.LookupParameter("Iрас").Set(current);
            powers.LookupParameter("cosf").Set(cosf);
            powers.LookupParameter("Pрас").Set(trueload);
            powers.LookupParameter("Pуст").Set(power);

            FamilyInstance panelMark = Work.PlaceFamily(View, "ЛАН_ГС_Метка щита", new XYZ(Work.ConvertMetersToFeet(0.05), Work.ConvertMetersToFeet(0.24), 0));
            panelMark.LookupParameter("Имя панели").Set(panel.Name);
            panelMark.LookupParameter("Корпус панели").Set(panel.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString());
            panelMark.LookupParameter("Количество модулей").Set(panel.get_Parameter(BuiltInParameter.RBS_ELEC_MAX_POLE_BREAKERS).AsValueString());
        }
        public void SetNominals()
        {
            foreach (LanElement lanelement in Elements)
            {
                lanelement.SetNominal();
            }
        }
        public void SetCables(Dictionary<ElementId, string> baseofcables)
        {
            foreach (LanElement lanelement in Elements)
            {
                lanelement.SetCable(baseofcables);
            }
        }
        public void CalcAndSetVoltageDrop()
        {
            foreach (LanElement lanelement in Elements)
            {
                lanelement.CalcAndSetVoltageDrop();
            }
        }
        public void SetPipes(Dictionary<ElementId, double> baseofpipes, double loadCoefficient)
        {
            foreach (LanElement lanelement in Elements)
            {
                lanelement.SetPipe(baseofpipes, loadCoefficient);
            }
        }
        public void SetRoomNumbersToCircuits()
        {
            string circuitRoomNumber;
            string elementRoomNumber;

            foreach (ElectricalSystem circuit in Circuits)
            {
                circuitRoomNumber = "";
                int charCount = 0;
                foreach (Element el in circuit.Elements)
                {
                    

                    try
                    {
                        elementRoomNumber = el.LookupParameter("Номер помещения").AsValueString();
                    }
                    catch
                    {
                        elementRoomNumber = "None";
                    }
                    if (elementRoomNumber == null) { elementRoomNumber = "None"; }
                    if ((!circuitRoomNumber.Contains(elementRoomNumber)) & (elementRoomNumber != "None"))
                    {
                        charCount += elementRoomNumber.Length + 1;
                        if (charCount > 12)
                        {
                            circuitRoomNumber += ", " + elementRoomNumber;
                            charCount = 0;
                        }
                        else
                        {
                            circuitRoomNumber += "," + elementRoomNumber;
                        }
                    }
                }
                circuitRoomNumber = circuitRoomNumber.TrimStart(new char[] { ',' });
                try
                {
                    circuit.LookupParameter("Номер помещения").Set(circuitRoomNumber);
                }
                catch { }
                foreach (LanElement lanElement in Elements)
                {
                    try
                    {
                        lanElement.Room = lanElement.Circuit.LookupParameter("Номер помещения").AsValueString();
                    }
                    catch { }
                }

            }
        }
    }
}
