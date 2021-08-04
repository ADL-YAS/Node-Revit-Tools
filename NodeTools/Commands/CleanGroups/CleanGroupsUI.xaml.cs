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

namespace NodeTools.Commands.CleanGroups
{
    /// <summary>
    /// Interaction logic for CleanGroupsUI.xaml
    /// </summary>
    public partial class CleanGroupsUI : Window
    {
        Document _doc;
        public ObservableCollection<GroupCustomObj> Groups { get; private set; }
        public CleanGroupsUI(Document doc, List<GroupType> gtypes)
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            _doc = doc;
            
           
            var list = gtypes.Select(x => new GroupCustomObj() { Name = x.Name, id = x.Id, Count = x.Groups.Size }).OrderBy(x=>x.Name).ToList();
            Groups = new ObservableCollection<GroupCustomObj>(list);
            GroupUi.ItemsSource = Groups;
        }
      
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var toDelete = Groups.Where(x => x.IsChecked == true).ToList();
            if (toDelete.Count != 0)
            {
                Delete(toDelete);
            }
            else
            {
                TaskDialog.Show("Info", "No item selected");
            }
           
        }

        private void Unused_Clicked(object sender, RoutedEventArgs e)
        {
            var toDelete = Groups.Where(x => x.Count == 0).ToList();
            if(toDelete.Count != 0)
            {
                Delete(toDelete);
            }
            else
            {
                TaskDialog.Show("Info", "No unused found");
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
                        Groups.Remove(item);
                    }
                }
                catch
                {

                }
                tr.Commit();
                TaskDialog.Show("Info", $"{objs.Count} items deleted");
                if(Groups.Count == 0)
                {
                    this.Close();
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #region Key Press Event handlers
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        #endregion

    }

    public class GroupCustomObj : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ElementId id { get; set; }
        public string  Name { get; set; }
        public bool IsChecked { get; set; }
        public int Count { get; set; }

    }
}
