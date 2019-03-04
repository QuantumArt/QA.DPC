var QA = QA || {};
QA.Product = QA.Product || {};

QA.Product.TabStrip =
  QA.Product.TabStrip ||
  (function(window) {
    var localStorage = window.localStorage;

    var onSelect = function(e) {
      var title = $(e.item).text();
      var order = $(e.item).data("order");

      if (order && order != 0) {
        title = title + "<>" + order;
      }
      var root = $(e.item)
        .parents(".tabstrip")
        .first();
      var name = root.data("name");

      if (name) {
        var key = name;
        localStorage.setItem(key, title);
      }
    };

    var findSelected = function($tabstrip, defaultItemId, modelUId) {
      var key = $tabstrip.data("name");

      var selected = false;
      if (key) {
        var savedTabName = localStorage.getItem(key);
        var tabstrip = $tabstrip.data("kendoTabStrip");

        if (tabstrip && savedTabName && savedTabName != "") {
          $tabstrip.find(".tabstrip-header-item").each(function(i, item) {
            var $item = $(item);
            var title = $item.text();

            var order = $item.data("order");

            if (order && order != 0) {
              title = title + "<>" + order;
            }

            if (savedTabName === title) {
              tabstrip.activateTab($item);
              selected = true;
              return false;
            }
          });
        }
      }

      if (!selected) {
        var first = null;

        if (defaultItemId && defaultItemId != "") {
          first = $("#" + defaultItemId);
        } else {
          first = $("#" + modelUId + " > ul > .tabstrip-header-item")[0];
        }

        if (first) {
          $tabstrip.data("kendoTabStrip").activateTab(first);
        }
      }
    };

    var scrolling = function(tabStripElement) {
      var scrollingPositionTab = localStorage.getItem("scrollPosition");
      if (scrollingPositionTab) {
        if (scrollingPositionTab != 0) {
          tabStripElement
            .children(".k-state-active")
            .scrollTo(scrollingPositionTab + "px");
        }
        localStorage.removeItem("scrollPosition");
        localStorage.removeItem("activeTabName");
      }
    };

    var resize = function($elem) {
      $elem.data("kendoTabStrip").resize();
    };

    var init = function(modelUId, defaultItemId, fullSize, options) {
      var defaults = {
        select: onSelect,
        animation: false,
        tabPosition: top,
        collapsible: false
      };

      defaults = $.extend(defaults, options);

      var tabStripElement = $("#" + modelUId)
        .kendoTabStrip(defaults)
        .removeClass("hidden");

      findSelected(tabStripElement, defaultItemId, modelUId);

      var resizeAll = function() {
        resize(tabStripElement);
      };

      if (fullSize) {
        var t = resizeAll;
        resizeAll = function() {
          expandContentDivs(tabStripElement.children(".k-content"));
          resize(tabStripElement);
        };

        var tabStrip = tabStripElement.data("kendoTabStrip");

        var expandContentDivs = function(divs) {
          divs.height(
            tabStripElement.innerHeight() -
              tabStripElement.children(".k-tabstrip-items").outerHeight() -
              16
          );
        };

        tabStripElement.parent().attr("id", "tabstrip-parent");
      }

      $(window).resize(
        $.throttle(250, function() {
          resizeAll();
        })
      );

      resizeAll();

      var scrollingTab = localStorage.getItem("activeTabName");
      if (scrollingTab == tabStripElement.data("name")) {
        scrolling(tabStripElement);
      }
    };

    return { init: init };
  })(window);
