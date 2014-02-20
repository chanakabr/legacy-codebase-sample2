var ano;
var Mmes;
var mes;
var dia;
var textBoxName;
var oCalendarDiv = "";

function diasDelMes(ano,Nmes) {
       if ((Nmes==1)||(Nmes==3)||(Nmes==5)||(Nmes==7)||(Nmes==8)||(Nmes==10)||(Nmes==12)) dias=31
	else if ((Nmes==4)||(Nmes==6)||(Nmes==9)||(Nmes==11)) dias=30
	else if ((((ano % 100)==0) && ((ano % 400)==0)) || (((ano % 100)!=0) && ((ano % 4)==0))) dias = 29
	else dias = 28;
	return dias;
};

function crearSelectorMes(mesActual) {
	var selectorMes = "";
	selectorMes = "<input class=calendarFormInput readonly=true type=text size=11 name='mes' value=" + MonthNumber[mesActual] + ">";
	return selectorMes;
}

function crearSelectorAno(anoActual) {
	var selectorAno = "";
	selectorAno = "<input class=calendarFormInput readonly=true type=text size=5 name='ano' value='" + anoActual + "'>";
	return selectorAno;
}

function crearTablaDias(numeroAno,numeroMes) {
	//var sLanguage = top.gCurrentUser.sLanguage;
	var fechaInicio = new Date();
	fechaInicio.setYear(numeroAno);
	fechaInicio.setMonth(numeroMes-1);
	fechaInicio.setDate(1);
	ajuste = fechaInicio.getDay();

	var tabla = "<table dir='rtl' border='0' cellpadding='6' cellspacing='0'><tr class='calendarTableFooter'>";
		
	//if(sLanguage == "en")
		tabla += "<td align='center'>" + Su + "</td><td align='center'>" + Mo + "</td>";
		tabla += "<td align='center'>" + Tu + "</td><td align='center'>" + We + "</td>";
		tabla += "<td align='center'>" + Th + "</td><td align='center'>" + Fr + "</td><td align='center'>" + Sa + "</td></tr>";
	//else if(sLanguage == "he")
	//	tabla += "<td align='center'>Su</td><td align='center'>Mo</td><td align='center'>Tu</td><td align='center'>We</td><td align='center'>Th</td><td align='center'>Fr</td><td align='center'>Sa</td></tr>";
	
	for (var j=1; j<=ajuste; j++) {
		tabla = tabla + "<td>&nbsp;</td>";
	}
	for (var i=1; i<10; i++) {
		tabla = tabla + "  <td align=center class=calendar_table_cell "
		if ((i == diaHoy()) && (numeroMes == mesHoy()) && (numeroAno == anoHoy())) tabla = tabla + " bgcolor='#E3E7FB'";
		tabla = tabla + "><a class='calendar_smallheader' href='javascript: mes="+Nmes+" ;ano="+ano+"; dia=" + i + "; WriteToTextBox();'>0" + i + "</a></td>";
		if (((i+ajuste) % 7)==0) tabla = tabla + "</tr><tr>";
	}
	for (var i=10; i<=diasDelMes(numeroAno,numeroMes); i++) {
		tabla = tabla + "  <td align=center  class=calendar_table_cell "
		if ((i == diaHoy()) && (numeroMes == mesHoy()) && (numeroAno == anoHoy())) tabla = tabla + " class=calendarTableFooter";
		tabla = tabla + "><a class='calendar_smallheader' href='javascript: mes="+Nmes+" ;ano="+ano+"; dia=" + i + ";WriteToTextBox(); '>"
		if ((i == diaHoy()) && (numeroMes == mesHoy()) && (numeroAno == anoHoy())) tabla = tabla + " <font class=calendarTableFooter>"
		else tabla = tabla + "";
		tabla = tabla + "" + i + "</a></td>";
		if (((i+ajuste) % 7)==0) tabla = tabla + "</tr><tr>";
	}
	tabla = tabla + "</tr></table>";
	return tabla;
}

