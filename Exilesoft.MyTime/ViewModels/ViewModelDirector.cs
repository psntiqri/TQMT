// -------------DEVELOPER COMMENT---------------------------//
//
// Filename     : ViewModelDirector.cs
// Created By   : Harinda Dias
// Date         : 2013-Jul-11, Thu
// Description  : View Model Creational Director for the Object Construction. 

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
    /// View Model Creational Director for the
    /// Object Construction. 
    /// </summary>
    public class ViewModelDirector
    {
        /// <summary>
        /// Use this method to provide the object construction
        /// Will handle internally the internal implementation
        /// </summary>
        /// <param name="baseModel">Base view model object for the contruction</param>
        public void Construct(ViewModelBase baseModel)
        {
            // Function calling for the internal object Initialization.
            // will use the base implementation by default
            // If required, then overide the default implementation 
            // on the concrete class inside.
            baseModel.InitializeViewModel();

            // Function calling for the bindable properties in the User interface.
            // will use the base implementation by default
            // If required, then overide the default implementation 
            // on the concrete class inside.
            baseModel.FillBindableModelData();

            // Function calling for Object data filling if it is from the database.
            // will use the base implementation by default
            // If required, then overide the default implementation 
            // on the concrete class inside.
            baseModel.UpdateObjectModelData();
        }
    }
}