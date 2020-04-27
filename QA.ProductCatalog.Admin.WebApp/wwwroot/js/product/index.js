var QA = QA || {};
QA.Product = QA.Product || {};

QA.Product.Index =
  QA.Product.Index ||
  (function() {
    var ls = window.localStorage;
    var init = function() {
      Quantumart.QP8.Interaction.checkHost(
        QA.Utils.hostId(),
        QA.Utils.getParent(),
        function(args) {
          if (args && args.success) {
          } else {
            console.error("QP is not available.");
          }
        }
      );

      $(".actionlink.active").click(function() {
        setParamsToLS();
        var $this = $(this);
        var create = $this.data("action-name") == "new_article";
        var fts = decodeURIComponent($this.data("fields-to-init"));
        var ftb = decodeURIComponent($this.data("fields-to-block"));
        var fth = decodeURIComponent($this.data("fields-to-hide"));

        if (fts) {
          try {
            var fieldsToSet = JSON.parse(fts);
          } catch (ex) {
            console.log(ex);
            fieldsToSet = [];
          }
        }

        if (ftb) {
          try {
            var fieldsToBlock = JSON.parse(ftb);
          } catch (ex) {
            console.log(ex);
            fieldsToBlock = [];
          }
        }

        if (fth) {
          try {
            var fieldsToHide = JSON.parse(fth);
          } catch (ex) {
            console.log(ex);
            fieldsToHide = [];
          }
        }

        QA.Integration.showQPForm(
          $this.data("entityid"),
          $this.data("parentid"),
          function(eventType, args) {
            console.log(args);
            if (args) {
              if (
                args.actionCode == "update_article" ||
                args.actionCode == "archive_article" ||
                args.actionCode == "remove_article" ||
                args.actionCode == "update_article_and_up" ||
                args.actionCode == "save_article_and_up" ||
                args.actionCode == "copy_article"
              ) {
                QA.Utils.refresh();
              }
            }
          },
          $this.data("action-window") == "true",
          create,
          fieldsToSet,
          fieldsToBlock,
          fieldsToHide,
          $this.data("action-name")
        );
      });

      $(".anchor").click(function() {
        var href = $(this).attr("href");
        if (href) {
          $(href)
            .parents(".collapsed")
            .removeClass("collapsed");
          $.scrollTo($(href), 500);
          return false;
        }
      });

      $(".toggle-all").click(function() {
        var numCollapsed = 0;
        var items = $(this)
          .closest(".collapsible")
          .find(".group-row.collapsible")
          .each(function(i, item) {
            if ($(item).hasClass("collapsed")) {
              numCollapsed += 1;
            }
          });

        items.each(function(i, item) {
          $.proxy(toggleCollapsing, item)(items.length == numCollapsed);
        });
      });

      $(".handle, .click-handle").click(function() {
        var p = $(this).closest(".collapsible");
        $.proxy(toggleCollapsing, p)();
      });

      $(".collapsible").each(function(i, elem) {
        var $e = $(elem);
        if (ls) {
          var id = $e.attr("id");
          var state = ls.getItem(id);
          if (state && !$e.hasClass(state)) $e.addClass(state);
        }
      });

      $(".control-image__img").each(function() {
        if (this.naturalWidth && this.naturalHeight) {
          let d = this.naturalWidth + "x" + this.naturalHeight;
          $(this)
            .parent()
            .find(".control-image__tooltip")
            .html(d)
            .attr("data-dimensions", d);
        }
      });

      $(".control-image__container").hover(
        function() {
          $(this)
            .find(".control-image__tooltip")
            .filter("[data-dimensions]")
            .removeClass("hidden");
        },
        function() {
          $(this)
            .find(".control-image__tooltip")
            .addClass("hidden");
        }
      );

      function setParamsToLS() {
        var activeElem = $("#" + QA.Utils.hostId()).context.activeElement;
        var elemID = $(activeElem).attr("aria-activedescendant");
        var tabElem = $("#" + elemID)
          .parents(".tabstrip")
          .first()
          .data("name");

        ls.setItem("activeTabName", tabElem);

        var childElem = $(activeElem).children(".k-state-active");

        ls.setItem("scrollPosition", childElem.scrollTop());
      }

      function toggleCollapsing(foldingMode) {
        if (foldingMode != undefined && foldingMode == false) {
          $(this).addClass("collapsed");
        } else if (foldingMode != undefined && foldingMode == true) {
          $(this).removeClass("collapsed");
        } else {
          $(this).toggleClass("collapsed");
        }
        if (ls) {
          var id = $(this).attr("id");
          ls.setItem(id, $(this).hasClass("collapsed") ? "collapsed" : "");
        }
      }
    };
    return { init: init };
  })();
