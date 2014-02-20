/* This describes a single list, the duallist, well.. uses two of these */
var List = function (listId, listTitle) {
    var listComponentId = listId;
    var listComponentTitle = listTitle;
    var listWrapper, listItems, listItemsArray, quickSearchInput;

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
                        listItem.classList.remove('hidden');
                    } else {
                        listItem.classList.add('hidden');
                        listItem.classList.remove('active');
                        listItem.nextSibling.classList.add('hidden');
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
        listWrapper.classList.add('ListComponent');
        listWrapper.setAttribute("id", listComponentId);

        var listTitle = divElmPrototype.cloneNode(false);
        listTitle.innerHTML = listComponentTitle;
        listTitle.classList.add('list-title');

        listWrapper.appendChild(listTitle);

        var quickSearch = divElmPrototype.cloneNode(false);
        quickSearch.classList.add('quick-search');

        quickSearchInput = document.createElement('input');
        quickSearchInput.setAttribute('type', 'search');
        quickSearchInput.setAttribute('placeholder', 'Quick Search');

        quickSearch.appendChild(quickSearchInput);

        listWrapper.appendChild(quickSearch);

        listItems = document.createElement('ul');
        listItems.classList.add('items-list');

        listWrapper.appendChild(listItems);
    };

    var addItemsToList = function (items,type,toChangeStatus) {
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
                    listLiItem.innerHTML = "<span class='ppvm-text' title='"+items[i].Title+"'>" + items[i].Title + "</span>";
                    var addRemoveIcon = document.createElement('a');
                    addRemoveIcon.classList.add('add-remove-icon');
                    addRemoveIcon.classList.add(type);
                    addRemoveIcon.setAttribute('href', 'javascript:;');
                    listLiItem.appendChild(addRemoveIcon);
                    var infoIcon = document.createElement('a');
                    infoIcon.classList.add('info-icon');
                    infoIcon.setAttribute('href', 'javascript:;');
                    listLiItem.appendChild(infoIcon);
                    listItems.appendChild(listLiItem);
                    new Info(items[i].Info, listLiItem);

                    // adding the calendar component
                    if (type == 'remove'){
                        new Calendar(items[i], listLiItem);
                    }
                    if (toChangeStatus) {
                        changeItemStatus(items[i].ID);
                    }
                }
            }
            
        }
    };

    var removeItemsFromList = function (itemsId, toRemoveAllItems) {
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
                    if (liItem.dataset.id && itemsId.indexOf(parseInt(liItem.dataset.id)) !== -1) {
                        liItem.nextSibling.remove();
                        liItem.remove();
                        break;
                    }
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
        getComponentElement: getComponentElement
    }
};