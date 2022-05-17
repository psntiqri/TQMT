
var _finalSearchURL = null;
var _finalSearchObject = null;
var _finalSearchOnSucsuss = null;
var _finalSearchOnFailure = null;
var _finalSearchTextBox = null;
var _lastUpdatedNumber = 0;
var _pageSize = 10;

function initLoadObjectCall(searchUrl, searchObject, onSucsuss, onFailiur, txtBox) {
    _lastUpdatedNumber++;
    _finalSearchURL = searchUrl;
    _finalSearchObject = searchObject;
    _finalSearchOnSucsuss = onSucsuss;
    _finalSearchOnFailure = onFailiur;
    _finalSearchTextBox = txtBox;
    setTimeout("InlineGlobalSearchList(" + _lastUpdatedNumber + ")", 500);
}

function InlineGlobalSearchList(key) {

    if (key == _lastUpdatedNumber) {
        _finalSearchTextBox.className = "globalSearchTextBox_busy";
        _finalSearchTextBox.blur();

        document.getElementById("TD_SearchResult").innerHTML = $("#DIV_BusyState").html();
        azyncPost(_finalSearchURL, _finalSearchObject, _finalSearchOnSucsuss, _finalSearchOnFailure);
    }

}

function initFunctionDelayedCall(onSucsuss) {
    _lastUpdatedNumber++;
    _finalSearchOnSucsuss = onSucsuss;
    setTimeout("InlineFunctionCallComplete(" + _lastUpdatedNumber + ")", 500);
}

function InlineFunctionCallComplete(key) {

    if (key == _lastUpdatedNumber) {
        _finalSearchOnSucsuss();
    }

}





