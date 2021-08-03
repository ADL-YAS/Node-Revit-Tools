using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NodeTools.Commands.CleanDimension;

namespace NodeTools.Commands.RemoveDuplicateDimension
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class RemoveDuplicateDimensionTypeCommand : IExternalCommand
    {
        Document doc;
        List<ElementId> UsedDimTypesIds = new List<ElementId>();
        List<ElementId> DocsDimTypeIds = new List<ElementId>();

        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            doc = commandData.Application.ActiveUIDocument.Document;

            List<Dimension> dimensions = new DimensionProcessor(doc).DimsUsed;
            List<DimensionType> dimensionTypes = new DimensionTypeProcessor(doc).DocDimTypes;
            DocsDimTypeIds = dimensionTypes.Select(x => x.Id).ToList();
            int initialDimInstanceCount = dimensions.Count();
            bool prompt = false;
            IEnumerable<IGrouping<string,Dimension>> dimensionDimTypeGroup = dimensions.GroupBy(RemoveNonPrintableChar);

            //taskdialog menu
            TaskDialog td = new TaskDialog("Purge Dimension Type");
            td.AllowCancellation = true;
            td.MainInstruction = "Purge Options";
            td.CommonButtons = TaskDialogCommonButtons.Close;
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Purge unused with dependents","If unused Dimension type has dependent Dimension type that is used in the drawings, all of it's instance will also gets deleted");
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Purge unused without dependents","This will delete only those Dimension type that doesn't have dependents resulting of some Dimension type that has duplicate name may get's retain");

            switch (td.Show())
            {
                case TaskDialogResult.CommandLink1:
                    prompt = true;
                    break;

                case TaskDialogResult.CommandLink2:
                    prompt = false;
                    break;
                default:
                    // handle any other case.
                    break;
            }
            using (Transaction tr = new Transaction(doc, "Remove Unuse and Duplicate"))
            {
                tr.Start();

                ReplaceDuplicate(dimensionDimTypeGroup);

                GetAllUnused(dimensions);

                List<ElementId> unusedIds = DocsDimTypeIds.Where(x => UsedDimTypesIds.All(i => i != x)).ToList();

                List<ElementId> selectedIdsTodelete = GetValidUnusedTodelete(unusedIds);

                if (prompt)
                {
                    //delete all unused but other used dimtype that has dependency to one of id to delete will also get deleted
                    Delete(unusedIds);
                }
                else
                {
                    //will try to remove duplicate and unused ids which all the used dimtype has no depenedency
                    Delete(selectedIdsTodelete);
                }
                tr.Commit();
            }
            if(prompt == true)
            {
                int newCount = new DimensionProcessor(doc).DimsUsed.Count();
                TaskDialog.Show("Info", $"{initialDimInstanceCount - newCount} out of {initialDimInstanceCount} Dimension instance where deleted in this operation");
            }
            
            return Result.Succeeded;
        }

        void ReplaceDuplicate(IEnumerable<IGrouping<string, Dimension>> dimensionGroups)
        {

            //getting dimensiontype that has duplicate only
            Dictionary<string, List<Dimension>> dimtypeWithDuplicate = new Dictionary<string, List<Dimension>>();
            foreach (var item in dimensionGroups)
            {
                if(item.Select(x=>x.DimensionType.Id).Distinct().Count() > 1)
                {
                    dimtypeWithDuplicate[item.Key] = item.Select(x => x).ToList();
                }
            }


            List<ElementId> usedDimensionTypeId = new List<ElementId>();
            List<ElementId> allIds = new List<ElementId>();

            //getting dependents
            foreach (var kv in dimtypeWithDuplicate)
            {
                List<Dimension> dims = kv.Value;
                //get all dimensiontype that is use by the id
                var typIds = dims.Select(x => x.DimensionType.Id).Distinct().ToList();
                //convert to dimensiontype
                var types = typIds.Select(x => doc.GetElement(x) as DimensionType).ToList();
                Dictionary<DimensionType, List<ElementId>> dimTypeWithDependents = new Dictionary<DimensionType, List<ElementId>>();
                //getting dependent elements of each types and put to dictionary
                foreach (var item in types)
                {
                   var dep = item.GetDependentElements(new ElementClassFilter(typeof(DimensionType))).ToList();
                    dimTypeWithDependents[item] = dep;
                }
                //add all ids used by dupplicates
                allIds.AddRange(dimTypeWithDependents.Select(x=>x.Key).Select(k=>k.Id).ToList());
                //get dimensiontype that has more dependent
                var dimensionType = dimTypeWithDependents.Where(x=>x.Value.Count() > 1).Select(i=>i.Key).FirstOrDefault();
                //select dimensionttype to use
                DimensionType selectedType = null;
                if (dimensionType != null)
                {
                    selectedType = dimensionType;
                }
                else
                {
                    DimensionType dimType = dimTypeWithDependents.FirstOrDefault().Key;
                    selectedType = dimType;
                }

                //replace each dimension's dimentionType by the selected type
                foreach (Dimension dimension in dims)
                {
                    dimension.DimensionType = selectedType;
                }
                usedDimensionTypeId.Add(selectedType.Id);
            }

            ////gett all replaced ids
            List<ElementId> replacedIds = allIds.Where(x => usedDimensionTypeId.All(i => i != x)).ToList();
        }

        public void GetAllUnused(List<Dimension> dimensionList)
        {
           var allUsedDimtypIds =  dimensionList.Where(x => x.DimensionType.FamilyName != "Diameter Dimension").Select(x => x.DimensionType.Id).Distinct().ToList();
            UsedDimTypesIds.AddRange(allUsedDimtypIds);
        }

        List<ElementId> GetValidUnusedTodelete(List<ElementId> ids)
        {
            var toDelete = new List<ElementId>();
            foreach (var id in ids)
            {
                var dimType = doc.GetElement(id) as DimensionType;
                if(dimType != null)
                {
                    var dependent = dimType.GetDependentElements(new ElementClassFilter(typeof(DimensionType)));
                    var c = UsedDimTypesIds.Intersect(dependent).Count();
                    if(c == 0)
                    {
                        toDelete.Add(id);
                    }
                }
            }
            return toDelete;
        }

        void Delete(List<ElementId> ids)
        {
            foreach (var item in ids)
            {
                try
                {
                    doc.Delete(item);
                }
                catch
                {
                }
            }
        }
        public string RemoveNonPrintableChar(Dimension dim)
        {
            string oldName = dim.Name;
            string newName = Regex.Replace(oldName, @"\p{C}+", string.Empty);
            return newName;
        }
    }
}
