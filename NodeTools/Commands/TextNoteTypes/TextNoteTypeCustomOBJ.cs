using Autodesk.Revit.DB;
using NodeTools.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeTools.Commands.TexnoteTypes
{
    
    public class TextNoteTypeCustomOBJ : IGenericComparer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) =>
        {

        };

        public String Name { get; set; }
        public string UniqueId { get; set; }
        public string FamilyName { get; set; }

        public ElementId EleId { get; set; }


    }
}
