<%@ Page Language="C#" AutoEventWireup="true" CodeFile="GoogleInApp.aspx.cs" Inherits="GoogleInApp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>
    <script type="text/javascript">
        google.load('payments', "1.0", {
            'packages': ["sandbox_config"]
        });
      </script>
    
     
<script type="text/javascript">
    google.load('payments', '1.0', {
        'packages': ['sandbox_config']

    });

    function purchaseSubscription(callback) {
        goog.payments.inapp.buy({
            "parameters": {},
            "jwt": "<%=theJWT(99159) %>",
            "success": function (result) {
                if (typeof callback === "function") {
                    callback(true, result);
                }
            },
            "failure": function (result) {
                if (typeof callback === "function") {
                    callback(false, result);
                }
            }
        }
        )
    };

    function purchasePPV(callback) {
        goog.payments.inapp.buy({
            "parameters": {},
            "jwt": "<%=theJWT(99220) %>",
            "success": function (result) {
                if (typeof callback === "function") {
                    callback(true, result);
                }
            },
            "failure": function (result) {
                if (typeof callback === "function") {
                    callback(false, result);
                }
            }
        }
        )
    };

    /*** S A M P L E   O N L Y ****
    *******************************
    !You should verify server side!
    *******************************                
    */
    var sampleParseResult = function (isgood, data) {

        var _console = (typeof window.console === "undefined");
        if (isgood) {
            var _str = "Verify Order No. " + data.response.orderId;
            _str += "\nDetails:\n";
            _str += data.request.name + " " + data.request.description + "\n";
            _str += data.request.price + "\n Eroor Type:";
            _str += data.response.errorType;
            alert(_str);
            if (!_console) {
                console.log(data);
            }
        } else {
            alert("failed");
            var _str = "Verify Order No. " + data.response.orderId;
            _str += "\nDetails:\n";
            _str += data.request.name + " " + data.request.description + "\n";
            _str += data.request.price + "\n Eroor Type:";
            _str += data.response.errorType;
            alert(_str);
            if (!_console) {
                console.log(data);
            }
        }
    };
     </script>
</head>
<body>
     <form id="form1" runat="server">
    <div class="container_12" style="margin-top: 20px;">
        <div>
            <h1>
                In-App Payments Demo:
            </h1>
            <h5>
               Unlimited Subscription </h5>
            <ul>
                <li>You will be automatically charged €6.99 EUR plus any applicable tax every month starting February 12, 2013 until you cancel your subscription.  </li>
                <li>Price: $6.99</li>
            </ul>
            <asp:Button ID="foo" Text="Buy subscription" runat="server" OnClientClick="purchaseSubscription(sampleParseResult); return false;"
                Style="padding: 5px;" />
                <div>-----------------------------------</div>

                 <h5>
               PPV Buy item </h5>
            <ul>
                <li>Buy now for 10 years</li>
                <li>Price: $19.99</li>
            </ul>
            <asp:Button ID="Button1" Text="Buy PPV" runat="server" OnClientClick="purchasePPV(sampleParseResult); return false;"
                Style="padding: 5px;" />
        </div>
   
    </form>
</body>
</html>
