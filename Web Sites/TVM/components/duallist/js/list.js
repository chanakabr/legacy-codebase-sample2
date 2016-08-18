/* This describes a single list, the duallist, well.. uses two of these */
var List = function (listId, listTitle, pageName, withCalendar, dualListParent, WithOrderByButtons) {
    var listComponentId = listId;
    var listComponentTitle = listTitle;    
    var listWrapper, listItems, listItemsArray, quickSearchInput;
    var listParent = dualListParent;

    var init = function () {

        createListLayout();
        bindEvents();
    };

    var bindEvents = function () {
        safe_addEventListener(quickSearchInput, 'keyup', quickSearch);
        safe_addEventListener(quickSearchInput, 'search', quickSearch);
    };

    /* Grabs the value from the search input and filters according */
    var quickSearch = function () {
        var keyword = quickSearchInput.value;
        var listItemsChildren = listItems.children;
        var listItemsChildrenLength = listItemsChildren.length;
        if (listItemsChildrenLength) {
            for (var i = 0; i < listItemsChildrenLength; i++) {
                var listItem = listItemsChildren[i];
                if (listItem.className.indexOf('info-wrapper') === -1) {
                    if (listItem.innerText.indexOf(keyword) !== -1) {
                        $(listItem).removeClass('hidden');
                    } else {
                        $(listItem).addClass('hidden');
                        $(listItem).removeClass('active');
                        $(listItem.nextSibling).addClass('hidden');
                    }
                }
            }
        }
    };


    var createListLayout = function () {
        var divElmPrototype = document.createElement('div');
        listWrapper = divElmPrototype.cloneNode(false);

        listComponentId = listComponentId || "ListComponent_" + (new Date).getTime().toString();
        listComponentTitle = listComponentTitle || "ListComponentTitle";
        $(listWrapper).addClass('ListComponent');
        listWrapper.setAttribute("id", listComponentId);

        var listTitle = divElmPrototype.cloneNode(false);
        listTitle.innerHTML = listComponentTitle;
        $(listTitle).addClass('list-title');

        listWrapper.appendChild(listTitle);

        var quickSearch = divElmPrototype.cloneNode(false);
        $(quickSearch).addClass('quick-search');

        quickSearchInput = document.createElement('input');
        quickSearchInput.setAttribute('type', 'search');
        quickSearchInput.setAttribute('placeholder', 'Quick Search');
        $(quickSearchInput).addClass('has-placeholder');
        quickSearch.appendChild(quickSearchInput);

        listWrapper.appendChild(quickSearch);

        var ulListWrapper = divElmPrototype.cloneNode(false);
        $(ulListWrapper).addClass('ul-list-wrapper');
        listItems = document.createElement('ul');
        $(listItems).addClass('items-list');

        ulListWrapper.appendChild(listItems);

        listWrapper.appendChild(ulListWrapper);
    };

    var addItemsToList = function (items, type, toChangeStatus) {
        var itemsLength = items.length;
        if (!listItems) {
            throw "List Item UL is not defined";
            return;
        }
        if (items && itemsLength) {
            for (var i = 0; i < itemsLength; i++) {
                if (items[i].Title) {
                    var listLiItem = document.createElement('li');
                    listLiItem.setAttribute("data-id", items[i].ID);
                    listLiItem.innerHTML = "<span class='ppvm-text' title='" + items[i].Title + "'>" + items[i].Title + "</span>";
                    var addRemoveIcon = document.createElement('a');
                    $(addRemoveIcon).addClass('add-remove-icon');
                    $(addRemoveIcon).addClass(type);
                    addRemoveIcon.setAttribute('href', 'javascript:;');
                    listLiItem.appendChild(addRemoveIcon);

                    if (withCalendar === true) {
                        var infoIcon = document.createElement('a');
                        $(infoIcon).addClass('info-icon');
                        infoIcon.setAttribute('href', 'javascript:;');
                        listLiItem.appendChild(infoIcon);
                    }
                    
                    if (WithOrderByButtons === true) {
                        listLiItem.setAttribute("data-orderNum", items[i].OrderNum);
                        var moveUpIcon = document.createElement('a');
                        $(moveUpIcon).addClass('add-move-up-icon');
                        moveUpIcon.setAttribute('href', 'javascript:;');
                        listLiItem.appendChild(moveUpIcon);
                        var moveDownIcon = document.createElement('a');
                        $(moveDownIcon).addClass('add-move-down-icon');
                        moveDownIcon.setAttribute('href', 'javascript:;');
                        listLiItem.appendChild(moveDownIcon);
                    }

                    //if the Calendar and WithOrderByButtons are not used the width of ppvm-text should be 100% of the list
                    if (withCalendar === false && WithOrderByButtons === false) {
                        listLiItem.childNodes[0].style.width = "100%";
                    }
                    else {
                        listLiItem.childNodes[0].className = "ppvm-text-withButtons";
                    }

                    $(listLiItem).hide();
                    if (type == 'add') {
                        $(listLiItem).show(800).effect("slide", { direction: "left" }, 800);
                    } else {
                        $(listLiItem).show(800).effect("slide", { direction: "right" }, 800);
                    }

                    $(listItems).append($(listLiItem));
                    new Info(items[i].Info, listLiItem);

                    // adding the calendar component
                    if (type == 'remove') {
                        if (withCalendar === true) {
                            new Calendar(items[i], listLiItem, pageName);
                        }
                        else {
                            new Number(items[i], listLiItem);
                        }
                    }
                    if (toChangeStatus) {
                        changeItemStatus(items[i].ID, pageName, listParent);
                    }
                }
            }

        }
    };

    var removeItemsFromList = function (itemsId, toRemoveAllItems,listType) {
        if (toRemoveAllItems) {
            while (listItems.firstChild) {
                listItems.removeChild(listItems.firstChild);
            }
        } else {
            var itemsLength = itemsId.length;
            if (itemsId && itemsLength) {
                var liItems = listItems.children;
                var liItemsLength = liItems.length;
                for (var j = 0; j < liItemsLength; j++) {
                    var liItem = liItems[j];
                    if ($(liItem).data('id') && itemsId.indexOf(parseInt($(liItem).data('id'))) !== -1) {
                        var direction = 'right';
                        if (listType == 'add') {
                            direction = 'left';
                        }
                        $(liItem).hide("slide", { direction: direction }, 800, function () {
                            $($(liItem)[0].nextSibling).remove();
                            $(liItem).remove();
                        });
                        
                        break;
                    }
                }
            }
        }
    };

    var moveItemUp = function (itemsId) {
        var itemsLength = itemsId.length;
        if (itemsId && itemsLength) {
            var liItems = listItems.children;
            var liItemsLength = liItems.length;
            for (var j = 0; j < liItemsLength; j++) {
                var liItem = liItems[j];
                if ($(liItem).data('id') && itemsId.indexOf(parseInt($(liItem).data('id'))) !== -1) {
                    if (j == 0) {
                        break;
                    }
                    var previousItem = liItems[j - 2];
                    var newOrderNumValue = previousItem.getAttribute("data-orderNum") - 1;
                    liItem.setAttribute("data-orderNum", newOrderNumValue);
                    $(liItem).hide("slide", { direction: 'up' }, 200, function () { });
                    $(previousItem).hide("slide", { direction: 'down' }, 200, function () { });
                    $(listItems).insertBefore($(previousItem));
                    $(liItem).show("slide", { direction: 'up' }, 200, function () { });
                    $(previousItem).show("slide", { direction: 'down' }, 200, function () { });
                    break;
                }
            }
        }
    };

    var moveItemDown = function (itemsId) {
        var itemsLength = itemsId.length;
        if (itemsId && itemsLength) {
            var liItems = listItems.children;
            var liItemsLength = liItems.length;
            for (var j = 0; j < liItemsLength; j++) {
                var liItem = liItems[j];
                if ($(liItem).data('id') && itemsId.indexOf(parseInt($(liItem).data('id'))) !== -1) {
                    if (j == liItemsLength - 1) {
                        break;
                    }
                    var direction = 'down';
                    $(liItem).hide("slide", { direction: direction }, 200, function () {
                        $($(liItem)[0].nextSibling).remove();
                        $(liItem).remove();
                    });

                    break;
                }
            }
        }
    };

    var getComponentElement = function () {
        return listWrapper;
    };

    init();

    return {
        addItemsToList: addItemsToList,
        removeItemsFromList: removeItemsFromList,
        getComponentElement: getComponentElement,
        moveItemUp: moveItemUp,
        moveItemDown: moveItemDown
    }
};