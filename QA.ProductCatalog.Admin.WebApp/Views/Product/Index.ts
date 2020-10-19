window.QA = window.QA || {};
const QA = window.QA;
QA.Product = QA.Product || {};

QA.Product.Index =
  QA.Product.Index ||
  (function() {
    const localStorage = window.localStorage;
    const init = function() {
      window.Quantumart.QP8.Interaction.checkHost(QA.Utils.hostId(), QA.Utils.getParent(), args => {
        if (args && args.success) {
        } else {
          console.error("QP is not available.");
        }
      });

      $(".actionlink.active").on("click", function() {
        const $this = $(this);
        const create = $this.data("action-name") == "new_article";
        const fts = decodeURIComponent($this.data("fields-to-init"));
        const ftb = decodeURIComponent($this.data("fields-to-block"));
        const fth = decodeURIComponent($this.data("fields-to-hide"));

        let fieldsToSet, fieldsToBlock, fieldsToHide;

        if (fts) {
          try {
            fieldsToSet = JSON.parse(fts);
          } catch (ex) {
            console.log(ex);
            fieldsToSet = [];
          }
        }

        if (ftb) {
          try {
            fieldsToBlock = JSON.parse(ftb);
          } catch (ex) {
            console.log(ex);
            fieldsToBlock = [];
          }
        }

        if (fth) {
          try {
            fieldsToHide = JSON.parse(fth);
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

      $(".anchor").on("click", function() {
        const href = $(this).attr("href");
        if (href) {
          $(href)
            .parents(".collapsed")
            .removeClass("collapsed");
          $.scrollTo($(href), 500);
        }
      });

      $(".toggle-all").on("click", function() {
        let numCollapsed = 0;
        const items = $(this)
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

      $(".handle, .click-handle").on("click", function() {
        const p = $(this).closest(".collapsible");
        $.proxy(toggleCollapsing, p)(p);
      });

      $(".collapsible").each(function(i, elem) {
        const $e = $(elem);
        if (localStorage) {
          const id = $e.attr("id");
          const state = localStorage.getItem(id);
          if (state && !$e.hasClass(state)) $e.addClass(state);
        }
      });

      $(".control-image__img").each(function(i: number, el: HTMLImageElement) {
        if (el.naturalWidth && el.naturalHeight) {
          let d = el.naturalWidth + "x" + el.naturalHeight;
          $(el)
            .parent()
            .find(".control-image__tooltip")
            .html(d)
            .attr("data-dimensions", d);
        }
      });

      $(".control-image__container").on(
        "hover",
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

      function toggleCollapsing(foldingMode) {
        if (foldingMode != undefined && foldingMode == false) {
          $(this).addClass("collapsed");
        } else if (foldingMode != undefined && foldingMode == true) {
          $(this).removeClass("collapsed");
        } else {
          $(this).toggleClass("collapsed");
        }
        if (localStorage) {
          const id = $(this).attr("id");
          localStorage.setItem(id, $(this).hasClass("collapsed") ? "collapsed" : "");
        }
      }
    };
    return { init: init };
  })();

QA.Product.TabStrip =
  QA.Product.TabStrip ||
  (function(window: Window) {
    const localStorage = window.localStorage;
    let initialized = false;

    const activateTab = (index: number, $tabs: JQuery, $panels: JQuery) => {
      if (!initialized) return;
      $tabs.each(function(i, el) {
        if (i === index) {
          $(el).attr("aria-selected", "true");
        } else {
          $(el).attr("aria-selected", "false");
        }
      });
      $panels.each(function(i, el) {
        if (i === index) {
          $(el).removeClass("hidden");
        } else {
          $(el).addClass("hidden");
        }
      });
    };

    const findSelected = ($tabStripParent, defaultItemId, modelUId) => {
      const key = $tabStripParent.data("name");
      const $tabListParent = $tabStripParent.children(".bp3-tab-list");
      const $tabs = $tabListParent.children();
      const $panels = $tabStripParent.children(".bp3-tab-panel");

      let selected = false;
      if (key) {
        const savedTabName = localStorage.getItem(key);
        if (savedTabName && savedTabName !== "") {
          $tabs.each(function(i, item) {
            const $item = $(item);
            let title = $item.text();
            const order: number = $item.data("order");
            if (order && order !== 0) {
              title = `${title} <> ${order}`;
            }
            if (savedTabName === title) {
              activateTab(i, $tabs, $panels);
              selected = true;
            }
          });
        }
      }

      if (!selected) {
        let first: JQuery;
        if (defaultItemId && defaultItemId != "") {
          first = $("#" + defaultItemId).first();
        } else {
          first = $("#" + modelUId + " > ul > .tabstrip-header-item").first();
        }
        if (first) {
          activateTab(0, $tabs, $panels);
        }
      }
    };

    const init = function(
      modelUId: string,
      defaultItemId: string,
      fullSize: boolean,
      options: {
        tabPosition: string;
        collapsible: boolean;
      }
    ) {
      const $tabStripParent = $("#" + modelUId);
      const $tabListParent = $tabStripParent.children(".bp3-tab-list");
      const $tabs = $tabListParent.children();
      const $panels = $tabStripParent.children(".bp3-tab-panel");
      $tabStripParent.removeClass("hidden");
      initialized = true;

      $tabs.on("click", function(e) {
        const $tab = $(e.target);
        const order = $tab.data("order");
        activateTab(order, $tabs, $panels);
        let title = $tab.text();
        if (order && order != 0) {
          title = `${title} <> ${order}`;
        }
        const name = $tabStripParent.data("name");
        if (name) {
          localStorage.setItem(name, title);
        }
      });

      findSelected($tabStripParent, defaultItemId, modelUId);
    };

    return { init: init };
  })(window);
