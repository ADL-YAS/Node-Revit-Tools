using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NodeTools.Utility;
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
using System.Windows.Threading;

namespace NodeTools.Commands.CleanUnusedFilters
{
    /// <summary>
    /// Interaction logic for CleanUnusedFilterUI.xaml
    /// </summary>
    public partial class CleanUnusedFilterUI : Window
    {
        internal List<CustomFilterObj> CustomFilterObjs { get; private set; }
        public CleanUnusedFilterUI(Document doc)
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);

            if (true)
            {
                TaskDialog.Show("test", "testing");
                this.Close();
            }
        }



        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //var toDelete = customLineStyleObjs.Where(x => x.IsChecked == true).Select(i => i.EleId).ToList();

            //if (toDelete != null)
            //{
            //    try
            //    {
            //        TaskDialogResult result = TaskDialog.Show("Info", "Would you like to proceed?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.Cancel, TaskDialogResult.Cancel);

            //        if (result == TaskDialogResult.Yes)
            //        {
            //            using (Transaction tr = new Transaction(_doc, "Delete unused Line Styles"))
            //            {
            //                tr.Start();
            //                _doc.Delete(toDelete);
            //                tr.Commit();
            //                DialogResult = true;
            //            }
            //            TaskDialog.Show("Result", $"{toDelete.Count()} Items deleted");
            //            AnimateClose();
            //        }
            //        else
            //        {
            //            return;
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        throw;
            //    }
            //}

        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CustomFilterObj item in CustomFilterObjs)
            {
                item.IsChecked = true;
            }

        }
        private void UnChecked_Click(object sender, RoutedEventArgs e)
        {
            foreach (CustomFilterObj item in CustomFilterObjs)
            {
                item.IsChecked = false;
            }
        }


        #region UI Functions
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                AnimateClose();
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            AnimateClose();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            AnimateClose();
        }

        private void AnimateClose()
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            this.Close();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => WindowStyle = WindowStyle.None));
        }
        #endregion

    }
}
