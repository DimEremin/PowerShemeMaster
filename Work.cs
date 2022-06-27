using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;

namespace PowerShemeMaster
{
    public static class Work
    {

        public static ElementId LinkID { get; set; }
        public static bool? RoomNumbersAreInLink { get; set; }

        // Get the param value and return as string
        public static string GetParameterValue(Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.ElementId:
                    return parameter.AsElementId().IntegerValue.ToString();

                case StorageType.Integer:
                case StorageType.None:
                case StorageType.Double:
                    return parameter.AsValueString();

                case StorageType.String:
                    return parameter.AsString();

                default:
                    return "";

            }
        }
        //Конвертация футов в метры
        public static double ConvertFeetToMeters(double f)
        {
            double m = f * 0.3048;
            return m;
        }
        //Конвертация метров в футы
        public static double ConvertMetersToFeet(double m)
        {
            double f = m / 0.3048;
            return f;
        }
        //Создание чертежного вида с заданным именем (при совпадении имен добаляется цифровой индекс)
        public static ViewDrafting CreateViewDrafting(Document doc, string viewDraftingName)
        {
            string viewName = "Схема " + viewDraftingName;
            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            collector2.OfCategory(BuiltInCategory.OST_Views);
            List<string> viewNames = new List<string>();
            foreach (Element view in collector2)
            {
                viewNames.Add(view.Name);
            }
            int i = 1;
            while (viewNames.Contains(viewName))
            {
                viewName = $"Схема {viewDraftingName}({i})";
                i++;
            }

            // For simplicity, we'll look what drafting view types are already available,
            // and we'll use the first one we can find to create our new drafting view.
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ViewFamilyType));
            ViewFamilyType viewFamilyType = collector.Cast<ViewFamilyType>().First(vft => vft.ViewFamily == ViewFamily.Drafting);

            // Create a new ViewDrafting instance
            ViewDrafting viewDrafting = ViewDrafting.Create(doc, viewFamilyType.Id);

            viewDrafting.Name = viewName;

            return viewDrafting;
        }
        //Поиск семейства
        public static Element FindElementByName(Document doc, Type targetType, string targetName)
        {
            return new FilteredElementCollector(doc)
              .OfClass(targetType)
              .FirstOrDefault<Element>(
                e => e.Name.Equals(targetName));
        }
        //Загрузка семейства
        public static Family LoadFamily(Document doc, string FamilyFolder, string FamilyName)
        {
            Family family;
            /// <summary>
            /// Расширение файла семейств - RFA.
            /// </summary>
            const string familyExtention = "rfa";

            /// <summary>
            /// Возвращает полный путь к файлу семейства
            /// </summary>
            string FamilyPath = Path.Combine(FamilyFolder, FamilyName);
            FamilyPath = Path.ChangeExtension(FamilyPath, familyExtention);

            // Проверим наличие файла семейства
            // перед загрузкой его в проект
            if (!File.Exists(FamilyPath))
            {
                return null;
            }

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Загрузка семейства");
                doc.LoadFamily(FamilyPath, out family);
                tx.Commit();
            }
            return family;
        }
        //Размещение семейства 
        public static FamilyInstance PlaceFamily(View view, string FamilyName, XYZ coords)
        {
            // Получаем семейство из активного документа по имени.
            Document doc = view.Document;
            //MessageBox.Show(FamilyName);

            FamilySymbol familySymbol = new FilteredElementCollector(doc)
                         .OfClass(typeof(FamilySymbol))
                         .Where(q => q.Name == FamilyName)
                         .First() as FamilySymbol;


            //Переводим координаты точки вставки из миллиметров в футы
            //XYZ coords = new XYZ(xCoord, 0, 0).ToFeets();

            //Вставляем семейство 
            FamilyInstance familyInstance = doc.Create.NewFamilyInstance(coords, familySymbol, view);
            return familyInstance;
        }
        //Назначение номеров помещений элементам
        public static void SetRoomNumbersToElements(Document doc)
        {
            FilteredElementCollector rooms;
            if (RoomNumbersAreInLink == null)
            {
                RoomNumbersAreInLink = false;
                FilteredElementCollector roomCollector = new FilteredElementCollector(doc);
                rooms = roomCollector.OfCategory(BuiltInCategory.OST_Rooms);


                if (rooms.Count() < 1)
                {
                    RoomNumbersAreInLink = true;
                    FilteredElementCollector linkInstanceCollector = new FilteredElementCollector(doc);
                    linkInstanceCollector.OfClass(typeof(RevitLinkInstance));
                    List<string> linkNames = new List<string>();
                    foreach (RevitLinkInstance linkInstance in linkInstanceCollector)
                    {
                        linkNames.Add(linkInstance.Name);
                    }
                    LinkForm linkForm = new LinkForm(linkNames);
                    linkForm.ShowDialog();
                    if (linkForm.Link == "")
                    {
                        return;
                    }

                    foreach (RevitLinkInstance linkInstance in linkInstanceCollector)
                    {
                        if (linkInstance.Name == linkForm.Link)
                        {
                            LinkID = linkInstance.Id;
                        }
                    }
                }


                if (LinkID != null)
                {
                    RevitLinkInstance linkAR = (RevitLinkInstance)doc.GetElement(LinkID);
                    Document linkDoc = linkAR.GetLinkDocument();
                    FilteredElementCollector linkCollector = new FilteredElementCollector(linkDoc);
                    rooms = linkCollector.OfCategory(BuiltInCategory.OST_Rooms);
                }

                if (rooms == null)
                {
                    return;
                }


                ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));

                ElementCategoryFilter lightingCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_LightingFixtures);
                ElementCategoryFilter ElFixturesCategoryfilter = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures);


                foreach (Room room in rooms)
                {
                    BoundingBoxXYZ roomBBox = room.get_BoundingBox(null);
                    //MessageBox.Show("Комната № "+ room.Number+" Bbox=" +roomBBox.Max.ToString());
                    Outline roomOutline = new Outline(roomBBox.Min, roomBBox.Max);
                    //MessageBox.Show("Комната № " + room.Number +  " Outline= " +roomOutline.MaximumPoint.ToString());
                    if (!roomOutline.IsEmpty)
                    {
                        BoundingBoxIsInsideFilter fixtureInsideFilter = new BoundingBoxIsInsideFilter(roomOutline);
                        BoundingBoxIntersectsFilter fixtureIntersectFilter = new BoundingBoxIntersectsFilter(roomOutline);


                        LogicalAndFilter lightingInstancesFilter = new LogicalAndFilter(familyInstanceFilter, lightingCategoryfilter);
                        LogicalAndFilter electricalInstancesFilter = new LogicalAndFilter(familyInstanceFilter, ElFixturesCategoryfilter);
                        LogicalOrFilter fixtureInstances = new LogicalOrFilter(lightingInstancesFilter, electricalInstancesFilter);

                        LogicalAndFilter fixtureInstancesInsideFilter = new LogicalAndFilter(fixtureInstances, fixtureInsideFilter);
                        LogicalAndFilter fixtureInstancesIntersectFilter = new LogicalAndFilter(fixtureInstances, fixtureIntersectFilter);

                        LogicalOrFilter FixtureInsideOrIntersectFilter = new LogicalOrFilter(fixtureInstancesIntersectFilter, fixtureInstancesInsideFilter);



                        FilteredElementCollector lcollector = new FilteredElementCollector(doc);
                        IList<Element> fixtures = lcollector.WherePasses(FixtureInsideOrIntersectFilter).ToElements();

                        Dictionary<string, string> fixNumRooms = new Dictionary<string, string>();


                        foreach (Element fixture in fixtures)
                        {

                            var fixtureRoomNumber = fixture.LookupParameter("Номер помещения");
                            if (fixtureRoomNumber != null)
                            {
                                fixtureRoomNumber.Set(room.Number);

                                string fixtureRoomNumberString = fixtureRoomNumber.AsString();

                                string fixtureCircuit = fixture.LookupParameter("Номер цепи").AsString();
                                if (fixtureCircuit != null)
                                {
                                    if (fixNumRooms.ContainsKey(fixtureCircuit))
                                    {
                                        if (!fixNumRooms[fixtureCircuit].Contains(fixtureRoomNumberString))
                                        {
                                            fixNumRooms[fixtureCircuit] += (", " + fixtureRoomNumberString);
                                        }
                                    }
                                    else
                                    {
                                        fixNumRooms.Add(fixtureCircuit, fixtureRoomNumberString);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }
        //Получение элементов ключевой спецификации с заданным именем
        public static FilteredElementCollector GetKeyScheduleElements(Document doc, string specName)
        {
            FilteredElementCollector viewcollector = new FilteredElementCollector(doc);
            var schedules = viewcollector.OfCategory(BuiltInCategory.OST_Schedules);

            Element keySchedule = null;
            foreach (Element schedule in schedules)
            {

                if (schedule.Name == specName)
                {
                    keySchedule = schedule;
                    break;
                }
            }

            if (keySchedule == null)
            {
                MessageBox.Show("Не найдена ключевая спецификация " + specName + ". Расчет по этому блоку проводиться не будет.");
                return null;
            }
            else
            {
                FilteredElementCollector keyElements = new FilteredElementCollector(doc, keySchedule.Id);
                return keyElements;
            }
        }
        //Загрузка нового общего параметра в проект
        public static bool SetNewParameterToInstanceFamily(UIApplication app, DefinitionFile myDefinitionFile, string parameterName, ParameterType parameterType, BuiltInCategory builtInCategory)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Ниже идет создания общего параметра
            // create a new group in the shared parameters file
            DefinitionGroups myGroups = myDefinitionFile.Groups;

            DefinitionGroup myGroup = myGroups.get_Item("12_Автоматизация");

            if (myGroup == null)
            {
                try
                {
                     myGroup = myGroups.Create("12_Автоматизация");
                }
                catch
                {
                    MessageBox.Show($"Ошибка при создании группы 12_Автоматизация в ФОП. Вероятно файл защищен от записи. Попробуйте указать локальный путь к ФОП.");
                    return false;
                }
            }

            //Получение или создание параметра
                Definition myDefinition = myGroup.Definitions.get_Item(parameterName);

            if (myDefinition == null)
            {
                
                // create an instance definition in definition group MyParameters
                ExternalDefinitionCreationOptions option = new ExternalDefinitionCreationOptions(parameterName, parameterType);
                // Don't let the user modify the value, only the API
                option.UserModifiable = true;
                // Set tooltip
                option.Description = "";

                try
                {
                    myDefinition = myGroup.Definitions.Create(option);
                }
               catch
                {
                    MessageBox.Show($"Ошибка при загрузке параметра {parameterName} в ФОП. Вероятно файл защищен от записи. Попробуйте указать локальный путь к ФОП.") ;
                    return false;
                }
            }


            //Ниже идет код для привязки параметра к конкретной категории семейств

            // create a category set and insert category of wall to it
            CategorySet myCategories = app.Application.Create.NewCategorySet();
            // use BuiltInCategory to get category of wall
            Category myCategory = Category.GetCategory(app.ActiveUIDocument.Document, builtInCategory);

            myCategories.Insert(myCategory);

            //Create an instance of InstanceBinding
            InstanceBinding instanceBinding = app.Application.Create.NewInstanceBinding(myCategories);

            // Get the BingdingMap of current document.
            BindingMap bindingMap = app.ActiveUIDocument.Document.ParameterBindings;

            // Bind the definitions to the document
            bool instanceBindOK = bindingMap.Insert(myDefinition,
                                            instanceBinding, BuiltInParameterGroup.PG_TEXT);

            return instanceBindOK;
        }
    }
}
