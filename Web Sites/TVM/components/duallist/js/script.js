/* dual list main script */
function initDualList(data)
{
    //add: check if size exists
    if (data["size"] > 1)
    {
        initMultipleLists(data);
    }
    else
    {
        var first = {
            Title: data.FirstListTitle,
            WithOrderByButtons: data.FirstListWithOrderByButtons != null ? data.FirstListWithOrderByButtons : false,
            data: getListData(data.Data, true)
        };
        var second = {
            Title: data.SecondListTitle,
            WithOrderByButtons: data.SecondListWithOrderByButtons != null ? data.SecondListWithOrderByButtons : false,
            data: getListData(data.Data, false)
        };
        window.components = window.components || {};
        window.components.dualList = new DualList(first, second, document.getElementById('DualListPH'), data.pageName, data.withCalendar, data.withQuota);
        $('.has-placeholder').placeholder();
    }
}

function initMultipleLists(dataLists) {
    var listSize = dataLists["size"];
    for (var i = 0; i < listSize; i++)
    {
        var currentDualList = dataLists[i];
        var first = {
            Title: currentDualList.FirstListTitle,
            WithOrderByButtons: currentDualList.FirstListWithOrderByButtons != null ? currentDualList.FirstListWithOrderByButtons : false,
            data: getListData(currentDualList.Data, true)
        };
        var second = {
            Title: currentDualList.SecondListTitle,
            WithOrderByButtons: currentDualList.SecondListWithOrderByButtons != null ? currentDualList.SecondListWithOrderByButtons : false,
            data: getListData(currentDualList.Data, false)
        };
        window.components = window.components || {};
        window.components.dualList = new DualList(first, second, document.getElementById(currentDualList.name), currentDualList.pageName, currentDualList.withCalendar, currentDualList.withQuota);
        $('.has-placeholder').placeholder();
    }
}

function safe_addEventListener(element, type, callback) {
    if (!element.addEventListener) {
        element.attachEvent("on" + type, callback);
    }
    else {
        element.addEventListener(type, callback, false);
    }
}

function callback_init_dobj(ret) {
    if (ret) {
        var data = JSON.parse(ret);
        var dualList = initDualList(data);
    }
}

function getListData(data, isInCurrentList) {
    
    var ListData = [];
    var dataLen = data.length;
    for (var i = 0; i < dataLen; i++) {
        if (data[i].InList == isInCurrentList) {
            var res = {
                StartDate: data[i].StartDate,
                EndDate: data[i].EndDate,
                ID: data[i].ID,
                Info: data[i].Description,
                Title: data[i].Title,
                OrderNum: data[i].OrderNum,
                NumberField: data[i].NumberField
            };
            ListData.push(res);
        }
    }
    return ListData
}

function changeItemStatus(sID, pageName, dualListParent) {
    RS.Execute(pageName, "changeItemStatus", sID, dualListParent, callback_status_changed, errorCallback);
}

function changeItemOrder(sID, pageName, updatedOrderNum) {
    RS.Execute(pageName, "changeItemOrder", sID, updatedOrderNum, callback_order_changed, errorCallback);
}

function changeItemDates(sID, startDate, endDate, pageName) {
    RS.Execute(pageName, "changeItemDates", sID, startDate, endDate, callback_dates_changed, errorCallback);
}


function changeNumberField(sID, pageName, numberField) {
    RS.Execute(pageName, "changeNumberField", sID, numberField, callback_number_changed, errorCallback);
}

function initDualObj() {
    RS.Execute("adm_media_files_ppvmodules.aspx", "initDualObj", callback_init_dobj, errorCallback);
}

function initDuallistObj(page) {
    RS.Execute(page, "initDualObj", callback_init_dobj, errorCallback);
}


function errorCallback(res) {
    //
}

function callback_dates_changed(res) {
    //
}

function callback_status_changed(res) {
    //
}

function callback_order_changed(res) {
    //
}


function callback_number_changed(res) {
    //
}