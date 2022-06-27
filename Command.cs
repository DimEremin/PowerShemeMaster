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

            //��������� ��������� �����
            FilteredElementCollector circuitCollector = new FilteredElementCollector(doc);
            var circuits = circuitCollector.OfCategory(BuiltInCategory.OST_ElectricalCircuit);

            ForgeTypeId ff = new ForgeTypeId("autodesk.spec.aec:length-1.0.0");

            //�������� �� ������� ����������� ���������� (�� �������)
            Dictionary<string, ParameterType> requiredParameters = new Dictionary<string, ParameterType>()
        {
            {"���_������������ �������������� ��������", ParameterType.Text },
            {"���_������� �������� ������", ParameterType.Integer},
            {"���_������ ����������", ParameterType.Text},
            {"���_���������� �������������� ����������� ��������", ParameterType.Text },
            {"���_��� ����. ������ (��� ���� � ���)",ParameterType.Text},
            {"���_������� ����. ������(��� ���� � ���)", ParameterType.Text}
        };
            if (!Check.ParameterCheck(uiapp, requiredParameters, BuiltInCategory.OST_ElectricalCircuit))
            {
                return Result.Cancelled;
            }


            //�������� �� ������� ����������� �������� (�� ������)
            List<string> requiredFeamiliesList = new List<string>()
        {
            "���_��_������� �����",
            "���_��_�������� ����",
            "���_��_��������� �����",
            "���_��_��_��",
            "���_��_��_���",
            "���_��_������� �����",
            "���_��_����� ����",
        };
            Check.FamilyCheck(doc, requiredFeamiliesList);

            //��������� �������� ������������ ����� ��������� ������. ����� ���� �������� �� ���������
            var keybreakers = Work.GetKeyScheduleElements(doc, "���_�����_���� ��������� ������");
            if (keybreakers != null)
            {
                foreach (Element keybreaker in keybreakers)
                {
                    if (keybreaker.Name == "��")
                    {
                        LanElement.DefaultBreakerTypeId = keybreaker.Id;
                    }
                }
            }


            var loadtypes = Work.GetKeyScheduleElements(doc, "���_�����_������������ ����� ��������");
            if (loadtypes != null)
            {
                foreach (Element loadtype in loadtypes)
                {
                    if (loadtype.Name == "�����")
                    {
                        LanElement.DefaultLoadType = loadtype.Id;
                    }
                }
            }



            var keycables = Work.GetKeyScheduleElements(doc, "�_���_��_������������� ����_���������� ������");
            var cabletypes = new List<string>();

            if (keycables != null)
            {
                foreach (Element keycable in keycables)
                {

                    if (!cabletypes.Contains(keycable.LookupParameter("����� ����������_������������� ����").AsString()))
                    {
                        cabletypes.Add(keycable.LookupParameter("����� ����������_������������� ����").AsString());
                    }

                }
            }
            else
            {
                cabletypes.Add("���");
            }

            var keypipes = Work.GetKeyScheduleElements(doc, "�_���_��_������������� ����_���������� ����");
            var pipetypes = new List<string>();

            if (keypipes != null)
            {
                foreach (Element keypipe in keypipes)
                {

                    if (!pipetypes.Contains(keypipe.LookupParameter("����� �����_������������� ����").AsString()))
                    {
                        pipetypes.Add(keypipe.LookupParameter("����� �����_������������� ����").AsString());
                    }
                }
            }
            else
            {
                pipetypes.Add("���");
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
                panelslist.Add("���");
            }

            //����� ��������� �������
            CableTypeForm cableForm = new CableTypeForm(cabletypes, pipetypes, panelslist);

            cableForm.ShowDialog();

            string cableType = cableForm.CableType;
            string pipeType = cableForm.PipeType;
            List<string> panels = cableForm.SelectedPanels;
            List<string> actions = cableForm.SelectedActions;
            double loadCoefficient = cableForm.LoadCoefficient;
            int defaultDiameter = cableForm.DefaultDiameter;

            if (cableType == "Cancel") { return Result.Cancelled; }

            //������� ������� ������� �� ����, ���������� �������������
            var baseofcables = new Dictionary<ElementId, string>();
            foreach (Element keycable in keycables)
            {
                if (keycable.LookupParameter("����� ����������_������������� ����").AsString() == cableType)
                {
                    baseofcables.Add
                        (
                        keycable.Id,
                        keycable.LookupParameter("���������� ��� � ������� ����������_������������� ����").AsString()
                        );
                }
            }

            //������� ������� ���� �� ����, ���������� �������������
            var baseofpipes = new Dictionary<ElementId, double>();
            foreach (Element keypipe in keypipes)
            {
                if (
                    (keypipe.LookupParameter("����� �����_������������� ����").AsString() == pipeType) &
                    (Convert.ToInt32(keypipe.LookupParameter("������� �� ��������� �����_������������� ����").AsValueString()) >= defaultDiameter)
                    )
                {
                    baseofpipes.Add
                        (
                        keypipe.Id,
                        Convert.ToDouble(keypipe.LookupParameter("���������� ������� �����_������������� ����").AsValueString())
                        );
                }
            }
            Dictionary<ElementId, double> sortedbaseofpipes = baseofpipes.OrderBy(key => key.Value).ToDictionary(x => x.Key, y => y.Value);



            //�������������� ������ � �������� � ���, ���� ������ �� �����
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

            //��������� ���������� ��� �������� ��������� � ������ �����

            transaction.Start("������");

            if (actions.Contains("��������� ������ ��������� ��������� ����� (��� ������)"))
            {
                Work.SetRoomNumbersToElements(doc);
            }


            foreach (LanPanel lanPanel in lanPanels)
            {

                if (actions.Contains("��������� ������ ��������� ����� (��������� ������)"))
                {
                    lanPanel.SetRoomNumbersToCircuits();
                }

                if (actions.Contains("���������� �������� ��������� ������ ��������� �����"))
                {
                    lanPanel.SetNominals();
                }

                if (actions.Contains("���������� ��� � ������� ��������� �����"))
                {
                    lanPanel.SetCables(baseofcables);
                    lanPanel.CalcAndSetVoltageDrop();
                }

                if (actions.Contains("���������� ������� ������������� ����"))
                {
                    lanPanel.SetPipes(sortedbaseofpipes, loadCoefficient);
                }

                if (actions.Contains("��������� ������������ �����"))
                {
                    lanPanel.Build();
                }
            }

            transaction.Commit();

            return Result.Succeeded;
        }
    }
}
