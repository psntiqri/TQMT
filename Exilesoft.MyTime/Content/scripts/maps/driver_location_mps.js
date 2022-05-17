var map;
var marker = null;

function initializeMap() {

    var myOptions = {
        zoom: 14,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);

    google.maps.event.addListener(map, 'click', function (event) {
        mapClickHandler(event.latLng);
    });

    checkExistingDeafultLocation();
}

function checkExistingDeafultLocation() {
    var _defaultLocation = $("#LastKnownLocation").val();
    if (_defaultLocation != "") {
        var _lat = _defaultLocation.substring(0, _defaultLocation.indexOf("@"));
        var _long = _defaultLocation.substring(_defaultLocation.indexOf("@") + 1);
        var center = new google.maps.LatLng(
            _lat, _long);
        var zoom = 15;
        map.setCenter(center, zoom);
        var latlng = new google.maps.LatLng(_lat, _long);
        placeMarker(latlng);
    }
    else {
        var center = new google.maps.LatLng(
            6.87, 79.90);
        var zoom = 6;
        map.setCenter(center, zoom);

        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(CurrentLocationSucsess, CurrentLocationError);
        } else {
            alert('geolocation not supported');
        }
    }
}

function mapClickHandler(location) {
    var geocoder = new google.maps.Geocoder();
    var lat = parseFloat(location.lat());
    var lng = parseFloat(location.lng());
    var _defaultLocation = location.lat() + '@' + location.lng();
    $("#LastKnownLocation").val(_defaultLocation);
    var latlng = new google.maps.LatLng(lat, lng);
    placeMarker(latlng);
    var driverID = $('#ObjectID').val();

    var _driverLocationObj = {
        driverId: driverID,
        location: _defaultLocation
    };
    azyncPost("/Drivers/UpdateLocation", _driverLocationObj, LocationUpdatedStatus, LocationUpdatedStatus);
}

function placeMarker(latlng) {
    var geocoder = new google.maps.Geocoder();
    geocoder.geocode({ 'latLng': latlng }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            if (results[1]) {
                if (marker != null)
                    marker.setMap(null);

                marker = new google.maps.Marker({
                    position: latlng,
                    map: map
                });

                var infowindow = new google.maps.InfoWindow();
                infowindow.setContent('<span class="page_hedder_mid">' + results[1].formatted_address + '</span>');
                infowindow.open(map, marker);
            }
        } else {
            alert("Geocoder failed due to: " + status);
        }
    });
}

function CurrentLocationSucsess(position) {
    var center = new google.maps.LatLng(
            position.coords.latitude,
            position.coords.longitude
            );
    var zoom = 15;
    map.setCenter(center, zoom);
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