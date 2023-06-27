﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tekla.Structures;
using Tekla.Structures.Datatype;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Operations;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;
using TSG = Tekla.Structures.Geometry3d;

namespace MB_CutObject.Models
{
    public class PluginData
    {
        #region Fields
        //
        // Define the fields specified on the Form.
        //
        [StructuresField("height")]
        public double height;
        [StructuresField("height1")]
        public double height1;
        [StructuresField("height2")]
        public double height2;
        [StructuresField("width")]
        public double width;
        [StructuresField("width1")]
        public double width1;
        [StructuresField("width2")]
        public double width2;
        [StructuresField("width3")]
        public double width3;
        [StructuresField("width4")]
        public double width4;
        [StructuresField("radius")]
        public double radius;
        [StructuresField("offsetH")]
        public double offsetH;
        [StructuresField("offsetL")]
        public double offsetL;
        [StructuresField("typeCut")]
        public int typeCut;
        [StructuresField("mirror")]
        public int mirror;
        #endregion
    }

    [Plugin("MB_CutObject")]
    [PluginUserInterface("MB_CutObject.Views.MainWindow")]
    public class MB_CutObject : PluginBase
    {
        #region Fields
        private Model _Model;
        private PluginData _Data;
        //
        // Define variables for the field values.
        //
        private double _Height = 0.0;
        private double _Height1 = 0.0;
        private double _Height2 = 0.0;
        private double _Width = 0.0;
        private double _Width1 = 0.0;
        private double _Width2 = 0.0;
        private double _Width3 = 0.0;
        private double _Width4 = 0.0;
        private double _Radius = 0.0;
        private double _OffsetH = 0.0;
        private double _OffsetL = 0.0;
        private int _TypeCut = 0;
        private int _Mirror = 0;

        #endregion

        #region Properties
        private Model Model
        {
            get { return this._Model; }
            set { this._Model = value; }
        }

        private PluginData Data
        {
            get { return this._Data; }
            set { this._Data = value; }
        }
        #endregion

        #region Constructor
        public MB_CutObject(PluginData data)
        {
            Model = new Model();
            Data = data;
        }
        #endregion

        
        /// <summary>
        /// Запрос к пользователю
        /// </summary>
        /// <returns></returns>
        public override List<InputDefinition> DefineInput()
        {
            List<InputDefinition> Input = new List<InputDefinition>();
            Picker Picker = new Picker();
            Part part =  (Part)Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART);
            ArrayList PickedPoints = Picker.PickPoints(Picker.PickPointEnum.PICK_TWO_POINTS);

            Input.Add(new InputDefinition(PickedPoints));
            Input.Add(new InputDefinition(part.Identifier));

