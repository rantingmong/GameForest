var GFX_SUBJ_INIT_CONNECTION = "GFX_INIT_CONNECTION";
var GFX_SUBJ_STOP_CONNECTION = "GFX_STOP_CONNECTION";

var GFXUserInfo = (function () {
    function GFXUserInfo() {
    }
    return GFXUserInfo;
})();

var GFXClientCore = (function () {
    function GFXClientCore() {
        this.connectionStringGot = false;
        this.webSocketConnection = new WebSocket("ws://localhost:1193");

        this.sesssionId = "";
        this.connectionId = "";
    }
    GFXClientCore.prototype.createWebSocket = function () {
        this.webSocketConnection.onopen = this.webSocketOnOpen;
        this.webSocketConnection.onclose = this.webSocketOnClose;
        this.webSocketConnection.onmessage = this.webSocketOnMessage;
    };

    GFXClientCore.prototype.webSocketOnOpen = function () {
    };
    GFXClientCore.prototype.webSocketOnClose = function () {
    };
    GFXClientCore.prototype.webSocketOnMessage = function (message) {
        var parsedMessage = JSON.parse(message.data);

        if (this.connectionStringGot == false) {
            this.connectionId = parsedMessage;
            this.connectionStringGot = true;
        } else {
            switch (parsedMessage.Subject) {
            }
        }
    };

    GFXClientCore.prototype.login = function (username, password) {
    };

    GFXClientCore.prototype.logout = function () {
    };

    GFXClientCore.prototype.getUserInfo = function (userNameOrUserId) {
        return new GFXUserInfo();
    };

    GFXClientCore.prototype.setUserInfo = function (userInfo) {
    };
    return GFXClientCore;
})();
//@ sourceMappingURL=GameForest.js.map
