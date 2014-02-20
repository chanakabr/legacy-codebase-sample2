var ErrNDSCode=0;	
var ErrType=1;
var ErrCode=2;
var ErrDescription=3;
var ErrPlayerBufferAction=4;
var ErrPlayerAction=5;
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
    ['1, 0, 0, 8, 38','MESSAGE','TSP_CANT_ACQUIRE_LICENSE','אנו מתנצלים ,אך לא ניתן לבצע את הפעולה כעת, אנא נסה מאוחר יותר','message','none']

]

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