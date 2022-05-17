// -------------DEVELOPER COMMENT---------------------------//
//
// Filename     : ViewModelBase.cs
// Created By   : Harinda Dias
// Date         : 2013-Jul-11, Thu
// Description  : View Model Base Object for the object construction

//
// Modified By  : 
// Date         : 
// Purpose      : 
//
// ---------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    /// <summary>
    /// View Model Base Object for the
    /// Object Construction. 
    /// </summary>
    public abstract class ViewModelBase
    {
        // Function calling for the internal object Initialization.
        // will use the base implementation by default
        // If required, then overide the default implementation 
        // on the concrete class inside.
        public void InitializeViewModel()
        {
            // Default function calling
            this.InitializeViewModelImpl();
        }

        // Function calling for the bindable properties in the User interface.
        // will use the base implementation by default
        // If required, then overide the default implementation 
        // on the concrete class inside.
        public void FillBindableModelData()
        {
            // Default function calling
            this.FillBindableModelDataImpl();
        }

        // Function calling for Object data filling if it is from the database.
        // will use the base implementation by default
        // If required, then overide the default implementation 
        // on the concrete class inside.
        public void UpdateObjectModelData()
        {
            // Default function calling
            this.UpdateObjectModelDataImpl();
        }

        /// <summary>
        /// Default Function implementation
        /// Overide this if required in the concrete implementation
        /// </summary>
        protected virtual void InitializeViewModelImpl()
        {
            ///Overide and do the implementation in the concrete class
        }

        /// <summary>
        /// Default Function implementation
        /// Overide this if required in the concrete implementation
        /// </summary>
        protected virtual void FillBindableModelDataImpl()
        {
            ///Overide and do the implementation in the concrete class
        }

        /// <summary>
        /// Default Function implementation
        /// Overide this if required in the concrete implementation
        /// </summary>
        protected virtual void UpdateObjectModelDataImpl()
        {
            ///Overide and do the implementation in the concrete class
        }
    }
}