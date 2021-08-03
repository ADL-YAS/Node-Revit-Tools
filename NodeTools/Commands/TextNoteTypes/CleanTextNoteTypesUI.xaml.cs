using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace NodeTools.Commands.TextNoteTypes
{
    /// <summary>
    /// Interaction logic for CleanTextNoteTypesUI.xaml
    /// </summary>
    public partial class CleanTextNoteTypesUI : Window
    {
        ObservableCollection<TextNoteType> NodeSelectedTextNoteTypes = new ObservableCollection<TextNoteType>();
        string storageName = "Node_AnnotationType_Storage";
        string fieldName = "Node_Annotation_Types";
        string GUID = "a8ec4b4d-7666-4bf3-9a35-dfa893850fc7";
        Document _doc;
        List<TextNote> DocTextNotes = new List<TextNote>();
        List<TextNoteType> DocTextNoteTypes = new List<TextNoteType>();
        public CleanTextNoteTypesUI(Document doc, List<TextNoteType> textNoteTypes, List<TextNote> textNotes)
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            DocTextNoteTypes = textNoteTypes;
            DocTextNotes = textNotes;
            _doc = doc;
            InitilizeData();
        }


        internal void InitilizeData()
        {
            List<TextNoteType> sortedTypes = DocTextNoteTypes.OrderBy(x => x.Name).ToList();
            UITextnoteTypes.ItemsSource = sortedTypes;
            Dictionary<string, string> dict = DataStorageGenerator.GetDataFromStorage<Dictionary<string, string>>(_doc, GUID, fieldName, storageName);
            if (dict != null)
            {
                foreach (var item in dict)
                {
                    TextNoteType obj = DocTextNoteTypes.Where(x => x.UniqueId == item.Value).FirstOrDefault();
                    if (obj != null)
                    {
                        NodeSelectedTextNoteTypes.Add(obj);
                    }
                }
                NodeTextnoteTypes.ItemsSource = NodeSelectedTextNoteTypes;
            }
            else
            {
                NodeTextnoteTypes.ItemsSource = NodeSelectedTextNoteTypes;
            }
        }


        private void Add_Click(object sender, RoutedEventArgs e)
        {
            foreach (TextNoteType item in UITextnoteTypes.SelectedItems)
            {
                if (!NodeSelectedTextNoteTypes.Select(x => x.Name).Contains(item.Name) && HasNonPrintableChar(item.Name))
                {
                    NodeSelectedTextNoteTypes.Add(item);
                }
                else
                {
                    return;
                }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            List<TextNoteType> items = NodeTextnoteTypes.SelectedItems.Cast<TextNoteType>().ToList();
            foreach (TextNoteType obj in items)
            {
                NodeSelectedTextNoteTypes.Remove(obj);
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

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }


        //void WriteTofile(string[] array, string name)
        //{
        //    // Filename  
        //    string fileName = $"{name}.txt";
        //    // Fullpath. You can direct hardcode it if you like.  
        //    string fullPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"NodeTools\");

        //    string[] info = array;

        //    try
        //    {
        //        if (Directory.Exists(fullPath))
        //        {
        //            File.WriteAllLines(fullPath + fileName, info);
        //        }
        //        else
        //        {
        //            Directory.CreateDirectory(fullPath);
        //            File.WriteAllLines(fullPath + fileName, info);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    //Read a file
        //    //string readText = File.ReadAllText(fullPath);
        //    //Console.WriteLine(readText);
        //}

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (NodeSelectedTextNoteTypes.Count() != 0)
            {
                using (Transaction tr = new Transaction(_doc, "Node Replace TextNoteTypes"))
                {
                    tr.Start();

                    //UngroupGroups();

                    ReplaceAnnotationType(NodeSelectedTextNoteTypes.ToList());

                    AddDataToStorage(NodeSelectedTextNoteTypes.ToList());

                    DeleteUnused();

                    tr.Commit();

                }
            }
        }

        public void ReplaceAnnotationType(List<TextNoteType> selectedTexttype)
        {
            foreach  (TextNoteType tType in selectedTexttype)
            {
                var text = tType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
                var background = tType.get_Parameter(BuiltInParameter.TEXT_BACKGROUND).AsValueString();
                if (text != null && background != null)
                {
                    List<TextNote> allTextnotes = DocTextNotes.Where(x=> x.TextNoteType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString() == text
                                                        && x.TextNoteType.get_Parameter(BuiltInParameter.TEXT_BACKGROUND).AsValueString() == background).ToList();

                    foreach (TextNote tNote in allTextnotes)
                    {
                        var t = tNote.TextNoteType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
                        var x = tNote.TextNoteType.get_Parameter(BuiltInParameter.TEXT_BACKGROUND).AsValueString();
                        try
                        {
                            tNote.TextNoteType = tType;
                        }
                        catch (Exception e)
                        {
                            TaskDialog.Show("Error", e.Message);
                            throw;
                        }
                    }
                }
               
            }
        }

        public void DeleteUnused()
        {
            List<ElementId> selectedTypes = NodeSelectedTextNoteTypes.Select(x => x.Id).ToList();
            List<ElementId> docsDimtypeIds = DocTextNoteTypes.Select(x => x.Id).ToList();

            List<ElementId> toDelete = new List<ElementId>();
            foreach (ElementId id in docsDimtypeIds)
            {
                if (!selectedTypes.Contains(id))
                {
                    toDelete.Add(id);
                }
            }
            foreach (var i in toDelete)
            {
                try
                {
                    _doc.Delete(i);
                }
                catch
                {
                }
            }
        }

        public void AddDataToStorage(List<TextNoteType> types)
        {
            Dictionary<string, string> dict;
            try
            {
                dict = types.ToDictionary(k => k.Name, v => v.UniqueId);
            }
            catch (Exception e)
            {
                TaskDialog.Show("Info", "A dumplicate TextNote Type may have been added to Node List\ncommand will be aborted");
                return;
            }
            DataStorageGenerator.SetDataToStorage(_doc, GUID, storageName, fieldName, dict);
        }


        //public void UngroupGroups()
        //{
        //    var groupsCol = new FilteredElementCollector(_doc).OfClass(typeof(Autodesk.Revit.DB.Group)).Cast<Autodesk.Revit.DB.Group>().ToList();
        //    if (groupsCol.Count() != 0)
        //    {
        //        try
        //        {
        //            foreach (Autodesk.Revit.DB.Group item in groupsCol)
        //            {
        //                item.UngroupMembers();
        //            }
        //        }
        //        catch 
        //        {

        //        }

        //    }
        //}

        public bool HasNonPrintableChar(string name)
        {
            string oldName = name;
            string newName = Regex.Replace(oldName, @"\p{C}+", string.Empty);
            if (name.Count() != newName.Count())
            {
                return false;
            }
            return true;
        }


        //public void AddDataToStorage(List<DimTypeCustomObj> dimTypeCustomObjs)
        //{
        //    Dictionary<string, string> dict;
        //    try
        //    {
        //        dict = dimTypeCustomObjs.ToDictionary(k => k.Name, v => v.UniqueId);
        //    }
        //    catch (Exception e)
        //    {
        //        TaskDialog.Show("Info", "A dumplicate Dimension Type may have been added to Node List\ncommand will be aborted");
        //        return;
        //    }
        //    DataStorageGenerator.SetDataToStorage(_doc, GUID, storageName, fieldName, dict);
        //}

    }
}
