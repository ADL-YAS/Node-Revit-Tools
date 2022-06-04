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

        internal List<DimensionType> DocDimtype { get; private set; }
        internal List<Dimension> AllUsed_Dims { get; private set; }


     
        List<DimTypeCustomObj> AllUsed_DimType = new List<DimTypeCustomObj>();
        ObservableCollection<DimTypeCustomObj> NodeSelectedDimTypes = new ObservableCollection<DimTypeCustomObj>();

        internal Document _doc { get; private set; }

        public DimensionCleanUpUI(Document doc,List<DimensionType> docDimtypes,List<Dimension> dims)
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
            //get all used dimensiontype in the document 
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

            // intialize the item to be display in the UI
            List<DimTypeCustomObj> customlist = DocDimtype.Select(x => new DimTypeCustomObj() 
                                                { Name = x.Name, FamilyName = x.FamilyName, EleId = x.Id, UniqueId = x.UniqueId }).ToList();

            customlist.Sort(new NameSortHelper());

            UIDimTypes.ItemsSource = customlist.Where(x => x.FamilyName != "Diameter Dimension Style"
                                                         && x.FamilyName != "Alignment Station Labels"
                                                         && x.FamilyName != "Spot Coordinates"
                                                         && x.FamilyName != "Spot Slopes"
                                                         && x.FamilyName != "Spot Elevations"
                                                         && x.FamilyName != "Radial Dimension Style").ToList();


            //retrieve datastorage if there is existing selected node dimesiontype
            Dictionary<string,string> dict = DataStorageGenerator.GetDataFromStorage<Dictionary<string, string>>(_doc, GUID, fieldName, storageName);

            //if found then set the node selected dimensiontype in the UI which represent the Node Standard 
            if(dict != null)
            {
                foreach (var item in dict)
                {
                    DimTypeCustomObj obj = customlist.Where(x => x.UniqueId == item.Value).FirstOrDefault();
                    if(obj != null)
                    {
                        NodeSelectedDimTypes.Add(new DimTypeCustomObj() { Name = obj.Name, EleId = obj.EleId, FamilyName = obj.FamilyName, UniqueId = obj.UniqueId });
                    }
                }
                NodeDimTypes.ItemsSource = NodeSelectedDimTypes;
            }
            //if not found then just initialized the Node dimensiontypes Ui observable collection
            else
            {
                NodeDimTypes.ItemsSource = NodeSelectedDimTypes;
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {

            AnimateClose();
            if (NodeSelectedDimTypes.Count() > 1)
            {
                using (TransactionGroup tg = new TransactionGroup(_doc,"Node Standard DimensionType"))
                {
                   using(Transaction tr = new Transaction(_doc,"Duplicate Node Dims"))
                    {
                        tg.Start();

                        tr.Start("First");
                        List<DimensionType> newdtypes = DuplicateDimtype(NodeSelectedDimTypes.ToList());
                        //replace remaining dimensiontype of other dimensions
                        ReplaceDimensionDimType(newdtypes);
                        tr.Commit();


                        tr.Start("Second");
                        //retrieve fresh set of doctypes from the document
                        List<DimensionType> newDocDimtypes = new DimensionTypeProcessor(_doc).DocDimTypes;

                        //get all fresh doc dimtype that are not in the new dtypes
                        var result = newDocDimtypes.Where(p => !newdtypes.Any(p2 => p2.Id == p.Id)).Where(d => d.FamilyName != "Diameter Dimension Style").ToList();
                        

                        DeleteUnused(result);
                        AddDataToStorage(newdtypes.Select(v => new DimTypeCustomObj() { Name = v.Name, FamilyName = v.FamilyName, EleId = v.Id, UniqueId = v.UniqueId }).ToList());
                        tr.Commit();

                        //var newdimtypelist = new DimensionTypeProcessor(_doc).DocDimTypes;
                    }
                    tg.Assimilate();
                }
            }
            else if(NodeSelectedDimTypes.Count() == 1)
            {
                TaskDialog.Show("Info", "Please select more than 1 Dimension Type");
            }
            else
            {
                TaskDialog.Show("Info", "No selected Dimension Type found");
            }
        }

        private List<DimensionType> DuplicateDimtype(List<DimTypeCustomObj> nodedimTypeCustomObjs)
        {
            List<DimensionType> newSetOfDimtype = new List<DimensionType>();

            //get the selected dimtype to be use as reference for setting properties of newly created dimtype
            var ids = nodedimTypeCustomObjs.Select(x => x.EleId);
            List<DimensionType> dimensionTypes = DocDimtype.Where(dt => ids.Any(id => id == dt.Id)).ToList();

            //get document dimtype that has the most dependent element to be use as base for duplicating
            DimensionType baseDtype = DocDimtype.Where(x => x.GetDependentElements(new ElementClassFilter(typeof(DimensionType))).Count > 1)
                                        .Where(f=>f.FamilyName == "Linear Dimension Style").FirstOrDefault();

            //get the first Linear dim item in the list to be use as base dimensiontype
            IEnumerable<DimTypeCustomObj> nIds = NodeSelectedDimTypes.Where(x => x.FamilyName == "Linear Dimension Style");
            DimensionType firstItem = DocDimtype.Where(x => x.Id == nIds.FirstOrDefault().EleId).FirstOrDefault();


            //get other type without the first item
            List<DimTypeCustomObj> items = NodeSelectedDimTypes.Where(p => p.EleId != firstItem.Id).ToList();
            List<DimensionType> othertypes = DocDimtype.Where(p => items.Any(p2 => p2.EleId == p.Id)).ToList();

            //set parameter of the basetype base using the first item in selection
            SetParameter(baseDtype, firstItem);

            //set all dimension instance that uses the oldtype to new base type
            SetAllDimensionInstances(firstItem, baseDtype);
            //set the name of base dimetype as per reference dimtype
            string name = String.Copy(firstItem.Name);
            baseDtype.Name = name;
            newSetOfDimtype.Add(baseDtype);


            foreach (DimensionType item in othertypes)
            {
                DimensionType newdimensionType = baseDtype.Duplicate("newdimtype") as DimensionType;
                SetParameter(newdimensionType, item);
                string name2 = String.Copy(item.Name);
                //get all dimensions that uses the old dimtype
                SetAllDimensionInstances(item, newdimensionType);
                newdimensionType.Name = name2;
                newSetOfDimtype.Add(newdimensionType);
            }
            return newSetOfDimtype;
        }

        void SetParameter(DimensionType typetoUpdate, DimensionType baseType)
        {

            //set the parameters of storage type not equals to none
            foreach (Parameter par in baseType.Parameters)
            {
                try
                {
                    switch (par.StorageType)
                    {
                        case StorageType.None:
                            typetoUpdate.SetUnitsFormatOptions(baseType.GetUnitsFormatOptions());
                            typetoUpdate.SetAlternateUnitsFormatOptions(baseType.GetAlternateUnitsFormatOptions());
                            typetoUpdate.SetEqualityFormula(baseType.GetEqualityFormula());
                            typetoUpdate.SetOrdinateDimensionSetting(baseType.GetOrdinateDimensionSetting());
                            break;
                        case StorageType.Integer:
                            Parameter itgr = baseType.get_Parameter(par.Definition);
                            if (itgr != null)
                            {
                                int value = itgr.AsInteger();
                                if (!par.IsReadOnly)
                                {
                                    typetoUpdate.get_Parameter(par.Definition).Set(value);
                                }

                            }
                            break;
                        case StorageType.Double:
                            Parameter dbl = baseType.get_Parameter(par.Definition);
                            if (dbl != null)
                            {
                                double value = dbl.AsDouble();
                                if (!par.IsReadOnly)
                                {
                                    typetoUpdate.get_Parameter(par.Definition).Set(value);
                                }

                            }
                            break;
                        case StorageType.String:
                            Parameter str = baseType.get_Parameter(par.Definition);
                            if (str != null)
                            {
                                string value = str.AsString();
                                if (!par.IsReadOnly)
                                {
                                     typetoUpdate.get_Parameter(par.Definition).Set(value);
                                }

                            }
                            break;
                        case StorageType.ElementId:
                            Parameter eId = baseType.get_Parameter(par.Definition);
                            if (eId != null)
                            {
                                ElementId value = eId.AsElementId();
                                if (!par.IsReadOnly)
                                {
                                    typetoUpdate.get_Parameter(par.Definition).Set(value);
                                }

                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                   
                }
                
            }
            
        }
        void SetAllDimensionInstances(DimensionType oldtype,DimensionType newDimType)
        {
            List<Dimension> dims = AllUsed_Dims.Where(x => x.DimensionType.Id == oldtype.Id).ToList();
            foreach (Dimension dimension in dims)
            {
                dimension.DimensionType = newDimType;
            }
            //try
            //{
            //    _doc.Delete(oldtype.Id);
            //}
            //catch (Exception)
            //{

            //}
        }



        public void ReplaceDimensionDimType(List<DimensionType> selectedDimtype)
        {
            foreach (DimensionType obj in selectedDimtype)
            {
                List<Dimension> dims = AllUsed_Dims.Where(x => x.DimensionType.Id != obj.Id).ToList();
                foreach (Dimension item in dims)
                {
                    if(item.DimensionType.StyleType == obj.StyleType)
                    {
                        var font = obj.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
                        var size = obj.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
                        if(obj.GetUnitsFormatOptions().UseDefault || item.DimensionType.GetUnitsFormatOptions().UseDefault)
                        {
                            var fnt = item.DimensionType.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
                            var s = item.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
                            if (font == fnt && size == s)
                            {
                                item.DimensionType = obj;
                            }
                        }
                        else
                        {
                            var format = obj.GetUnitsFormatOptions().GetUnitTypeId().TypeId;
                            var fnt = item.DimensionType.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
                            var s = item.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
                            var f = item.DimensionType.GetUnitsFormatOptions().GetUnitTypeId().TypeId;
                            if (font == fnt && size == s && format == f)
                            {
                                item.DimensionType = obj;
                            }
                        }


                    }
                }
            }
        }

        public void DeleteUnused(List<DimensionType> dtypes)
        {
            foreach (DimensionType item in dtypes)
            {
                //if (item.StyleType != DimensionStyleType.Diameter
                //&& item.StyleType != DimensionStyleType.SpotCoordinate
                //&& item.StyleType != DimensionStyleType.SpotElevation
                //&& item.StyleType != DimensionStyleType.SpotSlope)
                //{
                //}
                try
                {
                    _doc.Delete(item.Id);
                }
                catch (Exception)
                {

                }
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

        //-----------UI and Events-------//////
        #region For UI and Button Events
        private void Add_Click(object sender, RoutedEventArgs e)
        {

            foreach (DimTypeCustomObj item in UIDimTypes.SelectedItems)
            {
                if (!NodeSelectedDimTypes.Select(x => x.Name).Contains(item.Name) && HasNonPrintableChar(item.Name))
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
        private void Window_Activated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => WindowStyle = WindowStyle.None));
        }

        private void AnimateClose()
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            this.Close();
        }
        #endregion


    }
}
