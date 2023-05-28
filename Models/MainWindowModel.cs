using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tekla.Structures;
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
        [StructuresField("typeCut")]
        public int typeCut;
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
        private int _TypeCut = 0;

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


                Solid solidpart = selectPart.GetSolid();
                double centerpart = (solidpart.MaximumPoint.Z + solidpart.MinimumPoint.Z)/2;


                ContourPlate booleanCP = new ContourPlate();
                double thicknessCut = Math.Round(Math.Abs(solidpart.MaximumPoint.Z) + Math.Abs(solidpart.MinimumPoint.Z)) + 10;
                booleanCP.Profile.ProfileString = $"PL{thicknessCut}";
                booleanCP.Material.MaterialString = "Steel_Undefined";

                switch (_TypeCut)
                {
                    case 0:
                        ContourPoint point1 = new ContourPoint(new TSG.Point(0, 0, centerpart), null);
                        ContourPoint point2 = new ContourPoint(new TSG.Point(0, _Height, centerpart), null);
                        ContourPoint point3 = new ContourPoint(new TSG.Point(_Width, _Height, centerpart), null);
                        ContourPoint point4 = new ContourPoint(new TSG.Point(_Width, 0, centerpart), null);
                        booleanCP.AddContourPoint(point1);
                        booleanCP.AddContourPoint(point2);
                        booleanCP.AddContourPoint(point3);
                        booleanCP.AddContourPoint(point4);
                        break;
                    default:
                        break;
                }
                

                booleanCP.Class = BooleanPart.BooleanOperativeClassName;
                booleanCP.Insert();

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
            _TypeCut = Data.typeCut;


            if (IsDefaultValue(_Height))
                _Height = 50;

            if (IsDefaultValue(_Width))
                _Width = 50;
            if (IsDefaultValue(_TypeCut))
                _TypeCut = 50;

        }

        // Write your private methods here.

        #endregion
    }
}
