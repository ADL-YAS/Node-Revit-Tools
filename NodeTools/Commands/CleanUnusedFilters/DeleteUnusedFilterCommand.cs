using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using System.Windows.Interop;
using System.Windows;

namespace NodeTools.Commands.CleanUnusedFilters
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class DeleteUnusedFilterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            CleanUnusedFilterUI ui = new CleanUnusedFilterUI(doc);
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
                return Result.Failed;
                throw;
            }
            return Result.Succeeded;
        }
    }
}