            return Input;
        }
        /// <summary>
        /// Тело библиотеки
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public override bool Run(List<InputDefinition> Input)
        {
            try
            {
                GetValuesFromDialog();

                if (Input == null)
                {
                    return false;
                }
                ArrayList Points = (ArrayList)Input[0].GetInput();
                Part selectedPart = (Part)Model.SelectModelObject((Identifier)Input[1].GetInput());
                ContourPoint selectedpoint1 = new ContourPoint(Points[0] as TSG.Point, null);
                ContourPoint selectedpoint2 = new ContourPoint(Points[1] as TSG.Point, null);

                Solid solidpart = selectedPart.GetSolid();
                double centerpart = (solidpart.MaximumPoint.Z + solidpart.MinimumPoint.Z)/2;

                ContourPlate booleanCP = new ContourPlate();
                double thicknessCut = Math.Round(Math.Abs(solidpart.MaximumPoint.Z) + Math.Abs(solidpart.MinimumPoint.Z)) + 10;
                booleanCP.Profile.ProfileString = $"PL{thicknessCut}";
                booleanCP.Material.MaterialString = "Steel_Undefined";
                // В зависимости от выбранного типа выреза добавляем точки контурной пластины
                // Вырез центрируем относитально детали с помощью centerpart
                switch (_TypeCut)
                {
                    case 0:
                        {
                            AddContourPoint(0 - _OffsetL, 0 - _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint(0 - _OffsetL, _Height, centerpart, booleanCP, null);
                            AddContourPoint(_Width, _Height, centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                            AddContourPoint(_Width, -_OffsetH, centerpart, booleanCP, null);
                        }
                        break;
                    case 1:
                        {
                            AddContourPoint(0 + _Width + _Radius, _Height, centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                            AddContourPoint(0 + _Width - _Width1, _Height, centerpart, booleanCP, null);
                            AddContourPoint(0 + _Width - _Width1, 0 - _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint(0 - _OffsetL, 0 - _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint(0 - _OffsetL, _Height + 2 * _Radius, centerpart, booleanCP, null);
                            AddContourPoint(_Width + _Radius, _Height + 2 * _Radius, centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                        }
                        break;
                    case 2:
                        {
                            //Подготовка данных для получения точки касательной к окружности
                            double hyp = Math.Sqrt(Math.Pow(_Width + _Width1 / 2, 2) + Math.Pow(_Height + _Height1, 2));
                            double angle1 = Math.Acos(_Radius / hyp);
                            double angle2 = Math.Acos((_Height + _Height1) / hyp);
                            double angle3 = (angle1 + angle2) - 90 * Math.PI / 180;
                            double hkat = _Radius * Math.Cos(angle3);
                            double vkat = _Radius * Math.Sin(angle3);
                            //Координата Х для удлинения выреза
                            double offsetX = _OffsetH * Math.Tan(angle3);

                            AddContourPoint(0 - _Width - _Width1 / 2 - offsetX, 0 - _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint(0 - hkat, _Height + _Height1 + vkat, centerpart, booleanCP, null);
                            AddContourPoint(0, _Height + _Height1 + _Radius, centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                            AddContourPoint(hkat, _Height + _Height1 + vkat, centerpart, booleanCP, null);
                            AddContourPoint(_Width + _Width1 / 2 + offsetX, 0 - _OffsetH, centerpart, booleanCP, null);
                        }
                        break;
                    case 3:
                        {
                            //Подготовка данных для получения точки касательной к окружности
                            double type3hyp = Math.Sqrt(Math.Pow(_Width + _Width1 - _Width3, 2) + Math.Pow(_Height + _Height1, 2));
                            double type3angle1 = Math.Acos(_Radius / type3hyp);
                            double type3angle2 = Math.Acos((_Height + _Height1) / type3hyp);
                            double type3angle3 = (type3angle1 + type3angle2) - 90 * Math.PI / 180;
                            double type3hkat = _Radius * Math.Cos(type3angle3);
                            double type3vkat = _Radius * Math.Sin(type3angle3);
                            //Подготовка данных для получения точки на окружности при смещении от продольного ребра
                            double type3vkat1 = Math.Sqrt(Math.Pow(_Radius, 2) - Math.Pow((_Width2 + _Width3), 2));
                            //Координата Х для удлинения выреза
                            double type3offsetX = _OffsetH * Math.Tan(type3angle3);

                            AddContourPoint(_Width1 / 2 + _Width2 + _Height2 + _OffsetH, _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2, 0 - _Height2, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2, 0 - (_Height + _Height1 - type3vkat1), centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 - _Width3, 0 - (_Height + _Height1 + _Radius), centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                            AddContourPoint(_Width1 / 2 - _Width3 - type3hkat, 0 - (_Height + _Height1 + type3vkat), centerpart, booleanCP, null);
                            AddContourPoint(0 - _Width - _Width1 / 2 - type3offsetX, _OffsetH, centerpart, booleanCP, null);
                        }
                        break;
                    case 4:
                        {
                            //Подготовка данных для получения точки касательной к окружности
                            double type4hyp = Math.Sqrt(Math.Pow(_Width + _Width1 - _Width3, 2) + Math.Pow(_Height + _Height1, 2));
                            double type4angle1 = Math.Acos(_Radius / type4hyp);
                            double type4angle2 = Math.Acos((_Height + _Height1) / type4hyp);
                            double type4angle3 = (type4angle1 + type4angle2) - 90 * Math.PI / 180;
                            double type4hkat = _Radius * Math.Cos(type4angle3);
                            double type4vkat = _Radius * Math.Sin(type4angle3);
                            //Подготовка данных для получения точки на окружности при смещении от продольного ребра
                            double type4vkat1 = Math.Sqrt(Math.Pow(_Radius, 2) - Math.Pow((_Width2 + _Width3), 2));
                            //Координата Х для удлинения выреза
                            double type4offsetX = _OffsetH * Math.Tan(type4angle3);

                            AddContourPoint(_Width1 / 2 + _Width2 + _Height2, _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2 + _Height2, 0 - _Height2, centerpart, booleanCP, new Chamfer(_Height2, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                            AddContourPoint(_Width1 / 2 + _Width2, 0 - _Height2, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2, 0 - (_Height + _Height1 - type4vkat1), centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 - _Width3, 0 - (_Height + _Height1 + _Radius), centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                            AddContourPoint(_Width1 / 2 - _Width3 - type4hkat, 0 - (_Height + _Height1 + type4vkat), centerpart, booleanCP, null);
                            AddContourPoint(0 - _Width - _Width1 / 2 - type4offsetX, _OffsetH, centerpart, booleanCP, null);
                        }
                        break;
                    case 5:
                        {
                            //Подготовка данных для получения точки касательной к окружности
                            double hyp = Math.Sqrt(Math.Pow(_Width + _Width1 - _Width4, 2) + Math.Pow(_Height + _Height1, 2));
                            double angle1 = Math.Acos(_Radius / hyp);
                            double angle2 = Math.Acos((_Height + _Height1) / hyp);
                            double angle3 = (angle1 + angle2) - 90 * Math.PI / 180;
                            double hkat = _Radius * Math.Cos(angle3);
                            double vkat = _Radius * Math.Sin(angle3);
                            //Подготовка данных для получения точки на окружности при смещении от продольного ребра
                            double vkat1 = Math.Sqrt(Math.Pow(_Radius, 2) - Math.Pow((_Width2 + _Width3), 2));
                            //Координата Х для удлинения выреза
                            double offsetX = _OffsetH * Math.Tan(angle3);

                            AddContourPoint(_Width1 / 2 + _Width2 + _Height2 + _OffsetH, _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2, 0 - _Height2, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2, 0 - (_Height + _Height1 - _Radius), centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2 + _Width3 + _Radius, 0 - (_Height + _Height1 - _Radius), centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                            AddContourPoint(_Width1 / 2 + _Width2 + _Width3 + _Radius, 0 - (_Height + _Height1 + _Radius), centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));

                            AddContourPoint(_Width1 / 2 + _Width3, 0 - (_Height + _Height1 + _Radius), centerpart, booleanCP, null);

                            double hkat1 = (_Radius - vkat) * Math.Tan(angle3);

                            AddContourPoint(_Width1 / 2 - _Width4 - hkat + hkat1, 0 - (_Height + _Height1 + _Radius), centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                            AddContourPoint(0 - _Width - _Width1 / 2 - offsetX, _OffsetH, centerpart, booleanCP, null);
                        }
                        break;
                    case 6:
                        {
                            //Подготовка данных для получения точки касательной к окружности
                            double type5hyp = Math.Sqrt(Math.Pow(_Width + _Width1 - _Width4, 2) + Math.Pow(_Height + _Height1, 2));
                            double type5angle1 = Math.Acos(_Radius / type5hyp);
                            double type5angle2 = Math.Acos((_Height + _Height1) / type5hyp);
                            double type5angle3 = (type5angle1 + type5angle2) - 90 * Math.PI / 180;
                            double type5hkat = _Radius * Math.Cos(type5angle3);
                            double type5vkat = _Radius * Math.Sin(type5angle3);
                            //Подготовка данных для получения точки на окружности при смещении от продольного ребра
                            double type5vkat1 = Math.Sqrt(Math.Pow(_Radius, 2) - Math.Pow((_Width2 + _Width3), 2));
                            //Координата Х для удлинения выреза
                            double type5offsetX = _OffsetH * Math.Tan(type5angle3);

                            AddContourPoint(_Width1 / 2 + _Width2 + _Height2, _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2 + _Height2, 0 - _Height2, centerpart, booleanCP, new Chamfer(_Height2, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                            AddContourPoint(_Width1 / 2 + _Width2, 0 - _Height2, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2, 0 - (_Height + _Height1 - _Radius), centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2 + _Width3 + _Radius, 0 - (_Height + _Height1 - _Radius), centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                            AddContourPoint(_Width1 / 2 + _Width2 + _Width3 + _Radius, 0 - (_Height + _Height1 + _Radius), centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));

                            AddContourPoint(_Width1 / 2 + _Width3, 0 - (_Height + _Height1 + _Radius), centerpart, booleanCP, null);

                            double type5hkat1 = (_Radius - type5vkat) * Math.Tan(type5angle3);

                            AddContourPoint(_Width1 / 2 - _Width4 - type5hkat + type5hkat1, 0 - (_Height + _Height1 + _Radius), centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                            AddContourPoint(0 - _Width - _Width1 / 2 - type5offsetX, _OffsetH, centerpart, booleanCP, null);
                        }
                        break;
                    case 7:
                        {
                            //Подготовка данных для получения точки касательной к окружности
                            double hyp = Math.Sqrt(Math.Pow(_Width + _Width1 - _Width4, 2) + Math.Pow(_Height + _Height1, 2));
                            double angle1 = Math.Acos(_Radius / hyp);
                            double angle2 = Math.Acos((_Height + _Height1) / hyp);
                            double angle3 = (angle1 + angle2) - 90 * Math.PI / 180;
                            double hkat = _Radius * Math.Cos(angle3);
                            double vkat = _Radius * Math.Sin(angle3);
                            //Подготовка данных для получения точки на окружности при смещении от продольного ребра
                            double vkat1 = Math.Sqrt(Math.Pow(_Radius, 2) - Math.Pow((_Width1 / 2 + _Width2), 2));

                            AddContourPoint( - _Width1 / 2 - _Width2 - _Height2 - _OffsetH, - _OffsetH, centerpart, booleanCP, null);
                            AddContourPoint( - _Width1 / 2 - _Width2, _Height2, centerpart, booleanCP, null);
                            AddContourPoint( - _Width1 / 2 - _Width2, _Height + _Height1 - vkat1, centerpart, booleanCP, null);
                            AddContourPoint(0, _Height + _Height1 + _Radius, centerpart, booleanCP, new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ARC_POINT));
                            AddContourPoint(_Width1 / 2 + _Width2, _Height + _Height1 - vkat1, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2, _Height2, centerpart, booleanCP, null);
                            AddContourPoint(_Width1 / 2 + _Width2 + _Height2 + _OffsetH, -_OffsetH, centerpart, booleanCP, null);
                        }
                        break;
                }
                //Для перемещения выреза по центру детали
                selectedpoint1.Z = centerpart;
                CoordinateSystem startCS = new CoordinateSystem(
                    new TSG.Point(0, 0, centerpart),
                    new TSG.Vector(1,0, 0),
                    new TSG.Vector(0, 0, 1000));
                CoordinateSystem endCS = new CoordinateSystem(
                    selectedpoint1,
                    new TSG.Vector(selectedpoint2.X - selectedpoint1.X, selectedpoint2.Y - selectedpoint1.Y, 0),
                    new TSG.Vector(0, 0, 1000));
                //Тип отзеркаливания выреза 
                switch (_Mirror)
                {
                    case 0:
                        break;
                    case 1:
                        endCS.AxisX = new TSG.Vector(- (selectedpoint2.X - selectedpoint1.X), - (selectedpoint2.Y - selectedpoint1.Y), 0); //что то не так. разобраться
                        endCS.AxisY = new TSG.Vector(0, 0, -1000);
                        break;
                    case 2:
                        endCS.AxisY = new TSG.Vector(0, 0, -1000);
                        break;
                }
                
                booleanCP.Class = BooleanPart.BooleanOperativeClassName;
                booleanCP.Insert();
                Operation.MoveObject(booleanCP, startCS, endCS);

                BooleanPart booleanPart = new BooleanPart();
                booleanPart.Father = selectedPart;
                booleanPart.SetOperativePart(booleanCP);
                booleanPart.Insert();
                booleanCP.Delete();
                Model.CommitChanges();
                Operation.DisplayPrompt("Готово");
            }
            catch (Exception Exc)
            {
                MessageBox.Show("Ошибка в start");
                MessageBox.Show(Exc.ToString());
            }
            return true;
            
            //Добавление контурных точек в контурную пластину
            void AddContourPoint(double x, double y, double z, ContourPlate cp, Chamfer chamfer)
            {
                ContourPoint point = new ContourPoint(new TSG.Point(x, y, z), chamfer);
                cp.AddContourPoint(point);
            }
        }

        #region Private methods
        /// <summary>
        /// Gets the values from the dialog and sets the default values if needed
        /// </summary>
        private void GetValuesFromDialog()
        {
            _Height = Data.height;
            _Height1 = Data.height1;
            _Height2 = Data.height2;
            _Width = Data.width;
            _Width1 = Data.width1;
            _Width2 = Data.width2;
            _Width3 = Data.width3;
            _Width4 = Data.width4;
            _Radius = Data.radius;
            _OffsetH = Data.offsetH;
            _OffsetL = Data.offsetL;
            _TypeCut = Data.typeCut;
            _Mirror = Data.mirror;


            if (IsDefaultValue(_Height))
                _Height = 50;
            if (IsDefaultValue(_Height1))
                _Height1 = 0;
            if (IsDefaultValue(_Height2))
                _Height2 = 50;
            if (IsDefaultValue(_Width))
                _Width = 50;
            if (IsDefaultValue(_Width1))
                _Width1 = 30;
            if (IsDefaultValue(_Width2))
                _Width2 = 50;
            if (IsDefaultValue(_Width3))
                _Width3 = 50;
            if (IsDefaultValue(_Width4))
                _Width4 = 50;
            if (IsDefaultValue(_Radius))
                _Radius = 12.5;
            if (IsDefaultValue(_OffsetH))
                _OffsetH = 0;
            if (IsDefaultValue(_OffsetL))
                _OffsetL = 0;
            if (IsDefaultValue(_TypeCut))
                _TypeCut = 0;
            if (IsDefaultValue(_Mirror))
                _Mirror = 0;
        }

        // Write your private methods here.

        #endregion
    }
}
