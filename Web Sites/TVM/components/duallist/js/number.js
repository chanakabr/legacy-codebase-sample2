var Number = function (params, attachToElement) {
    var channelNumber, channelNumberNotEmpty, oldChannelNumber;
    if (params.ChannelNumber) {
        oldChannelNumber = channelNumber = params.ChannelNumber || "No number";
        channelNumberNotEmpty = (params.ChannelNumber) ? true : false;
    } else {
        channelNumber = "";
        channelNumberNotEmpty = false;
    }

    var numberComponent;

    var init = function () {
        createNumberComponent();
        bindEvents();
    };

    var createNumberComponent = function () {
        $(attachToElement).append($('<input class="channel-number" />').val(channelNumber));
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
        oldChannelNumber = this.value;
    };

    var inputBlurHandler = function () {
        var isTruthy = verifyNumber(this);
        var $this = $(this);
        var $numberComp = $(numberComponent);
        var $inputContainer = $this.parent('.date');
        var channelNumber = $(this).val();
        if (!isTruthy) {
            this.value = oldChannelNumber;
        } else {
            var itemId = this.parentElement.getAttribute('data-id');
            changeItemNumber(itemId, channelNumber);
        }

        if ($inputContainer.hasClass('channel-number')) {
            params.ChannelNumber = channelNumber || "No number";
        };
    }

    init();
}