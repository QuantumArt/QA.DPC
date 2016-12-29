(function (Global) {
    'use strict';

    function getUrlHelpers() {
        var rootPath;
        return {
            Content: function (relativeUrl) {
                if (relativeUrl.substring(0, 1) === '~') {
                    relativeUrl = relativeUrl.substring(1);
                }

                if (relativeUrl.substring(0, 1) === '/') {
                    relativeUrl = relativeUrl.substring(1);
                }

                return rootPath + relativeUrl;
            },
            SetRootPath: function (rootUrl) {
                rootPath = rootUrl;
            }
        };
    }

    window.Url = window.Global.UrlHelpers = getUrlHelpers();
}(window.Global = window.Global || {}));
