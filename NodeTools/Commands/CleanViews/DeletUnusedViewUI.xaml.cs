using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NodeTools.Utility;

namespace NodeTools.Commands.CleanViews
{
    /// <summary>
    /// Interaction logic for DeletUnusedViewUI.xaml
    /// </summary>
    public partial class DeletUnusedViewUI : Window
    {
        //instance
        ViewSheetProcessor sheetProcessor;
        Document _doc;
        #region all placed
        public List<View> AllPlacedViews { get; private set; }
        public List<ViewSchedule> AllPlacedSchedule { get; private set; }
        #endregion

        #region AllViews
        public List<View> AllDocsViews { get; private set; }
        public List<ViewSchedule> AllDocsViewSchedule { get; private set; }
        public List<View> AllParentViews = new List<View>();
        #endregion

        #region AllUnused
        public List<ViewsCustomObject> AllUnUsedViews = new List<ViewsCustomObject>();
        public List<ViewsCustomObject> AllUnusedSchedules = new List<ViewsCustomObject>();
        public List<ViewsCustomObject> AllUnusedLegends = new List<ViewsCustomObject>();
        #endregion


        public DeletUnusedViewUI(Document doc)
        {
            #region Initialized components and events
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            _doc = doc;
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            #endregion

            //execute sheets collector
            sheetProcessor = new ViewSheetProcessor(doc);
            AllPlacedViews = sheetProcessor.AllPlacedViewIds.Select(x => doc.GetElement(x) as View).ToList();
            AllPlacedSchedule = sheetProcessor.AllPlacedScheduleIds.Select(x => doc.GetElement(x) as ViewSchedule).Where(x => !x.IsTitleblockRevisionSchedule && !x.IsTemplate).ToList();

            //get all document views
            AllDocsViews = Collector.GetElementOfCategory(doc, BuiltInCategory.OST_Views)
                .Cast<View>().Where(x => !x.IsTemplate).Select(x => x as View).ToList();
            //get all document schedule
            AllDocsViewSchedule = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule)).Cast<ViewSchedule>()
                .Where(x => !x.IsTitleblockRevisionSchedule && !x.IsTemplate).ToList();


            CallFunctions();

            if (AllUnusedLegends.Count == 0 && AllUnusedSchedules.Count == 0 && AllUnUsedViews.Count == 0)
            {
                this.Close();
                TaskDialog.Show("Info", "No unused Views in this document");
            }

            #region For Debugging
            //var allViews = Utils.CheckListNames(AllDocsViews);
            //var allScheds = Utils.CheckListNames(AllDocsViewSchedule);
            ////var allPlcedViews = Utils.CheckListNames(AllPlacedViews);
            //var allPlcedScheds = Utils.CheckListNames(AllPlacedSchedule);

            //var uV = AllUnUsedViews.Select(x => x.Name);
            //var uS = AllUnusedSchedules.Select(x => x.Name);
            //var uL = AllUnusedLegends.Select(x => x.Name);
            #endregion



