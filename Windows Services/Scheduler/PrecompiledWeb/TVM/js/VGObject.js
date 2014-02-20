// VGDK Wrapper Class


var ErrNDSCode=0;	
var ErrType=1;
var ErrCode=2;
var ErrDescription=3;
var ErrPlayerBufferAction=4;
var ErrPlayerAction=5;

/*

var errCodes = [ 
 // [{identifier token}, {Code}, {Description}]
	['1, 0, 0, 9, 1', "ACTION", 'STARTED_BUFFERING',"באפר התחיל", "buffer_start" , "none"],
	['0, 0, 0, 9, 1', "ACTION", 'STOPPED_BUFFERING',"באפר הפסיק" , "buffer_stop" , "none"],
	['1, 0, 0, 0, 0','MESSAGE','E_GENERAL_FAILURE','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 0, 2','MESSAGE','E_GENERAL_MALFORMED_PATH','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 0, 3','MESSAGE','E_GENERAL_PATH_NOT_FOUND','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 0, 4','MESSAGE','E_GENERAL_PATH_READ_ONLY','אין הרשאה למשתמש זה לבצע התקנות תוכנה במחשב. אנא הכנס עם שם משתמש בעל הרשאות מתאימות. ','message','none'],
    ['1, 0, 0, 0, 5','MESSAGE','E_GENERAL_FILE_NOT_FOUND','לא ניתן להמשיך בביצוע הפעולה, אנא אתחל מחדש את הדפדפן. במידה והבעיה חוזרת  עליך להתקין מחדש את התוכנה. ','message','none'],
    ['1, 0, 0, 0, 6','MESSAGE','E_GENERAL_IO','לא ניתן להמשיך בביצוע הפעולה, אנא בדוק האם קיים מקום פנוי בדיסק ואת תקינותו. ','message','none'],
    ['1, 0, 0, 0, 7','MESSAGE','E_GENERAL_DISK_FULL ','לא ניתן להמשיך בביצוע הפעולה בשל מגבלת מקום, אנא פנה מקום בדיסק. ','message','none'],
    ['1, 0, 0, 0, 8','MESSAGE','E_GENERAL_CATALOG_FULL','לא ניתן להמשיך בביצוע הפעולה בשל מגבלת מקום, אנא פנה מקום בדיסק ','message','none'],
    ['1, 1, 0, 0, 9','MESSAGE','E_GENERAL_FATAL_INTERNAL','לא ניתן להמשיך בביצוע הפעולה, אנא אתחל מחדש את המחשב והדפדפן. במידה והבעיה חוזרת  עליך להתקין מחדש את התוכנה. ','message','none'],
    ['1, 0, 0, 0, 10','MESSAGE','E_GENERAL_NO_LOGIN','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 0, 11','MESSAGE','E_GENERAL_LOGIN_EXPIRED','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 0, 12','MESSAGE','E_GENERAL_SOFTWARE_OUTDATED','לביצוע פעולה זו, נדרש לבצע שדרוג לתוכנה. לחץ כאן לעדכון גרסה.  ','message','none'],
    ['1, 0, 0, 0, 13','MESSAGE','E_GENERAL_CATALOG_CORRUPTED','לא ניתן להמשיך בביצוע פעולה זו, בשל בעיות בכתיבה לדיסק. אנא בדוק תקינותו. ','message','none'],
    ['1, 0, 0, 0, 14','MESSAGE','E_GENERAL_BAD_INPUT','לא ניתן להמשיך בביצוע הפעולה, אנא אתחל מחדש את הדפדפן. במידה והבעיה חוזרת, עליך להתקין מחדש את התוכנה. ','message','none'],
    ['1, 0, 0, 0, 15','MESSAGE','E_GENERAL_CONFIG_ERROR','לא ניתן להמשיך בביצוע הפעולה, עליך להתקין מחדש את התוכנה. ','message','none'],
    ['1, 0, 0, 0, 16','MESSAGE','E_GENERAL_WINDOW_ALREADY_OPEN','על מנת להמשיך ולצפות בוידיאו עליך לסגור את החלון הנוסף','message','none'],
    ['1, 0, 0, 0, 17','MESSAGE','E_GENERAL_CONFIG_UNABLE_TO_SET_PARAM','לא ניתן להמשיך בביצוע הפעולה, עליך להתקין מחדש את התוכנה. ','message','none'],
    ['1, 0, 0, 0, 18','MESSAGE','E_GENERAL_CONFIG_UNKNOWN_PARAM','לא ניתן להמשיך בביצוע הפעולה, עליך להתקין מחדש את התוכנה. ','message','none'],
    ['1, 0, 0, 0, 19','MESSAGE','E_GENERAL_ASSET_NOT_FOUND','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 0, 20','MESSAGE','E_GENERAL_ASSET_EXISTS','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 0, 21','MESSAGE','E_GENERAL_BAD_ASSET_ID','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 1, 0','MESSAGE','E_LOGIN_GENERAL','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 1, 1','MESSAGE','E_LOGIN_WRONG_CREDENTIALS','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 1, 2','MESSAGE','E_LOGIN_ANOTHER_LOGGED_IN','ניתן לצפות בוידיאו רק באמצעות חשבון אחד, אנא סגור את את החשבון הנוסף שפתוח כעת. ','message','none'],
    ['1, 0, 0, 1, 3','MESSAGE','E_LOGIN_UPDATE_REQUIRED','לטובת ביצוע הפעולה, עליך לבצע עדכון גרסה לתוכנה. לחץ כאן לשדרוג. ','message','none'],
    ['1, 0, 0, 1, 4','MESSAGE','E_LOGIN_NO_COMM','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 1, 5','MESSAGE','E_LOGIN_SD_REQUIRED','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 3, 1','MESSAGE','E_BIZMODEL_INVALID_USER','המשתמש אינו קיים במערכת, אנא בדוק שנית את הפרטים שהזנת. ','message','none'],
    ['1, 0, 0, 3, 2','MESSAGE','E_BIZMODEL_NOT_FOUND','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 8, 21','MESSAGE','TSP_UNKNOWN_ERR','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none'],
    ['1, 0, 0, 8, 22','MESSAGE','TSP_NOT_ENOUGH_MEM_ERR','על מנת לצפות באופן מיטבי בוידיאו, סגור את החלונות והיישומים הפתוחים כעת במחשבך','message','none'],
    ['1, 0, 0, 8, 29','MESSAGE','TSP_CONFIG_ERR','לטובת ביצוע הפעולה, עליך לבצע עדכון גרסה לתוכנה. לחץ כאן לשדרוג. ','message','none'],
    ['1, 0, 0, 8, 37','MESSAGE','TSP_ALREADY_LOADED','ניתן לצפות בוידיאו רק בחלון אחד. אנא סגור את החלון הנוסף. ','message','none'],
    ['1, 0, 0, 8, 38','MESSAGE','TSP_CANT_ACQUIRE_LICENSE','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none']]

]
//*/
function findErr(errCode) 
{  
    if (errCode != null && errCode != 'undefined') 
    {             		
    	for(j=0;j<errCodes.length;j++) 
	    {
		    code=errCodes[j][ErrNDSCode];
		    
  		    if(errCode.indexOf(code)!=-1) 
		    {
			    return errCodes[j];
  		    }
	    }
	}else
	{
	    errCode = -1;
	}
	    
    return [errCode, "MESSAGE", 'UNKNOWN_ERROR',"תקלה זמנית – אנא נסו שנית בעוד דקות מספר" , "message" , "none"];
}


