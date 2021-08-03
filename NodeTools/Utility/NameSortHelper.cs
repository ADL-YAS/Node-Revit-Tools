using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeTools.Utility
{
    class NameSortHelper : IComparer<IGenericComparer>
    {
        public int Compare(IGenericComparer x, IGenericComparer y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
