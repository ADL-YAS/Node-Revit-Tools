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

namespace NodeTools.Commands.CleanGroups
{
    /// <summary>
    /// Interaction logic for CleanGroupsUI.xaml
    /// </summary>
    public partial class CleanGroupsUI : Window
    {
        public CleanGroupsUI()
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

       

        private void Delete_Click(object sender, RoutedEventArgs e)
        {


        }

       


        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
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
}