function getFlashObject()
{    
	var element = document.getElementsByName("player");
		if (element != 'undefined' && element != null)
		{
    		return element[0];
	}else
	{
	    return null;
	}
}

function SendEventToFlash(eventCode , theMessage , theType , theAction)
{
	try
	{
		getFlashObject().jsEventHandler(eventCode, theMessage, theType, theAction);	
	}
	catch(theException){}
}

	
	

// ************************* START OF Constructor *************************
function VGObject() {

    // fields
    this._vgdk = null;    
    this._version= "?";
    this._requiresInstallOrUpgrade = true;	    
    this._isInitialized = false;
            
    // private methods
    this.login = loginMethod;
    this.logout = logoutMethod;        
    this.createVgdk = createVgdkMethod;
    
    // public methods    
    this.initializeVgdk = initializeVgdkMethod;            
    this.verifyLogin = verifyLoginMethod;
    
    // public properties
    this.getVersion = getVersionMethod;
    this.getRequiresInstallOrUpgrade = getRequiresInstallOrUpgradeMethod;	
    this.getIsInitialized = getIsInitializedMethod;                        
}             
// ************************* END OF Constructor *************************


// ************************* START OF Public Methods *************************
 
function verifyLoginMethod(userName, password)
{
    TvinciPlayerAddToLog("verifyLogin - for username '" + userName + "' password '" + password + "'");
    

    if (!this._isInitialized)
    {
        throw("verifyLogin - You must perform initialization before executing the method");
    } 
    
    if (this._vgdk == null)
	{	    
	    return false;
	}
            
    if (userName == "" || password == "")    
    {                    
        throw ("verifyLogin - Username and password must be set");
    }
                
    try
    {                
        TvinciPlayerAddToLog("verifyLogin - executing 'IsUserLoggedIn' method");
        if (this._vgdk.IsUserLoggedIn(userName))
        {
            TvinciPlayerAddToLog("verifyLogin - executing 'IsUserLoggedIn' returned with 'true'");
            return true;
        }
        else
        {
            TvinciPlayerAddToLog("verifyLogin - the user is not logged. performing logging");
            this.logout();                        
            return this.login(userName,password);                                          
        }
    }catch(e)
    {
        TvinciPlayerAddToLog("verifyLogin - executing 'IsUserLoggedIn' method failed! performing manual logout & login");
        this.logout();                        
        return this.login(userName,password);                                          
    }               				                    
}

