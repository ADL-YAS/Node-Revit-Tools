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

            List<DimTypeCustomObj> list = AllDocs_Dimtypes.Select(x => new DimTypeCustomObj() { Name = x.Name, FamilyName = x.FamilyName, EleId = x.Id , UniqueId = x.UniqueId}).ToList();
            list.Sort(new NameSortHelper());

            ShowUi(commandData,list);

            return Result.Succeeded;
        }

        public void ShowUi(ExternalCommandData commandata,List<DimTypeCustomObj> docsDimtype)
        {
            DimensionCleanUpUI ui = new DimensionCleanUpUI(commandata.Application.ActiveUIDocument.Document,docsDimtype, AllUsed_Dims);
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
            catch (Exception)
            {
                throw;
            }
        }

    }
}
