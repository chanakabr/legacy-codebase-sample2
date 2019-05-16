

//// MAKE TEXT DARK ////

function inputFocus(whichId,insertText)
{
    if (document.getElementById(whichId))
    {
        var d = document.getElementById(whichId);

        if (d.value==insertText)
        {
            d.style.color='#333333';
            d.value='';
        }
    }
}

//// RESET TEXT - MAKE IT LIGHT ////

function inputBlur(whichId,insertText,color)
{
    if (document.getElementById(whichId))
    {
        var d = document.getElementById(whichId);

        if (d.value=='')
        {
            if (!color)
            {
                d.style.color='#848484';
            }
            d.value=insertText;
        }
    }
}

