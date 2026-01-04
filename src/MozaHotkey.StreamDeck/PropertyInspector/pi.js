// Property Inspector JavaScript for MozaHotkey Stream Deck Plugin

let websocket = null;
let uuid = null;
let actionInfo = null;
let settings = {};

function connectElgatoStreamDeckSocket(inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inPropertyInspectorUUID;
    actionInfo = JSON.parse(inActionInfo);
    settings = actionInfo.payload.settings || {};

    websocket = new WebSocket('ws://127.0.0.1:' + inPort);

    websocket.onopen = function() {
        // Register the Property Inspector
        websocket.send(JSON.stringify({
            event: inRegisterEvent,
            uuid: uuid
        }));

        // Update UI with current settings
        updateUI();
    };

    websocket.onmessage = function(evt) {
        const data = JSON.parse(evt.data);
        if (data.event === 'didReceiveSettings') {
            settings = data.payload.settings || {};
            updateUI();
        }
    };
}

function updateUI() {
    // Override this function in each PI page
    if (typeof onSettingsLoaded === 'function') {
        onSettingsLoaded(settings);
    }
}

function saveSettings() {
    if (websocket && websocket.readyState === 1) {
        websocket.send(JSON.stringify({
            event: 'setSettings',
            context: uuid,
            payload: settings
        }));
    }
}

function setSetting(key, value) {
    settings[key] = value;
    saveSettings();
}

function getSetting(key, defaultValue) {
    return settings[key] !== undefined ? settings[key] : defaultValue;
}
