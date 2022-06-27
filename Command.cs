#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace PowerShemeMaster
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            //UIControlledApplication ff = commandData.Application As UIControlledApplication;
            //DefinitionFile myDefinitionFile = ControlledApplication.OpenSharedParameterFile();

            //DefinitionFile myDefinitionFile = uiapp.Application.OpenSharedParameterFile();

            Transaction transaction = new Transaction(doc);

            //Получение коллекции цепей
            FilteredElementCollector circuitCollector = new FilteredElementCollector(doc);
            var circuits = circuitCollector.OfCategory(BuiltInCategory.OST_ElectricalCircuit);

            ForgeTypeId ff = new ForgeTypeId("autodesk.spec.aec:length-1.0.0");

            //Проверка на наличие необходимых параметров (из словаря)
            Dictionary<string, ParameterType> requiredParameters = new Dictionary<string, ParameterType>()
        {
            {"ЛАН_Времятоковая характеристика автомата", ParameterType.Text },
            {"ЛАН_Номинал аппарата защиты", ParameterType.Integer},
            {"ЛАН_Потеря напряжения", ParameterType.Text},
            {"ЛАН_Предельная коммутационная способность автомата", ParameterType.Text },
            {"ЛАН_Тип дифф. защиты (для АВДТ и УЗО)",ParameterType.Text},
            {"ЛАН_Уставка дифф. защиты(для АВДТ и УЗО)", ParameterType.Text}
        };
            if (!Check.ParameterCheck(uiapp, requiredParameters, BuiltInCategory.OST_ElectricalCircuit))
            {
                return Result.Cancelled;
            }


            //Проверка на наличие необходимых семейств (из списка)
            List<string> requiredFeamiliesList = new List<string>()
        {
            "ЛАН_ГС_Боковик схемы",
            "ЛАН_ГС_Мощности щита",
            "ЛАН_ГС_Окончание схемы",
            "ЛАН_ГС_РП_КЛ",
            "ЛАН_ГС_РП_УГО",
            "ЛАН_РС_Элемент схемы",
            "ЛАН_ГС_Метка щита",
        };
            Check.FamilyCheck(doc, requiredFeamiliesList);

            //Получение ключевой спецификации типов аппаратов защиты. Выбор типа аппарата по умолчанию
            var keybreakers = Work.GetKeyScheduleElements(doc, "ЛАН_Схема_Типы аппаратов защиты");
            if (keybreakers != null)
            {
                foreach (Element keybreaker in keybreakers)
                {
                    if (keybreaker.Name == "АВ")
                    {
                        LanElement.DefaultBreakerTypeId = keybreaker.Id;
                    }
                }
            }


            var loadtypes = Work.GetKeyScheduleElements(doc, "ЛАН_Схема_Спецификация типов нагрузки");
            if (loadtypes != null)
            {
                foreach (Element loadtype in loadtypes)
                {
                    if (loadtype.Name == "Линия")
                    {
                        LanElement.DefaultLoadType = loadtype.Id;
                    }
                }
            }



            var keycables = Work.GetKeyScheduleElements(doc, "В_ЭОМ_КТ_Электрические цепи_Справочник кабеля");
            var cabletypes = new List<string>();

            if (keycables != null)
            {
                foreach (Element keycable in keycables)
                {

                    if (!cabletypes.Contains(keycable.LookupParameter("Марка проводника_Электрические цепи").AsString()))
                    {
                        cabletypes.Add(keycable.LookupParameter("Марка проводника_Электрические цепи").AsString());
                    }

                }
            }
            else
            {
                cabletypes.Add("Нет");
            }

            var keypipes = Work.GetKeyScheduleElements(doc, "В_ЭОМ_КТ_Электрические цепи_Справочник труб");
            var pipetypes = new List<string>();

            if (keypipes != null)
            {
                foreach (Element keypipe in keypipes)
                {

                    if (!pipetypes.Contains(keypipe.LookupParameter("Марка трубы_Электрические цепи").AsString()))
                    {
                        pipetypes.Add(keypipe.LookupParameter("Марка трубы_Электрические цепи").AsString());
                    }
                }
            }
            else
            {
                pipetypes.Add("Нет");
            }

            var panelslist = new List<string>();
            foreach (ElectricalSystem circuit in circuits)
            {
                if ((!panelslist.Contains(circuit.PanelName)) & (circuit.PanelName != ""))
                {
                    panelslist.Add(circuit.PanelName);
                }
            }
            if (panelslist.Count == 0)
            {
                panelslist.Add("Нет");
            }

            //Вызов основного диалога
            CableTypeForm cableForm = new CableTypeForm(cabletypes, pipetypes, panelslist);

            cableForm.ShowDialog();

            string cableType = cableForm.CableType;
            string pipeType = cableForm.PipeType;
            List<string> panels = cableForm.SelectedPanels;
            List<string> actions = cableForm.SelectedActions;
            double loadCoefficient = cableForm.LoadCoefficient;
            int defaultDiameter = cableForm.DefaultDiameter;

            if (cableType == "Cancel") { return Result.Cancelled; }

            //Обрезка таблицы кабелей до типа, выбранного пользователем
            var baseofcables = new Dictionary<ElementId, string>();
            foreach (Element keycable in keycables)
            {
                if (keycable.LookupParameter("Марка проводника_Электрические цепи").AsString() == cableType)
                {
                    baseofcables.Add
                        (
                        keycable.Id,
                        keycable.LookupParameter("Количество жил и сечение проводника_Электрические цепи").AsString()
                        );
                }
            }

            //Обрезка таблицы труб до типа, выбранного пользователем
            var baseofpipes = new Dictionary<ElementId, double>();
            foreach (Element keypipe in keypipes)
            {
                if (
                    (keypipe.LookupParameter("Марка трубы_Электрические цепи").AsString() == pipeType) &
                    (Convert.ToInt32(keypipe.LookupParameter("Диаметр по стандарту трубы_Электрические цепи").AsValueString()) >= defaultDiameter)
                    )
                {
                    baseofpipes.Add
                        (
                        keypipe.Id,
                        Convert.ToDouble(keypipe.LookupParameter("Внутренний диаметр трубы_Электрические цепи").AsValueString())
                        );
                }
            }
            Dictionary<ElementId, double> sortedbaseofpipes = baseofpipes.OrderBy(key => key.Value).ToDictionary(x => x.Key, y => y.Value);



            //Инициализируем панели и элементы в них, беря данные из цепей
            List<LanPanel> lanPanels = new List<LanPanel>();
            foreach (string panel in panels)
            {
                List<ElectricalSystem> panelCircuits = new List<ElectricalSystem>();
                foreach (ElectricalSystem circuit in circuits)
                {
                    if (panel == circuit.PanelName)
                    {
                        panelCircuits.Add(circuit);
                    }
                }
                if (panelCircuits.Count > 0)
                {
                    lanPanels.Add(new LanPanel(panelCircuits));
                }
            }

            //Открываем транзакцию для внесения изменений в модель Ревит

            transaction.Start("Ошибка");

            if (actions.Contains("Назначить номера помещений элементам цепей (все панели)"))
            {
                Work.SetRoomNumbersToElements(doc);
            }


            foreach (LanPanel lanPanel in lanPanels)
            {

                if (actions.Contains("Назначить номера помещений цепям (выбранные панели)"))
                {
                    lanPanel.SetRoomNumbersToCircuits();
                }

                if (actions.Contains("Установить номиналы аппаратов защиты кабельных линий"))
                {
                    lanPanel.SetNominals();
                }

                if (actions.Contains("Установить тип и сечение кабельных линий"))
                {
                    lanPanel.SetCables(baseofcables);
                    lanPanel.CalcAndSetVoltageDrop();
                }

                if (actions.Contains("Установить диаметр гофрированных труб"))
                {
                    lanPanel.SetPipes(sortedbaseofpipes, loadCoefficient);
                }

                if (actions.Contains("Построить однолинейные схемы"))
                {
                    lanPanel.Build();
                }
            }

            transaction.Commit();

            return Result.Succeeded;
        }
    }
}
