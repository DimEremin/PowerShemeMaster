#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using VCRevitRibbonUtil;
using System.Resources;

#endregion

namespace PowerShemeMaster
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            Ribbon.GetApplicationRibbon(a)
                .Tab("���_������")
                .Panel("���")
                .CreateButton<Command>("�����",
                    "�����", b =>
                    {
                        b.SetLargeImage(Properties.Resources.LAN_EOM_Logo_25);
                        b.SetLongDescription("������ � ���������� ������������ ����");
                    });
            
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
