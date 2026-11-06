// Runs inside the canvas iframe. Draws hover and selection outlines, works out where a drag would
// drop, and handles in-place text editing. It never changes the page structure: every change is
// posted to the editor, which updates the JSON on the server and sends back fresh HTML.
(() => {
    'use strict';

    const root = document.getElementById('pk-canvas-root');
    const supportsPlainText = (() => {
        const probe = document.createElement('div');
        probe.setAttribute('contenteditable', 'plaintext-only');
        return probe.contentEditable === 'plaintext-only';
    })();

    let selectedId = null;
    let hoverEl = null;
    let drag = null;
    let dropTarget = null;
    let editing = null;
    let pendingRender = null;
    let retainEditing = false;
    let savedRange = null;
    let preview = false;
    let frameRequested = false;

    const post = (message) => window.parent.postMessage(Object.assign({ __pk: true, index: 0 }, message), '*');

    const icons = {
        parent: '<path d="M12 19V5M6 11l6-6 6 6"/>',
        move: '<path d="M12 3v18M3 12h18M8 7l4-4 4 4M8 17l4 4 4-4M7 8l-4 4 4 4M17 8l4 4-4 4"/>',
        duplicate: '<rect x="8" y="8" width="12" height="12" rx="2"/><path d="M16 8V5a1 1 0 0 0-1-1H5a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h3"/>',
        template: '<path d="m12 3 2.8 5.7 6.2.9-4.5 4.4 1 6.2L12 17.3 6.5 20.2l1-6.2L3 9.6l6.2-.9z"/>',
        delete: '<path d="M4 7h16M10 11v6M14 11v6M6 7l1 13h10l1-13M9 7V4h6v3"/>',
    };

    const svg = (path) => `<svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${path}</svg>`;

    const element = (className, parent, html) => {
        const el = document.createElement('div');
        el.className = className;
        if (html) {
            el.innerHTML = html;
        }

        parent.appendChild(el);
        return el;
    };

    const chrome = element('pk-chrome', document.body);
    const hoverBox = element('pk-hover-box', chrome);
    const hoverLabel = element('pk-badge pk-badge-hover', hoverBox);
    const selectBox = element('pk-select-box', chrome);
    const toolbar = element('pk-toolbar', chrome);
    const toolbarLabel = element('pk-toolbar-label', toolbar);
    const dropZone = element('pk-drop-zone', chrome);
    const dropLine = element('pk-drop-line', chrome);
    const rte = element('pk-rte', chrome);

    [
        ['parent', 'Select parent (Esc)'],
        ['move', 'Drag to move'],
        ['duplicate', 'Duplicate (Ctrl+D)'],
        ['template', 'Save as template'],
        ['delete', 'Delete (Del)'],
    ].forEach(([command, title]) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.title = title;
        button.setAttribute('aria-label', title);
        button.dataset.command = command;
        button.innerHTML = svg(icons[command]);
        if (command === 'move') {
            button.draggable = true;
        }

        toolbar.appendChild(button);
    });

    [
        ['bold', '<b>B</b>', 'Bold'],
        ['italic', '<i>I</i>', 'Italic'],
        ['underline', '<u>U</u>', 'Underline'],
        ['createLink', '🔗', 'Link'],
        ['unlink', '⛓', 'Remove link'],
        ['insertUnorderedList', '•', 'Bulleted list'],
        ['insertOrderedList', '1.', 'Numbered list'],
        ['removeFormat', '⌫', 'Clear formatting'],
    ].forEach(([command, label, title]) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.title = title;
        button.setAttribute('aria-label', title);
        button.dataset.rte = command;
        button.innerHTML = label;
        rte.appendChild(button);
    });

    [hoverBox, selectBox, toolbar, dropZone, dropLine, rte].forEach((el) => { el.hidden = true; });

    function blockOf(node) {
        while (node && node !== document) {
            if (node.nodeType === 1 && node.hasAttribute('data-pk-id')) {
                return node;
            }

            node = node.parentNode;
        }

        return null;
    }

    function byId(id) {
        return id ? root.querySelector(`[data-pk-id="${CSS.escape(id)}"]`) : null;
    }

    function isRoot(el) {
        return el && el.hasAttribute('data-pk-root');
    }

    function place(box, el) {
        const rect = el.getBoundingClientRect();
        box.style.left = `${rect.left + window.scrollX}px`;
        box.style.top = `${rect.top + window.scrollY}px`;
        box.style.width = `${rect.width}px`;
        box.style.height = `${rect.height}px`;
        box.hidden = false;
        return rect;
    }

    function refresh() {
        frameRequested = false;
        if (preview) {
            return;
        }

        const selected = byId(selectedId);
        if (selected && !isRoot(selected)) {
            const rect = place(selectBox, selected);
            toolbarLabel.textContent = selected.getAttribute('data-pk-label') || '';
            toolbar.hidden = !!editing;
            const toolbarHeight = 26;
            const above = rect.top - toolbarHeight - 2;
            const below = rect.bottom + 2;
            let top = above;
            if (above < 0) {
                top = below + toolbarHeight <= window.innerHeight ? below : Math.max(rect.top, 0) + 2;
            }

            toolbar.style.top = `${top + window.scrollY}px`;
            toolbar.style.left = `${Math.max(window.scrollX + 2, rect.left + window.scrollX)}px`;
        } else {
            selectBox.hidden = true;
            toolbar.hidden = true;
        }

        if (hoverEl && hoverEl.isConnected && hoverEl !== selected && !isRoot(hoverEl) && !drag) {
            place(hoverBox, hoverEl);
            hoverLabel.textContent = hoverEl.getAttribute('data-pk-label') || '';
        } else {
            hoverBox.hidden = true;
        }

        if (editing) {
            positionRte();
        }
    }

    function scheduleRefresh() {
        if (!frameRequested) {
            frameRequested = true;
            requestAnimationFrame(refresh);
        }
    }

    function select(id, notify) {
        selectedId = id;
        scheduleRefresh();
        if (notify) {
            post({ type: 'select', id });
        }
    }

    function applyRender(html, selected) {
        root.innerHTML = html;
        selectedId = selected;
        hoverEl = null;
        scheduleRefresh();
    }

    // Drop targeting --------------------------------------------------------------------------

    function slotOf(node) {
        const el = node && node.nodeType === 1 ? node : node?.parentElement;
        return el ? el.closest('[data-pk-slot]') : null;
    }

    function isHorizontal(slot) {
        const style = getComputedStyle(slot);
        return (style.display.includes('flex') && style.flexDirection.startsWith('row')) || slot.tagName === 'TR';
    }

    function findDropTarget(x, y, target) {
        let slot = slotOf(target) || root.querySelector('[data-pk-root] [data-pk-slot], [data-pk-root][data-pk-slot]');
        if (!slot) {
            return null;
        }

        // Near the top or bottom edge of a container, drop beside it instead of inside it, otherwise
        // a section that fills the page could never have anything placed above or below it.
        const owner = byId(slot.getAttribute('data-pk-owner'));
        if (owner && !isRoot(owner)) {
            const rect = owner.getBoundingClientRect();
            const edge = Math.min(14, rect.height / 4);
            if (y - rect.top < edge || rect.bottom - y < edge) {
                const outer = slotOf(owner.parentElement);
                if (outer) {
                    slot = outer;
                }
            }
        }

        if (drag && drag.kind === 'move') {
            const moving = byId(drag.id);
            if (moving && (moving === slot || moving.contains(slot))) {
                return null;
            }
        }

        const children = Array.from(slot.children).filter((child) => child.hasAttribute('data-pk-id'));
        const horizontal = isHorizontal(slot);
        let index = children.length;
        for (let i = 0; i < children.length; i++) {
            const rect = children[i].getBoundingClientRect();
            const middle = horizontal ? rect.left + rect.width / 2 : rect.top + rect.height / 2;
            if ((horizontal ? x : y) < middle) {
                index = i;
                break;
            }
        }

        return { slot, children, index, horizontal, parentId: slot.getAttribute('data-pk-owner'), slotName: slot.getAttribute('data-pk-slot') };
    }

    function showDrop(target) {
        const zone = place(dropZone, target.slot);
        const { children, index, horizontal } = target;
        let left;
        let top;
        let length;
        if (children.length === 0) {
            left = zone.left + 4;
            top = zone.top + zone.height / 2;
            length = zone.width - 8;
        } else if (index < children.length) {
            const rect = children[index].getBoundingClientRect();
            left = horizontal ? rect.left - 2 : rect.left;
            top = horizontal ? rect.top : rect.top - 2;
            length = horizontal ? rect.height : rect.width;
        } else {
            const rect = children[children.length - 1].getBoundingClientRect();
            left = horizontal ? rect.right + 1 : rect.left;
            top = horizontal ? rect.top : rect.bottom + 1;
            length = horizontal ? rect.height : rect.width;
        }

        dropLine.classList.toggle('is-vertical', horizontal && children.length > 0);
        dropLine.style.left = `${left + window.scrollX}px`;
        dropLine.style.top = `${top + window.scrollY}px`;
        dropLine.style[horizontal && children.length > 0 ? 'height' : 'width'] = `${Math.max(length, 20)}px`;
        dropLine.style[horizontal && children.length > 0 ? 'width' : 'height'] = '';
        dropLine.hidden = false;
    }

    function hideDrop() {
        dropTarget = null;
        dropZone.hidden = true;
        dropLine.hidden = true;
    }

    // In-place editing ------------------------------------------------------------------------

    function startEditing(target, block) {
        if (editing) {
            stopEditing();
        }

        const kind = target.getAttribute('data-pk-edit-kind') === 'rich' ? 'rich' : 'plain';
        editing = {
            el: target,
            id: block.getAttribute('data-pk-id'),
            prop: target.getAttribute('data-pk-edit'),
            kind,
            last: kind === 'rich' ? target.innerHTML : target.innerText.replace(/\s+/g, ' ').trim(),
            timer: 0,
        };

        target.setAttribute('contenteditable', kind === 'rich' || !supportsPlainText ? 'true' : 'plaintext-only');
        target.classList.add('pk-editing');
        target.addEventListener('blur', onEditBlur);
        target.addEventListener('paste', onPaste);
        target.addEventListener('input', onEditInput);
        target.focus({ preventScroll: true });
        rte.hidden = kind !== 'rich';
        scheduleRefresh();
    }

    function stopEditing() {
        if (!editing) {
            return;
        }

        const current = editing;
        editing = null;
        current.el.removeAttribute('contenteditable');
        current.el.classList.remove('pk-editing');
        current.el.removeEventListener('blur', onEditBlur);
        current.el.removeEventListener('paste', onPaste);
        current.el.removeEventListener('input', onEditInput);
        rte.hidden = true;
        commitEdit(current);

        if (pendingRender) {
            const { html, selected } = pendingRender;
            pendingRender = null;
            applyRender(html, selected);
        }

        scheduleRefresh();
    }

    // Edits are sent while typing too, so a Save pressed in the host page never misses the last change.
    function commitEdit(current) {
        clearTimeout(current.timer);
        const value = current.kind === 'rich' ? current.el.innerHTML : current.el.innerText.replace(/\s+/g, ' ').trim();
        if (value !== current.last) {
            current.last = value;
            post({ type: 'inline', id: current.id, prop: current.prop, value, kind: current.kind });
        }
    }

    function onEditInput() {
        const current = editing;
        if (current) {
            clearTimeout(current.timer);
            current.timer = setTimeout(() => commitEdit(current), 400);
        }
    }

    function onEditBlur() {
        setTimeout(() => {
            if (!editing) {
                return;
            }

            if (rte.contains(document.activeElement) || editing.el.contains(document.activeElement)) {
                return;
            }

            // Focus moved to the host page, usually the token menu. Keep the caret until that menu inserts or closes.
            if (retainEditing && !document.hasFocus()) {
                return;
            }

            stopEditing();
        }, 0);
    }

    function offsetWithin(element, container, offset) {
        try {
            const range = document.createRange();
            range.selectNodeContents(element);
            range.setEnd(container, offset);
            return range.toString().length;
        } catch {
            return element.innerText.length;
        }
    }

    function caretOffsets(element) {
        const selection = window.getSelection();
        const length = element.innerText.length;
        if (!selection || selection.rangeCount === 0 || !element.contains(selection.anchorNode)) {
            return { start: length, end: length };
        }

        const range = selection.getRangeAt(0);
        return {
            start: offsetWithin(element, range.startContainer, range.startOffset),
            end: offsetWithin(element, range.endContainer, range.endOffset),
        };
    }

    function insertTextAtSelection(text) {
        if (document.execCommand('insertText', false, text)) {
            return true;
        }

        const selection = window.getSelection();
        if (!selection || selection.rangeCount === 0) {
            return false;
        }

        const range = selection.getRangeAt(0);
        range.deleteContents();
        const node = document.createTextNode(text);
        range.insertNode(node);
        range.setStartAfter(node);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
        return true;
    }

    window.pkCaptureTokenCaret = () => {
        if (!editing || !editing.el.hasAttribute('data-pk-token')) {
            return null;
        }

        retainEditing = true;
        const selection = window.getSelection();
        savedRange = selection && selection.rangeCount > 0 ? selection.getRangeAt(0).cloneRange() : null;
        const offsets = caretOffsets(editing.el);
        return {
            kind: 'canvas',
            nodeId: editing.id,
            prop: editing.prop,
            start: offsets.start,
            end: offsets.end,
        };
    };

    window.pkInsertTokenText = (text) => {
        if (!editing || !editing.el.hasAttribute('data-pk-token')) {
            return false;
        }

        editing.el.focus({ preventScroll: true });
        const selection = window.getSelection();
        if (savedRange && selection) {
            selection.removeAllRanges();
            selection.addRange(savedRange);
        }

        savedRange = null;
        retainEditing = false;
        if (!insertTextAtSelection(text)) {
            return false;
        }

        commitEdit(editing);
        return true;
    };

    window.pkReleaseTokenCaret = () => {
        retainEditing = false;
        savedRange = null;
        if (editing && !document.hasFocus()) {
            stopEditing();
        }
    };

    function onPaste(event) {
        event.preventDefault();
        const text = event.clipboardData ? event.clipboardData.getData('text/plain') : '';
        document.execCommand('insertText', false, text);
    }

    function positionRte() {
        if (!editing || rte.hidden) {
            return;
        }

        const rect = editing.el.getBoundingClientRect();
        const top = rect.top + window.scrollY - 34;
        rte.style.top = `${top < window.scrollY ? rect.bottom + window.scrollY + 6 : top}px`;
        rte.style.left = `${rect.left + window.scrollX}px`;
    }

    rte.addEventListener('mousedown', (event) => event.preventDefault());
    rte.addEventListener('click', (event) => {
        const button = event.target.closest('[data-rte]');
        if (!button || !editing) {
            return;
        }

        const command = button.dataset.rte;
        if (command === 'createLink') {
            const url = window.prompt('Link address', 'https://');
            if (url) {
                document.execCommand('createLink', false, url);
            }
        } else {
            document.execCommand(command, false, null);
        }
    });

    // Events ----------------------------------------------------------------------------------

    toolbar.addEventListener('mousedown', (event) => event.stopPropagation());
    toolbar.addEventListener('click', (event) => {
        const button = event.target.closest('[data-command]');
        if (!button || button.dataset.command === 'move') {
            return;
        }

        event.preventDefault();
        event.stopPropagation();
        const command = button.dataset.command === 'template' ? 'save-template' : button.dataset.command;
        post({ type: 'command', command, id: selectedId });
    });

    toolbar.addEventListener('dragstart', (event) => {
        if (!event.target.closest('[data-command="move"]') || !selectedId) {
            return;
        }

        drag = { kind: 'move', id: selectedId };
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData('text/plain', 'pagekrafter');
        const selected = byId(selectedId);
        if (selected) {
            event.dataTransfer.setDragImage(selected, 12, 12);
        }

        scheduleRefresh();
    });

    toolbar.addEventListener('dragend', () => {
        drag = null;
        hideDrop();
        scheduleRefresh();
    });

    document.addEventListener('mouseover', (event) => {
        if (preview || drag) {
            return;
        }

        const block = blockOf(event.target);
        if (block !== hoverEl) {
            hoverEl = block;
            scheduleRefresh();
        }
    });

    document.addEventListener('mouseleave', () => {
        hoverEl = null;
        scheduleRefresh();
    });

    document.addEventListener('click', (event) => {
        if (chrome.contains(event.target)) {
            return;
        }

        if (editing && editing.el.contains(event.target)) {
            return;
        }

        event.preventDefault();
        if (preview) {
            return;
        }

        const block = blockOf(event.target) || root.querySelector('[data-pk-root]');
        if (block) {
            select(block.getAttribute('data-pk-id'), true);
        }
    }, true);

    document.addEventListener('dblclick', (event) => {
        if (preview || chrome.contains(event.target)) {
            return;
        }

        const target = event.target instanceof Element ? event.target.closest('[data-pk-edit]') : null;
        const block = target ? blockOf(target) : null;
        if (target && block) {
            if (block.getAttribute('data-pk-id') !== selectedId) {
                select(block.getAttribute('data-pk-id'), true);
            }

            startEditing(target, block);
        }
    });

    document.addEventListener('submit', (event) => event.preventDefault(), true);

    document.addEventListener('keydown', (event) => {
        if (editing) {
            if (event.key === 'Escape' || (event.key === 'Enter' && editing.kind === 'plain')) {
                event.preventDefault();
                editing.el.blur();
                stopEditing();
            }

            return;
        }

        if (preview) {
            return;
        }

        const mod = event.ctrlKey || event.metaKey;
        const key = event.key.toLowerCase();
        let command = null;
        if (mod && key === 'z') {
            command = event.shiftKey ? 'redo' : 'undo';
        } else if (mod && key === 'y') {
            command = 'redo';
        } else if (mod && key === 'd') {
            command = 'duplicate';
        } else if (mod && event.key === 'ArrowUp') {
            command = 'up';
        } else if (mod && event.key === 'ArrowDown') {
            command = 'down';
        } else if (!mod && (event.key === 'Delete' || event.key === 'Backspace')) {
            command = 'delete';
        } else if (event.key === 'Escape') {
            command = 'parent';
        }

        if (command) {
            event.preventDefault();
            post({ type: 'command', command, id: selectedId });
        }
    });

    document.addEventListener('dragover', (event) => {
        if (!drag) {
            return;
        }

        const target = findDropTarget(event.clientX, event.clientY, event.target);
        if (!target) {
            hideDrop();
            return;
        }

        event.preventDefault();
        event.dataTransfer.dropEffect = drag.kind === 'move' ? 'move' : 'copy';
        dropTarget = target;
        showDrop(target);
    });

    document.addEventListener('dragleave', (event) => {
        if (!event.relatedTarget) {
            hideDrop();
        }
    });

    document.addEventListener('drop', (event) => {
        if (!drag || !dropTarget) {
            return;
        }

        event.preventDefault();
        const target = dropTarget;
        const payload = drag;
        hideDrop();
        drag = null;
        post({
            type: 'drop',
            parentId: target.parentId,
            slot: target.slotName,
            index: target.index,
            kind: payload.kind,
            blockType: payload.blockType,
            templateId: payload.templateId,
            id: payload.id,
        });
    });

    window.addEventListener('message', (event) => {
        if (event.source !== window.parent || !event.data || event.data.__pk !== true) {
            return;
        }

        const message = event.data;
        switch (message.type) {
            case 'render':
                if (editing) {
                    pendingRender = { html: message.html, selected: message.selectedId };
                } else {
                    applyRender(message.html, message.selectedId);
                }

                break;
            case 'select':
                selectedId = message.id;
                scheduleRefresh();
                if (message.scroll) {
                    byId(message.id)?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
                }

                break;
            case 'drag':
                drag = message.payload || null;
                if (!drag) {
                    hideDrop();
                }

                scheduleRefresh();
                break;
            case 'preview':
                preview = !!message.on;
                if (preview) {
                    stopEditing();
                    [hoverBox, selectBox, toolbar, dropZone, dropLine, rte].forEach((el) => { el.hidden = true; });
                }

                document.body.classList.toggle('pk-preview', preview);
                scheduleRefresh();
                break;
        }
    });

    window.addEventListener('scroll', scheduleRefresh, { passive: true });
    window.addEventListener('resize', scheduleRefresh);
    document.addEventListener('load', scheduleRefresh, true);
    new ResizeObserver(scheduleRefresh).observe(root);

    post({ type: 'ready' });
})();
