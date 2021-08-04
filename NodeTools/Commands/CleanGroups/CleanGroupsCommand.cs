using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NodeTools.Commands.CleanGroups
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class CleanGroupsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<GroupType> gtypes = new FilteredElementCollector(doc).OfClass(typeof(GroupType)).Cast<GroupType>().ToList();
            if (gtypes.Count == 0)
            {
                TaskDialog.Show("Info", "No groups found");
                return Result.Cancelled;
            }
            var ui = new CleanGroupsUI(doc, gtypes);
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
