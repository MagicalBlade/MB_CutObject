using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Tekla.Structures.Dialog;
using TD = Tekla.Structures.Datatype;

namespace MB_CutObject.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields
        private double height = 0.0;
        private double width = 0.0;

        #endregion

        #region Properties
        [StructuresDialog("height", typeof(TD.Double))]
        public double Height
        {
            get { return height; }
            set { height = value; OnPropertyChanged("Height"); }
        }
        [StructuresDialog("width", typeof(TD.Double))]
        public double Width
        {
            get { return width; }
            set { width = value; OnPropertyChanged("Width"); }
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
