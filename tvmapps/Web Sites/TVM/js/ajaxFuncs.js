// JScript File
function getFile(pURL , pHandlerFunc) 
{
   if (window.XMLHttpRequest) { // code for Mozilla, Safari, etc 
      xmlhttp=new XMLHttpRequest();
      xmlhttp.onreadystatechange=pHandlerFunc;
      xmlhttp.open("GET", pURL, true);
      xmlhttp.send(null);
   } 
   else if (window.ActiveXObject) 
   { //IE 
      xmlhttp=new ActiveXObject('Microsoft.XMLHTTP'); 
      if (xmlhttp) 
      {
         xmlhttp.onreadystatechange=pHandlerFunc;
         xmlhttp.open('GET', pURL, true);
         xmlhttp.send();
      }
   }
}

function postFile(pURL , pHandlerFunc) 
{
    pParameters = "";
    if (pURL.split("?").length >1)
        pParameters = pURL.split("?")[1];
    if (window.XMLHttpRequest) { // code for Mozilla, Safari, etc 
      xmlhttp=new XMLHttpRequest();
      xmlhttp.onreadystatechange=pHandlerFunc;
      xmlhttp.open("POST", pURL, true);
      xmlhttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
      xmlhttp.send(pParameters);
    } 
    else if (window.ActiveXObject) 
    { //IE 
      xmlhttp=new ActiveXObject('Microsoft.XMLHTTP'); 
      if (xmlhttp) 
      {
         xmlhttp.onreadystatechange=pHandlerFunc;
         xmlhttp.open('POST', pURL, true);
         xmlhttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
         xmlhttp.send(pParameters);
      }
    }
}

function loginToSite(sEmail , sPass)
{
    sURL = "AjaxLogin.aspx?email=" + escape(sEmail) + "&pass=" + escape(sPass);
    postFile(sURL , callback_Login);
}

function getToken(sEmail, sPass) {
    sURL = "AjaxToken.aspx?email=" + escape(sEmail);
    postFile(sURL, callback_Token);
}

function getTokenApprove(sEmail, sPass) {
    sURL = "AjaxTokenApprove.aspx?email=" + escape(sEmail);
    postFile(sURL, callback_TokenApprove);
}

function callback_Login() {
   
    if (xmlhttp.readyState==4) 
    { 
      if (xmlhttp.status==200) 
      { 
        
        result1=xmlhttp.responseText; 
        result = result1.split("~~|~~")[0];
        if (result == "WRONG_USERNAME_PASS")
        {
            errorMessage = "<p>Wrong user name or password or too many mistakes(login will be available in a few minutes).</p>";
            document.getElementById("password").className = "error";
            document.getElementById("name").className = "error";
            document.getElementById("error").innerHTML = errorMessage;
            OpenID("error");
            
        }
        else if (result == "ACOUNT_LOCK")
        {
            errorMessage = "<p>Wrong user name or password or too many mistakes(login will be available in a few minutes).</p>";
            document.getElementById("password").className = "error";
            document.getElementById("name").className = "error";
            document.getElementById("error").innerHTML = errorMessage;
            OpenID("error");
        }
        else if (result == "HTTPS_REQUIERED")
        {
            errorMessage = "<p>Login should be done only on HTTPS. </p>";
            document.getElementById("error").innerHTML = errorMessage;
            OpenID("error");
        }
        else if (result == "IP_NOT_ALLOWED")
        {
            errorMessage = "<p>The IP you are trying to login from is not allowed. You will be redirected to the token request page in 5 seconds</p>";
            document.getElementById("password").className = "error";
            document.getElementById("name").className = "error";
            document.getElementById("error").innerHTML = errorMessage;
            OpenID("error");
            setTimeout("window.document.location.href = 'token.html'", 5000);
            
        }
        else if (result == "ACOUNT_LOGOUT_LOCK")
        {
            errorMessage = "<p>Logout was done wrong - login will be available in a few minutes. </p>";
            document.getElementById("password").className = "error";
            document.getElementById("name").className = "error";
            document.getElementById("error").innerHTML = errorMessage;
            OpenID("error");
        }
        else {
            if (result != "")
                window.document.location.href = result;
            else
                window.document.location.href = "adm_media.aspx";
        }
	  }
    }
}

