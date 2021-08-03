using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace NodeTools.Utility
{
    class Utils
    {
        public static Dictionary<string, string> CheckDimTypesInstances(List<DimensionType> UsedDims, List<Dimension> dims)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            foreach (DimensionType item in UsedDims)
            {
                var c = dims.Where(x => x.DimensionType.Id == item.Id).Count().ToString();

                keyValuePairs[$" NAME = {item.Name}"] = $"----COUNT = {c}";
            }
            return keyValuePairs;

        }

        public static List<ElementId> CheckListIds<T>(List<T> list) where T : Element
        {
            return list.Select(x => x.Id).ToList();
        }
    }




    public static class Extensions
    {
        public static string RemoveNonAsCII(this String str)
        {
            return Regex.Replace(str, @"\p{Cc}+", string.Empty);
        }
    }
}
