/**
 * WMPInterface
 * createWMPInterface returns a object that communicate with flash object - to support wmv with flash.
 * after creating the interface:
 * - add interface to FlashUtils.js by calling addInterfaceToCall the default name is 'VideoInterface'.
 * - flash commuincate with callJSFromFlash in FlashUtils and passes 'VideoInterface' as interface name.
 * - call interface.init with appropriate params to start.
 */
var isIE  = (navigator.appVersion.indexOf("MSIE") != -1) ? true : false;
var aWMPInterfaces = new Array();
var aWMPobjs = new Array();
var WMPObject;
var WMPFullscreenWMPObj;
var WMPCurrentURL = "";
var WMPWasFullscreen;
function OnDSPlayStateChangeEvt(newState) {
	for(var x = 0 ; x < aWMPInterfaces.length ; x ++){
		aWMPInterfaces[x].checkPlayStateChange(newState);
	}
}
function OnDSStatusChangeEvt(newState) {
	for(var x = 0 ; x < aWMPInterfaces.length ; x ++){
		aWMPInterfaces[x].statusChange(newState);
	}
}

function onOpenStateChange(stat) {
    // Only for fire-fox - route to normal event handler
//    for (var x = 0; x < aWMPInterfaces.length; x++) {
//        aWMPInterfaces[x].openStateChange(stat);
//    }
}

function alertObj(obj){
	var str = "";
	for(var key in obj){
		str += key + " = " + obj[key] + "\n";
	}
	alert(str);
}

function cleanPropValue(str){
	return Number(str.split("px")[0].split("pt")[0]);
}

