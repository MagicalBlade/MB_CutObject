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
        private double height1 = 0.0;
        private double height2 = 0.0;
        private double width = 0.0;
        private double width1 = 0.0;
        private double width2 = 0.0;
        private double width3 = 0.0;
        private double width4 = 0.0;
        private double radius = 0.0;
        private double offsetH = 0.0;
        private double offsetL = 0.0;
        private int typeCut = 0;
        private int mirror = 0;

        #endregion

        #region Properties
        [StructuresDialog("height", typeof(TD.Double))]
        public double Height
        {
            get { return height; }
            set { height = value; OnPropertyChanged("Height"); }
        }
        [StructuresDialog("height1", typeof(TD.Double))]
        public double Height1
        {
            get { return height1; }
            set { height1 = value; OnPropertyChanged("Height1"); }
        }
        [StructuresDialog("height2", typeof(TD.Double))]
        public double Height2
        {
            get { return height2; }
            set { height2 = value; OnPropertyChanged("Height2"); }
        }
        [StructuresDialog("width", typeof(TD.Double))]
        public double Width
        {
            get { return width; }
            set { width = value; OnPropertyChanged("Width"); }
        }
        [StructuresDialog("width1", typeof(TD.Double))]
        public double Width1
        {
            get { return width1; }
            set { width1 = value; OnPropertyChanged("Width1"); }
        }
        [StructuresDialog("width2", typeof(TD.Double))]
        public double Width2
        {
            get { return width2; }
            set { width2 = value; OnPropertyChanged("Width2"); }
        }
        [StructuresDialog("width3", typeof(TD.Double))]
        public double Width3
        {
            get { return width3; }
            set { width3 = value; OnPropertyChanged("Width3"); }
        }
        [StructuresDialog("width4", typeof(TD.Double))]
        public double Width4
        {
            get { return width4; }
            set { width4 = value; OnPropertyChanged("Width4"); }
        }
        [StructuresDialog("radius", typeof(TD.Double))]
        public double Radius
        {
            get { return radius; }
            set { radius = value; OnPropertyChanged("Radius"); }
        }
        [StructuresDialog("offsetH", typeof(TD.Double))]
        public double OffsetH
        {
            get { return offsetH; }
            set { offsetH = value; OnPropertyChanged("OffsetH"); }
        }
        [StructuresDialog("offsetL", typeof(TD.Double))]
        public double OffsetL
        {
            get { return offsetL; }
            set { offsetL = value; OnPropertyChanged("OffsetL"); }
        }
        [StructuresDialog("typeCut", typeof(TD.Integer))]
        public int TypeCut
        {
            get { return typeCut; }
            set { typeCut = value; OnPropertyChanged("TypeCut"); }
        }
        [StructuresDialog("mirror", typeof(TD.Integer))]
        public int Mirror
        {
            get { return mirror; }
            set { mirror = value; OnPropertyChanged("Mirror"); }
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
