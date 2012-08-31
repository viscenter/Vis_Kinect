using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GestureControls;
using GestureControls.Controls;

namespace Viewers.UserControls
{
    /// <summary>
    /// Interaction logic for VidViewPlay.xaml
    /// </summary>
    public partial class VidViewPlay : UserControl
    {
        public VidViewPlay()
        {
            InitializeComponent();
        }

        private void MakeVisible(object sender, KinectCursorEventArgs e)
        {
            Imagen.Opacity = 1;
        }

        private void MakeInvisible(object sender, KinectCursorEventArgs e)
        {
            Imagen.Opacity = 0;
        }
    }
}
