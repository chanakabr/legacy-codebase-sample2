var Info = function (content, parentElement) {
    var infoContent = content || "Info Content";
    var infoWrapper;

    var init = function () {
        createInfoWrapper();
        bindEvents();
    };

    var createInfoWrapper = function () {
        infoWrapper = document.createElement('div');
        infoWrapper.innerHTML = infoContent;
        infoWrapper.classList.add('info-wrapper');
        infoWrapper.classList.add('hidden');

        var triangle = document.createElement('span');
        triangle.classList.add('triangle');

        infoWrapper.appendChild(triangle);

        parentElement.parentNode.insertBefore(infoWrapper, parentElement.nextSibling);
    };

    var bindEvents = function () {
        var infoIcon;
        var children = parentElement.children;
        var childrenLength = children.length;
        for (var i = 0; i < childrenLength; i++) {
            if (children[i].className.indexOf('info-icon') !== -1) {
                infoIcon = children[i];
                break;
            }
        }
        if (infoIcon) {
            safe_addEventListener(infoIcon, 'click', function () {
                if (infoWrapper.className.indexOf('hidden') !== -1) {
                    infoWrapper.classList.remove('hidden');
                    infoIcon.parentElement.classList.add('active');
                } else {
                    infoWrapper.classList.add('hidden');
                    infoIcon.parentElement.classList.remove('active');
                }
            });
        }

    };

    init();
};