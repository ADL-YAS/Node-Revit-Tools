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
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace NodeTools.Commands.CleanDimension
{
    /// <summary>
    /// Interaction logic for DimensionCleanUpUI.xaml
    /// </summary>
    public partial class DimensionCleanUpUI : Window
    {

        string storageName = "Node_DimensionType_Storage";
        string fieldName = "Node_DimensionType_Types";
        string GUID = "9c7c23bb-df6f-4bde-999d-42728dcce8ec";

        internal List<DimTypeCustomObj> DocDimtype { get; private set; }
        List<Dimension> AllUsed_Dims = new List<Dimension>();
        List<DimTypeCustomObj> AllUsed_DimType = new List<DimTypeCustomObj>();
        ObservableCollection<DimTypeCustomObj> NodeSelectedDimTypes = new ObservableCollection<DimTypeCustomObj>();

        internal Document _doc { get; private set; }

        public DimensionCleanUpUI(Document doc,List<DimTypeCustomObj> docDimtypes,List<Dimension> dims)
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            _doc = doc;
            DocDimtype = docDimtypes;
            AllUsed_Dims = dims;

            InitilizeData();
            
        }

        internal void InitilizeData()
        {
            List<ElementId> dimTypeIds = AllUsed_Dims.GroupBy(dim => dim.DimensionType.Id).Select(g => g.Key).ToList();
            foreach (ElementId item in dimTypeIds)
            {
                var dtype = _doc.GetElement(item) as DimensionType;
                if(dtype != null)
                {
                    AllUsed_DimType.Add(new DimTypeCustomObj() { Name = dtype.Name, EleId = dtype.Id, FamilyName = dtype.FamilyName });
                }
            }
            AllUsed_DimType.Sort(new NameSortHelper());


            //find settings if there is CACHE uniqids to assign to node dimtypes
            UIDimTypes.ItemsSource = DocDimtype;

            Dictionary<string,string> dict = DataStorageGenerator.GetDataFromStorage<Dictionary<string, string>>(_doc, GUID, fieldName, storageName);
            if(dict != null)
            {
                foreach (var item in dict)
                {
                    DimTypeCustomObj obj = DocDimtype.Where(x => x.UniqueId == item.Value).FirstOrDefault();
                    if(obj != null)
                    {
                        NodeSelectedDimTypes.Add(new DimTypeCustomObj() { Name = obj.Name, EleId = obj.EleId, FamilyName = obj.FamilyName, UniqueId = obj.UniqueId });
                    }
                }
                NodeDimTypes.ItemsSource = NodeSelectedDimTypes;
            }
            else
            {
                NodeDimTypes.ItemsSource = NodeSelectedDimTypes;
            }
           
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {

            foreach (DimTypeCustomObj item in UIDimTypes.SelectedItems)
            {
                if(!NodeSelectedDimTypes.Select(x=>x.Name).Contains(item.Name) && HasNonPrintableChar(item.Name))
                {
                    NodeSelectedDimTypes.Add(item);
                }
                else
                {
                    return;
                }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            List<DimTypeCustomObj> items = NodeDimTypes.SelectedItems.Cast<DimTypeCustomObj>().ToList();
            foreach (DimTypeCustomObj obj in items)
            {
                NodeSelectedDimTypes.Remove(obj);
            }
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            AnimateClose();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            AnimateClose();
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                AnimateClose();
        }


        void WriteTofile(string[] array, string name)
        {
            // Filename  
            string fileName = $"{name}.txt";
            // Fullpath. You can direct hardcode it if you like.  
            string fullPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"NodeTools\");

            string[] info = array;

            try
            {
                if (Directory.Exists(fullPath))
                {
                    File.WriteAllLines(fullPath + fileName, info);
                }
                else
                {
                    Directory.CreateDirectory(fullPath);
                    File.WriteAllLines(fullPath + fileName, info);
                }
            }
            catch (Exception)
            {
                throw;
            }

            // Read a file  
            //string readText = File.ReadAllText(fullPath);
            //Console.WriteLine(readText);
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {

            AnimateClose();
            if (NodeSelectedDimTypes.Count() != 0)
            {
                using(Transaction tr = new Transaction(_doc,"Node Replace DimensionType"))
                {
                    tr.Start();

                    ReplaceDimType(NodeSelectedDimTypes.ToList());

                    DeleteUnused();

                    AddDataToStorage(NodeSelectedDimTypes.ToList());
                    tr.Commit();
                   
                }
            }
            else
            {
                TaskDialog.Show("Info", "No selected Dimension Type found");
            }
        }

        public void ReplaceDimType(List<DimTypeCustomObj> selectedDimtype)
        {
            foreach (DimTypeCustomObj obj in selectedDimtype)
            {
                DimensionType dtyp = _doc.GetElement(obj.EleId) as DimensionType;
                if(dtyp != null)
                {
                    var text = dtyp.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
                    var usedUnit = dtyp.GetUnitsFormatOptions();

                    if (usedUnit.UseDefault == false)
                    {
                        string unitType = usedUnit.GetUnitTypeId().TypeId;
                        List<Dimension> allDimsOfFamilyName = AllUsed_Dims.Where(x => x.DimensionType.FamilyName == dtyp.FamilyName
                                                           && x.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString() == text
                                                           && x.DimensionType.GetUnitsFormatOptions().UseDefault == false)
                                                           .Where(d => d.DimensionType.GetUnitsFormatOptions().GetUnitTypeId().TypeId == unitType).ToList();

                        foreach (Dimension dimension in allDimsOfFamilyName)
                        {
                            var t = dimension.DimensionType.GetUnitsFormatOptions().GetUnitTypeId().TypeId;
                            var x = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
                            try
                            {
                                dimension.DimensionType = dtyp;
                            }
                            catch (Exception)
                            {
                                //TaskDialog.Show("TEST", $"{dimension.DimensionType.Id} == {dimension.DimensionType.Name}");
                            }
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Info", $"{dtyp.Name} uses default unit format and will not be processed against dimension instances");
                        continue;
                    }
                }
            }
        }

        public void DeleteUnused()
        {
            List<DimTypeCustomObj> toDelete = new List<DimTypeCustomObj>();
            var familyGroup = NodeSelectedDimTypes.GroupBy(x => x.FamilyName);
            foreach (var item in familyGroup)
            {
                if(item.Key == "Diameter Dimension Style")
                {
                    continue;
                }
                //cast the selected dimtype items to list
                var list = item.Select(x => x).ToList();
                //get all same familyname in document dimtypes
                List<DimTypeCustomObj> dimTypeCustomObjs = DocDimtype.Where(x => x.FamilyName == item.Key).ToList();
                //get all items in the list of document dimtypes that are not in selected items
                var unused = dimTypeCustomObjs.Where(d => list.All(i => i.EleId != d.EleId)).ToList();
                toDelete.AddRange(unused);
            }
            try
            {
                _doc.Delete(toDelete.Select(x => x.EleId).ToList());
            }
            catch (Exception)
            {

            }
        }


        public bool HasNonPrintableChar(string name)
        {
            string oldName = name;
            string newName = Regex.Replace(oldName, @"\p{C}+", string.Empty);
            if(name.Count() != newName.Count())
            {
                return false;
            }
            return true;
        }


        public void AddDataToStorage(List<DimTypeCustomObj> dimTypeCustomObjs)
        {
            Dictionary<string, string> dict;
            try
            {
                dict = dimTypeCustomObjs.ToDictionary(k => k.Name, v => v.UniqueId);
            }
            catch (Exception e)
            {
                TaskDialog.Show("Info", "A dumplicate Dimension Type may have been added to Node List\ncommand will be aborted");
                return;
            }
            DataStorageGenerator.SetDataToStorage(_doc, GUID, storageName, fieldName, dict);
        }

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
}