            #region Set all datasource
            if (AllUnUsedViews.Count == 0)
            {
                Views_Tab.IsEnabled = false;
            }
            else
            {
                AllUnUsedViews.Sort(new NameSortHelper());
                UIListViews.ItemsSource = AllUnUsedViews;
            }
            if (AllUnusedSchedules.Count == 0)
            {
                Schedule_Tab.IsEnabled = false;
            }
            else
            {
                AllUnusedSchedules.Sort(new NameSortHelper());
                UIScheduleViews.ItemsSource = AllUnusedSchedules;
            }
            if (AllUnusedLegends.Count == 0)
            {
                Legend_Tab.IsEnabled = false;
            }
            else
            {
                AllUnusedSchedules.Sort(new NameSortHelper());
                UILegendViews.ItemsSource = AllUnusedLegends;
            }
            #endregion

        }


        private void CallFunctions()
        {
            GetUnusedViews();
            GetUnusedlegend();
            GetUnusedSchedule();
            CheckParentDependentView();
        }

        #region Function for initialization
        private void GetUnusedlegend()
        {
            foreach (View v in AllDocsViews)
            {
                if (!AllPlacedViews.Select(x => x.Id).Contains(v.Id))
                {
                    if (v.ViewType == ViewType.Legend)
                    {
                        AllUnusedLegends.Add(new ViewsCustomObject() { Name = v.Name, EleId = v.Id, });
                    }
                }
            }

        }
        private void GetUnusedViews()
        {
            foreach (View v in AllDocsViews.Where(x => x.ViewType != ViewType.Legend))
            {
                if (!AllPlacedViews.Select(x => x.Id).Contains(v.Id))
                {
                    //check if no dependent views
                    if (v.GetDependentViewIds().Count == 0)
                    {
                        AllUnUsedViews.Add(new ViewsCustomObject() { Name = v.Name, EleId = v.Id });
                    }
                    else
                    {
                        AllParentViews.Add(v);
                    }
                }
            }
        }

        /// <summary>
        /// Get unused views that don't have dependent
        /// </summary>
        private void CheckParentDependentView()
        {
            bool isPlace = false;
            foreach (View parent in AllParentViews)
            {
                //check if one of dependent is in sheet, if true parent will not be deleted
                foreach (ElementId id in parent.GetDependentViewIds())
                {
                    View v = _doc.GetElement(id) as View;
                    if (!string.IsNullOrEmpty(v.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NAME).AsString()) || !string.IsNullOrEmpty(v.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsString()))
                    {
                        isPlace = true;
                    }
                }
                if (!isPlace)
                {
                    AllUnUsedViews.Add(new ViewsCustomObject() { Name = parent.Name, EleId = parent.Id });
                }
            }
        }
        private void GetUnusedSchedule()
        {
            foreach (View v in AllDocsViewSchedule)
            {
                if (!AllPlacedSchedule.Select(x => x.Id).Contains(v.Id))
                {
                    AllUnusedSchedules.Add(new ViewsCustomObject() { Name = v.Name, EleId = v.Id });
                }
            }

        }
        #endregion





        #region Key Press Event handlers
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            List<ElementId> unusedViews = AllUnUsedViews.Where(x => x.IsChecked == true && _doc.ActiveView.Id != x.EleId).Select(v => v.EleId).ToList();
            List<ElementId> unusedScheds = AllUnusedSchedules.Where(x => x.IsChecked == true && _doc.ActiveView.Id != x.EleId).Select(v => v.EleId).ToList();
            List<ElementId> unusedLegends = AllUnusedLegends.Where(x => x.IsChecked == true && _doc.ActiveView.Id != x.EleId).Select(v => v.EleId).ToList();

            List<ElementId> toDeleteIds = new List<ElementId>();
            if (unusedViews != null)
            {
                toDeleteIds.AddRange(unusedViews);
            }
            if (unusedScheds != null)
            {
                toDeleteIds.AddRange(unusedScheds);
            }
            if (unusedLegends != null)
            {
                toDeleteIds.AddRange(unusedLegends);
            }
            if(toDeleteIds.Count() != 0)
            {
                try
                {
                    TaskDialogResult result = TaskDialog.Show("Info", "Would you like to proceed?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.Cancel, TaskDialogResult.Cancel);

                    if (result == TaskDialogResult.Yes)
                    {
                        using (Transaction tr = new Transaction(_doc, "Delete Unused Views"))
                        {
                            tr.Start();
                            _doc.Delete(toDeleteIds);

                            if (tr.Commit() == TransactionStatus.Committed)
                            {
                                DialogResult = true;
                                this.Close();
                                var format = toDeleteIds.Count() == 1 ? "View" : "Views";
                                TaskDialog.Show("Result", $"{toDeleteIds.Count()} {format} deleted");
                            }
                        }
                    }
                    else
                    {
                        return;
                    }

                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                TaskDialog.Show("Result", "No item selected");
                this.Close();
            }
            

        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            if (Views_Tab.IsSelected)
            {
                foreach (ViewsCustomObject item in AllUnUsedViews)
                {
                    item.IsChecked = true;
                }
            }
            if (Schedule_Tab.IsSelected)
            {
                foreach (ViewsCustomObject item in AllUnusedSchedules)
                {
                    item.IsChecked = true;
                }
            }
            if (Legend_Tab.IsSelected)
            {
                foreach (ViewsCustomObject item in AllUnusedLegends)
                {
                    item.IsChecked = true;
                }
            }

        }

        private void UnChecked_Click(object sender, RoutedEventArgs e)
        {
            if (Views_Tab.IsSelected)
            {
                foreach (ViewsCustomObject item in AllUnUsedViews)
                {
                    item.IsChecked = false;
                }
            }
            if (Schedule_Tab.IsSelected)
            {
                foreach (ViewsCustomObject item in AllUnusedSchedules)
                {
                    item.IsChecked = false;
                }
            }
            if (Legend_Tab.IsSelected)
            {
                foreach (ViewsCustomObject item in AllUnusedLegends)
                {
                    item.IsChecked = false;
                }
            }
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
