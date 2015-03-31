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
        /// create icon + channelNumber        
        //var protoTypeDOM = document.createElement('div');
        //numberComponent = protoTypeDOM.cloneNode(false);
        //$(numberComponent).addClass('calendar-comp');

        $(attachToElement).append($('<input class="channel-number" />').val(channelNumber));

        //debugger;
        //var numberComponent = document.createElement('div');//protoTypeDOM.cloneNode(false);
        //$(numberComponent).addClass('date');
        //$(numberComponent).addClass('start-date');

        //var numberInput = document.createElement('input');        
        //numberInput.setAttribute('data-date-verify', 'true');
        //numberInput.setAttribute('placeholder', '');
        ////$(numberInput).addClass('has-placeholder');
        //numberInput.value = channelNumber;
        //var minusSpan = document.createElement('span');
        //minusSpan.innerHTML = '-';

        //var calendarIcon = document.createElement('a');
        //calendarIcon.setAttribute('href', 'javascript:;');
        //$(calendarIcon).addClass('calendar-icon');

        //startDateTooltip.appendChild(tooltipTriangle.cloneNode(true));
        //startDivInput.appendChild(startInput);
        //startDivInput.appendChild(startDateTooltip);

        //numberComponent.appendChild(numberInput);
        //numberComponent.appendChild(minusSpan);
        //numberComponent.appendChild(endDivInput);
        //if (startDateNotEmpty || endDateNotEmpty) {
        //    $(attachToElement).addClass('has-date');
        //    $(calendarComponent).find('input').attr('disabled','disabled');
        //} else {
        //    $(calendarComponent).addClass('hidden');
        //}        
        //attachToElement.appendChild(calendarIcon);
    };

    var bindEvents = function () {
        var $numberComponent = $(attachToElement);
        //$calendarComponent.on('click', '.calendar-icon', calendarIconClickedHandler);
        $numberComponent.find('input[class="channel-number"]').on('blur', inputBlurHandler);
        $numberComponent.find('input[class="channel-number"]').on('focus', inputFocusHandler);
    };

    // Handlers
    //var calendarIconClickedHandler = function (event) {
    //    var $target = $(event.target);
    //    $target.toggleClass('active');
    //    var $listItem = $target.parents('li');
    //    if ($listItem.hasClass('has-date')) {
    //        $listItem.removeClass('has-date');
    //        $listItem.find('input').removeAttr('disabled');
    //        $listItem.find('.start-date input').val(params.StartDate);
    //        $listItem.find('.end-date input').val(params.EndDate);
    //    }
    //    var $calendarComp = $listItem.find('.calendar-comp');
    //    if ($target.hasClass('active')) {
    //        $calendarComp.removeClass('hidden');
    //    } else {
    //        $calendarComp.addClass('hidden');
    //    }
    //};

    var verifyNumber = function (comp) {
        var value = comp.value;
        if (!isNaN(value))
            return true;
        else
            return false;
    }

    var inputFocusHandler = function () {
        oldChannelNumber = this.value;
        //    var $this = $(this);
        //    var $inputContainer = $this.parent('.date');
        //    $inputContainer.find('.tooltip').addClass('hidden');
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

        


        


        //var verifyDate = function (dateInput) {
        //    var value = $(dateInput).val();
        //    var dateArray = value.split('/');
        //    if(!value){
        //        return true;
        //    }
        //    if (dateArray.length != 3) {
        //        return false;
        //    }

        //    var day = parseInt(dateArray[0]);
        //    var month = parseInt(dateArray[1]);
        //    var year = parseInt(dateArray[2]);

        //    if (day && day < 1 && day > 31) {
        //        return false;
        //    }

        //    if (month && month < 1 && month > 12) {
        //        return false;
        //    }

        //    if (year && year < 2000 && year > 2200) {
        //        return false;
        //    }
        //    return true;
        //};

        
    }

    init();
}