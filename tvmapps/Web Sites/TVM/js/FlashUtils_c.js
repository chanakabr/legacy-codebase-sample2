/**
 * 
 */
var oInterfaces = new Object();

function addInterfaceToCall(interface, interfaceName) {
    if (oInterfaces[interfaceName] != null)
        oInterfaces[interfaceName] = null;
	oInterfaces[interfaceName] = interface;
}

function callJSFromFlash(interfaceName, functionName){
	var params = new Array();
	for(var x = 2; x < arguments.length ; x++){
		params.push(arguments[x])
	}
	var interface = oInterfaces[interfaceName];

        
	if (interface == null || typeof interface == 'undefined') {
	    if (interfaceName != "VideoWMPCommInterface" && interfaceName != "")
	    {
	        TvinciPlayerAddToLog("callJSFromFlash - interface not found, looking for interface name:'" + interfaceName + "' to execute method '" + functionName + "'");
	        return;
	    }
	}
		
	return interface[functionName].apply(null, params);
}

function trace(s){
	var input = document.getElementById("input_txt");
	if(input != null){
		input.value += s + "\n";
		_updateScroll();
	}
	function _updateScroll(){
		input.scrollTop = input.scrollHeight;
	}
}

function createFlashVarsString(obj){
	var str = "";
	for(var key in obj){
		str += key + "=" + obj[key] + "&";
	}
	str = str.substr(0, str.length - 1)
	return str;
}