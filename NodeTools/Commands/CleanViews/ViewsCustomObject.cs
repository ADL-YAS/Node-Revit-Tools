using NodeTools.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace NodeTools.Commands.CleanViews
{
    public class ViewsCustomObject : IGenericComparer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) =>
        {

        };

        public String Name { get; set; }
        public bool IsChecked { get; set; }

        public ElementId EleId { get; set; }


    }
}
