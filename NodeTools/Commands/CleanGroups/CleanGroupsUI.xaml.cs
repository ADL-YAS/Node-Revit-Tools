using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace NodeTools.Commands.CleanGroups
{
    /// <summary>
    /// Interaction logic for CleanGroupsUI.xaml
    /// </summary>
    public partial class CleanGroupsUI : Window
    {
        Document _doc;
        public ObservableCollection<GroupCustomObj> DetailGroups { get; private set; }
        public ObservableCollection<GroupCustomObj> ModelGroups { get; private set; }
        public CleanGroupsUI(Document doc, List<GroupType> gtypes)
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            _doc = doc;
            
           
            var dlist = gtypes.Where(g=>g.FamilyName == "Detail Group").Select(x => new GroupCustomObj() { Name = x.Name, id = x.Id, Count = x.Groups.Size, FamilyName = x.FamilyName }).OrderBy(x=>x.Name).ToList();
            var mlist = gtypes.Where(g => g.FamilyName == "Model Group").Select(x => new GroupCustomObj() { Name = x.Name, id = x.Id, Count = x.Groups.Size, FamilyName = x.FamilyName }).OrderBy(x => x.Name).ToList();
            DetailGroups = new ObservableCollection<GroupCustomObj>(dlist);
            ModelGroups = new ObservableCollection<GroupCustomObj>(mlist);


            if (DetailGroups.Count == 0)
            {
                Detail_Tab.IsEnabled = false;
            }
            else
            {
                DetailGroupUI.ItemsSource = DetailGroups;
            }
            if (ModelGroups.Count == 0)
            {
                Models_Tab.IsEnabled = false;
            }
            else
            {
                ModelGroupUI.ItemsSource = ModelGroups;
            }
        }
      
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            List<GroupCustomObj> list = new List<GroupCustomObj>();
            var detailtoDelete = DetailGroups.Where(x => x.IsChecked == true).ToList();
            var modeltoDelete = ModelGroups.Where(x => x.IsChecked == true).ToList();
            if (detailtoDelete.Count != 0)
            {
                list.AddRange(detailtoDelete);
            }
            if(modeltoDelete.Count != 0)
            {
                list.AddRange(modeltoDelete);
            }
            if(list.Count != 0)
            {
                Delete(list);
            }
            else
            {
                TaskDialog.Show("Info", "No items selected");
            }
           
        }

        private void Unused_Clicked(object sender, RoutedEventArgs e)
        {
            List<GroupCustomObj> list = new List<GroupCustomObj>();
            var modeltoDelete = ModelGroups.Where(x => x.Count == 0).ToList();
            var detailtoDelete = DetailGroups.Where(x => x.Count == 0).ToList();
            if (detailtoDelete.Count != 0)
            {
                list.AddRange(detailtoDelete);
            }
            if (modeltoDelete.Count != 0)
            {
                list.AddRange(modeltoDelete);
            }
            if (list.Count != 0)
            {
                Delete(list);
            }
            else
            {
                TaskDialog.Show("Info", "No unused groups");
            }


        }
        void Delete(List<GroupCustomObj> objs)
        {
            using(Transaction tr = new Transaction(_doc,"Delete Groups"))
            {
                tr.Start();
                try
                {
                    foreach (var item in objs)
                    {
                        _doc.Delete(item.id);
                        if(item.FamilyName == "Detail Group")
                        {
                            DetailGroups.Remove(item);
                        }
                        else
                        {
                            ModelGroups.Remove(item);
                        }
                    }
                }
                catch
                {

                }
                tr.Commit();
                TaskDialog.Show("Info", $"{objs.Count} items deleted");
                if (DetailGroups.Count == 0 && ModelGroups.Count == 0)
                {
                    AnimateClose();
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            AnimateClose();
        }
        #region Key Press Event handlers
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                AnimateClose();
        }

        #endregion

        private void Window_Activated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => WindowStyle = WindowStyle.None));
        }
        private void AnimateClose()
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            this.Close();
        }
    }

    public class GroupCustomObj : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ElementId id { get; set; }
        public string FamilyName { get; set; }
        public string  Name { get; set; }
        public bool IsChecked { get; set; }
        public int Count { get; set; }

    }
}
