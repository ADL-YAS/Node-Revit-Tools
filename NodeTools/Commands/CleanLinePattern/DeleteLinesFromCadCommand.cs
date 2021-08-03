using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeTools.Commands.CleanLinePattern
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class DeleteLinesFromCadCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            try
            {
                int count = 0;
                StringBuilder sb = new StringBuilder();
                List<Element> toDelete = new List<Element>();
                List<Element> LinePatterns = new FilteredElementCollector(doc).OfClass(typeof(LinePatternElement)).ToList();
                foreach (Element item in LinePatterns)
                {
                    if(item.Name.StartsWith("IMPORT"))
                    {
                        toDelete.Add(item);
                        sb.Append(item.Name + "\n");
                        count += 1;
                    }
                }
                
                if(toDelete.Count() != 0)
                {
                    using (Transaction tr = new Transaction(doc, "Delete CadLines Import"))
                    {
                        tr.Start();

                        doc.Delete(toDelete.Select(x => x.Id).ToList());

                        if (tr.Commit() == TransactionStatus.Committed)
                        {
                            TaskDialog td = new TaskDialog("Info");
                            td.MainInstruction = $"Deleted {count} Cad lines";
                            td.MainContent = sb.ToString();
                            td.Show();
                        }

                    }
                }
                else
                {
                    TaskDialog.Show("Info", "There is no Cad lines found");
                    return Result.Cancelled;
                }
                
            }
            catch (Exception)
            {
                return Result.Failed;
                throw;
            }
           
            return Result.Succeeded;
        }
    }
}
