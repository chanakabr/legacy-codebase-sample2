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

function callback_Login()
{
    if (xmlhttp.readyState==4) 
    { 
      if (xmlhttp.status==200) 
      { 
        
        result1=xmlhttp.responseText; 
        result = result1.split("~~|~~")[0];
        if (result == "WRONG_USERNAME_PASS")
        {
            errorMessage = "<p>Wrong user name or password</p>";
            document.getElementById("password").className = "error";
            document.getElementById("name").className = "error";
            document.getElementById("error").innerHTML = errorMessage;
            OpenID("error");
            
        }
        else if (result == "ACOUNT_LOCK")
        {
            errorMessage = "<p>Too many mistakes - login will be available in a few minutes. </p>";
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
            errorMessage = "<p>The IP you are trying to login from is not allowed</p>";
            document.getElementById("password").className = "error";
            document.getElementById("name").className = "error";
            document.getElementById("error").innerHTML = errorMessage;
            OpenID("error");
        }
        else if (result == "ACOUNT_LOGOUT_LOCK")
        {
            errorMessage = "<p>Logout was done wrong - login will be available in a few minutes. </p>";
            document.getElementById("password").className = "error";
            document.getElementById("name").className = "error";
            document.getElementById("error").innerHTML = errorMessage;
            OpenID("error");
        }
        else
        {
            window.document.location.href = result;
        }
	  }
    }
}
