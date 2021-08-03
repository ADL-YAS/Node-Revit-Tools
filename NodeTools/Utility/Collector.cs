using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeTools.Utility
{
    class Collector
    {
        public static List<Element> GetElementOfCategory(Document doc, BuiltInCategory cat)
        {
            return new FilteredElementCollector(doc).OfCategory(cat).ToElements().ToList();
        }
    }
}
