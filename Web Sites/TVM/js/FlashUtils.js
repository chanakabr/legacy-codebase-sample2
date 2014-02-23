/**
 * 
 */
var oInterfaces = new Object();
 
function addInterfaceToCall(interfaceObj, interfaceName){
	oInterfaces[interfaceName] = interfaceObj;
}


function createAsyncJSCall(){
	var params = new Array();
	for(var x = 1; x < arguments.length ; x++){
		params.push(arguments[x]);
	}
	var func = eval(arguments[0]);
	setTimeout(_asyncJS, 0);
	function _asyncJS(){
		func.apply(null, params);
	}
}

function callJSFromFlash(interfaceName, functionName){
	var ret;
	var params = new Array();
	for(var x = 2; x < arguments.length ; x++){
		params.push(arguments[x])
	}
	var interfaceObj = oInterfaces[interfaceName];
	try{
		ret = interfaceObj[functionName].apply(null, params);
	}catch(e){
		//alert(e);
	}
	return ret;
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