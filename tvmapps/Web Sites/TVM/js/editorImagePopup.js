function $(id) {
	return document.getElementById(id);
}
var byClass = function(classname, node) {
    var re = new RegExp('(^| )' + classname + '( |$)');
    var children = (node || document).getElementsByTagName("*");

    for (var i = 0, j = children.length; i < j; i++)
        if (re.test(children[i].className)) return children[i];
}

function IsPic(file) 
{
    extArray = new Array(".gif", ".jpg", ".png", ".jpeg");
    bExists = false;
    if (!file) 
        return false;
    while (file.indexOf("\\") != -1)
        file = file.slice(file.indexOf("\\") + 1);
    ext = file.slice(file.indexOf(".")).toLowerCase();
    for (var i = 0; i < extArray.length; i++) 
    {
        if (extArray[i] == ext) 
        { 
            bExists = true; 
            break; 
        }
    }
    return bExists;
}

function submitASPForm(sNewFormAction)
{
    document.forms[0].action = sNewFormAction;
    document.forms[0].__VIEWSTATE.name = 'NOVIEWSTATE';
    document.forms[0].submit();
}

function show_error(the_error)
{
    document.getElementById("message_place1").innerHTML = the_error;
    OpenID("message_place");
}

function addEvent(obj, type, fn) {
	if (obj.addEventListener) {
		obj.addEventListener(type, fn, false);
		EventCache.add(obj, type, fn);
	}
	else if (obj.attachEvent) {
		var typefn = type + fn;
		obj["e"+typefn] = fn;
		obj[typefn] = function() { obj["e"+typefn](window.event); }
		obj.attachEvent("on"+type, obj[typefn]);
		EventCache.add(obj, type, fn);
	}
	else
		obj["on"+type] = obj["e"+type+fn];
}
var EventCache = {
	listEvents: [],
	add: function(node, sEventName, fHandler) {
		EventCache.listEvents.push(arguments);
	},
	flush: function() {
		var i, item, listEvents = EventCache.listEvents;
		for (i = listEvents.length - 1; item = listEvents[i]; i--) {
			if(item[0].removeEventListener)
				item[0].removeEventListener(item[1], item[2], item[3]);
			if(item[1].substring(0, 2) != "on")
				item[1] = "on" + item[1];
			if(item[0].detachEvent)
				item[0].detachEvent(item[1], item[2]);
			item[0][item[1]] = null;
		};
	}
}
addEvent(window, "unload", EventCache.flush);

addEvent(window, "load", function() {
	addEvent(byClass("upload"), "click", function() {
		addEvent(window, "unload", textEditor.addImagesEnd);

		var location2 = '';
		if ($('imgLocation1').checked)
			location2 = 'right';
		else if ($('imgLocation2').checked)
			location2 = 'center';
		else if ($('imgLocation3').checked)
			location2 = 'left';

		// add a div with align to the document
		textEditor.addImagesAlign(location2);

		// check if 'upload local file' text field has a local address
		if ($('upload').value) {
		    //Check the validity of the file
		    if (IsPic($('upload').value) == false)
		    {
		        show_error(error2);
		    }
		    else
		    {
			    // submit the local address to the iframe
			    Thinking(true);
			    submitASPForm("editorUploader.aspx");
			    //$('form').submit();

			    // when the upload is complete grab the image's address and append it
			    addEvent($('iframe'), "load", function() {
				    var url = $('iframe').contentWindow.document.getElementById("image_url").innerHTML;
				    if (url != "")
					    textEditor.addImagesAppend(url);
				    else
					    alert("uploading failed")
				    window.close();
			    });
			}
		}

		// check if 'upload file by url' text field has an address and it begins with 'http://'
		else if ($('upload2').value.length > 7 && $('upload2').value.indexOf('http://') == 0) {
			textEditor.addImagesAppend($('upload2').value);

			// if we're not waiting for a local file to finish uploading, close the window
			if (!$('upload').value)
				window.close();
		}
		else
		{
		    show_error(error1);
		}
	});
});