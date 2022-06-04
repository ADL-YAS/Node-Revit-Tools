using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace NodeTools.Ribbon
{
    class NodeRibbon
    {
        internal static void AddRibbonPanel(UIControlledApplication application)
        {
            string TabName = "Node AEC";

            application.CreateRibbonTab(TabName);

            string AssemblyPath = Assembly.GetExecutingAssembly().Location;

            #region CreatePanel for button
            RibbonPanel CleanDocs_Panel = application.CreateRibbonPanel(TabName, "Clean Document");

            #endregion



            #region Create Image for button
            BitmapImage UnUsedView_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/UnusedView32x32.png"));
            BitmapImage CLeanSheet_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/CleanSheets32x32.png"));
            BitmapImage Groups_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/Groups32x32.png"));
            //image for lines
            BitmapImage LinesPulldown_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/LinesPullDownImage32x32.png"));
            BitmapImage UnUsedLineStyle_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/UnusedLineStyle32x32.png"));
            BitmapImage UnUsedLinePattern_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/UnusedLinePattern32x32.png"));
            BitmapImage CadLines_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/CadLines32x32.png"));
            
            //dimtyp images
            BitmapImage DimTypePulldown_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/CleanDims32x32.png"));
            BitmapImage RestartDim_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/RestartDimType32x32.png"));
            BitmapImage PurgeDimType_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/PurgeDim32x32.png"));

            //anno image
            BitmapImage Anno_Image = new BitmapImage(new Uri("pack://application:,,,/NodeTools;component/Resources/Annotation32x32.png"));
            
            #endregion

            #region Create button data
            PushButtonData UnusedViewButtonData = new PushButtonData("UnusedViewButton", "Delete\nUnused Views", AssemblyPath, "NodeTools.Commands.CleanViews.DeleteUnusedViewsCommand");
            PushButtonData CleanSheetButtonData = new PushButtonData("CleanSheetButton", "Delete\nSheets", AssemblyPath, "NodeTools.Commands.CleanSheets.CleanSheetsCommand");
            PushButtonData GroupsButtonData = new PushButtonData("GroupsButton", "Document Groups", AssemblyPath, "NodeTools.Commands.CleanGroups.CleanGroupsCommand");
            //lines commands
            PushButtonData UnusedLineStyleButtonData = new PushButtonData("UnusedLineStyleButton", "Delete\nUnused Line Styles", AssemblyPath, "NodeTools.Commands.CleanLines.DeleteUnuseLineStyleCommand");
            PushButtonData UnusedLinePatternButtonData = new PushButtonData("UnusedLinePatternButton", "Delete\nUnused Line Pattern", AssemblyPath, "NodeTools.Commands.CleanLinePattern.DeleteUnusedLinePatternCommand");
            PushButtonData CadLinesButtonData = new PushButtonData("CadLinePatternButton", "Delete\nImported Line Pattern", AssemblyPath, "NodeTools.Commands.CleanLinePattern.DeleteLinesFromCadCommand");
            //dimtype commands
            PushButtonData RestartDimTypeButtonData = new PushButtonData("RestartDimTypeButton", "Dimension Types", AssemblyPath, "NodeTools.Commands.CleanDimension.CleanDocDimensionTypesCommand");
            PushButtonData PurgeDimtypeButtonData = new PushButtonData("PurgeDimTypeButton", "Purge\nDimension Types", AssemblyPath, "NodeTools.Commands.RemoveDuplicateDimension.RemoveDuplicateDimensionTypeCommand");
            //anno command
            PushButtonData AnnotationButtonData = new PushButtonData("AnnotationButton", "Annotation Text Types", AssemblyPath, "NodeTools.Commands.TextNoteTypes.RemoveUnusedTexnoteTypeCommand");
            #endregion

            #region Create push button from Pushbuttondata
            PushButton UnusedViewPushButton = CleanDocs_Panel.AddItem(UnusedViewButtonData) as PushButton;
            PushButton CleanSheetPushButton = CleanDocs_Panel.AddItem(CleanSheetButtonData) as PushButton;
            PushButton PurgeDimtypeButton = CleanDocs_Panel.AddItem(PurgeDimtypeButtonData) as PushButton;
            PushButton GroupsButton = CleanDocs_Panel.AddItem(GroupsButtonData) as PushButton;
            #endregion

            #region ToolTips
            UnusedViewPushButton.ToolTip = "Delete all Views, Schedule, Legends that are not in Sheet";
            CleanSheetPushButton.ToolTip = "Delete all Sheets";
            PurgeDimtypeButton.ToolTip = "Purge Dimension Type base on option provided";
            GroupsButton.ToolTip = "Purge groups in document";
            #endregion


            #region Sett button image
            UnusedViewPushButton.LargeImage = UnUsedView_Image;
            CleanSheetPushButton.LargeImage = CLeanSheet_Image;
            PurgeDimtypeButton.LargeImage = PurgeDimType_Image;
            GroupsButton.LargeImage = Groups_Image;
            #endregion



            #region Setting for pulldowndata
            //lines settings
            UnusedLineStyleButtonData.LargeImage = UnUsedLineStyle_Image;
            UnusedLineStyleButtonData.ToolTip = "This wil Delete unused Line Style, Revit default will not be included as it is not posible to delete by default";
            UnusedLinePatternButtonData.LargeImage = UnUsedLinePattern_Image;
            UnusedLinePatternButtonData.ToolTip = "This will check unused line pattern and will takes time for the command to be completed as line pattern is used application wide";
            CadLinesButtonData.LargeImage = CadLines_Image;
            CadLinesButtonData.ToolTip = "This will delete suspected Autocad lines imports";

            //dimtypes settings
            RestartDimTypeButtonData.ToolTip = "It is recommended to use Dimension Type from standard company template by using transfer project standard\n" +
                "to avoid dependency from current document";
            RestartDimTypeButtonData.LargeImage = RestartDim_Image;
            //anno settings
            AnnotationButtonData.ToolTip = "It is recommended to use Annotation Text Type from standard company template by using transfer project standard\n" +
                 "to avoid dependency from current document";
            AnnotationButtonData.LargeImage = Anno_Image;

            #endregion

            #region Create Pulldown
            //line style pulldown
            PulldownButtonData LineStyleAndPatternPulldownData = new PulldownButtonData("LineStyleAndPatternPulldown", "Purge\nDocument Lines");
            PulldownButton LineStyleAndPatternPulldownButton = CleanDocs_Panel.AddItem(LineStyleAndPatternPulldownData) as PulldownButton;
            LineStyleAndPatternPulldownButton.LargeImage = LinesPulldown_Image;
            LineStyleAndPatternPulldownButton.AddPushButton(UnusedLineStyleButtonData);
            LineStyleAndPatternPulldownButton.AddPushButton(UnusedLinePatternButtonData);
            LineStyleAndPatternPulldownButton.AddPushButton(CadLinesButtonData);

            //Dim Type pulldown
            PulldownButtonData DocDimTypePulldownData = new PulldownButtonData("NodePulldown", "Node\nStandards");
            PulldownButton DocDimTypePulldownButton = CleanDocs_Panel.AddItem(DocDimTypePulldownData) as PulldownButton;
            DocDimTypePulldownButton.LargeImage = DimTypePulldown_Image;
            DocDimTypePulldownButton.AddPushButton(RestartDimTypeButtonData);
            DocDimTypePulldownButton.AddPushButton(AnnotationButtonData);
            #endregion

        }
    }
}
