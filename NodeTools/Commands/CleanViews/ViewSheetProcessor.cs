using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace NodeTools.Commands.CleanViews
{
    class ViewSheetProcessor
    {
        Document _doc;
        private List<ViewSheet> _viewSheets;
        private List<ScheduleSheetInstance> _scheduleSheetInstance;
        public List<ElementId> AllPlacedViewIds = new List<ElementId>();
        public List<ElementId> AllPlacedScheduleIds = new List<ElementId>();

        public ViewSheetProcessor(Document doc)
        {
            _doc = doc;
            _viewSheets = new FilteredElementCollector(_doc).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().ToList();
            _scheduleSheetInstance = new FilteredElementCollector(_doc).OfClass(typeof(ScheduleSheetInstance)).Cast<ScheduleSheetInstance>().ToList();
            GetAllPlacedViews();
            GetAllPlacedSchedules();
        }


        /// <summary>
        /// this doesn't include schedule and keynote
        /// </summary>
        void GetAllPlacedViews()
        {
            foreach (ViewSheet vsheets in _viewSheets)
            {
                foreach (ElementId id in vsheets.GetAllPlacedViews())
                {
                    View view = _doc.GetElement(id) as View;
                    if (view.ViewType == ViewType.Legend)
                    {
                        if (!AllPlacedViewIds.Contains(id))
                        {
                            AllPlacedViewIds.Add(id);
                        }
                    }
                    else
                    {
                        AllPlacedViewIds.Add(id);
                    }

                }
            }
        }

        void GetAllPlacedSchedules()
        {
            foreach (ScheduleSheetInstance sheetInstance in _scheduleSheetInstance)
            {
                if (!AllPlacedScheduleIds.Contains(sheetInstance.ScheduleId))
                {
                    AllPlacedScheduleIds.Add(sheetInstance.ScheduleId);
                }

            }
        }


    }
}
