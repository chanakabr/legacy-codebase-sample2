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
        $(infoWrapper).addClass('info-wrapper');
        $(infoWrapper).addClass('hidden');

        var triangle = document.createElement('span');
        $(triangle).addClass('triangle');

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

            $(infoIcon).bind('click',function () {
                if (infoWrapper.className.indexOf('hidden') !== -1) {
                    $(infoWrapper).removeClass('hidden');
                    $(infoIcon.parentElement).addClass('active');
                    $(infoWrapper).hide();
                    $(infoWrapper).toggle("blind", function () {
                        
                    });
                    var $listWrapper = $(infoWrapper).parents('ul');
                    var infoWrapperHeight = $(infoWrapper).outerHeight(true);
                        $listWrapper.animate({ scrollTop: '+=' + infoWrapperHeight }, "blind");
                    
                } else {
                    $(infoWrapper).toggle("blind", function () {
                        $(infoWrapper).addClass('hidden');
                        $(infoWrapper).css('display', '');
                        $(infoIcon.parentElement).removeClass('active');
                    });
                    // here need to change the scrollTop value of th UL - List Wrapper
                    var $listWrapper = $(infoWrapper).parents('ul');
                    var infoWrapperHeight = $(infoWrapper).outerHeight(true);
                    $listWrapper.animate({ scrollTop: '-=' + infoWrapperHeight }, "blind");

                }
            });
        }

    };

    init();
};