/*  Rich Text Editor
 *  July 2007, by Assaf Rafaeli
 */

function richTextEditor(id, backgroundColorClass, maxWidth, initValue) {
	this.imgWin = '';
	this.linkSel = false;
	this.blurSel = false;
	this.isHTML = false;
	this.selected = false;
	this.maxWidth = maxWidth;
	this.backgroundColorClass = backgroundColorClass;
	this.initValue = initValue;

	this.buttons = {};
	this.paste = {
		dontCheck: false,
		lastNodes: []
	}
	this.clicked = {
		colors: false,
		size: false
	}
	this.pressed = {
		'bold': false,
		'italic': false,
		'underline': false,
		'color': false,
		'size': false,
		'justifyleft': false,
		'justifycenter': false,
		'justifyright': false,
		'justifyfull': false,
		'ol': false,
		'ul': false
	}

	this.browser = {
		firefox: (function() {try { return InstallTrigger } catch(e) { return false }})(),
		safari: (window.getSelection && window.getSelection().setBaseAndExtent),
		ie: (!this.firefox && !this.safari && !window.opera && window.ActiveXObject)
	}

	this.buildDOM(id);
	this.window = this.iframe.contentWindow;

	this.press('justifyright', true);

	this.events.add(this.iframe, "load", this.init.are_bind(this)); // continue after the iframe will be loaded
	this.iframe.src = "edit.htm"; // now that we have an onload event, we can trigger it

	this.events.add(this.preview, "load", function() {
		this.previewDoc = this.preview.contentWindow.document;
	}.are_bind(this));
	this.preview.src = "edit.htm";
}
richTextEditor.prototype.init = function() {
	if (this.browser.ie) {
		if (!this.document) {
			this.document = this.xbDesignmode(this.iframe);
			return false;
		}
	}
	else
		this.document = this.xbDesignmode(this.iframe);

	if (this.browser.ie)
		this.document.body.style.marginTop = "8px";
	this.document.body.style.direction = "rtl";
	this.document.body.style.font = "11pt arial, verdana, sans-serif";

	/* disable image drag-n-drop feature
	 *
	 * When the user drags and image from a diffrent document,
	 * the drag-n-drop handle is disabled because the docuemnt
	 * is in 'readonly' mode.
	 * note that when 'readonly' is false, you cannot edit the
	 * document.
	 */
	if (!window.opera) { // Opera doesn't have drag n drop
		this.richEdit('readonly', false);
		if (this.browser.ie || this.browser.safari)
			var edit = this.window;
		else
			var edit = this.document;

		this.events.add(edit, "focus", function() {this.richEdit('readonly', true)}.are_bind(this));
		this.events.add(edit, "blur", function() {this.richEdit('readonly', false)}.are_bind(this));
	}

	var areasOfColors = this.map.getElementsByTagName("area");
	var sizes = this.size.getElementsByTagName("a");

	if (!this.browser.ie) {
		if (this.browser.safari)
			this.document.body.onpaste = this.removeFormatOnPaste.are_bind(this)

		if (this.browser.firefox) {
			/* let paste check happen the user pressed ctrl + v or shift + insert */
			this.document.addEventListener("keypress", function(e) {
				if ((e.ctrlKey && e.charCode == 118) || (e.shiftKey && e.keyCode == e.DOM_VK_INSERT))
					this.paste.dontCheck = false;
				else
					this.paste.dontCheck = true;
			}.are_bind(this), false);

			/* let paste check happen the user opened the context menu */
			this.document.addEventListener("contextmenu", function(e) {
				this.paste.dontCheck = false;
			}.are_bind(this), false);
			this.document.addEventListener("DOMNodeInserted", function(e) {
				var el = e.originalTarget;
				if (el.nodeName != "BR") {
					if (el.nodeType != Node.TEXT_NODE) {
						if (!this.paste.dontCheck) {
							if (this.paste.lastNodes.pop() != el)
								this.removeFormatOnPaste(el)
						}
					}
				}
			}.are_bind(this), false);

			/* for delete, undo, and redo from the context menu */
			this.document.addEventListener("DOMNodeRemoved", function(e) {
				var el = e.originalTarget;
				if (el.nodeName != "BR" && el.nodeType != Node.TEXT_NODE) {
					if (!this.paste.dontCheck)
						this.paste.lastNodes.push(el)
				}
			}.are_bind(this), false);

			/* Attempt at removing the Adblock tag off <embed>s */
			this.document.addEventListener("DOMNodeInserted", function(e) {
				var el = e.originalTarget;
				if (el.nodeName == "DIV" && el.getAttribute('adblockframe') == 'true')
					el.parentNode.removeChild(el);
			}, false);

			/* firefox bug: body.innerHTML == "<br>\n", can't align text before writing something */
			this.document.body.innerHTML = "<br>";
		}

		this.events.add(this.document, "keypress", this.delLinkOnBackspace.are_bind(this));
	}
	else { // IE
		this.document.onkeydown = function() {
			setTimeout(this.removeEmptyP, 1);
		}.are_bind(this);
		this.events.add(this.document, "keydown", this.ctrlShiftAlignIE.are_bind(this));
		this.document.body.onpaste = this.removeFormatOnPaste.are_bind(this);
		this.document.body.innerHTML = "<div></div>"
	}

	this.events.add(this.buttons.bold, "click", this.bold.are_bind(this));
	this.events.add(this.buttons.italic, "click", this.italic.are_bind(this));
	this.events.add(this.buttons.underline, "click", this.underline.are_bind(this));

	this.events.add(this.buttons.fontColor, "click", this.openColors.are_bind(this));

		this.events.add(areasOfColors[0],  "click", function() {this.setColor('#ffffff')}.are_bind(this));
		this.events.add(areasOfColors[1],  "click", function() {this.setColor('#989898')}.are_bind(this));
		this.events.add(areasOfColors[2],  "click", function() {this.setColor('#000000')}.are_bind(this));
		this.events.add(areasOfColors[3],  "click", function() {this.setColor('#FF0000')}.are_bind(this));
		this.events.add(areasOfColors[4],  "click", function() {this.setColor('#980000')}.are_bind(this));
		this.events.add(areasOfColors[5],  "click", function() {this.setColor('#FF0065')}.are_bind(this));
		this.events.add(areasOfColors[6],  "click", function() {this.setColor('#FFFF98')}.are_bind(this));
		this.events.add(areasOfColors[7],  "click", function() {this.setColor('#FFFF00')}.are_bind(this));
		this.events.add(areasOfColors[8],  "click", function() {this.setColor('#FF9800')}.are_bind(this));
		this.events.add(areasOfColors[9],  "click", function() {this.setColor('#98FF98')}.are_bind(this));
		this.events.add(areasOfColors[10], "click", function() {this.setColor('#32FF32')}.are_bind(this));
		this.events.add(areasOfColors[11], "click", function() {this.setColor('#009800')}.are_bind(this));
		this.events.add(areasOfColors[12], "click", function() {this.setColor('#00CCFF')}.are_bind(this));
		this.events.add(areasOfColors[13], "click", function() {this.setColor('#0000FF')}.are_bind(this));
		this.events.add(areasOfColors[14], "click", function() {this.setColor('#000098')}.are_bind(this));

	this.events.add(this.buttons.fontSize, "click", this.openSize.are_bind(this));

		this.events.add(sizes[0], "click", function() {this.setSize(2)}.are_bind(this));
		this.events.add(sizes[1], "click", function() {this.setSize(3)}.are_bind(this));
		this.events.add(sizes[2], "click", function() {this.setSize(4)}.are_bind(this));
		this.events.add(sizes[3], "click", function() {this.setSize(5)}.are_bind(this));

	this.events.add(document, "click", this.closeOptionBoxes.are_bind(this));
	this.events.add(this.document, "click", this.closeOptionBoxes.are_bind(this));

	this.events.add(this.buttons.justifyright,  "click", function() {this.align('justifyright')}.are_bind(this));
	this.events.add(this.buttons.justifycenter, "click", function() {this.align('justifycenter')}.are_bind(this));
	this.events.add(this.buttons.justifyleft,   "click", function() {this.align('justifyleft')}.are_bind(this));
	this.events.add(this.buttons.justifyfull,   "click", function() {this.align('justifyfull')}.are_bind(this));

	if (this.browser.ie)
		this.events.add(this.window, "blur", function() {this.blurSel = this.document.selection.createRange().duplicate();}.are_bind(this));
	this.events.add(this.buttons.ol, "click", function() {this.list('OL')}.are_bind(this));
	this.events.add(this.buttons.ul, "click", function() {this.list('UL')}.are_bind(this));

	this.events.add(this.buttons.link, "click", this.toggleLinkWin.are_bind(this));
	this.events.add(this.buttons.closeLinkWin, "click", this.toggleLinkWin.are_bind(this));
	this.events.add(this.buttons.addLink, "click", this.addLink.are_bind(this));
	this.events.add(this.linkInput, "keydown", function(ev) {
		if (ev.keyCode == 13) {
			this.addLink();
			if (ev.preventDefault) {
				ev.stopPropagation();
				ev.preventDefault();
			}
			else {
				ev.returnValue = false;
				ev.cancelBubble = true;
			}
		}
	}.are_bind(this));

	this.events.add(this.buttons.image, "click", this.openImgWin.are_bind(this));

	this.events.add(this.buttons.html, "click", this.toggleHTML.are_bind(this));

	this.events.add(this.buttons.preview, "click", this.togglePreview.are_bind(this));

	this.events.add(this.document, "mouseup", this.pressStyles.are_bind(this));
	this.events.add(this.document, "keyup", this.pressStyles.are_bind(this));

	//this.events.add(window, "resize", this.linkHeights.are_bind(this));

	if (this.browser.ie)
		this.document.onkeypress = this.shortcutKeys.are_bind(this);
	else
		this.events.add(this.document, "keypress", this.shortcutKeys.are_bind(this));

	this.events.add(this.document, "keypress", this.restoreStylesOnSelectAll.are_bind(this));

	if (this.initValue) {
		this.paste.dontCheck = true;
		this.setContent("<div>" + this.initValue + "</div>");
		this.paste.dontCheck = false;
	}
}
richTextEditor.prototype.end = function() {
	this.events.cache.flush();
	this.document.body.onpaste = null;
	this.document.onkeydown = null;
	this.document.onkeypress = null;
}
richTextEditor.prototype.buildDOM = function(id) {
	var editorWrapper = document.createElement("div");
	editorWrapper.className = "editorWrapper";

		this.editor = document.createElement("div");
		this.editor.className = "editor";

			var icons = document.createElement("div");
			icons.className = "icons";

				this.colorsImg = document.createElement("img");
				this.colorsImg.src = "images/t.gif";
				this.colorsImg.className = "colors";
				this.colorsImg.setAttribute('useMap', '#colors');
				this.colorsImg.style.display = "none";
				icons.appendChild(this.colorsImg);

				this.map = null;
				// IE; this fails on standards-compliant browsers
				try {
					this.map = document.createElement('<map name="colors">');
				} catch (e) {
				}
				if (!this.map || this.map.nodeName != "MAP") {
					// Non-IE browser; use canonical method to create named element
					this.map = document.createElement("map");
					this.map.name = "colors";
				}

				this.map.innerHTML = '\
					<area shape="rect" coords="3,3,23,23" href="javascript:void(0)" title="לבן" alt="לבן" />\
					<area shape="rect" coords="24,3,44,23" href="javascript:void(0)" title="אפור" alt="אפור" />\
					<area shape="rect" coords="45,3,65,23" href="javascript:void(0)" title="שחור" alt="שחור" />\
					<area shape="rect" coords="3,24,23,44" href="javascript:void(0)" title="אדום" alt="אדום" />\
					<area shape="rect" coords="24,24,44,44" href="javascript:void(0)" title="חום" alt="חום" />\
					<area shape="rect" coords="45,24,65,44" href="javascript:void(0)" title="ורוד" alt="ורוד" />\
					<area shape="rect" coords="3,45,23,65" href="javascript:void(0)" title="צהוב בהיר" alt="צהוב בהיר" />\
					<area shape="rect" coords="24,45,44,65" href="javascript:void(0)" title="צהוב" alt="צהוב" />\
					<area shape="rect" coords="45,45,65,65" href="javascript:void(0)" title="כתום" alt="כתום" />\
					<area shape="rect" coords="3,66,23,86" href="javascript:void(0)" title="ירוק בהיר" alt="ירוק בהיר" />\
					<area shape="rect" coords="24,66,44,86" href="javascript:void(0)" title="ירוק" alt="ירוק" />\
					<area shape="rect" coords="45,66,65,86" href="javascript:void(0)" title="ירוק כהה" alt="ירוק כהה" />\
					<area shape="rect" coords="3,87,23,107" href="javascript:void(0)" title="כחול בהיר" alt="כחול בהיר" />\
					<area shape="rect" coords="24,87,44,107" href="javascript:void(0)" title="כחול" alt="כחול" />\
					<area shape="rect" coords="45,87,65,107" href="javascript:void(0)" title="כחול כהה" alt="כחול כהה" />';
				icons.appendChild(this.map);

				this.size = document.createElement("div");
				this.size.className = "size";
				this.size.style.display = "none";
				this.size.innerHTML = '<a href="javascript:void(0)" class="s1">קטן</a> <a href="javascript:void(0)" class="s2">בינוני</a> <a href="javascript:void(0)" class="s3">גדול</a> <a href="javascript:void(0)" class="s4">ענק</a>';
				icons.appendChild(this.size);

				this.ul = document.createElement("ul");

					var spacer = document.createElement("li");
					spacer.innerHTML = '<img src="images/editor/iconsSpacer.png" alt="" />';

					this.buttons.bold = this.createButton("li", "icon01", "מודגש", "מודגש [shift + b]");
					this.ul.appendChild(this.buttons.bold);

					this.buttons.italic = this.createButton("li", "icon02", "נטוי", "נטוי [shift + i]");
					this.ul.appendChild(this.buttons.italic);

					this.buttons.underline = this.createButton("li", "icon03", "קו תחתון", "קו תחתון [shift + u]");
					this.ul.appendChild(this.buttons.underline);

					this.ul.appendChild(spacer);

					this.buttons.fontColor = this.createButton("li", "icon04", "צבע טקסט", "צבע טקסט");
					this.ul.appendChild(this.buttons.fontColor);

					this.ul.appendChild(spacer.cloneNode(true));

					this.buttons.fontSize = this.createButton("li", "icon05", "גודל גופן", "גודל גופן");
					this.ul.appendChild(this.buttons.fontSize);

					this.ul.appendChild(spacer.cloneNode(true));

					this.buttons.justifyright = this.createButton("li", "icon06", "יישור לימין", "יישור לימין [shift + r]");
					this.ul.appendChild(this.buttons.justifyright);

					this.buttons.justifycenter = this.createButton("li", "icon07", "מרכוז", "מרכוז [shift + c]");
					this.ul.appendChild(this.buttons.justifycenter);

					this.buttons.justifyleft = this.createButton("li", "icon08", "יישור לשמאל", "יישור לשמאל [shift + f]");
					this.ul.appendChild(this.buttons.justifyleft);

					this.buttons.justifyfull = this.createButton("li", "icon09", "יישור לשוליים", "יישור לשוליים [shift + h]");
					this.ul.appendChild(this.buttons.justifyfull);

					this.ul.appendChild(spacer.cloneNode(true));

					this.buttons.ol = this.createButton("li", "icon10", "הוספת/הסרת רשימה ממוספרת", "הוספת/הסרת רשימה ממוספרת");
					this.ul.appendChild(this.buttons.ol);

					this.buttons.ul = this.createButton("li", "icon11", "הוספת/הסרת רשימת נקודות", "הוספת/הסרת רשימת נקודות");
					this.ul.appendChild(this.buttons.ul);

					this.ul.appendChild(spacer.cloneNode(true));

					this.buttons.link = this.createButton("li", "icon17", "הוספת/עריכת קישור", "הוספת/עריכת קישור [shift + a]");
					this.ul.appendChild(this.buttons.link);

					this.buttons.image = this.createButton("li", "icon18", "הוספת תמונה", "הוספת תמונה [shift + m]");
					this.ul.appendChild(this.buttons.image);

				icons.appendChild(this.ul);

				this.buttons.preview = document.createElement("a");
				this.buttons.preview.href = "javascript:void(0)";
				this.buttons.preview.className = "preview";
				this.buttons.preview.innerHTML = "תצוגה מקדימה";
				icons.appendChild(this.buttons.preview);

				var spacer = document.createElement("img");
				spacer.src = "images/editor/iconsSpacer.png";
				spacer.className = 'spacer';
				icons.appendChild(spacer);

				this.buttons.html = document.createElement("a");
				this.buttons.html.href = "javascript:void(0)";
				this.buttons.html.className = "html";
				this.buttons.html.innerHTML = "HTML";
				icons.appendChild(this.buttons.html);

			this.editor.appendChild(icons);

			this.iframe = document.createElement("iframe");
			this.iframe.frameBorder = 'no';
			this.iframe.scrolling = 'yes';
			this.iframe.className = "edit";
			this.editor.appendChild(this.iframe);

			this.htmlWrap = document.createElement("div");
			this.htmlWrap.className = "htmlWrap";
			this.htmlWrap.style.display = "none";

				this.textarea = document.createElement("textarea");
				this.textarea.className = "html";
				this.textarea.setAttribute('dir', 'ltr');
				this.htmlWrap.appendChild(this.textarea);

			this.editor.appendChild(this.htmlWrap);

		editorWrapper.appendChild(this.editor);

		this.previewWrap = document.createElement("div");
		this.previewWrap.className = "previewWrap";
		this.previewWrap.style.display = "none";

			this.preview = document.createElement("iframe");
			this.preview.frameBorder = 'no';
			this.preview.className = "previewIframe";
			this.previewWrap.appendChild(this.preview);

		editorWrapper.appendChild(this.previewWrap);

		this.linkWrapper = document.createElement("div");
		this.linkWrapper.className = "are_linkWrapper";
		this.linkWrapper.style.display = "none";

			var euWrapper = document.createElement("div");
			euWrapper.className = "euWrapper";

				var inner = document.createElement("div");
				inner.className = "inner_linkWrapper";

					this.buttons.closeLinkWin = document.createElement("a");
					this.buttons.closeLinkWin.href = "javascript:void(0)";
					this.buttons.closeLinkWin.innerHTML = "סגור";
					this.buttons.closeLinkWin.className = "close";
					inner.appendChild(this.buttons.closeLinkWin);

					var h2 = document.createElement("h2");
					h2.innerHTML = "הוסף קישור";
					inner.appendChild(h2);

					var label = document.createElement("label");
					label.innerHTML = "קישור:";
					inner.appendChild(label);

					this.buttons.addLink = document.createElement("a");
					this.buttons.addLink.href = "javascript:void(0)";
					this.buttons.addLink.className = "upload";
					this.buttons.addLink.innerHTML = "אישור";
					inner.appendChild(this.buttons.addLink);

					this.linkInput = document.createElement("input");
					this.linkInput.type = "text";
					this.linkInput.setAttribute('dir', 'ltr');
					this.linkInput.value = "http:\/\/";
					this.linkInput.name = "url";
					inner.appendChild(this.linkInput);

				euWrapper.appendChild(inner);

			this.linkWrapper.appendChild(euWrapper);

		document.body.appendChild(this.linkWrapper);

	document.getElementById(id).appendChild(editorWrapper);
}
richTextEditor.prototype.xbDesignmode = function(el) {
	var mEditorDocument = null;

	if (el.contentDocument) {
		// Gecko
		mEditorDocument = el.contentDocument;
		mEditorDocument.designMode = "On";
	}
	else {
		// IE
		mEditorDocument = el.contentWindow.document;
		mEditorDocument.designMode = "On";   
		// IE needs to reget the document element after designMode was set
		mEditorDocument = el.contentWindow.document;
	}
	return mEditorDocument;
}
richTextEditor.prototype.events = {
	add: function(obj, type, fn) {
		if (obj.addEventListener) {
			obj.addEventListener(type, fn, false);
			this.cache.add(obj, type, fn);
		}
		else if (obj.attachEvent) {
			var typefn = type + fn;
			obj["e"+typefn] = fn;
			obj[typefn] = function() { obj["e"+typefn](window.event); }
			obj.attachEvent("on"+type, obj[typefn]);
			this.cache.add(obj, type, fn);
		}
		else
			obj["on"+type] = obj["e"+type+fn];
	},
	cache: {
		listEvents: [],
		add: function(node, sEventName, fHandler) {
			this.listEvents.push(arguments);
		},
		flush: function(node, sEventName, fHandler) {
			var i, item, listEvents = this.listEvents;
			if (node && sEventName && fHandler) {
				for (i = listEvents.length - 1; item = listEvents[i]; i--) {
					if (node == item[0] && sEventName == item[1] && fHandler == item[2]) {
						if(item[0].removeEventListener)
							item[0].removeEventListener(item[1], item[2], item[3]);
						if(item[1].substring(0, 2) != "on")
							item[1] = "on" + item[1];
						if(item[0].detachEvent)
							item[0].detachEvent(item[1], item[2]);
						item[0][item[1]] = null;
					}
				}
			}
			else {
				for (i = listEvents.length - 1; item = listEvents[i]; i--) {
					if(item[0].removeEventListener)
						item[0].removeEventListener(item[1], item[2], item[3]);
					if(item[1].substring(0, 2) != "on")
						item[1] = "on" + item[1];
					if(item[0].detachEvent)
						item[0].detachEvent(item[1], item[2]);
					item[0][item[1]] = null;
				}
			}
		}
	}
}
Function.prototype.are_bind = function(object) {
	var args = (function(iterable) {
		if (iterable.toArray) {
			return iterable.toArray();
		} else {
			var results = [];
			for (var i = 0, length = iterable.length; i < length; i++)
				results.push(iterable[i]);
			return results;
		}
	})(arguments);
	var __method = this, object = args.shift();
	return function(event) {
		return __method.apply(object, [event || object.window.event].concat(args));
	}
}
richTextEditor.prototype.getContent = function() {
	this.checkUnallowed();
	this.sizeSwitch("add");
	return this.document.body.innerHTML;
}
richTextEditor.prototype.setContent = function(html) {
	this.document.body.innerHTML = html;
	this.checkUnallowed();
	this.sizeSwitch("remove");
}
richTextEditor.prototype.checkUnallowed = function() {
	var children = this.document.getElementsByTagName("*");
	for (var i = 0, item; item = children[i]; i++) {
		var align = item.getAttribute('align');
		if (align != null && align != "") {
			item.setAttribute('align', '')
			item.style.textAlign = align;
		}
		/* check for javascript code in css props and disable them by removing "javascript:"
		 * more information about this hack over at: http://www.quirksmode.org/css/javascript.html
		 */
		if (item.style.background != '' && item.style.background.search(/rl\(\'?javascript:/))
			item.style.background = item.style.background.replace(/javascript:/, '');
		if (item.style.backgroundImage != '' && item.style.backgroundImage.search(/rl\(\'?javascript:/))
			item.style.backgroundImage = item.style.backgroundImage.replace(/javascript:/, '');
		if (item.style.listStyle != '' && item.style.listStyle.search(/rl\(\'?javascript:/))
			item.style.listStyle = item.style.listStyle.replace(/javascript:/, '');
		if (item.style.listStyleImage != '' && item.style.listStyleImage.search(/rl\(\'?javascript:/))
			item.style.listStyleImage = item.style.listStyleImage.replace(/javascript:/, '');

		/* remove javascript from all the events */
		if (item.getAttribute('onclick') != null){item.setAttribute('onclick', '')}
		if (item.getAttribute('onabort') != null){item.setAttribute('onabort', '')}
		if (item.getAttribute('onblur') != null){item.setAttribute('onblur', '')}
		if (item.getAttribute('onchange') != null){item.setAttribute('onchange', ' ')}
		if (item.getAttribute('ondblclick') != null){item.setAttribute('ondblclick', '')}
		if (item.getAttribute('onerror') != null){item.setAttribute('onerror', '')}
		if (item.getAttribute('onfocus') != null){item.setAttribute('onfocus', '')}
		if (item.getAttribute('onkeydown') != null){item.setAttribute('onkeydown', '')}
		if (item.getAttribute('onkeypress') != null){item.setAttribute('onkeypress', '')}
		if (item.getAttribute('onkeyup') != null){item.setAttribute('onkeyup', '')}
		if (item.getAttribute('onload') != null){item.setAttribute('onload', '')}
		if (item.getAttribute('onmousedown') != null){item.setAttribute('onmousedown', '')}
		if (item.getAttribute('onmousemove') != null){item.setAttribute('onmousemove', '')}
		if (item.getAttribute('onmouseout') != null){item.setAttribute('onmouseout', '')}
		if (item.getAttribute('onmouseover') != null){item.setAttribute('onmouseover', '')}
		if (item.getAttribute('onmouseup') != null){item.setAttribute('onmouseup', '')}
		if (item.getAttribute('onreset') != null){item.setAttribute('onreset', '')}
		if (item.getAttribute('onresize') != null){item.setAttribute('onresize', '')}
		if (item.getAttribute('onselect') != null){item.setAttribute('onselect', '')}
		if (item.getAttribute('onsubmit') != null){item.setAttribute('onsubmit', '')}
		if (item.getAttribute('onunload') != null){item.setAttribute('onunload', '')}
		if (item.getAttribute('oncontextmenu') != null){item.setAttribute('oncontextmenu', '')}
		if (item.getAttribute('oninput') != null){item.setAttribute('oninput', '')}
		if (item.getAttribute('onoverflow') != null){item.setAttribute('onoverflow', '')}
		if (item.getAttribute('onoverflowchanged') != null){item.setAttribute('onoverflowchanged', '')}
		if (item.getAttribute('onunderflow') != null){item.setAttribute('onunderflow', '')}
		if (item.getAttribute('onselect') != null){item.setAttribute('onselect', '')}

		if (item.nodeName == "IMG") {
			if (this.maxWidth) {
				if (item.width > this.maxWidth)
					item.width = this.maxWidth;
			}
		}
		else if (item.nodeName == "A") {
			if (item.getAttribute('target') != '_blank')
				item.setAttribute('target', '_blank');
		}
		else if (item.nodeName == "EMBED") {
			if (item.getAttribute('wmode') != 'transparent')
				item.setAttribute('wmode', 'transparent');
		}
		else if (item.nodeName == "OL") {
			if (item.style.listStyleType == '')
				item.style.listStyleType = 'decimal';
			if (item.style.listStylePosition != 'inside')
				item.style.listStylePosition = 'inside';
		}
		else if (item.nodeName == "UL") {
			if (item.style.listStylePosition != 'inside')
				item.style.listStylePosition = 'inside';
			if (item.style.listStyleType == '')
				item.style.listStyleType = 'disc';
		}
		else if (item.nodeName == "SCRIPT") /* remove <script> tags */
			item.parentNode.removeChild(item);
	}
	if (this.document.body.innerHTML == "")
		this.document.body.innerHTML = "<div></div>";
}
richTextEditor.prototype.bold = function() {
	this.paste.dontCheck = true;
	this.richEdit('bold');
	this.togglePressed('bold');
}
richTextEditor.prototype.italic = function() {
	this.paste.dontCheck = true;
	this.richEdit('italic');
	this.togglePressed('italic');
}
richTextEditor.prototype.underline = function() {
	this.paste.dontCheck = true;
	this.richEdit('underline');
	this.togglePressed('underline');
}
richTextEditor.prototype.openColors = function() {
	this.paste.dontCheck = true;
	this.clicked.colors = 1;
	if (this.colorsImg.style.display != "none")
		this.colorsImg.style.display = 'none';
	else
		this.colorsImg.style.display = '';
}
richTextEditor.prototype.openSize = function() {
	this.paste.dontCheck = true;
	this.clicked.size = 1;
	if (this.size.style.display != "none")
		this.size.style.display = 'none';
	else
		this.size.style.display = '';
}
richTextEditor.prototype.setColor = function(color) {
	if (!window.opera)
		this.window.focus();
	this.richEdit('forecolor', color);
	this.pressed['color'] = color;
}
richTextEditor.prototype.setSize = function(size) {
	if (!window.opera)
		this.window.focus();
	this.richEdit('fontsize', size);
	this.pressed['size'] = size;
}
richTextEditor.prototype.closeOptionBoxes = function() {
	if (!this.clicked.colors)
		this.colorsImg.style.display = 'none';
	else
		this.clicked.colors = 0;

	if (!this.clicked.size)
		this.size.style.display = 'none';
	else
		this.clicked.size = 0;
}
richTextEditor.prototype.align = function(align) {
	this.paste.dontCheck = true;

	var isBody = false, isEmpty = false;
	if (this.browser.ie) {
		var r = this.document.selection.createRange();
		if (r.parentElement() == "BODY")
			isBody = true;
		if (this.document.body.innerHTML == "")
			isEmpty = true;
	}

	if (!window.opera)
		this.window.focus();

	if (!this.pressed[align]) {
		switch (align) {
			case 'justifyright':
				this.press('justifycenter', false);
				this.press('justifyleft', false);
				this.press('justifyfull', false);
				this.press('justifyright', true);
			break;
			case 'justifycenter':
				this.press('justifycenter', true);
				this.press('justifyleft', false);
				this.press('justifyfull', false);
				this.press('justifyright', false);
			break;
			case 'justifyleft':
				this.press('justifycenter', false);
				this.press('justifyleft', true);
				this.press('justifyfull', false);
				this.press('justifyright', false);
			break;
			case 'justifyfull':
				this.press('justifycenter', false);
				this.press('justifyleft', false);
				this.press('justifyfull', true);
				this.press('justifyright', false);
			break;
		}
		this.richEdit(align);
	}
	else if (align != 'justifyright') {
		this.richEdit('justifyright');
		this.press('justifyright', true);
		this.press(align, false);
	}
	this.checkUnallowed();
}
richTextEditor.prototype.list = function(type) {
	this.paste.dontCheck = true;

	if (this.browser.ie) {
		var r = this.blurSel;
		var el = r.parentElement();
		var name = el.nodeName;
		for (var li = false, item = el; item.nodeName != "BODY"; item = item.parentNode) {
			if (item.nodeName == "LI" || item.nodeName == "UL" || item.nodeName == "OL") {
				li = item;
				break;
			}
		}
		if (!li) {
			if (type == "UL") {
				this.press('ol', false);
				this.richEdit('insertunorderedlist');
			}
			else if (type == "OL") {
				this.press('ul', false);
				this.richEdit('insertorderedlist');
			}
		}
		else {
			// get the closest ul or ol, note that it might return el itself
			var parent = this.getParent(el, "UL");
			if (!parent)
				el = this.getParent(el, "OL");
			else
				el = parent;
			name = el.nodeName;

			if (el) {
				var isDiffList = false;

				var start = r.duplicate();
				var end = r.duplicate();

				start.collapse();
				end.collapse(false);

				start = this.getParent(start.parentElement(), "LI");
				end = this.getParent(end.parentElement(), "LI");

				var mid = [];
				var remains = [];
				var afterend = false;
				var afterstart = false;
				var children = el.getElementsByTagName("li");
				for (var i = children.length-1, item; item = children[i]; i--) {
					if (!afterend && item == end)
						afterend = true;
					if (afterend) {
						if (afterstart)
							remains.push(item.innerHTML);
						else
							mid.push(item.innerHTML);
						item.parentNode.removeChild(item);
					}
					if (!afterstart && item == start)
						afterstart = true;
				}
				if ((type == "OL" && name == "UL") || (type == "UL" && name == "OL")) {
					var itemContent;
					var midEl = this.document.createElement(type);
					while (itemContent = mid.pop()) {
						var li = this.document.createElement("li");
						li.innerHTML = itemContent;
						midEl.appendChild(li);
					}
					if (midEl.innerHTML == "")
						midEl.innerHTML = "<li></li>";
					if (type == "UL")
						this.press('ol', false);
					else if (type == "OL")
						this.press('ul', false);

					isDiffList = true;
				}
				else {
					var midEl = this.document.createElement("div");
					mid.reverse();
					midEl.innerHTML = mid.join("<br>");
				}

				var itemContent, isRemains;
				var list = this.document.createElement(name);
				while (itemContent = remains.pop()) {
					var li = this.document.createElement("li");
					li.innerHTML = itemContent;
					list.appendChild(li);
					isRemains = true;
				}

				el.parentNode.insertBefore(midEl, el);
				if (isRemains)
					midEl.parentNode.insertBefore(list, midEl);
				if (el.innerHTML == "")
					el.parentNode.removeChild(el);

				if (isDiffList) {
					var rc = this.document.selection.createRange();
					rc.moveStart("character", -1);
					rc.collapse();
					rc.select();
				}
			}
			setTimeout(this.replacePWithBr, 1);
		}
	}
	else {
		if (type == "UL") {
			this.press('ol', false);
			this.richEdit('insertunorderedlist');
		}
		else if (type == "OL") {
			this.press('ul', false);
			this.richEdit('insertorderedlist');
		}
	}
	this.togglePressed(type.toLowerCase());
}
richTextEditor.prototype.replacePWithBr = function() {
	var r = this.document.selection.createRange();
	var el = r.parentElement();

	var item = el;
	while (item.nodeName != "BODY") {
		if (item.nodeName == "P") {
			el = item;
			break;
		}
		item = item.parentNode;
	}
	if (item.nodeName != "P")
		return false;

	var html = el.innerHTML;
	el.parentNode.removeChild(el);

	r.pasteHTML(html + "<br>");
	r.moveStart("character", -1);
	r.collapse();
	r.select();
}
richTextEditor.prototype.getParent = function(item, tag) {
	while (item.nodeName != "BODY") {
		if (item.nodeName == tag)
			return item;
		item = item.parentNode;
	}
	return false;
}
richTextEditor.prototype.togglePressed = function(name) {
	if (this.pressed[name])
		this.press(name, false)
	else
		this.press(name, true)
}
richTextEditor.prototype.press = function(name, bool) {
	if (this.pressed[name] != bool) {
		this.pressed[name] = bool;
		this.buttons[name].firstChild.className = (bool ? 'pressed' : '')
	}
}
richTextEditor.prototype.restoreStylesOnSelectAll = function(ev) {
	if (this.browser.ie) {
		var r = this.document.selection.createRange();
		var bodyEdge = r.duplicate();
		bodyEdge.moveToElementText(this.document.body);
		if (r.isEqual(bodyEdge) || this.document.body.innerHTML == "<P>&nbsp;</P>") {
			this.document.body.innerHTML = "<div></div>";
			r = this.document.selection.createRange();
			r.select();
			if (this.pressed['justifyright']) {
				this.pressed['justifyright'] = false;
				this.align('justifyright');
			}
			if (this.pressed['justifycenter']) {
				this.pressed['justifycenter'] = false;
				this.align('justifycenter');
			}
			if (this.pressed['justifyleft']) {
				this.pressed['justifyleft'] = false;
				this.align('justifyleft');
			}
			if (this.pressed['justifyfull']) {
				this.pressed['justifyfull'] = false;
				this.align('justifyfull');
			}
			if (this.pressed['bold'])
				this.richEdit('bold');
			if (this.pressed['italic'])
				this.richEdit('italic');
			if (this.pressed['underline'])
				this.richEdit('underline');
		}
	}
	else if (this.browser.firefox) {
		var key = ev.keyCode;

		if (ev.ctrlKey || key == 37 || key == 38 || key == 39 || key == 40 || key == 13) {
			return true;
		}
		var r = this.window.getSelection().getRangeAt(0);
		var el = r.endContainer;
		var data = el.data;
		var selected = false;
		if (el.nodeName == "BODY") {
			if (data) {
				if (r.endOffset == data.length && r.startOffset == 0)
					selected = true;
			}
			else {
				if (r.endOffset <= 2 && r.startOffset == 0)
					selected = true;
			}
		}
		if (selected) {
			seleted = false;
			this.document.body.innerHTML = "<br>";
			if (this.pressed['justifyright']) {
				this.pressed['justifyright'] = false;
				this.align('justifyright');
			}
			if (this.pressed['justifycenter']) {
				this.pressed['justifycenter'] = false;
				this.align('justifycenter');
			}
			if (this.pressed['justifyleft']) {
				this.pressed['justifyleft'] = false;
				this.align('justifyleft');
			}
			if (this.pressed['justifyfull']) {
				this.pressed['justifyfull'] = false;
				this.align('justifyfull');
			}
			if (this.pressed['color'])
				this.setColor(this.pressed['color'])
			if (this.pressed['size'])
				this.setSize(this.pressed['size'])
			if (this.pressed['bold'])
				this.richEdit('bold');
			if (this.pressed['italic'])
				this.richEdit('italic');
			if (this.pressed['underline'])
				this.richEdit('underline');
		}
	}
}
richTextEditor.prototype.pressStyles = function(ev) {
	if (this.browser.ie) {
		var tag = ev.srcElement.tagName;
		if (tag == "IMG" || tag == "OBJECT" || tag == "EMBED")
			return true;
	}

	var key = ev.keyCode;
	if (key == undefined)
		key = 0;

	if (key != 0) {
		if (!(key == 37 || key == 38 || key == 39 || key == 40))
			return true;
	}

	if (this.document.selection)
		var el = this.document.selection.createRange().parentElement();
	else if (this.window.getSelection)
		var el = this.window.getSelection().focusNode;
	else
		return true;

	var bold = false;
	var italic = false;
	var underline = false;
	var justifyleft = false;
	var justifycenter = false;
	var justifyfull = false;
	var orderedlist = false;
	var unorderedlist = false;
	var name = el.nodeName;
	var align, textAlign, justify;
	while (name != "BODY") {
		if (this.browser.ie || (Node && el.nodeType == Node.ELEMENT_NODE)) {
			if (!bold && (name == "B" || name == "STRONG" || el.style.fontWeight == "bold" || el.style.fontWeight == "bolder" || el.style.fontWeight >= 600))
				bold = true;
			if (!italic && (name == "I" || name == "EM" || el.style.fontStyle == "italic"))
				italic = true;
			if (!underline && (name == "U" || el.style.textDecoration == "underline"))
				underline = true;
			if (!justify) {
				align = el.align;
				textAlign = el.style.textAlign;
				if (align == "left" || textAlign == "left") {
					justifyleft = true;
					justify = true;
				}
				else if (align == "center" || textAlign == "center") {
					justifycenter = true;
					justify = true;
				}
				else if (align == "justify" || textAlign == "justify") {
					justifyfull = true;
					justify = true;
				}
			}
			if (!orderedlist) {
				if (name == "OL") {
					if (el.style.listStyleType == 'decimal' || el.style.listStyleType == '')
						orderedlist = true;
					else if (!unorderedlist && el.style.listStyleType == 'disc')
						unorderedlist = true;
				}
			}
			if (!unorderedlist) {
				if (name == "UL") {
					if (el.style.listStyleType == 'disc' || el.style.listStyleType == '')
						unorderedlist = true;
					else if (!orderedlist && el.style.listStyleType == 'decimal')
						orderedlist = true;
				}
			}
		}
		el = el.parentNode;
		name = el.nodeName;
	}
	this.press('bold', bold);
	this.press('italic', italic);
	this.press('underline', underline);
	this.press('justifyleft', justifyleft);
	this.press('justifycenter', justifycenter);
	this.press('justifyright', !justify);
	this.press('justifyfull', justifyfull);
	this.press('ol', orderedlist);
	this.press('ul', unorderedlist);
}

richTextEditor.prototype.linkTarget = function() {
	var children = this.previewDoc.getElementsByTagName("a");
	for (var i = 0, item; item = children[i]; i++) {
		if (item.target != "_blank")
			item.target = "_blank";
	}
}
richTextEditor.prototype.colorSwitch = function() {
	var children = this.previewDoc.getElementsByTagName("font");
	for (var i = 0, item; item = children[i]; i++) {
		if (item.color != "") {
			switch (item.color) {
				case '#ffffff':
					item.className = 'fontWhite';
					break;
				case '#989898':
					item.className = 'fontGrey';
					break;
				case '#000000':
					item.className = 'fontBlack';
					break;
				case '#ff0000':
					item.className = 'fontRed';
					break;
				case '#980000':
					item.className = 'fontDarkRed';
					break;
				case '#ff005a':
					item.className = 'fontPink';
					break;
				case '#ffff98':
					item.className = 'fontLightYellow';
					break;
				case '#ffff00':
					item.className = 'fontYellow';
					break;
				case '#ff9800':
					item.className = 'fontOrange';
					break;
				case '#98ff98':
					item.className = 'fontLightGreen';
					break;
				case '#32ff32':
					item.className = 'fontGreen';
					break;
				case '#009800':
					item.className = 'fontDarkGreen';
					break;
				case '#00ccff':
					item.className = 'fontLightBlue';
					break;
				case '#0000ff':
					item.className = 'fontBlue';
					break;
				case '#000098':
					item.className = 'fontDarkBlue';
					break;
			}
			item.color = '';
		}
	}
	var children = this.previewDoc.getElementsByTagName("*");
	for (var i = 0, item; item = children[i]; i++) {
		switch (item.style.color) {
			case '#ffffff':
			case 'rgb(255, 255, 255)':
				item.className = 'fontWhite';
				break;
			case '#989898':
			case 'rgb(152, 152, 152)':
				item.className = 'fontGrey';
				break;
			case '#000000':
			case 'rgb(0, 0, 0)':
				item.className = 'fontBlack';
				break;
			case '#ff0000':
			case 'rgb(255, 0, 0)':
				item.className = 'fontRed';
				break;
			case '#980000':
			case 'rgb(152, 0, 0)':
				item.className = 'fontDarkRed';
				break;
			case '#ff005a':
			case 'rgb(255, 0, 90)':
				item.className = 'fontPink';
				break;
			case '#ffff98':
			case 'rgb(255, 255, 152)':
				item.className = 'fontLightYellow';
				break;
			case '#ffff00':
			case 'rgb(255, 255, 0)':
				item.className = 'fontYellow';
				break;
			case '#ff9800':
			case 'rgb(255, 152, 0)':
				item.className = 'fontOrange';
				break;
			case '#98ff98':
			case 'rgb(152, 255, 152)':
				item.className = 'fontLightGreen';
				break;
			case '#32ff32':
			case 'rgb(50, 255, 50)':
				item.className = 'fontGreen';
				break;
			case '#009800':
			case 'rgb(0, 152, 0)':
				item.className = 'fontDarkGreen';
				break;
			case '#00ccff':
			case 'rgb(0, 204, 255)':
				item.className = 'fontLightBlue';
				break;
			case '#0000ff':
			case 'rgb(0, 0, 255)':
				item.className = 'fontBlue';
				break;
			case '#000098':
			case 'rgb(0, 0, 152)':
				item.className = 'fontDarkBlue';
				break;
		}
		item.style.color = '';
	}
}
richTextEditor.prototype.sizeSwitch = function(action) {
	var children = this.document.getElementsByTagName("font");
	if (action == "add") {
		for (var i = 0, item; item = children[i]; i++) {
			if (item.size != "") {
				switch (item.size) {
					case '2':
						if (item.style.fontSize == "")
							item.style.fontSize = '10pt';
						break;
					case '3':
						if (item.style.fontSize == "")
							item.style.fontSize = '12pt';
						break;
					case '4':
						if (item.style.fontSize == "")
							item.style.fontSize = '18px';
						break;
					case '5':
						if (item.style.fontSize == "")
							item.style.fontSize = '18pt';
						break;
				}
			}
		}
	}
	else if (action == "remove") {
		for (var i = 0, item; item = children[i]; i++) {
			if (item.style.fontSize != "") {
				switch (item.style.fontSize) {
					case '10pt':
						if (item.size == 2 || item.size == "")
							item.style.fontSize = '';
						break;
					case '12pt':
						if (item.size == 3 || item.size == "")
							item.style.fontSize = '';
						break;
					case '18px':
						if (item.size == 4 || item.size == "")
							item.style.fontSize = '';
						break;
					case '18pt':
						if (item.size == 5 || item.size == "")
							item.style.fontSize = '';
						break;
				}
			}
		}
	}
}
richTextEditor.prototype.richEdit = function(aName, aArg) {
	if (aName == 'readonly') {
		if (this.browser.ie)
			this.document.body.contentEditable = aArg;
		else if (this.browser.safari)
			this.document.designMode = (aArg ? 'On' : 'Off')
		else
			this.document.execCommand(aName, false, aArg);
	}
	else {
		this.richEdit('readonly', true);
		this.document.execCommand(aName, false, aArg);
		if (!window.opera)
			this.window.focus();
	}
}
richTextEditor.prototype.removeFormatOnPaste = function(el) {
	if (this.browser.firefox) {
		el.removeAttribute('style');
		el.removeAttribute('align');
		if (el.nodeName == "FONT") {
			el.removeAttribute('size');
			el.removeAttribute('color');
		}
		var children = el.getElementsByTagName("*");
		for (var i = 0, item; item = children[i]; i++) {
			item.removeAttribute('style');
			item.removeAttribute('align');
			if (item.nodeName == "FONT") {
				item.removeAttribute('size');
				item.removeAttribute('color');
			}
		}
		setTimeout(function() {
			var userSelection = this.window.getSelection();
			userSelection.extend(el, 0);
			this.richEdit('removeformat');
			userSelection.collapseToEnd();
		}.are_bind(this), 5);
	}
	else {
		var r = this.document.selection.createRange();
		var text = this.window.clipboardData.getData("Text").replace(/>/g, "&gt;").replace(/</g, "&lt;").replace(/\n/g, "<br>");
		r.pasteHTML(text);
		this.textNodesToDivs();
		return false;
	}
}
richTextEditor.prototype.textNodesToDivs = function() {
	if (!this.browser.ie)
		return false;

	var item = this.document.body.firstChild;
	while (item) {
		if (item.data) {
			var div = this.document.createElement("div");
			div.innerHTML = item.data;
			item.parentNode.insertBefore(div, item);
			item.data = '';
		}
		item = item.nextSibling;
	}
}
richTextEditor.prototype.removeEmptyP = function() {
	var r = this.document.selection.createRange();
	var el = r.parentElement();
	if (el.nodeName == "P" && el.innerHTML == "") {
		el.parentNode.removeChild(el);
		r.pasteHTML("<div>&nbsp;</div>");
		r.moveStart("character", -1);
		r.collapse();
		r.select();
	}
}

/* this feature only exists on IE, this function
 * replicates it's behavior for other browsers
 */
richTextEditor.prototype.delLinkOnBackspace = function(ev) {
	if (window.getSelection && ev.keyCode == 8 && this.window.getSelection().isCollapsed) {
		var userSelection = this.window.getSelection();
		var anchor = userSelection.anchorNode;
		var prev = anchor.previousSibling;
		if (prev && prev.tagName == "A") { // if the element before the caret's node is a link
			if (userSelection.anchorOffset == 0) { // and the caret is at the beginning of the node
				ev.stopPropagation();
				ev.preventDefault();
				// select the text
				if (this.browser.safari)
					userSelection.setBaseAndExtent(prev, 0, prev, 1);
				else
					userSelection.extend(prev, 0);

				this.richEdit('unlink');
				userSelection.collapseToEnd()
			}
		}
		else {
			var el = anchor.parentNode;
			// if the element with the caret is a link, and the caret is at the end of the node
			if (el.tagName == "A" && userSelection.anchorOffset == el.firstChild.data.length) {
				ev.stopPropagation();
				ev.preventDefault();
				if (window.opera) {
					var text = this.document.createElement("span");
					text.innerHTML = el.innerHTML;
					el.parentNode.insertBefore(text, el.nextSibling)
					el.parentNode.removeChild(el);
				}
				else {
					// select the text
					if (this.browser.safari)
						userSelection.setBaseAndExtent(el, 0, el, 1);
					else
						userSelection.extend(el, 0);

					this.richEdit('unlink');
					userSelection.collapseToEnd()
				}
			}
		}
	}
}
richTextEditor.prototype.ctrlShiftAlignIE = function() {
	var ev = this.window.event;
	if (ev.shiftLeft) {
		if (ev.ctrlKey)
			this.align('justifyleft')
	}
	else if (ev.shiftKey) {
		if (ev.ctrlKey)
			this.align('justifyright')
	}
}
richTextEditor.prototype.shortcutKeys = function(e) {
	var ev = e || this.window.event;
	if (ev.shiftKey) {
		var key = String.fromCharCode(ev.which || ev.keyCode).toLowerCase();
		if	(key == "b")	{ this.bold() }
		else if (key == "i")	{ this.italic() }
		else if (key == "u")	{ this.underline() }
		else if (key == "r")	{ this.align('justifyright') }
		else if (key == "c")	{ this.align('justifycenter') }
		else if (key == "f")	{ this.align('justifyleft') }
		else if (key == "h")	{ this.align('justifyfull') }
		else if (key == "a")	{ this.toggleLinkWin() }
		else if (key == "m")	{ this.openImgWin() }
		else			{ return }

		if (ev.preventDefault) {
			ev.stopPropagation();
			ev.preventDefault();
		}
		else {
			ev.returnValue = false;
			ev.cancelBubble = true;
		}
	}
}
richTextEditor.prototype.createButton = function(type, className, text, alt) {
	var el = document.createElement(type.toUpperCase());
	if (className != "")
		el.className = className;

	var a = document.createElement("a");
	a.href = 'javascript:void(0)';
	if (alt != "") {
		a.title = alt;
		a.alt = alt;
	}
	a.innerHTML = text;

	el.appendChild(a);
	return el;
}
richTextEditor.prototype.toggleLinkWin = function() {
	this.paste.dontCheck = true;
	if (this.linkWrapper.style.display != "none") {
		this.linkWrapper.style.display = 'none';
		this.grayOut(0);
	}
	else {
		if (this.browser.ie) {
			this.window.focus();
			this.linkSel = this.document.selection.createRange().duplicate();
		}
		this.linkWrapper.style.display = '';
		this.grayOut(1);
		this.linkHeights();
		this.linkInput.focus();
		this.linkWrapper.focus();
	}
}
richTextEditor.prototype.linkHeights = function() {
	var height = this.getPageSize()[3];
	this.linkWrapper.style.top = ((height / 2) - 63) + 'px';
}
richTextEditor.prototype.grayOut = function(vis, options) {
	// Pass true to gray out screen, false to ungray  
	// options are optional.  This is a JSON object with the following (optional) properties  
	// opacity:0-100         
	// Lower number = less grayout higher = more of a blackout   
	// zindex: #             
	// HTML elements with a higher zindex appear on top of the gray out  
	// bgcolor: (#xxxxxx)    
	// Standard RGB Hex color code  
	// grayOut(true, {'zindex':'50', 'bgcolor':'#0000FF', 'opacity':'70'});  
	// Because options is JSON opacity/zindex/bgcolor are all optional and can appear  
	// in any order.  Pass only the properties you need to set.  
	var options = options || {};
	var zindex = options.zindex || 50;
	var opacity = options.opacity || 70;
	var opaque = (opacity / 100);
	var bgcolor = options.bgcolor || '#000000';
	var dark=document.getElementById('darkenScreenObject');
	if (!dark) {
		// The dark layer doesn't exist, it's never been created.  So we'll    
		// create it here and apply some basic styles.    
		// If you are getting errors in IE see: http://support.microsoft.com/default.aspx/kb/927917    
		var tbody = document.getElementsByTagName("body")[0];    
		var tnode = document.createElement('div');           
		// Create the layer.        
		tnode.style.position='absolute';                 
		// Position absolutely        
		tnode.style.top='0px';                           
		// In the top        
		tnode.style.left='0px';                          
		// Left corner of the page        
		tnode.style.overflow='hidden';                   
		// Try to avoid making scroll bars                    
		tnode.style.display='none';                      
		// Start out Hidden        
		tnode.id='darkenScreenObject';                   
		// Name it so we can find it later    
		tbody.appendChild(tnode);                            
		// Add it to the web page    
		dark=document.getElementById('darkenScreenObject');  
		// Get the object.  
	}  
	if (vis) {    		
		// Calculate the page width and height     
		if( document.body && ( document.body.scrollWidth || document.body.scrollHeight ) ) 
		{        
			var pageWidth = document.body.scrollWidth+'px';        
			var pageHeight = document.body.scrollHeight+'px';    
		} 
		else if( document.body.offsetWidth ) 
		{      
			var pageWidth = document.body.offsetWidth+'px';      
			var pageHeight = document.body.offsetHeight+'px';    
		} 
		else 
		{       
			var pageWidth='100%';       
			var pageHeight='100%';    
		}       
		//set the shader to cover the entire page and make it visible.    
		dark.style.opacity=opaque;
		dark.style.MozOpacity=opaque;                       
		dark.style.filter='alpha(opacity='+opacity+')';     
		dark.style.zIndex=zindex;            
		dark.style.backgroundColor=bgcolor;      
		dark.style.width= pageWidth;    
		dark.style.height= pageHeight;    
		dark.style.display='block';                            
	} 
	else {
		dark.style.display='none';  
	}
}
richTextEditor.prototype.getPageSize = function() {
	var xScroll, yScroll;
	
	if (window.innerHeight && window.scrollMaxY) {	
		xScroll = document.body.scrollWidth;
		yScroll = window.innerHeight + window.scrollMaxY;
	} else if (document.body.scrollHeight > document.body.offsetHeight){ // all but Explorer Mac
		xScroll = document.body.scrollWidth;
		yScroll = document.body.scrollHeight;
	} else { // Explorer Mac...would also work in Explorer 6 Strict, Mozilla and Safari
		xScroll = document.body.offsetWidth;
		yScroll = document.body.offsetHeight;
	}
	
	var windowWidth, windowHeight;
	if (self.innerHeight) {	// all except Explorer
		windowWidth = self.innerWidth;
		windowHeight = self.innerHeight;
	} else if (document.documentElement && document.documentElement.clientHeight) { // Explorer 6 Strict Mode
		windowWidth = document.documentElement.clientWidth;
		windowHeight = document.documentElement.clientHeight;
	} else if (document.body) { // other Explorers
		windowWidth = document.body.clientWidth;
		windowHeight = document.body.clientHeight;
	}	
	
	// for small pages with total height less then height of the viewport
	if(yScroll < windowHeight){
		pageHeight = windowHeight;
	} else { 
		pageHeight = yScroll;
	}

	// for small pages with total width less then width of the viewport
	if(xScroll < windowWidth){	
		pageWidth = windowWidth;
	} else {
		pageWidth = xScroll;
	}

	arrayPageSize = new Array(pageWidth,pageHeight,windowWidth,windowHeight) 
	return arrayPageSize;
}
richTextEditor.prototype.addLink = function() {
	this.toggleLinkWin();
	if (this.linkInput.value != "http:\/\/" && this.linkInput.value != "") {
		if (this.browser.ie) {
			this.linkSel.select();
			//var bodyEdge = this.linkSel.duplicate();
			//bodyEdge.moveToElementText(this.document.body);
			//if (this.linkSel.isEqual(bodyEdge))
			//	var html = this.document.body.firstChild.innerHTML;
			//else
			var html = this.linkSel.htmlText;
			html = html.replace(/<\/?[^>]+>/gi, '');
			if (html == false) {
				this.linkSel.pasteHTML("<a href='" + this.linkInput.value + "' target='_blank'>" + this.linkInput.value + "</a>");
			}
			else {
				this.richEdit('createlink', this.linkInput.value);
				//this.linkSel.pasteHTML("<a href='" + this.linkInput.value + "' target='_blank'>" + html + "</a>");

			}
		}
		else {
			if (this.window.getSelection() == "")
				this.richEdit('inserthtml', "<a href='" + this.linkInput.value + "' target='_blank'>" + this.linkInput.value + "</a>");
			else {
				//var div = this.document.createElement("div");
				//div.appendChild(this.window.getSelection().getRangeAt(0).cloneContents());
				this.richEdit('createlink', this.linkInput.value);
				//this.richEdit('inserthtml', "<a href='" + this.linkInput.value + "' target='_blank'>" + div.innerHTML + "</a>");
			}
		}
		this.linkInput.value = 'http:\/\/';
	}
}
richTextEditor.prototype.openImgWin = function() {
	this.paste.dontCheck = true;
	if (!this.imgWin.closed && this.imgWin.location) {
		this.imgWin.location.href = 'editorImagePopup.aspx'
	}
	else {
		this.imgWin = window.open('editorImagePopup.aspx','imgWin','height=335,width=700');
		this.imgWin.textEditor = this;
		this.addImages();
	}
	if (window.focus) {this.imgWin.focus()}
}
richTextEditor.prototype.toggleHTML = function() {
	this.paste.dontCheck = true;
	if (this.isHTML) {
		if (this.browser.ie) {
			var div = this.document.createElement("div");
			if (/^<object/.test(this.textarea.value))
				div.innerHTML = "<div>" + this.textarea.value + "</div>";
			else
				div.innerHTML = this.textarea.value;
			this.setContent(div.innerHTML);
			this.textNodesToDivs();
		}
		else
			this.setContent(this.textarea.value);
		this.iframe.style.display = '';
		this.htmlWrap.style.display = 'none';
		this.ul.style.display = '';
		this.buttons.html.className = 'html';
		this.isHTML = false;
		if (this.browser.firefox)
			setTimeout(function(){this.xbDesignmode(this.iframe)}.are_bind(this), 100);
	}
	else {
		this.textarea.value = this.getContent();
		this.ul.style.display = 'none';
		this.iframe.style.display = 'none';
		this.htmlWrap.style.display = '';
		this.buttons.html.className = 'html2';
		this.isHTML = true;
		if (this.browser.firefox)
			this.document.designMode = 'off';

		this.htmlWrap.focus();

		this.press('bold', false);
		this.press('italic', false);
		this.press('underline', false);
		this.press('justifyleft', false);
		this.press('justifycenter', false);
		this.press('justifyright', false);
		this.press('justifyfull', false);
		this.press('ol', false);
		this.press('ul', false);
	}
}
richTextEditor.prototype.togglePreview = function() {
	if (this.previewWrap.style.display != "none") {
		this.previewWrap.style.display = 'none';
		if (this.isHTML)
			this.htmlWrap.style.display = '';
		else {
			this.iframe.style.display = '';
			this.ul.style.display = '';
		}
		this.buttons.html.style.display = '';
		this.buttons.preview.innerHTML = "תצוגה מקדימה";
	}
	else {
		this.previewWrap.className = this.backgroundColorClass;
		this.previewDoc.body.className = this.backgroundColorClass;
		if (this.isHTML)
			this.htmlWrap.style.display = 'none';
		else {
			this.iframe.style.display = 'none';
			this.ul.style.display = 'none';
		}
		this.previewWrap.style.display = '';
		this.buttons.html.style.display = 'none';
		if (this.isHTML)
			this.setContent(this.textarea.value);

		this.previewDoc.body.innerHTML = this.getContent();
		this.linkTarget();
		this.colorSwitch();
		this.buttons.preview.innerHTML = "ערוך טקסט";

		this.preview.focus();
	}
}
richTextEditor.prototype.addImages = function() {
	this.ImagesDiv = this.document.createElement("div");
}
richTextEditor.prototype.addImagesAlign = function(align) {
	if (align == 'right') {
		if (this.browser.ie)
			this.ImagesDiv.style.styleFloat = 'right';
		else
			this.ImagesDiv.style.cssFloat = 'right';
	}
	if (align == 'center')
		this.ImagesDiv.style.textAlign = 'center';
	if (align == 'left') {
		if (this.browser.ie)
			this.ImagesDiv.style.styleFloat = 'left';
		else
			this.ImagesDiv.style.cssFloat = 'left';
	}
	this.document.body.insertBefore(this.ImagesDiv, this.document.body.firstChild)
}
richTextEditor.prototype.addImagesEnd = function() {
	this.imagesDiv = null;
}
richTextEditor.prototype.addImagesAppend = function(url) {
	var img = this.document.createElement("img");
	img.style.border = '0';
	img.style.display = 'block';
	img.style.margin = '5px';
	img.src = url;

	this.ImagesDiv.insertBefore(img, this.ImagesDiv.firstChild);
}