function dibujarMes(numeroAno,numeroMes) {
	//var oLangDictionaryHash = top.gCurrentUser.LanguageDictionaryHash;

	var html = "";
	html += "<table cellspacing='1' cellpadding='0' border='1' class='calendarBorder'>";
	html += "<tr><td>";
	html += "<table class='CalendarHeader' style='height:5px'><tr>";
	html += "<td class=calendar_table_cell align='right' onclick='closeCalendar()'>X</td>";
	html += "<td nowrap width='100%'></td>";
	html += "<td nowrap class=calendar_table_cell><B>";
	html += peek_date_string;
	html += "</b></td>";
	html += "</tr></table>";
	html += "</td></tr>";

	html += "<tr dir='rtl'><td class=TableInner>";
	html += "<table dir='rtl' cellspacing=4 cellpadding=0 border=0 width=100%><tr>";

	html += "<td><a href='javascript:PreMonth()' title='Previous month' ><img border=0 src='images/button_next_normal.gif' height=9 width=11 name=image11></a></td>"
	html += "<td>";
	html += crearSelectorMes(numeroMes);
	html += "</td>"

	html += "<td><a href='javascript:NextMonth()' title='Next month' ><img border=0 src='images/button_prev_normal.gif' height=9 width=11 height=15 width=10 name=image12></a></td>"

	html += "<td nowrap width=100%>&nbsp;&nbsp;</td>"
	html += "<td><a href='javascript:PreYear()' title='Previous year' alt='הבא'><img border=0 src='images/button_next_normal.gif' height=9 width=11 name=image13></a></td><td>"
	html += crearSelectorAno(numeroAno);
	html += "</td><td><a href='javascript:NextYear()' title='Next year' alt='קודם'><img border=0 src='images/button_prev_normal.gif' height=9 width=11 name=image14></a></td></tr></form></table>"
	html += crearTablaDias(numeroAno,numeroMes);
	
	document.getElementById("calendarDiv").innerHTML = html;
}


//////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////

function PreMonth()	{
	Nmes -= 1;

	if ( Nmes == 0 ){
		Nmes = 12;
		ano -= 1;
	}
	dibujarMes(ano,Nmes);
} 

function NextMonth(){
	Nmes += 1;
	
	if ( Nmes == 13 ){
		Nmes = 1;
		ano += 1;
	}
	dibujarMes(ano,Nmes);
}

function PreYear()	{
	ano -= 1;
	dibujarMes(ano,Nmes);
}

function NextYear()	{
	ano += 1;
	dibujarMes(ano,Nmes);
}
//////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////


function anoHoy() {
	var fecha = new Date();
	if (navigator.appName == "Netscape") return fecha.getYear() + 1900;
	else return fecha.getYear();
}

function mesHoy() {
	var fecha = new Date();
	return fecha.getMonth()+1;
}

function diaHoy() {
	var fecha = new Date();
	return fecha.getDate();
}

function Initialization(formItem) {
	var coordinates = getAnchorPosition(formItem.id);
	oCalendarDiv = document.getElementById("calendarDiv");
	oCalendarDiv.style.display = "block";
	oCalendarDiv.style.left = coordinates.x - 160;
	oCalendarDiv.style.top = coordinates.y + 10;
	textBoxName = formItem;
	ano = anoHoy();
	Nmes = mesHoy();
	dia = diaHoy();
	dibujarMes(ano,Nmes);
	Drag.init(oCalendarDiv);
}

function WriteToTextBox() {
	if(Nmes < 10) { Nmes = "0" + Nmes; } 
	
	if(dia< 10) { dia = "0" + dia; } 
	textBoxName.value = dia + "/" + Nmes + "/" + ano;
	closeCalendar();
	s = dia + "/" + Nmes + "/" + ano;
	AfterDateSelect('' , 1 , s);
}

function closeCalendar(){
	if(typeof(oCalendarDiv) == "object")
	{
		oCalendarDiv.innerHTML = "";
		oCalendarDiv.style.display = "none";
	}
}

//document.onclick = closeCalendar;

