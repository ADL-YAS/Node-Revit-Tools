using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;

namespace NodeTools.Commands.CleanLinePattern
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class DeleteUnusedLinePatternCommand : IExternalCommand
    {
		public static int modifiedByDeleteLinePattern = 0;
		public static bool checkForPurgeLinePatterns = false;
		public static Document _doc = null;
		public static string LinePatternName = "";
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			Document doc = commandData.Application.ActiveUIDocument.Document;
			_doc = doc;
			Application app = doc.Application;
			app.DocumentChanged += documentChanged_PurgeLinePatterns;
			List<Element> LinePatterns = new FilteredElementCollector(doc).OfClass(typeof(LinePatternElement)).ToList();
			string deletedLinePatterns = "";
			int unusedLinePatternCount = 0;
			foreach (Element LinePattern in LinePatterns)
			{
				modifiedByDeleteLinePattern = 0;
				LinePatternName = LinePattern.Name + " (id " + LinePattern.Id + ")";
				using (TransactionGroup tg = new TransactionGroup(doc, "Delete LinePattern: " + LinePatternName))
				{
					tg.Start();
					using (Transaction t = new Transaction(doc, "delete LinePattern"))
					{
						t.Start();
						checkForPurgeLinePatterns = true;
						doc.Delete(LinePattern.Id);

						// commit the transaction to trigger the DocumentChanged event
						t.Commit();
					}
					checkForPurgeLinePatterns = false;

					if (modifiedByDeleteLinePattern == 1)
					{
						unusedLinePatternCount++;
						deletedLinePatterns += LinePatternName + Environment.NewLine;
						tg.Assimilate();
					}
					else // rollback the transaction group to undo the deletion
						tg.RollBack();
				}
			}

			TaskDialog td = new TaskDialog("Info");
			td.MainInstruction = "Deleted " + unusedLinePatternCount + " LinePatterns";
			td.MainContent = deletedLinePatterns;
			td.Show();

			app.DocumentChanged -= documentChanged_PurgeLinePatterns;
			return Result.Succeeded;
		}
		private static void documentChanged_PurgeLinePatterns(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
		{
			// do not check when rolling back the transaction group
			if (!checkForPurgeLinePatterns)
			{
				return;
			}

			List<ElementId> deleted = e.GetDeletedElementIds().ToList();
			List<ElementId> modified = e.GetModifiedElementIds().ToList();

			// for debugging
			string s = "";
			Element modifiedElement;
			foreach (ElementId id in modified)
			{
				modifiedElement = _doc.GetElement(id);
				s += modifiedElement.Category.Name + " " + modifiedElement.Name + " (" + id.IntegerValue + ")" + Environment.NewLine;
			}
			//TaskDialog.Show("d", LinePatternName + Environment.NewLine + "Deleted = " + deleted.Count + ", Modified = " + modified.Count + Environment.NewLine + s);
			//
			// how many elements were modified and deleted when this LinePattern was deleted?
			// if 1, then the LinePattern is unused and should be deleted
			modifiedByDeleteLinePattern = deleted.Count + modified.Count;
		}
	}
}
