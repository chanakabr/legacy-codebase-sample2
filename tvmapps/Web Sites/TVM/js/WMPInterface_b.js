/**
 * WMPInterface
 * createWMPInterface returns a object that communicate with flash object - to support wmv with flash.
 * after creating the interface:
 * - add interface to FlashUtils.js by calling addInterfaceToCall the default name is 'VideoInterface'.
 * - flash commuincate with callJSFromFlash in FlashUtils and passes 'VideoInterface' as interface name.
 * - call interface.init with appropriate params to start.
 */

var aWMPInterfaces = new Array();
var WMPObject;
function OnDSPlayStateChangeEvt(newState) {
	for(var x = 0 ; x < aWMPInterfaces.length ; x ++){
		aWMPInterfaces[x].checkPlayStateChange(newState);
	}
}

function cleanPropValue(str){
	return Number(str.split("px")[0].split("pt")[0]);
}

function createWMPInterface(){
	var WMPIntreface = new Object();
	aWMPInterfaces.push(WMPIntreface);
	
	WMPIntreface.init = function(WMPObj, WMPHolder, SWFObj, SWFHolder){
		WMPIntreface.m_wmpObj = WMPObj;
		WMPIntreface.m_wmpHolder = WMPHolder;
		WMPIntreface.m_swfObj = SWFObj;
		WMPIntreface.m_swfHolder = SWFHolder;
		if(!window.addEventListener){
			WMPIntreface.m_wmpObj.attachEvent("PlayStateChange", WMPIntreface.checkPlayStateChange);
		}				
	}
	
	WMPIntreface.load = function(sURL, bAutoplay){
		WMPIntreface.m_currentRatio = null;
		//WMPIntreface.startDisptachProgress();
		
		WMPIntreface.m_wmpObj.URL = sURL;
		WMPIntreface.m_wmpObj.fileName = sURL;
		if(bAutoplay){
			WMPIntreface.m_wmpObj.controls.play();
		}
	}
	
	WMPIntreface.checkPlayStateChange = function (stat){
		switch(stat){
			case 0:
			
			break;
			case 1:
				WMPIntreface.m_swfObj.getEvent("as3.events.PlaybackEvent", "playback_stop");
			break;
			case 2:
				WMPIntreface.m_swfObj.getEvent("as3.events.PlaybackEvent", "playback_pause");
			break;
			case 3:
				WMPIntreface.m_swfObj.getEvent("as3.events.PlaybackEvent", "playback_play");
				//WMPIntreface.startDispatchPlaying(0);
				if(WMPIntreface.m_currentRatio == null)
					WMPIntreface.m_currentRatio = WMPIntreface.getOriginWidth() / WMPIntreface.getOriginHeight();
			break;
			case 8:
				WMPIntreface.m_swfObj.getEvent("as3.events.PlaybackEvent", "playback_stop");
				WMPIntreface.m_swfObj.getEvent("as3.events.PlaybackEvent", "playback_complete");
				WMPIntreface.stop();
			break;
			case 10:
				WMPIntreface.m_swfObj.getEvent("as3.events.PlaybackEvent", "playback_receive_meta");
			break;
		}
	}
	/*
	WMPIntreface.startDispatchPlaying = function(counter){
		WMPIntreface.m_swfObj.getEvent("as3.events.PlaybackEvent", "playback_playing");
		if(WMPIntreface.isPlaying()){
			setTimeout(WMPIntreface.startDispatchPlaying, 5000, counter + 1);
		}
	}
	//*/
									
	
	
	WMPIntreface.startDisptachProgress = function(){
		var prog = WMPIntreface.m_wmpObj.network.downloadProgress;
		if(prog != 100){
			//setTimeout(WMPIntreface.startDisptachProgress, 150);
		}
		//WMPIntreface.m_swfObj.getEvent("as3.events.PercentProgressEvent", "progress", prog);
	}
	
	WMPIntreface.isPlaying = function(){
		return WMPIntreface.getStatus() == 3;
	}
	
	WMPIntreface.getStatus = function(){
		return WMPIntreface.m_wmpObj.playState;
	}
	
	WMPIntreface.play = function(sURL){
		WMPIntreface.m_wmpObj.controls.play();
	}
	
	WMPIntreface.pause = function(){
		WMPIntreface.m_wmpObj.controls.pause();
	}
	
	WMPIntreface.stop = function(){
		//WMPIntreface.m_wmpObj.controls.stop();
		WMPIntreface.clear();
	}
	
	WMPIntreface.clear = function(){
		WMPIntreface.m_wmpObj.controls.stop();
		WMPIntreface.m_wmpObj.close();
	}
	
	WMPIntreface.setScaleMode = function(mode){
		trace("scaleMode >> " + mode);
	}
	
	WMPIntreface.getScaleMode = function(){
		return "";
	}
	
	WMPIntreface.setPosition = function(n){
		WMPIntreface.m_wmpObj.controls.currentPosition = n;
	}
	//var tempCounter = 0;
	WMPIntreface.getPosition = function(){
		//tempCounter ++;
		//window.status = "WMPIntreface.getPosition: " + tempCounter;
		return WMPIntreface.m_wmpObj.controls.currentPosition;
	}
	
	WMPIntreface.setVolume = function(n){
		WMPIntreface.m_wmpObj.settings.volume = n * 100;
	}
	
	WMPIntreface.getVolume = function(){
		return WMPIntreface.m_wmpObj.settings.volume;
	}
	
	WMPIntreface.getDuration = function(){
		return WMPIntreface.m_wmpObj.controls.currentItem.duration;
	}
	
	WMPIntreface.setWidth = function(n){
		WMPIntreface.m_wmpObj.width = n;
		WMPIntreface.m_wmpHolder.style.width = n + "px";
	}
	
	WMPIntreface.getWidth = function(){
		return WMPIntreface.m_wmpObj.width;
	}
	
	WMPIntreface.setHeight = function(n){
		WMPIntreface.m_wmpObj.height = n;
		WMPIntreface.m_wmpHolder.style.height = n + "px";
	}
	
	WMPIntreface.getHeight = function(){
		return WMPIntreface.m_wmpObj.height;
	}
	
	WMPIntreface.setX = function(n){
		WMPIntreface.m_wmpHolder.style.left = Number(n) + cleanPropValue(WMPIntreface.m_swfHolder.style.left);
	}
	
	WMPIntreface.getX = function(){
		return Number(WMPIntreface.m_wmpHolder.style.left) + cleanPropValue(WMPIntreface.m_swfHolder.style.left);
	}
	
	WMPIntreface.setY = function(n){
		WMPIntreface.m_wmpHolder.style.top = Number(n) + cleanPropValue(WMPIntreface.m_swfHolder.style.top);
	}
	
	WMPIntreface.getY = function(){
		return Number(WMPIntreface.m_wmpHolder.style.top) + cleanPropValue(WMPIntreface.m_swfHolder.style.top);
	}
	
	WMPIntreface.getVideoWidth = function(){
		return 0;
	}
	
	WMPIntreface.getVideoHeight = function(){
		return 0;
	}
	
	WMPIntreface.getOriginWidth = function(){
		return WMPIntreface.m_wmpObj.width;
	}
	
	WMPIntreface.getOriginHeight = function(){
		return WMPIntreface.m_wmpObj.height;
	}
	
	WMPIntreface.getVideoX = function(){
		return 0;
	}
	
	WMPIntreface.getVideoY = function(){
		return 0;
	}

    WMPIntreface.checkFullScreenStatus = function()
	{	
	    if(!WMPIntreface.m_wmpObj.fullScreen)
	    {
			WMPIntreface.m_swfObj.getEvent("flash.events.FullScreenEvent", "fullScreen" , false , false , false);
	        GetVG().changeScreenSaverStatus(true);
	    }else
	    {
	        setTimeout(WMPIntreface.checkFullScreenStatus, 10000);
	    }	
	}


	WMPIntreface.setFullscreen = function(b) {
	    GetVG().changeScreenSaverStatus(!b);
	    WMPIntreface.m_wmpObj.fullScreen = b;
	    if (b) {
	        WMPIntreface.checkFullScreenStatus();
	        WMPIntreface.m_swfObj.getEvent("flash.events.FullScreenEvent", "fullScreen", false, false, true);
	    }
	}
	
	WMPIntreface.getFullscreen = function(){
		return WMPIntreface.m_wmpObj.fullScreen;
	}
	
	
	WMPIntreface.setVisible = function(b){
		WMPIntreface.m_wmpObj.style.display = b ? "inline" : "none";
		WMPIntreface.m_wmpHolder.style.display = b ? "inline" : "none";
	}
		
	return WMPIntreface;
}