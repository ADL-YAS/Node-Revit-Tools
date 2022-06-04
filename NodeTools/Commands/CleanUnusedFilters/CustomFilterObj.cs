using Autodesk.Revit.DB;
using NodeTools.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeTools.Commands.CleanUnusedFilters
{
   
    public class CustomFilterObj : IGenericComparer, INotifyPropertyChanged
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
        public ElementId EleId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
