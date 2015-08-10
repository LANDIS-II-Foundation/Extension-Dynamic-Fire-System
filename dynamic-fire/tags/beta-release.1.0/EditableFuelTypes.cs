//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Fire
{

    /// <summary>
    /// Editable definition of a Fire severity.
    /// </summary>
    public interface IEditableFuelTypes
        : IEditable<IFuelTypeParameters>
    {
        /// <summary>
        /// The Fuel Type Initiation Probability
        /// </summary>
        InputValue<double> InitiationProbability
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Fuel type coefficient A.
        /// </summary>
        InputValue<int> A
        {
            get;
            set;
        }
        /// <summary>
        /// Fuel type coefficient B.
        /// </summary>
        InputValue<double> B
        {
            get;
            set;
        }
        /// <summary>
        /// Fuel type coefficient C.
        /// </summary>
        InputValue<double> C
        {
            get;
            set;
        }
        /// <summary>
        /// Fuel type coefficient Q.
        /// </summary>
        InputValue<double> Q
        {
            get;
            set;
        }
        /// <summary>
        /// Fuel type coefficient BUI.
        /// </summary>
        InputValue<int> BUI
        {
            get;
            set;
        }
        /// <summary>
        /// Fuel type coefficient maximum BE.
        /// </summary>
        InputValue<double> MaxBE
        {
            get;
            set;
        }
        /// <summary>
        /// Fuel type coefficient Crown Base Height (CBH).
        /// </summary>
        InputValue<int> CBH
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Editable definition of Fire Types.
    /// </summary>
    public class EditableFuelTypes
        : IEditableFuelTypes
    {
        private InputValue<double> initiationProbability;
        private InputValue<int> a;
        private InputValue<double> b;
        private InputValue<double> c;
        private InputValue<double> q;
        private InputValue<int> bui;
        private InputValue<double> maxBE;
        private InputValue<int> cbh;

        //---------------------------------------------------------------------

        /// <summary>
        /// Initiation Probability for Fuel type
        /// </summary>
        public InputValue<double> InitiationProbability
        {
            get {
                return initiationProbability;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0.0 || value.Actual > 1.0)
                        throw new InputValueException(value.String,
                            "Value must be between 0 and 1.0");
                }
                initiationProbability = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<int> A
        {
            get {
                return a;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 || value.Actual > 1000)
                        throw new InputValueException(value.String,
                            "Value must be between 0 and 1000");
                }
                a = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> B
        {
            get {
                return b;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0.0 || value.Actual > 1.0)
                        throw new InputValueException(value.String,
                            "Value must be between 0 and 1.0");
                }
                b = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> C
        {
            get {
                return c;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0.0 || value.Actual > 10.0)
                        throw new InputValueException(value.String,
                            "Value must be between 0 and 10.0");
                }
                c = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> Q
        {
            get {
                return q;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0.0 || value.Actual > 1.0)
                        throw new InputValueException(value.String,
                            "Value must be between 0 and 1.0");
                }
                q = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<int> BUI
        {
            get {
                return bui;
            }

            set {
                if (value != null) {
                    if (value.Actual < 1 || value.Actual > 500)
                        throw new InputValueException(value.String,
                            "Value must be between 1 and 500");
                }
                bui = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> MaxBE
        {
            get {
                return maxBE;
            }

            set {
                if (value != null) {
                    if (value.Actual < 1.0 || value.Actual > 2.0)
                        throw new InputValueException(value.String,
                            "Value must be between 1 and 2.0");
                }
                maxBE = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<int> CBH
        {
            get {
                return cbh;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 || value.Actual > 500)
                        throw new InputValueException(value.String,
                            "Value must be between 0 and 500");
                }
                cbh = value;
            }
        }
        //---------------------------------------------------------------------

        public EditableFuelTypes()
        {
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                foreach (object parameter in new object[]{ 
                                                           initiationProbability,
                                                           a,b,c,q,bui,maxBE,cbh}) {
                    if (parameter == null)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------

        public IFuelTypeParameters GetComplete()
        {
            if (IsComplete)
                return new FuelTypeParameters(
                                    initiationProbability.Actual,
                                    a.Actual,
                                    b.Actual,
                                    c.Actual,
                                    q.Actual,
                                    bui.Actual,
                                    maxBE.Actual,
                                    cbh.Actual);
            else
                return null;
        }
    }
}
