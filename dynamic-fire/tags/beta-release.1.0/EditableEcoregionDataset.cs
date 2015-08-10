using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Fire
{
    /// <summary>
    /// Editable dataset of ecoregion parameters.
    /// </summary>
    public interface IEditableEcoregionDataset
        : IEditable<IEcoregionDataset>, IList<IEditableEcoregionParameters>
    {
        //---------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the parameters for a ecoregion.
        /// </summary>
        /// <param name="name">
        /// The ecoregion's name.
        /// </param>
        /// <remarks>
        /// If there are no parameters for the specified name, then the get
        /// accessor returns null.  If the value for the set accessor is null,
        /// then the parameters for the specified name are removed from the
        /// dataset (it is not an error if the name is not in the dataset).
        /// </remarks>
        IEditableEcoregionParameters this[string name]
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Finds an ecoregion's parameters by its map code.
        /// </summary>
        /// <param name="mapCode">
        /// The ecoregion's map code.
        /// </param>
        IEditableEcoregionParameters Find(ushort mapCode);
    }

    /// <summary>
    /// Editable dataset of ecoregion parameters.
    /// </summary>
    public class EditableEcoregionDataset
        : List<IEditableEcoregionParameters>, IEditableEcoregionDataset
    {
        /// <summary>
        /// Is each set of parameters in the dataset complete?
        /// </summary>
        public bool IsComplete
        {
            get {
                foreach (IEditableEcoregionParameters parameters in this)
                    if (! parameters.IsComplete)
                        return false;
                return true;
            }
        }

        //---------------------------------------------------------------------

        public new IEditableEcoregionParameters this[int index]
        {
            get {
                return base[index];
            }

            set {
                if (value == null)
                    RemoveAt(index);
                else
                    base[index] = value;
            }
        }

        //---------------------------------------------------------------------

        public IEditableEcoregionParameters this[string name]
        {
            get {
                int index = IndexOf(name);
                if (index >= 0)
                    return this[index];
                else
                    return null;
            }

            set {
                int index = IndexOf(name);
                if (index >= 0) {
                    if (value != null)
                        this[index] = value;
                    else
                        RemoveAt(index);
                }
                else {
                    if (value != null)
                        Add(value);
                }
            }
        }

        //---------------------------------------------------------------------

        public EditableEcoregionDataset()
        {
        }

        //---------------------------------------------------------------------

        public IEditableEcoregionParameters Find(ushort mapCode)
        {
            foreach (IEditableEcoregionParameters parameters in this)
                if (parameters.MapCode != null && parameters.MapCode.Actual == mapCode)
                    return parameters;
            return null;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Gets the index of a ecoregion in the dataset.
        /// </summary>
        /// <returns>
        /// -1 if the ecoregion is not in the dataset.
        /// </returns>
        public int IndexOf(string name)
        {
            for (int index = 0; index < Count; ++index)
                if (this[index].Name == name)
                    return index;
            return -1;
        }

        //---------------------------------------------------------------------

        public IEcoregionDataset GetComplete()
        {
            if (IsComplete) {
                IEcoregionParameters[] parameters = new IEcoregionParameters[Count];
                for (int index = 0; index < Count; ++index) {
                    parameters[index] = this[index].GetComplete();
                }
                return new EcoregionDataset(parameters);
            }
            else
                return null;
        }
    }
}
