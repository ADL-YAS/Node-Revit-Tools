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
using NodeTools.Utility;

namespace NodeTools.Commands.CleanLines
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DeleteUnuseLineStyleCommand : IExternalCommand
    {

        List<ElementId> UsedIds = new List<ElementId>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            GetAllUsedLineStyles(doc);
            var list = GetUnusedLineStyles(doc);


            if (list.Count() != 0)
            {
                DeleteUnusedLineStyleUI ui = new DeleteUnusedLineStyleUI(doc, list);
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
            }
            else
            {
                TaskDialog.Show("Info", "No unused Line Style found");
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }

        public void GetAllUsedLineStyles(Document doc)
        {
            List<Element> lines = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Lines).ToList();

            foreach (Element line in lines)
            {
                View v = doc.GetElement(line.OwnerViewId) as View;
                if (v != null)
                {
                    if (line is DetailLine)
                    {
                        var type = line as DetailLine;
                        if (!UsedIds.Contains(type.LineStyle.Id))
                        {
                            UsedIds.Add(type.LineStyle.Id);
                        }
                    }
                    if (line is ModelLine)
                    {
                        var type = line as ModelLine;
                        if (!UsedIds.Contains(type.LineStyle.Id))
                        {
                            UsedIds.Add(type.LineStyle.Id);
                        }

                    }
                    if (line is DetailEllipse)
                    {
                        var type = line as DetailEllipse;
                        if (!UsedIds.Contains(type.LineStyle.Id))
                        {
                            UsedIds.Add(type.LineStyle.Id);
                        }
                    }
                    if (line is DetailArc)
                    {
                        var type = line as DetailArc;
                        if (!UsedIds.Contains(type.LineStyle.Id))
                        {
                            UsedIds.Add(type.LineStyle.Id);
                        }
                    }
                    if (line is DetailNurbSpline)
                    {
                        var type = line as DetailNurbSpline;
                        if (!UsedIds.Contains(type.LineStyle.Id))
                        {
                            UsedIds.Add(type.LineStyle.Id);
                        }
                    }
                }

            }
        }

        public List<CustomLineStyleObj> GetUnusedLineStyles(Document doc)
        {
            Category lineCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            List<Category> docLineStyles = lineCategory.SubCategories.Cast<Category>().ToList();

            var usedNames = UsedIds.Select(x => doc.GetElement(x).Name).ToList();

            #region Debugging purpose
            var all = docLineStyles.Select(x => $"{x.Name}  - {x.Id}").ToList();
            var used = UsedIds.Select(x => $"{doc.GetElement(x).Name}  - {doc.GetElement(x).Id}").ToList();
            #endregion
            List<CustomLineStyleObj> toDelete = new List<CustomLineStyleObj>();
            List<String> unused = new List<string>();
            foreach (Category category in docLineStyles)
            {
                if (!usedNames.Contains(category.Name))
                {
                    unused.Add(category.Name);
                    if (!category.Name.StartsWith("<") && !category.Name.EndsWith(">"))
                    {
                        toDelete.Add(new CustomLineStyleObj() { Name = category.Name, EleId = category.Id });
                    }
                }
            }

            toDelete.Sort(new NameSortHelper());


            return toDelete;
        }
    }
}
