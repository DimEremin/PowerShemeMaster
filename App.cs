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
                .Tab("ЛАН_Панель")
                .Panel("ЭОМ")
                .CreateButton<Command>("Схемы",
                    "Схемы", b =>
                    {
                        b.SetLargeImage(Properties.Resources.LAN_EOM_Logo_25);
                        b.SetLongDescription("Расчет и построение однолинейных схем");
                    });
            
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
