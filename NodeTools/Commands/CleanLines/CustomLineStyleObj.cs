using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.ComponentModel;
using NodeTools.Utility;

namespace NodeTools.Commands.CleanLines
{
    public class CustomLineStyleObj : IGenericComparer, INotifyPropertyChanged
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
        public ElementId EleId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
