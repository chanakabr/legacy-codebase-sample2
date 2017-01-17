var NumberField = function (params, attachToElement, pageName, toChangeStatus) {
    var numberField, numberFieldNotEmpty, oldnumberField;
    var pageNameToCall = pageName;
    if (toChangeStatus)
    {
        numberField = "";
        numberFieldNotEmpty = false;

    }
    else if (params.NumberField >= 0) {
        numberField = params.NumberField;
        numberFieldNotEmpty = true;
    }
    else {
        numberField = "";
        numberFieldNotEmpty = false;
    }

    var numberComponent;

    var init = function () {
        createNumberComponent();
        bindEvents();
    };

    var createNumberComponent = function () {
        $(attachToElement).prepend($('<input class="channel-number" />').val(numberField));
    };

    var bindEvents = function () {
        var $numberComponent = $(attachToElement);
        $numberComponent.find('input[class="channel-number"]').on('blur', inputBlurHandler);
        $numberComponent.find('input[class="channel-number"]').on('focus', inputFocusHandler);
    };

    var verifyNumber = function (comp) {
        var value = comp.value;
        if (!isNaN(value))
            return true;
        else
            return false;
    }

    // Handlers
    var inputFocusHandler = function () {
        oldnumberField = this.value;
    };

    var inputBlurHandler = function () {
        var isTruthy = verifyNumber(this);
        var $this = $(this);
        var $numberComp = $(numberComponent);
        var $inputContainer = $this.parent('.date');
        var numberField = $(this).val();
        if (!isTruthy) {
            this.value = oldnumberField;
        } else {
            var itemId = this.parentElement.getAttribute('data-id');
            changeNumberField(itemId, pageNameToCall, numberField);
        }

        if ($inputContainer.hasClass('channel-number')) {
            params.numberField = numberField || "No number";
        };
    }

    init();
}