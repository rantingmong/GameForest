var GFX_SUBJ_INIT_CONNECTION = "GFX_INIT_CONNECTION";
var GFX_SUBJ_STOP_CONNECTION = "GFX_STOP_CONNECTION";

class GFXUserInfo {

    public firstName:       string;
    public lastName:        string;

    public userName:        string;
    public description:     string;
}

class GFXClientCore {

    private webSocketConnection:    WebSocket;

    private sesssionId:             string;
    private connectionId:           string;

    private connectionStringGot:    boolean;

    constructor         () {

        this.connectionStringGot = false;
        this.webSocketConnection = new WebSocket("ws://localhost:1193");

        this.sesssionId     = "";
        this.connectionId   = "";
    }

    createWebSocket     () {

        this.webSocketConnection.onopen     = this.webSocketOnOpen;
        this.webSocketConnection.onclose    = this.webSocketOnClose;
        this.webSocketConnection.onmessage  = this.webSocketOnMessage;
    }

    webSocketOnOpen     () {

    }
    webSocketOnClose    () {

    }
    webSocketOnMessage  (message: MessageEvent) {
        
        var parsedMessage = JSON.parse(message.data);

        if (this.connectionStringGot == false) {

            this.connectionId           = parsedMessage;
            this.connectionStringGot    = true;
        }
        else {

            switch (parsedMessage.Subject) {

            }
        }
    }

    login               (username: string, password: string) {

        
    }

    logout              () {


    }

    getUserInfo         (userNameOrUserId: string) {

        return new GFXUserInfo();
    }

    setUserInfo         (userInfo: GFXUserInfo) {


    }
}
