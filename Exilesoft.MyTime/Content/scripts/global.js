var _lastUploadedImageSucsess = null;

function LoadMapServiceScripts() {
    var script = document.createElement("script");
    script.src = "http://maps.googleapis.com/maps/api/js?key=AIzaSyAtZ42gywIoM0ysQGc4E7iFlBOxDrIXmoQ&sensor=true&callback=initializeMap";
    document.body.appendChild(script);
}

function ShowLoginHome() {
    hidePageContent();
    var t = setTimeout("UpdateLoginPageContent('Home/Landing')", 500);
}

function AddLoginCompany() {
    hidePageContent();
    var t = setTimeout("UpdateLoginPageContent('Company/Index')", 500);
}

function AddServiceIntegrationCompany() {
    hidePageContent();
    var t = setTimeout("UpdateLoginPageContent('Company/ServiceIntegrationCompany')", 500);
}

function hidePageContent() {
    hide("Div_HomeSystemContent");
    show("Div_LodingCntent");
}

function UpdateLoginPageContent(loadUrl) {
    $('#Div_HomeSystemContent').html("");
    $('#Div_HomeSystemContent').load(loadUrl);
    var t = setTimeout("ShowLodedContent()", 1000);
}

function ShowLodedContent() {
    hide("Div_LodingCntent");
    show("Div_HomeSystemContent");
}

function blink(dataObj) {
    if (document.getElementById(dataObj).style.display == "none") {
        $("#" + dataObj).slideDown(500);
    }
    else {
        $("#" + dataObj).slideUp(500);
    }
}

function show(dataObj, fn) {
    $("#" + dataObj).slideDown(500, fn);
}

function hide(dataObj, fn) {
    $("#" + dataObj).slideUp(500, fn);
}

function ShowAlert(msg) {
    alert(msg);
}

function ShowSucsuss(msg) {
    alert(msg);
}

function AddWatermark(inputTextboxId, watermark) {
    $('#' + inputTextboxId).blur(function () {
        if ($(this).val().length == 0)
            $(this).val(watermark).addClass('watermark');
    }).focus(function () {
        if ($(this).val() == watermark)
            $(this).val('').removeClass('watermark');
    }).val(watermark).addClass('watermark');
}

function ConnectionError(err) {
    ShowAlert(err);
}

function BlinkFilterOptions() {
    blink("DIV_FilterOptionsLink");
    blink("DIV_FilterOptionsDetail");
}

function isValidDate(s) {
    try {
        var bits = s.split('/');
        var d = new Date(bits[2], bits[1] - 1, bits[0]);
        return d && (d.getMonth() + 1) == bits[1] && d.getDate() == Number(bits[0]);
    } catch (e) {
        return false;
    }
}

function isValidTime(s) {
    try {
        var bits = s.split(':');
        var hours = parseInt(bits[0]);
        var mins = parseInt(bits[1]);
        if (hours >= 0 && hours <= 23 && mins >= 0 && mins <= 59)
            return true;
        else
            return false;
    } catch (e) {
        return false;
    }
}

function getIframeWindow(iframe_object) {
    var doc;

    if (iframe_object.contentWindow) {
        return iframe_object.contentWindow;
    }

    if (iframe_object.window) {
        return iframe_object.window;
    }

    if (!doc && iframe_object.contentDocument) {
        doc = iframe_object.contentDocument;
    }

    if (!doc && iframe_object.document) {
        doc = iframe_object.document;
    }

    if (doc && doc.defaultView) {
        return doc.defaultView;
    }

    if (doc && doc.parentWindow) {
        return doc.parentWindow;
    }

    return undefined;
}