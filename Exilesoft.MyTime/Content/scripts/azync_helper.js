
// Maintain the lock for the operation.
// will be released after the completion of previous request.
var _systemUpdateOperationLock = false;

// Option to call the server operation with the locking facility. Ex. Saving/Deleting operation
// Param : URL - URL of the operation action to be called.
// Param : dataObj - Object to be passed to the server as a json. Ex-Viewmodel javascript object
// Param : succesFnc - Function to be called after the sucsessfull completion of the operation,
//         Required to implement the functionality in the implementing form.
// Param : errorFnc - Function to be called if any error occured in the function call
//         Required to implement the functionality in the implementing form.

function azyncPost(URL, dataObj, succesFnc, errorFnc) {
    
    // Check if the lock is on, If its locked then return without 
    // calling the server function.
    if (_systemUpdateOperationLock == true) {
        alert("Current operation is in progress. Please wait.");
        return;
    }

    // Server call with jQuery
    $.ajax({
        url: URL,
        data: JSON.stringify(dataObj),
        type: 'POST',
        contentType: 'application/json;',
        dataType: 'json',
        beforeSend: function () {
            // Locking the operation to avoid conflicts.
            _systemUpdateOperationLock = true;
            document.getElementById("SystemMainAzyncLoadingDiv").style.display = "block";
        },
        success: function (result) {
            // Comment by Harinda Dias
            // Releasing the lock for enable other operations.
            _systemUpdateOperationLock = false;
            document.getElementById("SystemMainAzyncLoadingDiv").style.display = "none";
            // Calling the sucesess function 
            succesFnc(result);
        },
        error: function (xhr, desc, err) {
            // Releasing the lock for enable other operations.
            _systemUpdateOperationLock = false;
            document.getElementById("SystemMainAzyncLoadingDiv").style.display = "none";
            console.log(xhr);
            console.log("Desc: " + desc + "\nErr:" + err);
            // Calling the function to be handled when an error occured.
            errorFnc(err);
        }
    });
}

// Option to call the server operation without any locking requirement. Ex. Selection operation
// Param : URL - URL of the operation action to be called.
// Param : dataObj - Object to be passed to the server as a json. Ex-Viewmodel javascript object
// Param : succesFnc - Function to be called after the sucsessfull completion of the operation,
//         Required to implement the functionality in the implementing form.
// Param : errorFnc - Function to be called if any error occured in the function call
//         Required to implement the functionality in the implementing form.
function azyncGet(URL, dataObj, succesFnc, errorFnc) {

    // Server call with jQuery
    $.ajax({
        url: URL,
        data: JSON.stringify(dataObj),
        type: 'POST',
        contentType: 'application/json;',
        dataType: 'json',
        beforeSend: function () {
            document.getElementById("SystemMainAzyncLoadingDiv").style.display = "block";
        },
        success: function (result) {
            document.getElementById("SystemMainAzyncLoadingDiv").style.display = "none";
            // Calling the sucesess function 
            succesFnc(result);
        },
        error: function (xhr, desc, err) {
            console.log(xhr);
            console.log("Desc: " + desc + "\nErr:" + err);
            document.getElementById("SystemMainAzyncLoadingDiv").style.display = "none";
            // Calling the function to be handled when an error occured.
            errorFnc(err);
        }
    });
}