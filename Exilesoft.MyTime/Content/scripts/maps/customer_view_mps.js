var map;
var marker = null;

function initializeMap() {
    var myOptions = {
        zoom: 12,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);    
    checkExistingDeafultLocation();
}

function checkExistingDeafultLocation() {
    var _defaultLocation = $("#DefaultLocation").val();
    if (_defaultLocation != "") {
        var _lat = _defaultLocation.substring(0, _defaultLocation.indexOf("@"));
        var _long = _defaultLocation.substring(_defaultLocation.indexOf("@") + 1);
        var center = new google.maps.LatLng(
            _lat, _long);
        var zoom = 12;
        map.setCenter(center, zoom);
        var latlng = new google.maps.LatLng(_lat, _long);
        placeMarker(latlng);
    }
    else {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(CurrentLocationSucsess, CurrentLocationError);
        } else {
            alert('geolocation not supported');
        }
    }
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

$(document).ready(function () {
    initializeMap();
});