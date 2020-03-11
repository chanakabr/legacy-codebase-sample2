var IE = document.all;
var zTrack = 0;
var curOpacity = 0;
var direction =0;

//constants
var INTERVAL = 40;
var TIME_TO_STAY = 5000;
var STEP = 3;

function initTextCommercial() {
	mHTML = "";
	for(i=0;i<mContent.length;i++) {
		mHTML+= "<div id=\"xContent\" name=\"xContent\" class=\"zContent\">" + mContent[i] + "</div>";
	}
	document.getElementById("mContainer").innerHTML = mHTML;
	setTimeout("rotate()",INTERVAL);
}

function rotate() {
	direction?curOpacity-=STEP:curOpacity+=STEP;
	IE?document.getElementsByName("xContent")[zTrack].style.filter="alpha(opacity="+ curOpacity+")":document.getElementsByName("xContent")[zTrack].style.MozOpacity=curOpacity/100;
	if(curOpacity>=90) {
		curOpacity=90;
		direction=1;
		setTimeout("rotate();",TIME_TO_STAY);
	} else if(curOpacity<=0) {
		curOpacity=0;
		direction=0;
		zTrack++;
		if(zTrack>mContent.length-1)
		    zTrack=0;
		setTimeout("rotate();",INTERVAL);
	}
	else
	    setTimeout("rotate();",INTERVAL);
	
}
