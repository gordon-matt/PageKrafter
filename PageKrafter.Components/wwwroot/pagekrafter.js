// Editor-side bridge. Relays messages between the canvas iframe and Blazor, and starts drags from
// the palette, templates and layers panels. All page structure lives on the server; this file only
// moves messages and never edits the document itself.

const editors = new Map();

export function mount(host, frame, dotnet) {
    const state = { host, frame, dotnet, ready: false, queue: [] };

    state.onMessage = (event) => {
        if (event.source !== frame.contentWindow || !event.data || event.data.__pk !== true) {
            return;
        }

        if (event.data.type === 'ready') {
            state.ready = true;
            const queued = state.queue.splice(0);
            queued.forEach((message) => post(state, message));
            return;
        }

        dotnet.invokeMethodAsync('OnCanvasMessage', event.data).catch(() => { });
    };

    state.onDragStart = (event) => {
        const source = event.target instanceof Element ? event.target.closest('[data-pk-drag]') : null;
        if (!source || !host.contains(source)) {
            return;
        }

        let payload;
        try {
            payload = JSON.parse(source.getAttribute('data-pk-drag'));
        } catch {
            return;
        }

        event.dataTransfer.effectAllowed = payload.kind === 'move' ? 'move' : 'copy';
        event.dataTransfer.setData('text/plain', 'pagekrafter');
        send(state, { type: 'drag', payload });
    };

    state.onDragEnd = () => send(state, { type: 'drag', payload: null });

    state.onKeyDown = (event) => {
        if (isTyping(event.target)) {
            return;
        }

        const command = shortcut(event);
        if (command) {
            event.preventDefault();
            dotnet.invokeMethodAsync('OnCanvasMessage', { type: 'command', command, index: 0 }).catch(() => { });
        }
    };

    window.addEventListener('message', state.onMessage);
    host.addEventListener('dragstart', state.onDragStart);
    host.addEventListener('dragend', state.onDragEnd);
    host.addEventListener('keydown', state.onKeyDown);
    editors.set(frame, state);
}

export function load(frame, html, selectedId, preview) {
    const state = editors.get(frame);
    if (!state) {
        return;
    }

    state.ready = false;
    state.queue = [{ type: 'select', id: selectedId ?? null }, { type: 'preview', on: !!preview }];
    frame.srcdoc = html;
}

export function render(frame, html, selectedId) {
    const state = editors.get(frame);
    if (state) {
        state.queue = state.queue.filter((message) => message.type !== 'render');
        send(state, { type: 'render', html, selectedId: selectedId ?? null });
    }
}

export function select(frame, id) {
    const state = editors.get(frame);
    if (state) {
        send(state, { type: 'select', id: id ?? null, scroll: true });
    }
}

export function setPreview(frame, on) {
    const state = editors.get(frame);
    if (state) {
        send(state, { type: 'preview', on: !!on });
    }
}

export function confirmAction(message) {
    return window.confirm(message);
}

export async function copyText(text) {
    try {
        await navigator.clipboard.writeText(text);
    } catch {
        const area = document.createElement('textarea');
        area.value = text;
        document.body.appendChild(area);
        area.select();
        document.execCommand('copy');
        area.remove();
    }
}

export function dispose(frame) {
    const state = editors.get(frame);
    if (!state) {
        return;
    }

    window.removeEventListener('message', state.onMessage);
    state.host.removeEventListener('dragstart', state.onDragStart);
    state.host.removeEventListener('dragend', state.onDragEnd);
    state.host.removeEventListener('keydown', state.onKeyDown);
    editors.delete(frame);
}

function send(state, message) {
    if (state.ready) {
        post(state, message);
    } else {
        state.queue.push(message);
    }
}

function post(state, message) {
    state.frame.contentWindow?.postMessage(Object.assign({ __pk: true }, message), '*');
}

function isTyping(target) {
    return target instanceof Element
        && (target.closest('input, textarea, select, [contenteditable=""], [contenteditable="true"], .pk-dialog') !== null);
}

export function shortcut(event) {
    const mod = event.ctrlKey || event.metaKey;
    const key = event.key.toLowerCase();
    if (mod && key === 'z') {
        return event.shiftKey ? 'redo' : 'undo';
    }

    if (mod && key === 'y') {
        return 'redo';
    }

    if (mod && key === 'd') {
        return 'duplicate';
    }

    if (mod && event.key === 'ArrowUp') {
        return 'up';
    }

    if (mod && event.key === 'ArrowDown') {
        return 'down';
    }

    if (!mod && (event.key === 'Delete' || event.key === 'Backspace')) {
        return 'delete';
    }

    if (event.key === 'Escape') {
        return 'parent';
    }

    return null;
}
