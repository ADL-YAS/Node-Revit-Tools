using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NodeTools.Commands.CleanViews
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class DeleteUnusedViewsCommand : IExternalCommand
    {
        DeletUnusedViewUI ui;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                ui = new DeletUnusedViewUI(doc);
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