function initializeVgdkMethod(userName, password) {  
      
    TvinciPlayerAddToLog("initializeVgdk - entering method with username '" + userName + "' pass '" + password + "'");
    
    this._isInitialized = true;
    
                    
    this.createVgdk();
    
    if (this._vgdk == null)
    {        
        this._version="?";
        this._requiresInstallOrUpgrade = true;	        
    }else
    {      
        try
        {            
		    this._version = this._vgdk.AppVersion;
                        
            if(this._version != null) 
            {        
                this._requiresInstallOrUpgrade = false ;// default value set to false to support situation if an error will occur while tring to check against activeX            
                
                if (this.verifyLogin(userName,password))
                {
                    var resUpgrade = this._vgdk.IsSoftwareUpgradeRequired();                    
                    TvinciPlayerAddToLog("initializeVgdk - calling 'IsSoftwareUpgradeRequired' returned with result '" + resUpgrade + "'");
                    
                    if (resUpgrade) 
                    {   
                        // need to upgrade
                        this._requiresInstallOrUpgrade = true;                                             
                    }
                    else 
                    {
                        // all good
                        this._requiresInstallOrUpgrade = false;		                
                    }
                }                                                                                                
            }
            else
            {
                // need to install
                this._requiresInstallOrUpgrade = true;
            }            
        }        
        catch(e)
        {
            this._vgdk = null;            
            this._version="?";
            this._requiresInstallOrUpgrade = true;	            
        }
    }                           				    
}



// ************************* END OF Public Methods *************************

// ************************* START OF Public Properties *************************

function getRequiresInstallOrUpgradeMethod() {
    return this._requiresInstallOrUpgrade; 
}

function getIsInitializedMethod() {
    return this._isInitialized;
}


function getVersionMethod() {
    return this._version;
}
// ************************* END OF Public Properties *************************

// ************************* START OF Private methods *************************

function createVgdkMethod() {	    
    this._vgdk = actualVgdk;
    return;	
}




function loginMethod(user,pswd) {
		    	
    if (!this._isInitialized)
    {
        throw("You must perform initialization before executing the method");
    }        
	
	if (this._vgdk == null)
	{	    
	    return false;
	}
	
    try 
    {        
        TvinciPlayerAddToLog("login - calling 'this._vgdk.Login' method with user '" + user + "' password '" + pswd + "'");    	
    	var res = this._vgdk.Login(user,pswd);    
    	TvinciPlayerAddToLog("login - calling 'this._vgdk.Login' method returned with result '" + res + "'");    	
    	return (res == 0);
    } catch(e) {
        TvinciPlayerAddToLog("login - calling 'this._vgdk.Login' throw exception");    	
        return false;
    }   			
}

function logoutMethod() {	
		
	if (!this._isInitialized)
    {
        throw("logout - You must perform initialization before executing the method");
    } 	    	
    	
    if (this._vgdk == null)
	{	    
	    return false;
	}
	
    try {
        return (this._vgdk.Logout() == 0);
    } catch(e) {
        TvinciPlayerAddToLog("logout - calling 'this._vgdk.Login' throw exception");    	
		return false;
    }
}
	
// ************************* END OF Private Methods *************************