/* dual list main script */
function initDualList(data)
{
    var first = {
        Title: data.FirstListTitle,
        data: getListData(data.Data, true)
    };
    var second = {
        Title: data.SecondListTitle,
        data: getListData(data.Data, false)
    };
    window.components = window.components || {};
    window.components.dualList = new DualList(first, second, document.getElementById('DualListPH'), data.pageName, data.withCalendar);
    $('.has-placeholder').placeholder();
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
                Title: data[i].Title
            };
            ListData.push(res);
        }
    }
    return ListData
}

function changeItemStatus(sID, pageName) {
    RS.Execute(pageName, "changeItemStatus", sID, "", callback_status_changed, errorCallback);
}

function changeItemDates(sID, startDate, endDate, pageName) {
    RS.Execute(pageName, "changeItemDates", sID, startDate, endDate, callback_dates_changed, errorCallback);
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