﻿using System;
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
        [StructuresDialog("width", typeof(TD.Double))]
        public double Width
        {
            get { return width; }
            set { width = value; OnPropertyChanged("Width"); }
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
