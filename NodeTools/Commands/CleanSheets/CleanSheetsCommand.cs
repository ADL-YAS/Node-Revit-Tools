using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NodeTools.Utility;

namespace NodeTools.Commands.CleanSheets
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class CleanSheetsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            View activeV = doc.ActiveView;
            if (activeV.ViewType == ViewType.DrawingSheet)
            {
                TaskDialog.Show("Result", "Active view is a Drawing Sheet, Please close to proceed");
                return Result.Failed;
            }

            ICollection<Element> collection = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).ToElements();
            //ICollection<Element> collection = null;
            if (collection != null)
            {
                
                if (collection.Count > 0)
                {
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Delete All Sheets");

                        int colCount = 0;
                        foreach (Element elem in collection)
                        {
                            try
                            {
                                doc.Delete(elem.Id);
                                colCount = colCount + 1;
                            }
                            catch { }
                        }

                        TaskDialog.Show("Delete All Sheets", colCount.ToString() + " sheets were deleted.");
                        tx.Commit();
                    }
                    return Result.Succeeded;
                }
                else
                {
                    TaskDialog.Show("Result", "No sheets found in this Document");
                    return Result.Failed;
                }
            }
            else
            {
                TaskDialog.Show("Result", "An error occur");
                return Result.Failed;
            }
           
        }
         
    }
}
