using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace NodeTools.Commands.CleanDimension
{
    public class DimensionTypeProcessor
    {
        private Document _doc;
        private List<DimensionType> _dimTypes;

        public List<DimensionType> DocDimTypes
        {
            get
            {
                return _dimTypes;
            }
        }

        public DimensionTypeProcessor(Document doc)
        {
            _doc = doc;
            _dimTypes = new FilteredElementCollector(_doc).OfClass(typeof(DimensionType))
                    .Cast<DimensionType>().Where(x => x.GetSimilarTypes().Count > 0).ToList();
        }
    }
}
