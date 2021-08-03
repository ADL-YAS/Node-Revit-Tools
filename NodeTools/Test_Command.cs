using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using NodeTools.Commands.CleanDimension;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace NodeTools
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class Test_Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            using (Transaction tr = new Transaction(doc, "Test"))
            {
                tr.Start();

                doc.Delete(new ElementId(455194));
                tr.Commit();
            }

            return Result.Succeeded;
        }
    }
}
