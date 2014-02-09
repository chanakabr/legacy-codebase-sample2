/// <reference name="MicrosoftAjax.js"/>
// Copyright (c) iucon GmbH. All rights reserved.
// For more information about our work, visit http://www.iucon.com

Type.registerNamespace("iucon.web.Controls");

iucon.web.Controls.PartialUpdatePanel = function(element) {
//    try    
//    {      
//        var objElemento = $find(element.id);      
//        if (objElemento != null)      
//        {        
//            Sys.Application.removeComponent(objElemento);      
//        }    
//    }    
//    catch(err)     
//    { }
    iucon.web.Controls.PartialUpdatePanel.initializeBase(this, [element]);
    
    this._userControlPath = null;
    this._parameters = new Array();
    this._showLoading = false;
    this._autoRefreshInterval = 0;
    this._autoRefreshTimerID = null;
    this._initiallyRenderFromClient = false;
    this._renderAfterPanel = null;
    this._loadHandler = null;
    this._renderAfterPanelHandler = null;
    this._displayLoadingAfter = 0;
    this._displayLoadingAfterTimerID = null;
    this._currentCulture = null;
    this._currentUICulture = null;
    this._siteLanguageKey = null;
    
    this._addComponentBackup = null;
    this._dataItems = null;
    this._executor = null;
        
    this._request = null;
}