function createWMPInterface(){
	var WMPIntreface = new Object();
	var init = false;
	aWMPInterfaces.push(WMPIntreface);
	WMPIntreface.init = function(WMPObj1, WMPHolder, SWFObj, SWFHolder, interfaceName) {
	    WMPIntreface.m_interfaceName = interfaceName;
	    WMPIntreface.m_wmpObj = WMPObj1;
//	    if(WMPIntreface.m_wmpObj.settings.mediaAccessRights != "full"){ 
//	    	WMPIntreface.m_wmpObj.settings.requestMediaAccessRights('full');
//	    }
	    WMPIntreface.m_wmpHolder = WMPHolder;
	    WMPIntreface.m_swfObj = SWFObj;
	    WMPIntreface.m_swfHolder = SWFHolder;
	    WMPIntreface.m_width = WMPIntreface.getWidth();
	    WMPIntreface.m_height = WMPIntreface.getHeight();
	    WMPIntreface.m_x = WMPIntreface.getX();
	   	WMPIntreface.m_y = WMPIntreface.getY();
	   	WMPIntreface.m_currentURL = "";
	   	WMPIntreface.m_duration = -1;
	   	WMPIntreface.m_seek_map = null;
	   	WMPIntreface.m_total_entries = 1;
	   	WMPIntreface.m_currentIndex = 0;
	   	WMPIntreface.m_lastPosition = 0;
	    WMPIntreface.setVisible(false);
	    try
	    {	    
	        if (navigator.appName == "Microsoft Internet Explorer") 
	        {
	            if (!window.addEventListener) 
	            {
	                WMPIntreface.m_wmpObj.attachEvent("PlayStateChange", WMPIntreface.checkPlayStateChange);	   
	                WMPIntreface.m_wmpObj.attachEvent("StatusChange", WMPIntreface.statusChange);	   
	                WMPIntreface.m_wmpObj.attachEvent("Buffering", WMPIntreface.bufferingHandler);	   
	                WMPIntreface.m_wmpObj.attachEvent("MediaError", WMPIntreface.mediaErrorHandler);	   
	                WMPIntreface.m_wmpObj.attachEvent("MediaChange", WMPIntreface.mediaChangeHandler);	   
	            }
	        }
	    }catch(e)
	    {	 
	        var eee = e.message;   
	    }	    
	   	init = true;
	   	 //WMPIntreface.testit();
	}

    WMPIntreface.testit = function()
    {
    	if (WMPIntreface.m_wmpObj.controls == null)
	   {	    
	        setTimeout(WMPIntreface.testit,1000);
	        return;
	   }else{
	   		//alert(WMPIntreface.m_interfaceName);
	   }
    }


    WMPIntreface.preExecute = function()
    {
        return !init || WMPIntreface.m_wmpObj.controls != null;            
    }

    WMPIntreface.load = function(sURL, bAutoplay) {
        WMPIntreface.m_lastPosition = 0;
        WMPIntreface.m_currentIndex = 0;
        WMPIntreface.m_total_entries = 1;
        WMPIntreface.m_duration = -1;
        WMPIntreface.m_currentURL = sURL;
        WMPCurrentURL = sURL;
        WMPIntreface.m_seek_map = null;
        if (WMPIntreface.m_wmpObj.controls == null) {
            //setTimeout(WMPIntreface.load,1000,sURL,bAutoplay);
            setTimeout(function() { WMPIntreface.load(sURL, bAutoplay); }, 1000);
            return;
        }
        WMPIntreface.m_currentRatio = null;
        try {
            WMPIntreface.m_wmpObj.URL = sURL;
            /*			
            var newMedia = WMPIntreface.m_wmpObj.newMedia(sURL);
            //if(WMPIntreface.m_wmpObj.currentPlaylist.count == 0){
            //alert(WMPIntreface.m_wmpObj.currentPlaylist.count);
            //if(WMPIntreface.m_wmpObj.currentMedia != null)
            //	WMPIntreface.m_wmpObj.currentPlaylist.removeItem(WMPIntreface.m_wmpObj.currentMedia);
            WMPIntreface.m_wmpObj.currentPlaylist.appendItem(newMedia);
            WMPIntreface.m_wmpObj.currentPlaylist.appendItem(newMedia);
            WMPIntreface.m_wmpObj.controls.currentItem = WMPIntreface.m_wmpObj.currentPlaylist.item(WMPIntreface.m_wmpObj.currentPlaylist.count-2);
            //}
            //*/
            if (bAutoplay) {
                WMPIntreface.setVisible(true);
                setTimeout(WMPIntreface.play, 1000);
            }
        } catch (e) {
            //alertObj(e);
        }
    }		

	WMPIntreface.statusChange = function (){
//		trace("statusChange >> " + WMPIntreface.m_interfaceName + " | " + WMPIntreface.m_wmpObj.status);
		if(WMPIntreface.m_wmpObj.status == null)
			return;
		switch(WMPIntreface.m_wmpObj.status){
			case "Finished":
				break;
			case "Media changing...":
				WMPWasFullscreen = WMPIntreface.getFullscreen();
				break;
			case "Paused":
				WMPWasFullscreen = WMPIntreface.getFullscreen();
				break;
			case "Opening media...":
				if(WMPIntreface.m_wmpObj.currentPlaylist.item(0).getItemInfo("seek_map") != "")
					WMPIntreface.m_seek_map = WMPIntreface.m_wmpObj.currentPlaylist.item(0).getItemInfo("seek_map");
				if(WMPIntreface.m_wmpObj.currentPlaylist.item(0).getItemInfo("total_entries") != "")
					WMPIntreface.m_total_entries = WMPIntreface.m_wmpObj.currentPlaylist.item(0).getItemInfo("total_entries") - 1;
				break;
			case "Ready":
				break;
			default:	
				try{		
					if(WMPIntreface.m_wmpObj.status.split(" ")[0] == "Playing"){				
						WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_receive_meta");
					}
				}catch(e){
					//trace(e);
				}
				break;
		}
	}
	
	WMPIntreface.mediaErrorHandler = function (e){
		//trace("mediaErrorHandler >> " + e);
	}
	
	WMPIntreface.mediaChangeHandler = function (e){
		//trace("mediaChangeHandler >> " + e);
	}
	WMPIntreface.bufferingHandler = function (e){
		if(e){
			WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_buffering_start");
		}else{
			WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_buffering_full");
		}
	}
	WMPIntreface.checkPlayStateChange = function (stat){
//		if(!WMPIntreface.getVisible()){
//			return;
//		}
	    try
	    {
	    	//trace(stat + ", " + WMPIntreface.m_interfaceName);
		    switch(stat){
			    case 0:
    				
			    break;
			    case 1:
				    WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_stop");
			    break;
			    case 2:			    
				    WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_pause");				    
			    break;
			    case 3:
			    	if(WMPIntreface.isMainMovie())
			    		WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "tvbox.events.CommercialEvent", "CommercialEventCommercialSeqEnd");
			    	else
			    		WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "tvbox.events.CommercialEvent", "CommercialEventCommercialStartPlay");
//			    	trace("=====");
//				    trace("seek_map: " + WMPIntreface.m_wmpObj.currentPlaylist.item(0).sourceURL/* + ", " + WMPIntreface.m_currentURL */);
//			    	trace("=++++=");
			    	//trace(WMPIntreface.m_interfaceName + " = "  + WMPIntreface.m_wmpObj.playlistCollection.getAll().count);
				    WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_play");
				    if(WMPIntreface.m_currentRatio == null)
					    WMPIntreface.m_currentRatio = WMPIntreface.getOriginWidth() / WMPIntreface.getOriginHeight();
			    break;
			    case 8:
			    	if(!WMPIntreface.isMainMovie())
			    		WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "tvbox.events.CommercialEvent", "CommercialEventCommercialSeqEnd");
			    	else
			    		WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "tvbox.events.CommercialEvent", "CommercialEventCommercialStartPlay");
			    	trace("WMPIntreface.m_currentIndex >> " + WMPIntreface.m_currentIndex + " / " + WMPIntreface.m_total_entries);
			    	if(WMPIntreface.m_currentIndex == WMPIntreface.m_total_entries - 1){
					    WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_stop");
					    WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_complete");
					    WMPIntreface.stop();
					    WMPIntreface.m_currentIndex = 0;
			    	}
			    	WMPIntreface.m_currentIndex ++;
		    	break;
			    case 9:
			    break;
			    case 10:
				    //WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "as3.events.PlaybackEvent", "playback_receive_meta");
			    break;
		    }
		}
		catch(e)
		{
			if (typeof (top.TvinciPlayerAddToLog) == 'function')
	        {
	            top.TvinciPlayerAddToLog("WMPIntreface.checkPlayStateChange - error '" + e.message + "'");
	        }

		}
	}
									
	WMPIntreface.startDisptachProgress = function(){
		var prog = WMPIntreface.m_wmpObj.network.downloadProgress;
		if(prog != 100){
			//setTimeout(WMPIntreface.startDisptachProgress, 150);
		}
		//WMPIntreface.sendEventToFlash("as3.events.PercentProgressEvent", "progress", prog);
	}
	
	WMPIntreface.isPlaying = function(){
		return WMPIntreface.getStatus() == 3;
	}
	
	WMPIntreface.getStatus = function(){
		return WMPIntreface.m_wmpObj.playState;
	}
	
	WMPIntreface.play = function(sURL){	
		WMPIntreface.setVisible(true);
	 	WMPIntreface.m_wmpObj.controls.play();
	}
	
	WMPIntreface.pause = function(){
		if(WMPIntreface.m_currentURL.substring(0,3) == "mms")
			WMPIntreface.m_wmpObj.controls.stop();
		else
			WMPIntreface.m_wmpObj.controls.pause();
	}
	
	WMPIntreface.stop = function(){
		//WMPIntreface.m_wmpObj.controls.stop();
		WMPIntreface.clear();
	}
	
	WMPIntreface.clear = function(){
		WMPIntreface.m_duration = -1;
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
		WMPIntreface.setVisible(n != 0);
//		trace("WMPIntreface.isMainMovie >> " + WMPIntreface.isMainMovie());
		if(WMPIntreface.isMainMovie() && WMPIntreface.m_seek_map != null){
			var index = WMPIntreface.getIndexByTime(n);
			WMPIntreface.m_currentIndex = index - 1;
			//trace("setPosition>> index: " + index + ", count: " + WMPIntreface.m_wmpObj.currentPlaylist.count);
			if(WMPIntreface.m_wmpObj.currentPlaylist.item(index) != WMPIntreface.m_wmpObj.currentMedia)
				WMPIntreface.m_wmpObj.controls.playItem(WMPIntreface.m_wmpObj.currentPlaylist.item(index));
			var addPosition = Number(WMPIntreface.getStartTimeByIndex(index));
			WMPIntreface.m_wmpObj.controls.currentPosition = n - addPosition;
		}else{
			WMPIntreface.m_wmpObj.controls.currentPosition = n;
		}
	}
	//var tempCounter = 0;
	WMPIntreface.getPosition = function(){
		if(WMPIntreface.isMainMovie()){
			try{
				var index = WMPIntreface.m_wmpObj.controls.currentItem.getItemInfo("item_index");
				var addPosition = Number(WMPIntreface.getStartTimeByIndex(index));
				WMPIntreface.m_lastPosition = WMPIntreface.m_wmpObj.controls.currentPosition + addPosition;
				trace(index + ", " + addPosition + ", " + WMPIntreface.m_wmpObj.controls.currentPosition);
				return WMPIntreface.m_lastPosition;
			}catch(e){
				return WMPIntreface.m_wmpObj.controls.currentPosition;
			}
		}else{
			//return WMPIntreface.m_wmpObj.controls.currentPosition;
			return WMPIntreface.m_lastPosition >=0 ? WMPIntreface.m_lastPosition : WMPIntreface.m_wmpObj.controls.currentPosition;
		}
	}
	
	WMPIntreface.isMainMovie = function(){
		var ret = !(WMPIntreface.m_wmpObj.controls.currentItem.getItemInfo("entry_type") != "main") || WMPIntreface.m_seek_map == null;
		//trace("isMainMovie >> " + ret + ", entry_type: " + WMPIntreface.m_wmpObj.controls.currentItem.getItemInfo("entry_type") + ", map: " + WMPIntreface.m_seek_map);
		return ret;
		var map = WMPIntreface.m_seek_map;
		if(map == null)
			return true;
		var arr = map.split(";");
		for(var i = 0 ; i < arr.length ; i ++){
			var arr2 = arr[i].split(":");
			trace("isMainMovie" + i + ": m_currentIndex: " + WMPIntreface.m_currentIndex + ", index: " + arr2[1]);
			if(Number(WMPIntreface.m_currentIndex) - 1 ==  Number(arr2[1])){
				return true;	
			}			
		}
		return false;
	}
	
	WMPIntreface.getIndexByTime = function(n){
		var lastIndex = -1;
		var map = WMPIntreface.m_seek_map;
		var arr = map.split(";");
		trace("getIndexByTime: pos: " + n + ", map: " + map);
		for(var i = 0 ; i < arr.length ; i ++){
			var arr2 = arr[i].split(":");
			if(Number(n) >= Number(arr2[0])){
				lastIndex = arr2[1];	
			}			
		}
		return lastIndex;
	}
	WMPIntreface.getStartTimeByIndex = function(index){
		var map = WMPIntreface.m_seek_map;
		var arr = map.split(";");
		for(var i = 0 ; i < arr.length ; i ++){
			var arr2 = arr[i].split(":");
			if(arr2[1] == index)
				return arr2[0];
		}
		return -1;
	}
	
	WMPIntreface.setVolume = function(n){
		WMPIntreface.m_wmpObj.settings.volume = n * 100;
	}
	
	WMPIntreface.getVolume = function(){
		return WMPIntreface.m_wmpObj.settings.volume;
	}
	
	WMPIntreface.getDuration = function(){
//		trace("WMPIntreface.getDuration: entry type: " + WMPIntreface.m_wmpObj.controls.currentItem.getItemInfo("entry_type"));
		if(WMPIntreface.m_duration != -1/* && WMPIntreface.isMainMovie()*/){
//			trace("WMPIntreface.getDuration: " + WMPIntreface.m_duration);
			return WMPIntreface.m_duration;
		}else{
			return WMPIntreface.m_wmpObj.controls.currentItem.duration;
		}
	}
	
	WMPIntreface.setDuration = function(n){
		//trace("WMPIntreface.setDuration > " + n);
		WMPIntreface.m_duration = n;
	}
	
	WMPIntreface.setWidth = function(n){
		WMPIntreface.m_width = n;
//		if(!WMPIntreface.m_visible && !isIE)
//			return;
		WMPIntreface.m_wmpObj.width = n;
		WMPIntreface.m_wmpHolder.style.width = n + "px";
	}
	
	WMPIntreface.getWidth = function(){
		return WMPIntreface.m_wmpObj.width;
	}
	
	WMPIntreface.setHeight = function(n){
		WMPIntreface.m_height = n;
//		if(!WMPIntreface.m_visible && !isIE)
//			return;
		WMPIntreface.m_wmpObj.height = n;
		WMPIntreface.m_wmpHolder.style.height = n + "px";
	}
	
	WMPIntreface.getHeight = function(){
		return WMPIntreface.m_wmpObj.height;
	}
	
	WMPIntreface.setX = function(n){
		WMPIntreface.m_x = n;
		if(!WMPIntreface.m_visible && !isIE)
			return;
		WMPIntreface.m_wmpHolder.style.left = n + "px";
	}
	
	WMPIntreface.getX = function(){
		if(!WMPIntreface.m_visible && !isIE)
			return WMPIntreface.m_x;
		return Number(WMPIntreface.m_wmpHolder.style.left);
	}
	
	WMPIntreface.setY = function(n){
		WMPIntreface.m_y = n;
		
		if(!WMPIntreface.m_visible && !isIE)
			return;
							    				    
		WMPIntreface.m_wmpHolder.style.top = n + "px";				
	}
	
	WMPIntreface.getY = function(){
		if(!WMPIntreface.m_visible && !isIE)
			return WMPIntreface.m_y;
		return Number(WMPIntreface.m_wmpHolder.style.top);
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
			WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "flash.events.FullScreenEvent", "fullScreen" , false , false , false);
	        try{
	        	GetVG().changeScreenSaverStatus(true);
	        }catch(e){
	        }
	    }else
	    {
	        setTimeout(WMPIntreface.checkFullScreenStatus, 10000);
	    }	
	}
	
	WMPIntreface.setFullscreen = function(b){
		WMPIntreface.m_fullscreen = b;
		try{
	    	GetVG().changeScreenSaverStatus(!b);
        }catch(e){
        }
		try{
     		WMPIntreface.m_wmpObj.fullScreen = b;
     		if(b)
				WMPFullscreenWMPObj = WMPIntreface.m_wmpObj;
        }catch(e){
        	//alertObj(e);
        }
	    if (b)
	    {
			WMPIntreface.checkFullScreenStatus();
			WMPIntreface.sendEventToFlash(WMPIntreface.m_interfaceName, "flash.events.FullScreenEvent", "fullScreen" , false , false , true); 
	    }
	    return "Guy Test";
	}
	
	WMPIntreface.getFullscreen = function(){
		return WMPIntreface.m_wmpObj.fullScreen;
	}
	
	WMPIntreface.swapFullscreen = function(){
		try{
			if(WMPFullscreenWMPObj != null){
					WMPFullscreenWMPObj.fullScreen = false;
			}
			setTimeout(WMPIntreface.swapFullscreen2, 0);
        }catch(e){
			setTimeout(WMPIntreface.swapFullscreen2, 200);
        }
	}
	
	WMPIntreface.swapFullscreen2 = function(){
		WMPIntreface.setFullscreen(true);
	}
	
	WMPIntreface.getVisible = function(){
		return WMPIntreface.m_visible;
	}
	
	WMPIntreface.setVisible = function(b){		        
	    this.m_visible = b;    
	    
		if(isIE){
			WMPIntreface.m_wmpObj.style.display = b ? "inline" : "none";
			WMPIntreface.m_wmpHolder.style.display = b ? "inline" : "none";
		}else{		
		
		    
			WMPIntreface.m_wmpHolder.style.top = b ? WMPIntreface.m_y + "px" : -1000 + "px";
			WMPIntreface.m_wmpHolder.style.left = b ? WMPIntreface.m_x + "px" : 0 + "px";
			WMPIntreface.m_wmpObj.style.top = b ? WMPIntreface.m_y + "px" : -1000 + "px";
			WMPIntreface.m_wmpObj.style.left = b ? WMPIntreface.m_x + "px" : 0 + "px";                                                                                   									
		}
	}

	WMPIntreface.sendEventToFlash = function(callerInterfaceName, className, eventType){
		var params = new Array();
		for(var i = 0 ; i < arguments.length ; i++){
			params.push(arguments[i]);
		}
		params = params.slice(3);
		WMPIntreface.m_swfObj.callFlashAction({action:"js_video_event",callerInterfaceName:callerInterfaceName, className:className, eventType:eventType, params:params});
	}
	return WMPIntreface;
}