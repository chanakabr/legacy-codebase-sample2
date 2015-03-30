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
    window.components.dualList = new DualList(first, second, document.getElementById('DualListPH'));
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
                ChannelNumber: data[i].ChannelNumber,
                ID: data[i].ID,
                Title: data[i].Title
            };
            ListData.push(res);
        }
    }
    return ListData
}

function changeItemStatus(sID) {
    RS.Execute("adm_group_regions_new.aspx", "changeItemStatus", sID, "", callback_status_changed, errorCallback);
}

function changeItemNumber(sID, channelNumber) {
    RS.Execute("adm_group_regions_new.aspx", "changeItemNumber", sID, channelNumber, callback_number_changed, errorCallback);
}
function initDualObj() {
    RS.Execute("adm_group_regions_new.aspx", "initDualObj", callback_init_dobj, errorCallback);
}

function errorCallback(res) {
    //
}

function callback_number_changed(res) {
    //
}

function callback_status_changed(res) {
    //
}