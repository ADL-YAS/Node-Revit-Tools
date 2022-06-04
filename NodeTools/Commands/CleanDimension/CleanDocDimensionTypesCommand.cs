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

namespace NodeTools.Commands.CleanDimension
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class CleanDocDimensionTypesCommand : IExternalCommand
    {
        Document doc;
        List<DimensionType> AllDocs_Dimtypes = new List<DimensionType>();
        List<Dimension> AllUsed_Dims = new List<Dimension>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            AllUsed_Dims = new DimensionProcessor(doc).DimsUsed;
            AllDocs_Dimtypes = new DimensionTypeProcessor(doc).DocDimTypes;


            //get all dimension types in docs and 
         

            ShowUi(commandData);

            return Result.Succeeded;
        }


        //showing UI
        public void ShowUi(ExternalCommandData commandata)
        {
            DimensionCleanUpUI ui = new DimensionCleanUpUI(commandata.Application.ActiveUIDocument.Document, AllDocs_Dimtypes, AllUsed_Dims);
            try
            {
                HwndSource hwnd = HwndSource.FromHwnd(commandata.Application.MainWindowHandle);
                Window wnd = hwnd.RootVisual as Window;
                if (wnd != null)
                {
                    ui.Owner = wnd;
                    ui.ShowInTaskbar = false;
                    ui.ShowDialog();
                }

            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
            }
        }

    }
}
