/*
This plugin is to create google like search (autocomplete) 

required feilds ,
sourceUrl: url for the controller action which should return ajax response with a list of "AutoCompleteResult" DTO
if you want to override jquery ui autocomple setting use  'jqueryAutoCompleteSettings' which will be use as the underline jquery ui autocomplete settings

sourceUrl: '', //required url to controller action
parameterMap:function () {} , //optional function to return json object for the filter parameters if there any
jqueryAutoCompleteSettings: {}//optional settings for jquery ui auto complete
selected: function (event, ui) {}//optional event fired after selected
fieldName:'',//optional name of the field to be edited

*/

(function ($) {
    jQuery.fn.myTimeAutoComplete = function (options) {
        var self = this;
        var currentUserText = '';

        self.constant = {

        };

        self.settings = {
            sourceUrl: '', //url to controller action
            parameterMap: function () { return {}; }, // data to use in search
            fieldName: '', //name of the feild to be edited
            fieldContext: '',
            jqueryAutoCompleteSettings: {//settings for jquery ui auto complete
                source: function (request, response) {
                    $.ajax({
                        url: options.sourceUrl,
                        dataType: "json",
                        contentType: "application/json; charset=utf-8",
                        type: "post",
                        data: JSON.stringify($.extend({ text: request.term }, options.parameterMap == undefined ? {} : options.parameterMap())),
                        success: function (ajaxResponse) {
                            response(ajaxResponse.Data);
                        }
                    });
                },
                select: function (event, ui) {

                    $('#' + self.settings.fieldName, self.settings.fieldContext).val(ui.item.Id);
                    self.val(ui.item.SelectingText);

                    setTimeout(function () {//fix for jquery ui timeout
                        self.val(ui.item.SelectingText);
                        // if (event.which == 13) {//if enter key
                        self.trigger('change').trigger('blur');
                        // }
                    }, 5);


                    self.settings.selected(event, ui);
                    // self.val(ui.item.SelectingText);
                },
                autoFocus: true,
                delay: 1000,
                focus: function (event, ui) {
                    if ($('ul li:hover', self.parent()).length) {
                        self.val(ui.item.SelectingText);
                    }
                }
            },
            selected: function (event, ui) {//event fired after selected

            }

        };

        if (options.jqueryAutoCompleteSettings != undefined) {
            options.jqueryAutoCompleteSettings = $.extend(self.settings.jqueryAutoCompleteSettings, options.jqueryAutoCompleteSettings);
        }

        self.settings = $.extend(self.settings, options);

        self.renderMenu = function (ul, items) {
            ul.append(items[0].HeaderHtml);
            $.each(items, function (index, item) {
                self.redermenuItem(ul, item);
            });
            //ul.find('span').css('width', 170);
        },

        self.redermenuItem = function (ul, item) {
            var icon = "";
            if (item.Icon != null)
                icon = "<img src='" + jConect.core.config.baseUrl + "" + item.Icon + "' width='15'> ";
            item.label = item.DisplayText;
            item.value = item.SelectingText;
            return $("<li>")
                .append("<a id=" + item.Id + ">" + icon + item.DisplayText + "</a>").data("item.autocomplete", item)
                .appendTo(ul);
        };

        self.onkeyUp = function (event) {
            currentUserText = $(this).val();
        };

        return this.each(function () {
            var element = $(this);
            if (element.hasClass('ui-autocomplete-input'))
                return;
            var data = element.autocomplete(self.settings.jqueryAutoCompleteSettings).data("autocomplete");
            data._renderMenu = self.renderMenu;

            if (self.settings.fieldName != '' && !$('#' + self.settings.fieldName, self.settings.fieldContext).is('*')) {
                $('<input type="text" name=' + self.settings.fieldName + ' id=' + self.settings.fieldName + ' />', self.settings.fieldContext).insertAfter(element).hide();
            }

            element.keyup(self.onkeyUp);
        });
    };
})(jQuery);
