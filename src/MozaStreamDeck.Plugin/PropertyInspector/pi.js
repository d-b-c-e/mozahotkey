// Property Inspector JavaScript for Moza Stream Deck Plugin

let websocket = null;
let uuid = null;
let actionInfo = null;
let settings = {};
let controllerType = 'Keypad'; // 'Keypad' for buttons, 'Encoder' for dials

function connectElgatoStreamDeckSocket(inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inPropertyInspectorUUID;
    actionInfo = JSON.parse(inActionInfo);
    settings = actionInfo.payload.settings || {};

    // Detect controller type (Keypad = button, Encoder = dial)
    controllerType = actionInfo.payload.controller || 'Keypad';

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
    // Show/hide direction based on controller type
    const directionSection = document.getElementById('direction-section');
    if (directionSection) {
        directionSection.style.display = controllerType === 'Encoder' ? 'none' : 'block';
    }

    // Override this function in each PI page
    if (typeof onSettingsLoaded === 'function') {
        onSettingsLoaded(settings);
    }
}

function isEncoder() {
    return controllerType === 'Encoder';
}

function isKeypad() {
    return controllerType === 'Keypad';
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
