/*
 * WMP embed (based on WMP/Flash Object by Geoff Stearns (geoff@deconcept.com, http://www.deconcept.com/)
 
 ****************************************************************
 * by Kovan Abdulla (kovan@imetasoft.com) - www.imetasoft.com
 ****************************************************************
 
 * v1.0.0 - 04-24-2006
 * Embeds a wmp object 
 * Usage:
 *

				var wmpVid = new WMPObject('pathtofile.asf', 'MMPlayer1', 240, 190);
				wmpVid.addParam('TYPE', 'application/x-mplayer2');
				wmpVid.addParam('PLUGINSPACE', 'http://www.microsoft.com/Windows/MediaPlayer/download/default.asp');
				wmpVid.addParam('Autostart', '1'); 
				wmpVid.addParam('ShowControls', '0'); 
				wmpVid.addParam('ShowDisplay', '0'); 
				wmpVid.addParam('ShowStatusBar', '0'); 
				wmpVid.addParam('DefaultFrame', 'Slide'); 
				wmpVid.write("video");
 */

WMPObject = function(src, id, w, h) {
	this.src = src;
	this.id = id;
	this.width = w;
	this.height = h;
	this.redirect = "";
	this.sq = window.document.location.search.split("?")[1] || "";
	this.altTxt = "This content requires the Media Player Plugin. <a href='http://www.microsoft.com/Windows/MediaPlayer/download/default.asp'>Download Media Player</a>.";
	this.bypassTxt = "<p>Already have Media Player Player? <a href='?detectWMP=false&"+ this.sq +"'>Click here.</a></p>";
	this.params = new Object();
	this.doDetect = getQueryParamValue('detectWMP');
}

WMPObject.prototype.addParam = function(name, value) {
	this.params[name] = value;
}

WMPObject.prototype.getParams = function() {
    return this.params;
}

WMPObject.prototype.getParam = function(name) {
    return this.params[name];
}

/*WMPObject.prototype.getParamTags = function() {
    var paramTags = "";
    for (var param in this.getParams()) {
        paramTags += param + '="' + this.getParam(param) + '" ';
    }
    if (paramTags == "") {
        paramTags = null;
    }
    return paramTags;
}*/

WMPObject.prototype.getParamTags = function() {
    var paramTags = "";
    for (var param in this.getParams()) {
        //paramTags += param + '="' + this.getParam(param) + '" ';
        paramTags += '<param name="' + param + '" value="' + this.getParam(param) + '" /> ';
    }
    if (paramTags == "") {
        paramTags = null;
    }
    return paramTags;
}


WMPObject.prototype.getHTML = function() {
    var WMPHTML = "";
	if (navigator.plugins && navigator.plugins.length) { // not ie
        WMPHTML += '<embed type="application/x-ms-wmp" src="' + this.src + '" width="' + this.width + '" height="' + this.height + '" id="' + this.id + '"';
        for (var param in this.getParams()) {
            WMPHTML += ' ' + param + '="' + this.getParam(param) + '"';
        }
        WMPHTML += '></embed>';
    }
    else { // pc ie
        WMPHTML += '<object id=' + this.id + ' CLASSID="CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6" width="' + this.width + '" height="' + this.height + '" >';
        //WMPHTML += '<embed width="' + this.width + '" height="' + this.height + '" id="' + this.id + '" ';
        this.addParam("src", this.src);
        if (this.getParamTags() != null) {
            WMPHTML += this.getParamTags();
        }
        //WMPHTML += '></embed>';
        WMPHTML += '></object>';
    }
    return WMPHTML;
}


WMPObject.prototype.getVariablePairs = function() {
    var variablePairs = new Array();
    for (var name in this.getVariables()) {
        variablePairs.push(name + "=" + escape(this.getVariable(name)));
    }
    if (variablePairs.length > 0) {
        return variablePairs.join("&");
    }
    else {
        return null;
    }
}

WMPObject.prototype.write = function(elementId) {
	
	if(isWMPInstalled() || this.doDetect=='false') {
		if (elementId) {
			window.document.getElementById(elementId).innerHTML = this.getHTML();
		} else {
			window.document.write(this.getHTML());
		}
	} else {
		if (this.redirect != "") {
			window.document.location.replace(this.redirect);
		} else {
			if (elementId) {
				window.document.getElementById(elementId).innerHTML = this.altTxt +""+ this.bypassTxt;
			} else {
				window.document.write(this.altTxt +""+ this.bypassTxt);
			}
		}
	}		
}

function isWMPInstalled() {
	var WMPInstalled = false;
	WMPObj = false;
	if (navigator.plugins && navigator.plugins.length) {
		for (var i=0; i < navigator.plugins.length; i++ ) {
         var plugin = navigator.plugins[i];
         if (plugin.name.indexOf("Media Player") > -1) {
			WMPInstalled = true;
         }
      }
	} else {
		execScript('on error resume next: WMPObj = IsObject(CreateObject("MediaPlayer.MediaPlayer.1"))','VBScript');
		WMPInstalled = WMPObj;
	}
	return WMPInstalled;
}

/* get value of querystring param */
function getQueryParamValue(param) {
	var q = window.document.location.search;
	var detectIndex = q.indexOf(param);
	var endIndex = (q.indexOf("&", detectIndex) != -1) ? q.indexOf("&", detectIndex) : q.length;
	if(q.length > 1 && detectIndex != -1) {
		return q.substring(q.indexOf("=", detectIndex)+1, endIndex);
	} else {
		return "";
	}
}
