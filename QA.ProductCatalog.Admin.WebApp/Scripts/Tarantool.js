(function () {
    function ModelData() {
        this.state = ko.observable('NotRunned');
        this.progress = ko.observable(0);
        this.canImport = ko.pureComputed(function () {
            var state = this.state();
            return state != 'Running' && state != 'Canceling' && state != 'NotAvailable';
        }, this);
        this.canStop = ko.pureComputed(function () {
            var state = this.state();
            return state == 'Running';
        }, this);
        this.getStateLogo = ko.pureComputed(function () {
            var state = this.state();
            var src = null;
            if (state == 'Running') {
                src = 'Content/img/TaskStates/Running16.png';
            } else if (state == 'Finished') {
                src = 'Content/img/TaskStates/Done16.png';
            } else if (state == 'Error' || state == 'NotAvailable') {
                src = 'Content/img/TaskStates/Failed16.png';
            } else if (state == 'Canceled' || state == 'Canceling') {
                src = 'Content/img/TaskStates/Cancelled16.png';
            } else if (state == 'NotRunned') {
                src = 'Content/img/TaskStates/New16.png';
            } else {
                src = 'Content/img/icons/0.gif';
            }

            return Url.Content(src);
        }, this);
    }

    var model = new ModelData();

    function update(json) {
        model.state(json.state);
        model.progress(json.progress);
    }

    function updateWithError() {
        model.state('NotAvailable');
        model.progress(0);
    }

    function updateModel(loop) {
        $.get(Url.Content('~/Tarantool/get?customerCode=' + getCustomerCode()))
            .done(update)
            .fail(updateWithError);

        if (loop) {
            setTimeout(function () {
                updateModel(loop);
            }, 2000);
        }
    }

    function getCustomerCode() {
        return $('#customercode').data('customerCode');
    }

    $(document).ready(function () {
        moment.locale('ru');
        ko.bindingHandlers.updateProgress = {
            init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                $(element).kendoProgressBar({
                    value: viewModel.progress(),
                    type: 'percent'
                });
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                $(element).data('kendoProgressBar').value(viewModel.progress());
            }
        };

        ko.applyBindings(model);
        updateModel(true);

        $('#import').click(function () {
            $.post(Url.Content('~/Tarantool/import?customerCode=' + getCustomerCode()))
                .done(update)
                .fail(updateWithError);
        });

        $('#stop').click(function () {
            $.post(Url.Content('~/Tarantool/stop?customerCode=' + getCustomerCode()))
                .done(update)
                .fail(updateWithError);
        });
    });
})();