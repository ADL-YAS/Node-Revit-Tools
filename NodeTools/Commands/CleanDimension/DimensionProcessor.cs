using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace NodeTools.Commands.CleanDimension
{
    public class DimensionProcessor
    {
        private List<Dimension> _dimsUsed = new List<Dimension>();

        public List<Dimension> DimsUsed
        {
            get { return _dimsUsed; }
        }

        public DimensionProcessor(Document doc)
        {
            List<Dimension> linearUsed = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Dimensions)
                .WhereElementIsNotElementType().Cast<Dimension>().ToList();

            List<Dimension> spotCoordinatesUsed = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_SpotCoordinates)
                .WhereElementIsNotElementType().Cast<Dimension>().ToList();

            List<Dimension> spotSlopeUsed = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_SpotSlopes)
                .WhereElementIsNotElementType().Cast<Dimension>().ToList();

            List<Dimension> spotElevationUsed = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_SpotElevations)
                .WhereElementIsNotElementType().Cast<Dimension>().ToList();

            List<Dimension> legendDims = GetLegendDimension(doc);


            if (linearUsed != null)
            {
                _dimsUsed.AddRange(linearUsed);
            }
            if (spotCoordinatesUsed != null)
            {
                _dimsUsed.AddRange(spotCoordinatesUsed);
            }
            if (spotSlopeUsed != null)
            {
                _dimsUsed.AddRange(spotSlopeUsed);
            }
            if (spotElevationUsed != null)
            {
                _dimsUsed.AddRange(spotElevationUsed);
            }
            if(legendDims != null)
            {
                _dimsUsed.AddRange(legendDims);
            }
        }

        internal List<Dimension> GetLegendDimension(Document doc)
        {
            List<View> legends = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).Cast<View>().Where(x => x.ViewType == ViewType.Legend).ToList();

            List<Dimension> legendsDims = new List<Dimension>();
            foreach (View view in legends)
            {
               List<Dimension> dims =  new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Dimensions).Cast<Dimension>().ToList();
                if(dims != null)
                {
                    legendsDims.AddRange(dims);
                }
            }

            var test = legendsDims.Select(x => x.DimensionType.Name).ToList();

            return legendsDims;
        }
    }
}