iucon.web.Controls.PartialUpdatePanel.prototype = {

    initialize: function() {
        iucon.web.Controls.PartialUpdatePanel.callBaseMethod(this, 'initialize');

        if (window._partialScriptReferences == null)
            window._partialScriptReferences = new Array();

        if (this._initiallyRenderFromClient) {
            if (this._renderAfterPanel == null)
                this.refresh();
            else {
                this._loadHandler = Function.createDelegate(this, this._onLoad);
                Sys.Application.add_load(this._loadHandler);
            }
        }
    },

    dispose: function() {
        if (this._loadHandler) {
            Sys.Application.remove_load(this._loadHandler);
            this._loadHandler = null;
        }

        iucon.web.Controls.PartialUpdatePanel.callBaseMethod(this, 'dispose');
    },

    _onLoad: function() {
        var control = $find(this._renderAfterPanel);
        if (control) {
            this._renderAfterPanelHandler = Function.createDelegate(this, this._onRenderAfterPanel);
            control.add_refreshComplete(this._renderAfterPanelHandler);

            // show loading panel        
            if (this._showLoading) {
                var loadingPanel = $get(this.get_element().id + '_LoadingPanel');
                if (loadingPanel) {
                    loadingPanel.style.display = 'block';
                    loadingPanel.style.visibility = 'visible';
                }
            }
        }
    },

    _onRenderAfterPanel: function(e) {
        var control = $find(this._renderAfterPanel);
        if (control && this._renderAfterPanelHandler) {
            control.remove_refreshComplete(this._renderAfterPanelHandler);
            this._renderAfterPanelHandler = null;
        }

        this.refresh();
    },

    /// ShowLoading
    get_ShowLoading: function() {
        return this._showLoading;
    },
    set_ShowLoading: function(value) {
        this._showLoading = value;
    },

    /// UserControlPath
    get_UserControlPath: function() {
        return this._userControlPath;
    },
    set_UserControlPath: function(value) {
        this._userControlPath = value;
    },

    /// Parameters
    get_Parameters: function() {
        return this._parameters;
    },
    set_Parameters: function(value) {
        this._parameters = new Array();

        if (value != null) {
            for (var i = 0; i < value.length; i++) {
                this._parameters[value[i]["Name"]] = value[i]["Value"];
            }
        }
    },

    /// AutoRefreshInterval
    get_AutoRefreshInterval: function() {
        return this._autoRefreshInterval;
    },
    set_AutoRefreshInterval: function(value) {
        this._autoRefreshInterval = value;

        if (this._autoRefreshInterval > 0) {
            var element = this.get_element();
            this._autoRefreshTimerID = setInterval('$find(\'' + element.id + '\').refresh()', this._autoRefreshInterval);
        }
        else if (this._autoRefreshTimerID != null) {
            clearTimeout(this._autoRefreshTimerID);
            this._autoRefreshTimerID = null;
        }
    },

    // DisplayLoadingAfter
    get_DisplayLoadingAfter: function() {
        return this._displayLoadingAfter;
    },
    set_DisplayLoadingAfter: function(value) {
        this._displayLoadingAfter = value;
    },

    /// InitialRenderBehaviour
    get_InitiallyRenderFromClient: function() {
        return this._initiallyRenderFromClient;
    },
    set_InitiallyRenderFromClient: function(value) {
        this._initiallyRenderFromClient = value;
    },

    /// RenderAfterPanel
    get_RenderAfterPanel: function() {
        return this._renderAfterPanel;
    },
    set_RenderAfterPanel: function(value) {
        this._renderAfterPanel = value;
    },

    // CurrentCulture
    get_CurrentCulture: function() {
        return this._currentCulture;
    },
    set_CurrentCulture: function(value) {
        this._currentCulture = value;
    },

    // CurrentUICulture
    get_CurrentUICulture: function() {
        return this._currentUICulture;
    },
    set_CurrentUICulture: function(value) {
        this._currentUICulture = value;
    },
    
    // SiteLanguageKey
    get_SiteLanguageKey: function() {
        return this._siteLanguageKey;
    },
    set_SiteLanguageKey: function(value) {
        this._siteLanguageKey = value;
    },
    
    

    // RefreshComplete Event
    add_refreshComplete: function(handler) {
        this.get_events().addHandler('refreshComplete', handler);
    },
    remove_refreshComplete: function(handler) {
        this.get_events().removeHandler('refreshComplete', handler);
    },
    raiseRefreshComplete: function(eventArgs) {
        var handler = this.get_events().getHandler('refreshComplete');
        if (handler) {
            handler(this, eventArgs);
        }
    },

    doPostBack: function(eventTarget, eventArgument) {
        this._refresh(eventTarget, eventArgument);
    },

    abortPostBack: function() {
        if (this._request == null) return;
        this._request.get_executor().abort();
        this._request = null;
        this._clearShowLoadingPanelTimer();
    },

    refresh: function() {
        this._refresh(null, null);
    },

    _showLoadingPanel: function() {
        this._clearShowLoadingPanelTimer();

        if (this._displayLoadingAfter > 0) {
            var element = this.get_element();
            this._displayLoadingAfterTimerID = setInterval('$find(\'' + element.id + '\')._internalShowLoadingPanel()', this._displayLoadingAfter);
        }
        else
            this._internalShowLoadingPanel();
    },

    _internalShowLoadingPanel: function() {
        var element = this.get_element();


        var loadingPanel = $get(element.id + '_LoadingPanel');
        var contentPanel = $get(element.id + '_ContentPanel');

        if (loadingPanel) {
            loadingPanel.style.display = 'block';
            loadingPanel.style.visibility = 'visible';
        }
        if (contentPanel) {
            contentPanel.innerHTML = '';
            contentPanel.style.visibility = 'hidden';
        }
    },

    _clearShowLoadingPanelTimer: function() {
        if (this._displayLoadingAfterTimerID != null) {
            clearTimeout(this._displayLoadingAfterTimerID);
            this._displayLoadingAfterTimerID = null;
        }
    },

    _refresh: function(eventTarget, eventArgument) {
        // a refresh is in progress => do nothing        
        if (this._request != null) return;

        var element = this.get_element();

        // get panels        
        var errorPanel = $get(element.id + '_ErrorPanel');

        // create request
        this._request = new Sys.Net.WebRequest();
        this._request.set_url('PartialUpdatePanelLoader.ashx');
        this._request.set_httpVerb('POST');
        // this is to set ScriptManager in IsInAsyncPostBack-mode
        this._request.get_headers()['X-MicrosoftAjax'] = 'Delta=true';
        this._request.set_body(this._createRequestBody(eventTarget, eventArgument));
        this._request.set_userContext(this);
        this._request.add_completed(this._loadingComplete);

        // show/hide panels
        if (errorPanel) errorPanel.style.display = 'none';

        if (this._showLoading)
            this._showLoadingPanel();

        // invoke request
        this._request.invoke();
    },

    _loadingComplete: function(sender, eventArgs) {
        var request = sender.get_webRequest();
        var _this = request.get_userContext();
        var element = _this.get_element();

        _this._clearShowLoadingPanelTimer();

        if (sender.get_responseAvailable()) {
            // loading succeeded            

            var loadingPanel = $get(element.id + '_LoadingPanel');
            var contentPanel = $get(element.id + '_ContentPanel');

            // show content
            if (contentPanel) {

                var newContent = sender.get_responseData();

                contentPanel.style.visibility = 'visible';
                contentPanel.innerHTML = newContent; //newContent.substr(1,newContent.length-1);

                var parentPanel = $get(element.id);

                if (newContent.substring(0, 10) == "<!-- 1 -->") {
                    parentPanel.style.display = 'block';
                } else {
                    parentPanel.style.display = 'none';
                }
            }

            // hide loading panel            
            if (loadingPanel) {
                loadingPanel.style.visibility = "hidden";
                loadingPanel.style.display = "none";
            }

            // run startup scripts
            var scripts = $get(element.id + '_SCRIPTS');
            if (scripts != null) {
                if (scripts.innerHTML != null && scripts.innerHTML != '') {
                    Sys._ScriptLoader.readLoadedScripts();

                    // hack: save reference to Sys.Application.addComponent, because 
                    // we have to overwrite it temporarily
                    if (Sys.Application.addComponent != _this._addComponent)
                        _this._addComponentBackup = Sys.Application.addComponent;

                    // set the new reference                    
                    Sys.Application.addComponent = _this._addComponent;

                    var scriptLoader = Sys._ScriptLoader.getInstance();
                    var result = Sys.Serialization.JavaScriptSerializer.deserialize(scripts.innerHTML);

                    // 1st process clientScriptInclude
                    for (var i = 0; i < result.length; i++) {
                        if (result[i]["Type"] == "clientScriptInclude") {
                            var url = result[i]["Url"];
                            url = url.replace(/&amp;/g, "&");

                            if (!Sys._ScriptLoader.isScriptLoaded(url) &&
                                (scriptLoader._scriptsToLoad == null ||
                                !Array.contains(scriptLoader._scriptsToLoad, { src: url })) &&
                                !Array.contains(window._partialScriptReferences, url)) {
                                Array.add(window._partialScriptReferences, url);
                                scriptLoader.queueScriptReference(url);
                            }
                        }
                    }

                    // 2nd process scriptStartupBlock, scriptBlock and dataItems
                    for (var i = 0; i < result.length; i++) {
                        if (result[i]["Type"] == "scriptStartupBlock" ||
                            result[i]["Type"] == "scriptBlock") {
                            var script = result[i]["Script"];
                            script = script.replace(/&amp;/g, "&");
                            scriptLoader.queueScriptBlock(script);
                        }
                        else if (result[i]["Type"] == "dataItem") {
                            if (_this._dataItems == null) _this._dataItems = {};
                            _this._dataItems[result[i]["ClientID"]] = result[i]["Script"]
                        }
                        else if (result[i]["Type"] == "dataItemJson") {
                            if (_this._dataItems == null) _this._dataItems = {};
                            _this._dataItems[result[i]["ClientID"]] = eval(result[i]["Script"]);
                        }
                    }

                    if (!scriptLoader._loading) {
                        _this._executor = request.get_executor();
                        scriptLoader.loadScripts(0, Function.createDelegate(_this, _this._scriptsLoadComplete), null, null);
                    }
                }

                // remove script content node from DOM Tree
                scripts.parentNode.removeChild(scripts);
            }
        }
        else {
            // loading error
            var loadingPanel = $get(element.id + '_LoadingPanel');
            var errorPanel = $get(element.id + '_ErrorPanel');

            if (sender.get_timedOut() ||
                sender.get_aborted()) {
                loadingPanel.style.visibility = "hidden";
                loadingPanel.style.display = "none";

                errorPanel.style.display = "block";
            }
        }

        // refresh completed
        _this._request = null;

        // raise event
        _this.raiseRefreshComplete(Sys.EventArgs.Empty);
    },

    _addComponent: function(component) {
        var id = component.get_id();
        if (typeof (Sys.Application._components[id]) === 'undefined')
            Sys.Application._components[id] = component;
    },

    _scriptsLoadComplete: function() {
        // execute changes made by dataItems    
        if (this._dataItems != null) {
            var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
            var handler = pageRequestManager._get_eventHandlerList().getHandler("endRequest");
            if (handler) {
                var eventArgs = new Sys.WebForms.EndRequestEventArgs(null, this._dataItems, this._executor);
                handler(pageRequestManager, eventArgs);
            }
            this._dataItems = null;
        }

        // restore old addComponent
        if (this._addComponentBackup != null)
            Sys.Application.addComponent = this._addComponentBackup;
    },

    _createRequestBody: function(eventTarget, eventArgument) {
        var element = this.get_element();
        var contentPanel = $get(element.id + '_ContentPanel');
        var viewState = $get(element.id + '_ViewState');

        var requestBody = "__USERCONTROLPATH=" + this.get_UserControlPath();
        requestBody += "&__CONTROLCLIENTID=" + element.id;

        if (viewState != null)
            requestBody += "&__VIEWSTATE=" + viewState.value;

        if (eventTarget != null)
            requestBody += "&__EVENTTARGET=" + eventTarget;

        if (eventArgument != null)
            requestBody += "&__EVENTARGUMENT=" + eventArgument;

        // add culture information
        requestBody += "&__CURRENTCULTURE=" + this.get_CurrentCulture();
        requestBody += "&__CURRENTUICULTURE=" + this.get_CurrentUICulture();
        requestBody += "&__SiteLanguageKey=" + this.get_SiteLanguageKey();

        // add parameters                
        requestBody += "&__PARAMETERS=" + this._createParameterValue();

        // add values from inner <select>-elements
        var selectElements = contentPanel.getElementsByTagName("select");
        for (var i = 0; i < selectElements.length; i++)
            requestBody += "&" + selectElements[i].name + "=" + selectElements[i].value;

        // add values from inner <textarea>-elements
        var textareaElements = contentPanel.getElementsByTagName("textarea");
        for (var i = 0; i < textareaElements.length; i++)
            requestBody += "&" + textareaElements[i].name + "=" + textareaElements[i].value;

        // add values from inner <input>-elements
        var inputElements = contentPanel.getElementsByTagName("input");
        for (var i = 0; i < inputElements.length; i++) {
            if (inputElements[i].name != element.id + "_ViewState" &&
                inputElements[i].type != 'button' &&
                inputElements[i].type != 'submit' &&
                (inputElements[i].type != 'checkbox' || inputElements[i].checked) &&
                (inputElements[i].type != 'radio' || inputElements[i].checked))
                requestBody += "&" + inputElements[i].name + "=" + inputElements[i].value;
        }

        return requestBody;
    },

    _createParameterValue: function() {
        var result = new Array();

        for (var key in this._parameters) {
            var obj = new Object();
            obj["Name"] = key;
            obj["Value"] = this._parameters[key];

            result.push(obj);
        }

        return Sys.Serialization.JavaScriptSerializer.serialize(result);
    }

}
iucon.web.Controls.PartialUpdatePanel.registerClass('iucon.web.Controls.PartialUpdatePanel', Sys.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();