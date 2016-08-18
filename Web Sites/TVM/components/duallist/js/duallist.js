var DualList = function (firstListParams, secondListParams, attachToElement, pageName, withCalendar) {

    var firstListTitle = (firstListParams) ? (firstListParams.Title) ? firstListParams.Title : "First List Title" : "First List Title";
    var firstListItems = (firstListParams) ? (firstListParams.data) ? firstListParams.data : {} : {};
    var firstListwithUpDownButtonOption = (firstListParams) ? (firstListParams.WithOrderByButtons) ? firstListParams.WithOrderByButtons : false : false;
    var secondListTitle = (secondListParams) ? (secondListParams.Title) ? secondListParams.Title : "Second List Title" : "Second List Title";
    var secondListItems = (secondListParams) ? (secondListParams.data) ? secondListParams.data : {} : {};
    var secondListwithUpDownButtonOption = (secondListParams) ? (secondListParams.WithOrderByButtons) ? secondListParams.WithOrderByButtons : false : false;
    var dualListName = attachToElement.id;
    var $firstList, $secondList, firstList, secondList

    var init = function () {
        createDualList();
        bindEvents();
    };

    var createDualList = function () {
        
        firstList = new List("selected-list", firstListTitle, pageName, withCalendar, dualListName, firstListwithUpDownButtonOption);
        secondList = new List("unselected-list", secondListTitle, pageName, withCalendar, dualListName, secondListwithUpDownButtonOption);
        firstList.addItemsToList(firstListItems, 'remove');
        secondList.addItemsToList(secondListItems, 'add');
        $firstList = $(firstList.getComponentElement());
        $secondList = $(secondList.getComponentElement());
        $(attachToElement).append($firstList);
        $(attachToElement).append($secondList);
    };

    var getSelectedListItems = function () {

        return firstListItems;
    };

    var getUnSelectedListItems = function () {

        return secondListItems;
    };

    var bindEvents = function () {
        $firstList.on('click', '.remove', moveItem);
        $firstList.on('click', '.add-move-up-icon', moveUp);
        $firstList.on('click', '.add-move-down-icon', moveDown);
        $secondList.on('click', '.add', moveItem);

    };

    var moveItem = function (event) {
        var $target = $(event.target);
        var $item = $target.parents('li');
        var itemId = $item.data('id');
        var isToAdd = $target.hasClass('add');
        if (isToAdd) {
            secondList.removeItemsFromList([itemId],false,'add');
            var itemToAdd = getItemData(secondListItems, itemId);
            removeFromListData(secondListItems, itemId);            
            firstList.addItemsToList([itemToAdd], 'remove',true);
            firstListItems.push(itemToAdd);
        } else {
            firstList.removeItemsFromList([itemId], false, 'remove');
            var itemToAdd = getItemData(firstListItems, itemId);
            removeFromListData(firstListItems, itemId);            
            secondList.addItemsToList([itemToAdd], 'add',true);
            secondListItems.push(itemToAdd);
        }
    };

    var moveUp = function (event) {
        var $target = $(event.target);
        var $item = $target.parents('li');
        var itemId = $item.data('id');
        firstList.moveItemUp([itemId]);        
    };

    var moveDown = function (event) {
        var $target = $(event.target);
        var $item = $target.parents('li');
        var itemId = $item.data('id');
        firstList.moveItemDown([itemId]);
    };

    var removeFromListData = function (listItemsData,itemId) {
        var listLength = listItemsData.length;
        var plc = -1;
        for (var i = 0; i < listLength; i++) {
            if (listItemsData[i].ID == itemId) {
                plc = i;
                break;
            }
        }
        if (plc != -1) {
            listItemsData = listItemsData.splice(plc, 1);
        }
    };

    var getItemData = function (listItemsData, itemId) {
        var listLength = listItemsData.length;
        for (var i = 0; i < listLength; i++) {
            if (listItemsData[i].ID == itemId) {
                return listItemsData[i];
            }
        }
    };

    init();

    return {
        getSelectedListItems: getSelectedListItems,
        getUnSelectedListItems: getUnSelectedListItems
    }
}