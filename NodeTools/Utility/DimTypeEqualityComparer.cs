using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeTools.Utility
{
    class DimTypeEqualityComparer : IEqualityComparer<DimensionType>
    {
        public bool Equals(DimensionType x, DimensionType y)
        {
            if(x.Id == y.Id && x.Name == y.Name)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(DimensionType obj)
        {
            return obj.GetHashCode();
        }
    }
}
