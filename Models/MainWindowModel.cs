using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            Part part = (Part)Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART);
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

                ArrayList Points = (ArrayList)Input[0].GetInput();
                Part part = (Part)Model.SelectModelObject((Identifier)Input[1].GetInput());
                ContourPoint point1 = new ContourPoint(Points[0] as TSG.Point, null);
                ContourPoint point2 = new ContourPoint(Points[1] as TSG.Point, null);
                ContourPoint point3 = new ContourPoint(new TSG.Point(point1.X, _Height, 0), null);

                ContourPlate contourPlate = new ContourPlate();
                contourPlate.Profile.ProfileString = "PL10";
                contourPlate.Material.MaterialString = "Steel_Undefined";
                contourPlate.AddContourPoint(point1);
                contourPlate.AddContourPoint(point2);
                contourPlate.AddContourPoint(point3);
                contourPlate.Class = BooleanPart.BooleanOperativeClassName;
                contourPlate.Insert();

                BooleanPart booleanPart = new BooleanPart();
                booleanPart.Father = part;
                booleanPart.SetOperativePart(contourPlate);
                booleanPart.Insert();
                contourPlate.Delete();
                Model.CommitChanges();
                Operation.DisplayPrompt("Test");

            }
            catch (Exception Exc)
            {
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


            if (IsDefaultValue(_Height))
                _Height = 50;

        }

        // Write your private methods here.

        #endregion
    }
}
