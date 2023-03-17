using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CheckMirrored
{
    [Transaction(TransactionMode.Manual)]

    public class CheckMirrored : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            int countD = 0;
            int countW = 0;
            int mirrorsD = 0;
            int mirrorsW = 0;
            Document doc = commandData.Application.ActiveUIDocument.Document;

            //все двери
            var doorsType = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();
            //все окна
            var windowsType = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            //семейство
            FamilySymbol familySymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("Сфера"))
                .FirstOrDefault();

            if (familySymbol == null)
            {
                TaskDialog.Show("Ошибка", "Семейство \"Сфера\" Не найдено");
                return Result.Cancelled;
            }

            Transaction transaction = new Transaction(doc);
            transaction.Start("Расстановка Сфер у зеркальных элементов");
            foreach (Element element in doorsType)
            {
                FamilyInstance doorFI = element as FamilyInstance;
                if (doorFI.Mirrored)
                {
                    LocationPoint doorLocationPoint = element.Location as LocationPoint;
                    XYZ doorLoc = doorLocationPoint.Point;
                    Level level = doc.GetElement(element.LevelId) as Level;

                    FamilyInstance tega = doc.Create.NewFamilyInstance(doorLoc
                        , familySymbol
                        , element
                        , level
                        , StructuralType.NonStructural);


                    mirrorsD += 1;
                    countD += 1;
                }
                else
                {
                    countD += 1;
                }
            }
            foreach (Element element in windowsType)
            {
                FamilyInstance windowFI = element as FamilyInstance;
                if (windowFI.Mirrored)
                {
                    LocationPoint windowLocationPoint = element.Location as LocationPoint;
                    XYZ windowLoc = windowLocationPoint.Point;
                    Level levelWindow = doc.GetElement(element.LevelId) as Level;

                    FamilyInstance tega = doc.Create.NewFamilyInstance(windowLoc
                        , familySymbol
                        , element
                        , levelWindow
                        , StructuralType.NonStructural);


                    mirrorsW += 1;
                    countW += 1;
                }
                else
                {
                    countW += 1;
                }
            }

            transaction.Commit();

            TaskDialog.Show("info", $"Всего обработал дверей: {countD}, из них зеркальных: {mirrorsD}");
            TaskDialog.Show("info", $"Всего обработал окон: {countW}, из них зеркальных: {mirrorsW}");

            return Result.Succeeded;
        }
    }
}
