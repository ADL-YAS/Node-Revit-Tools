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

namespace NodeTools.Commands.CleanLines
{
    /// <summary>
    /// Interaction logic for DeleteUnusedLineStyleUI.xaml
    /// </summary>
    public partial class DeleteUnusedLineStyleUI : Window
    {
        internal List<CustomLineStyleObj> customLineStyleObjs { get; private set; }
        internal Document _doc { get; private set; }
        public DeleteUnusedLineStyleUI(Document doc, List<CustomLineStyleObj> toDelete)
        {

            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            customLineStyleObjs = toDelete;
            _doc = doc;
            UILineStyle.ItemsSource = customLineStyleObjs;

        }


        #region Key Press Event handlers
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        #endregion


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var toDelete = customLineStyleObjs.Where(x => x.IsChecked == true).Select(i => i.EleId).ToList();

            if(toDelete != null)
            {
                try
                {
                    TaskDialogResult result = TaskDialog.Show("Info", "Would you like to proceed?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.Cancel, TaskDialogResult.Cancel);

                    if (result == TaskDialogResult.Yes)
                    {
                        using (Transaction tr = new Transaction(_doc, "Delete unused Line Styles"))
                        {
                            tr.Start();
                            _doc.Delete(toDelete);
                            tr.Commit();
                            DialogResult = true;
                            this.Close();
                        }
                        TaskDialog.Show("Result", $"{toDelete.Count()} Items deleted");
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

        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CustomLineStyleObj item in customLineStyleObjs)
            {
                item.IsChecked = true;
            }

        }

        private void UnChecked_Click(object sender, RoutedEventArgs e)
        {
            foreach (CustomLineStyleObj item in customLineStyleObjs)
            {
                item.IsChecked = false;
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
