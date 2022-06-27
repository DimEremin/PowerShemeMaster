using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;

namespace PowerShemeMaster
{
    public static class Check
    {
        // Проверка документа Revit doc на наличие семейств из списка. При отсутствии семейств будет предложено выбрать папку для их загрузки в документ
        public static bool FamilyCheck(Document doc, List<string> familyNames)
        {
            List<string> missingfamilyNames = new List<string>();
            foreach (string familyName in familyNames)
            {
                if (null == Work.FindElementByName(doc, typeof(Family), familyName))
                {
                    missingfamilyNames.Add(familyName);
                }
            }
            if (missingfamilyNames.Count == 0) { return true; }

            FamilyCheckForm familyCheckForm = new FamilyCheckForm(missingfamilyNames);
            familyCheckForm.ShowDialog();
            string directory;
            directory = familyCheckForm.FamilyDirectory;
            if ("Отмена" == directory)
            {
                return false;
            }

            foreach (string missingfamilyName in missingfamilyNames)
            {
                Work.LoadFamily(doc, directory, missingfamilyName);
            }

            return FamilyCheck(doc, missingfamilyNames);
        }
        // Проверка документа в приложении uiapp Revit на наличие общих параметров из словаря (имя параметра, тип параметра). При отсутствии параметров будет произведена попытка записать их в существующий ФОП (необходимо проверить на защиту файла ФОП от записи).
        public static bool ParameterCheck(UIApplication uiapp, Dictionary<string, ParameterType> requiredParameters, BuiltInCategory builtInCategory)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<bool> parameterCheckOk = new List<bool>() { true };
            DefinitionFile myDefinitionFile = uiapp.Application.OpenSharedParameterFile();

            Transaction transaction = new Transaction(doc);
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var elements = collector.OfCategory(builtInCategory);
            if (elements.Count() > 0)
            {
                transaction.Start("Ошибка");
                foreach (var requiredParameter in requiredParameters)
                {
                    if (elements.First().LookupParameter(requiredParameter.Key) == null)
                    {
                        if (myDefinitionFile == null)
                        {
                            System.Windows.MessageBox.Show("Отсутствует файл общих параметров");
                            return false;
                        }
                        parameterCheckOk.Add(Work.SetNewParameterToInstanceFamily(uiapp, myDefinitionFile, requiredParameter.Key, requiredParameter.Value, builtInCategory));
                    }
                }
                transaction.Commit();

            }
            else 
            {
                System.Windows.MessageBox.Show("В Модели отсутсвуют элементы требуемой категории");
                return false;
            }

            if (parameterCheckOk.Contains(false))
            {
                System.Windows.MessageBox.Show("Не удалось загрузить все необходимые параметры. Процесс не будет запущен");
                return false;
            }

            return true;

        }

        public static IEnumerable<Definition> GetDefinitionsByName(BindingMap bindingMap, string name)
        {
            var iterator = bindingMap.ForwardIterator();
            while (iterator.MoveNext())
            {
                var definition = iterator.Key;
                if (definition.Name.Equals(name))
                    yield return definition;
            }
        }


    }
}