function callback_PicResize() {
    if (xmlhttp.readyState == 4) {
        if (xmlhttp.status == 200) {

            alert('Done');
//            result1 = xmlhttp.responseText;
//            result = result1.split("~~|~~")[0];
//            if (result == "WRONG_USERNAME_PASS") {
//                errorMessage = "<p>Wrong user name or password or too many mistakes(login will be available in a few minutes).</p>";
//                document.getElementById("password").className = "error";
//                document.getElementById("name").className = "error";
//                document.getElementById("error").innerHTML = errorMessage;
//                OpenID("error");

//            }
//            else if (result == "ACOUNT_LOCK") {
//                errorMessage = "<p>Wrong user name or password or too many mistakes(login will be available in a few minutes).</p>";
//                document.getElementById("password").className = "error";
//                document.getElementById("name").className = "error";
//                document.getElementById("error").innerHTML = errorMessage;
//                OpenID("error");
//            }
//            else if (result == "HTTPS_REQUIERED") {
//                errorMessage = "<p>Login should be done only on HTTPS. </p>";
//                document.getElementById("error").innerHTML = errorMessage;
//                OpenID("error");
//            }
//            else if (result == "IP_NOT_ALLOWED") {
//                errorMessage = "<p>The IP you are trying to login from is not allowed. You will be redirected to the token request page in 5 seconds</p>";
//                document.getElementById("password").className = "error";
//                document.getElementById("name").className = "error";
//                document.getElementById("error").innerHTML = errorMessage;
//                OpenID("error");
//                setTimeout("window.document.location.href = 'token.html'", 5000);

//            }
//            else if (result == "ACOUNT_LOGOUT_LOCK") {
//                errorMessage = "<p>Logout was done wrong - login will be available in a few minutes. </p>";
//                document.getElementById("password").className = "error";
//                document.getElementById("name").className = "error";
//                document.getElementById("error").innerHTML = errorMessage;
//                OpenID("error");
//            }
//            else {
//                if (result != "")
//                    window.document.location.href = result;
//                else
//                    window.document.location.href = "adm_media.aspx";
//            }
        }
    }
}


function moveTo(url) {
    var referLink = document.createElement('a'); referLink.href = url; document.body.appendChild(referLink); referLink.click(); 
}
function moveToBlank(url) {
    var referLink = document.createElement('a'); referLink.target='_blank'; referLink.href = url; document.body.appendChild(referLink); referLink.click();
}


function callback_Token() {
    if (xmlhttp.readyState == 4) {
        if (xmlhttp.status == 200) {

            result1 = xmlhttp.responseText;
            result = result1.split("~~|~~")[0];
            if (result == "WRONG_USERNAME_PASS") {
                errorMessage = "<p>Unknown user name or password</p>";
                document.getElementById("name").className = "error";
                document.getElementById("error").innerHTML = errorMessage;
                OpenID("error");
            }
            else {
                errorMessage = "<p>The token will be sent to your account mail. In order to proceed you should enter the token to the approve page. You will be redirected to the token approve page in 5 seconds. </p>";
                document.getElementById("error").innerHTML = errorMessage;
                OpenID("error");
                setTimeout("window.document.location.href = '" + result + "'", 5000);
            }
        }
    }
}

function callback_TokenApprove() {
    if (xmlhttp.readyState == 4) {
        if (xmlhttp.status == 200) {

            result1 = xmlhttp.responseText;
            result = result1.split("~~|~~")[0];
            if (result == "WRONG_USERNAME_PASS") {
                errorMessage = "<p>Unknown token</p>";
                document.getElementById("name").className = "error";
                document.getElementById("error").innerHTML = errorMessage;
                OpenID("error");
            }
            else {
                errorMessage = "<p>Your IP is valid for the next 12 hours. You will be redirected to the Login page in 5 minutes.</p>";
                document.getElementById("error").innerHTML = errorMessage;
                OpenID("error");
                setTimeout("window.document.location.href = '" + result + "'", 5000);
                window.document.location.href = result;
            }
        }
    }
}
