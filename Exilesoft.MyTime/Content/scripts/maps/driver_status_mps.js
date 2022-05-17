var driver_status_viewmap;
var infowindow = null;
var markersArray = new Array();
var allDriverList = null;

function initializeMap() {

    var myOptions = {
        zoom: 12,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    driver_status_viewmap = new google.maps.Map(document.getElementById("driver_statusmap_canvas"), myOptions);
    checkExistingDeafultLocation();

    google.maps.event.addListenerOnce(driver_status_viewmap, 'idle', function () {
        google.maps.event.trigger(driver_status_viewmap, 'resize');
        driver_status_viewmap.setZoom(driver_status_viewmap.getZoom());
    });
}

function checkExistingDeafultLocation() {
    var center = new google.maps.LatLng(
            6.87, 79.90);
    var zoom = 6;
    driver_status_viewmap.setCenter(center, zoom);

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(CurrentLocationSucsess, CurrentLocationError);
    } else {
        alert('geolocation not supported');
    }

    azyncPost("/Drivers/GetAllDriverLocations", null, DriverLocationsListUpdateSucsussfull, DriverLocationsListUpdateError);
}

function DriverLocationsListUpdateSucsussfull(result) {
    allDriverList = result;
    ShowDriverLocations(allDriverList);
}

function ShowDriverLocations(result) {
    
    for (var i = 0; i < markersArray.length; i++) {
        markersArray[i].setMap(null);
    }
    $.each(result, function () {
        $.each(this, function () {
            var _defaultLocation = this.Location;
            if (_defaultLocation != "") {
                var _lat = _defaultLocation.substring(0, _defaultLocation.indexOf("@"));
                var _long = _defaultLocation.substring(_defaultLocation.indexOf("@") + 1);
                var latlng = new google.maps.LatLng(_lat, _long);
                placeMarker(latlng, this);
            }
        });
    });
}

function DriverLocationsListUpdateError(err) {
    ShowAlert(err);
}

function UpdateDriverList() {
    ShowDriverLocations(allDriverList);
}

function placeMarker(latlng, driver) {
    var contentString = $("#DIV_DriverInfoBox").html();
    contentString = contentString.replace("##Title", driver.Title);
    contentString = contentString.replace("##Telephone", driver.Telephone);
    contentString = contentString.replace("##Mobile", driver.Mobile);
    contentString = contentString.replace("##Status", driver.AvailabilityStatus);

    if (driver.ImagePath == "")
        contentString = contentString.replace("##ImagePath", "../../Content/images/default_user.png");
    else
        contentString = contentString.replace("##ImagePath", "../../Content/images/dynamic/" + driver.ImagePath);

    var mapIcon = '../../Content/images/taxi_availabel_marker.png';
    if (driver.AvailabilityStatus != "Available")
        mapIcon = '../../Content/images/taxi_notavailabel_marker.png';

    var _displayAvailable = $('#CheckBox_ViewAvailableDrivers').is(':checked');
    var _displayNotAvailable = $('#CheckBox_ViewNotAvailableDrivers').is(':checked');

    if ((_displayAvailable && driver.AvailabilityStatus == "Available") ||
    (_displayNotAvailable && driver.AvailabilityStatus != "Available")) {
        var marker = new google.maps.Marker({
            position: latlng,
            icon: mapIcon,
            map: driver_status_viewmap
        });

        markersArray.push(marker);

        google.maps.event.addListener(marker, 'click', function () {
            if (infowindow) {
                infowindow.close();
            }
            infowindow = new google.maps.InfoWindow({
                content: contentString
            });
            infowindow.open(driver_status_viewmap, marker);
        });
    }
}

function CurrentLocationSucsess(position) {
    var center = new google.maps.LatLng(
            position.coords.latitude,
            position.coords.longitude
            );
    var zoom = 15;
    driver_status_viewmap.setCenter(center, zoom);
}


function CurrentLocationError(msg) {
    alert('error: ' + msg);
}

function LocationUpdatedStatus(msg) {
    if (msg != "Success")
        ShowAlert(msg);
}

$(document).ready(function () {
    initializeMap();
});
