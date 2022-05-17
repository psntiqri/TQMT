
function AddWatermark(inputTextboxId, watermark) {
    $('#' + inputTextboxId).blur(function () {
        if ($(this).val().length == 0)
            $(this).val(watermark).addClass('watermark');
    }).focus(function () {
        if ($(this).val() == watermark)
            $(this).val('').removeClass('watermark');
    }).val(watermark).addClass('watermark');
}

function show(dataObj, fn) {
    $("#" + dataObj).slideDown(500, fn);
}

function hide(dataObj, fn) {
    $("#" + dataObj).slideUp(500, fn);
}

function ConnectionError() {
}

function GetDateFromText(txtID) {
    var dateStrArr = $("#" + txtID).val().split("/");
    return new Date(dateStrArr[2], dateStrArr[1] - 1, dateStrArr[0]);
}

function MonthDiff(d1, d2) {
    // Months between years.
    var months = (d2.getFullYear() - d1.getFullYear()) * 12;

    // Months between... months.
    months += d2.getMonth() - d1.getMonth();

    // Subtract one month if b's date is less that a's.
    if (d2.getDate() < d1.getDate()) {
        months--;
    }
    return months;
}

function setCookie(c_name, value, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var c_value = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());
    document.cookie = c_name + "=" + c_value;
}

function getCookie(c_name) {
    var c_value = document.cookie;
    var c_start = c_value.indexOf(" " + c_name + "=");
    if (c_start == -1) {
        c_start = c_value.indexOf(c_name + "=");
    }
    if (c_start == -1) {
        c_value = null;
    }
    else {
        c_start = c_value.indexOf("=", c_start) + 1;
        var c_end = c_value.indexOf(";", c_start);
        if (c_end == -1) {
            c_end = c_value.length;
        }
        c_value = unescape(c_value.substring(c_start, c_end));
    }
    return c_value;
}


function GetSearchTextBoxText(txt)
{
    var className = $('#' + txt).attr('class');
    if (className.indexOf("watermark") != -1)
        return "";
    else
        return $('#' + txt).val();
}

function IsStartNEndDatesAreNotValid(startDateControllerId, endDateControllerId) {
	var startDate = GetDateFromText(startDateControllerId);
	var endDate = GetDateFromText(endDateControllerId);

	if (startDate >= endDate) {
		alert("Invalid date range. Please try again.");
		return true;
	}
	return false;
}

function IsStartDateEndDateValidator(startDateControllerId, endDateControllerId) {
    var startDate = GetDateFromText(startDateControllerId);
    var endDate = GetDateFromText(endDateControllerId);

    if (startDate > endDate) {
        alert("Invalid date range. Please try again.");
        return true;
    }
    return false;
}
