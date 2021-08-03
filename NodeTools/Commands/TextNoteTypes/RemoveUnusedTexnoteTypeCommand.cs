using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NodeTools.Utility;

namespace NodeTools.Commands.TextNoteTypes
{
    
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class RemoveUnusedTexnoteTypeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;



            List<TextNoteType> textNoteTypes = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).Cast<TextNoteType>().ToList();

            List<TextNote> textNotes = new FilteredElementCollector(doc).OfClass(typeof(TextNote)).Cast<TextNote>().ToList();

            CleanTextNoteTypesUI ui = new CleanTextNoteTypesUI(doc,textNoteTypes,textNotes);
            try
            {
                HwndSource hwnd = HwndSource.FromHwnd(commandData.Application.MainWindowHandle);
                Window wnd = hwnd.RootVisual as Window;
                if (wnd != null)
                {
                    ui.Owner = wnd;
                    ui.ShowInTaskbar = false;
                    ui.ShowDialog();
                }

            }
            catch (Exception)
            {
                throw;
            }

            return Result.Succeeded;
        }
    }
}
