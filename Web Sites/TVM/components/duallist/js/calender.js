var Calendar = function (params, attachToElement, pageName) {
    var StartDate, EndDate, startDateNotEmpty, endDateNotEmpty;
    if (params.StartDate || params.EndDate) {
        StartDate = params.StartDate || "No start";
        EndDate = params.EndDate || "No end";
        startDateNotEmpty = (params.StartDate) ? true : false;
        endDateNotEmpty = (params.EndDate) ? true : false;
    } else {
        StartDate = EndDate = "";
        startDateNotEmpty = endDateNotEmpty = false;
    }
    
    var calendarComponent;

    var init = function () {
        createCalendarComponent();
        bindEvents();
    };

    var createCalendarComponent = function () {
        /// create icon + startInput + endInput + tooltip
        var protoTypeDOM = document.createElement('div');
        calendarComponent = protoTypeDOM.cloneNode(false);
        $(calendarComponent).addClass('calendar-comp');

        var startDivInput = protoTypeDOM.cloneNode(false);
        $(startDivInput).addClass('date');
        $(startDivInput).addClass('start-date');

        var startInput = document.createElement('input');
        startInput.setAttribute('data-date-verify', 'true');
        startInput.setAttribute('placeholder', 'dd/mm/yyyy');
        $(startInput).addClass('has-placeholder');
        startInput.value = StartDate;
        var minusSpan = document.createElement('span');
        minusSpan.innerHTML = '-';

        var endDivInput = protoTypeDOM.cloneNode(false);
        $(endDivInput).addClass('date');
        $(endDivInput).addClass('end-date');

        var endInput = document.createElement('input');
        endInput.setAttribute('data-date-verify', 'true');
        endInput.setAttribute('placeholder', 'dd/mm/yyyy');
        endInput.value = EndDate;

        var startDateTooltip = protoTypeDOM.cloneNode(false);
        $(startDateTooltip).addClass('tooltip');
        $(startDateTooltip).addClass('hidden');
        startDateTooltip.innerHTML = 'Date is invalid';

        var endDateTooltip = protoTypeDOM.cloneNode(false);
        $(endDateTooltip).addClass('tooltip');
        $(endDateTooltip).addClass('hidden');
        endDateTooltip.innerHTML = 'Date is invalid';

        var tooltipTriangle = protoTypeDOM.cloneNode(false);
        $(tooltipTriangle).addClass('tooltip-triangle');

        var calendarIcon = document.createElement('a');
        calendarIcon.setAttribute('href', 'javascript:;');
        $(calendarIcon).addClass('calendar-icon');

        startDateTooltip.appendChild(tooltipTriangle.cloneNode(true));
        endDateTooltip.appendChild(tooltipTriangle.cloneNode(true));
        startDivInput.appendChild(startInput);
        startDivInput.appendChild(startDateTooltip);
        endDivInput.appendChild(endInput);
        endDivInput.appendChild(endDateTooltip);

        calendarComponent.appendChild(startDivInput);
        calendarComponent.appendChild(minusSpan);
        calendarComponent.appendChild(endDivInput);
        if (startDateNotEmpty || endDateNotEmpty) {
            $(attachToElement).addClass('has-date');
            $(calendarComponent).find('input').attr('disabled','disabled');
        } else {
            $(calendarComponent).addClass('hidden');
        }
        attachToElement.appendChild(calendarComponent);
        attachToElement.appendChild(calendarIcon);
    };

    var bindEvents = function () {
        var $calendarComponent = $(attachToElement);
        $calendarComponent.on('click', '.calendar-icon', calendarIconClickedHandler);
        $calendarComponent.find('input[data-date-verify]').on('blur', inputBlurHandler);
        $calendarComponent.find('input[data-date-verify]').on('focus', inputFocusHandler);
    };

    // Handlers
    var calendarIconClickedHandler = function (event) {
        var $target = $(event.target);
        $target.toggleClass('active');
        var $listItem = $target.parents('li');
        if ($listItem.hasClass('has-date')) {
            $listItem.removeClass('has-date');
            $listItem.find('input').removeAttr('disabled');
            $listItem.find('.start-date input').val(params.StartDate);
            $listItem.find('.end-date input').val(params.EndDate);
        }
        var $calendarComp = $listItem.find('.calendar-comp');
        if ($target.hasClass('active')) {
            $calendarComp.removeClass('hidden');
        } else {
            $calendarComp.addClass('hidden');
        }
    };


    var inputBlurHandler = function () {
        var isTruthy = verifyDate(this);
        var $this = $(this);
        var $calendarComp = $(calendarComponent);
        var $inputContainer = $this.parent('.date');
        var startDate = $calendarComp.find('.start-date input').val();
        var endDate = $calendarComp.find('.end-date input').val();
        if (!isTruthy) {
            $inputContainer.find('.tooltip').removeClass('hidden');
        } else {
                var itemId = parseInt($calendarComp.parents('li').data('id'));
                changeItemDates(itemId, startDate, endDate, pageName);
        }

        if ($inputContainer.hasClass('start-date')) {
            params.StartDate = startDate || "No start";
        } else {
            params.EndDate = endDate || "No end";
        }
       
    };

    var inputFocusHandler = function () {
        var $this = $(this);
        var $inputContainer = $this.parent('.date');
        $inputContainer.find('.tooltip').addClass('hidden');
    };


    var verifyDate = function (dateInput) {
        var value = $(dateInput).val();
        var dateArray = value.split('/');
        if(!value){
            return true;
        }
        if (dateArray.length != 3) {
            return false;
        }

        var day = parseInt(dateArray[0]);
        var month = parseInt(dateArray[1]);
        var year = parseInt(dateArray[2]);

        if (day && day < 1 && day > 31) {
            return false;
        }

        if (month && month < 1 && month > 12) {
            return false;
        }

        if (year && year < 2000 && year > 2200) {
            return false;
        }
        return true;
    };

    init();
}