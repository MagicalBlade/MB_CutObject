using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tekla.Structures;
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
        [StructuresField("width")]
        public double width;
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
        private double _Width = 0.0;
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

        #region Overrides
        public override List<InputDefinition> DefineInput()
        {
            //
            // This is an example for selecting two points; change this to suit your needs.
            //
            List<InputDefinition> Input = new List<InputDefinition>();
            Picker Picker = new Picker();
            Part part =  (Part)Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART);
            ArrayList PickedPoints = Picker.PickPoints(Picker.PickPointEnum.PICK_TWO_POINTS);

            Input.Add(new InputDefinition(PickedPoints));
            Input.Add(new InputDefinition(part.Identifier));

            return Input;
        }

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
                Part selectPart = (Part)Model.SelectModelObject((Identifier)Input[1].GetInput());
                if (selectPart == null)
                {
                    MessageBox.Show("Выбранная позиция равно null");
                    return false;
                }
                ContourPoint selectpoint1 = new ContourPoint(Points[0] as TSG.Point, null);
                ContourPoint selectpoint2 = new ContourPoint(Points[1] as TSG.Point, null);
                WorkPlaneHandler workPlaneHandler = Model.GetWorkPlaneHandler();


                Solid solidpart = selectPart.GetSolid();
                double centerpart = (solidpart.MaximumPoint.Z + solidpart.MinimumPoint.Z)/2;


                ContourPlate booleanCP = new ContourPlate();
                double thicknessCut = Math.Round(Math.Abs(solidpart.MaximumPoint.Z) + Math.Abs(solidpart.MinimumPoint.Z)) + 10;
                booleanCP.Profile.ProfileString = $"PL{thicknessCut}";
                booleanCP.Material.MaterialString = "Steel_Undefined";

                switch (_TypeCut)
                {
                    case 0:
                        ContourPoint point1 = new ContourPoint(new TSG.Point(0 - _OffsetL, 0 - _OffsetH, centerpart), null);
                        ContourPoint point2 = new ContourPoint(new TSG.Point(0 - _OffsetL, _Height, centerpart), null);
                        ContourPoint point3 = new ContourPoint(new TSG.Point(_Width, _Height, centerpart), new Chamfer(_Radius, 0, Chamfer.ChamferTypeEnum.CHAMFER_ROUNDING));
                        ContourPoint point4 = new ContourPoint(new TSG.Point(_Width, 0 - _OffsetH, centerpart), null);
                        booleanCP.AddContourPoint(point1);
                        booleanCP.AddContourPoint(point2);
                        booleanCP.AddContourPoint(point3);
                        booleanCP.AddContourPoint(point4);
                        break;
                    default:
                        break;
                }
                
                CoordinateSystem startCS = new CoordinateSystem(
                    new TSG.Point(0,0,0),
                    new TSG.Vector(new TSG.Point(1, 0, 0)),
                    new TSG.Vector(new TSG.Point(0, 1, 0)));
                CoordinateSystem endCS = new CoordinateSystem(
                    selectpoint1,
                    new TSG.Vector(new TSG.Point(selectpoint2.X, selectpoint2.Y, 0)),
                    new TSG.Vector(new TSG.Point(selectpoint2.Y, selectpoint2.X, 0)));
                switch (_Mirror)
                {
                    case 0:
                        break;
                    case 1:
                        endCS.AxisX = new TSG.Vector(new TSG.Point(-selectpoint2.X, -selectpoint2.Y, 0));
                        break;
                    case 2:
                        endCS.AxisY = new TSG.Vector(new TSG.Point(selectpoint2.Y, -selectpoint2.X, 0));
                        break;
                }
                
                
                booleanCP.Class = BooleanPart.BooleanOperativeClassName;
                booleanCP.Insert();
                Operation.MoveObject(booleanCP, startCS, endCS);

                BooleanPart booleanPart = new BooleanPart();
                booleanPart.Father = selectPart;
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
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the values from the dialog and sets the default values if needed
        /// </summary>
        private void GetValuesFromDialog()
        {
            _Height = Data.height;
            _Width = Data.width;
            _Radius = Data.radius;
            _OffsetH = Data.offsetH;
            _OffsetL = Data.offsetL;
            _TypeCut = Data.typeCut;
            _Mirror = Data.mirror;


            if (IsDefaultValue(_Height))
                _Height = 50;
            if (IsDefaultValue(_Width))
                _Width = 50;
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
