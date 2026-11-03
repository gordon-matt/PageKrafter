// A stand-in file manager for the demo. It shows how a browser-based file manager plugs into PageKrafter:
// register it with services.AddPageKrafterJsFilePicker("DemoFiles.pick"), receive { kind, title, current },
// and return a promise for { url, name }, or null if the user closes it without choosing.
window.DemoFiles = (() => {
    const library = [
        { kind: "image", url: "/img/Logo.png", name: "Logo.png" },
        { kind: "image", url: "/img/Icon.png", name: "Icon.png" },
    ];

    function element(tag, className, text) {
        const node = document.createElement(tag);
        if (className) node.className = className;
        if (text) node.textContent = text;
        return node;
    }

    function pick(request) {
        return new Promise(resolve => {
            const files = library.filter(file => request.kind === "any" || file.kind === request.kind);

            const backdrop = element("div", "demo-files-backdrop");
            const dialog = element("div", "demo-files");
            dialog.setAttribute("role", "dialog");
            dialog.setAttribute("aria-label", request.title || "Choose a file");
            dialog.append(element("h2", null, request.title || "Choose a file"));

            const list = element("div", "demo-files-list");
            if (files.length === 0) {
                list.append(element("p", null, "The demo library has no " + request.kind + " files."));
            }
            for (const file of files) {
                const button = element("button", "demo-files-item");
                button.type = "button";
                const image = element("img");
                image.src = file.url;
                image.alt = "";
                button.append(image, element("span", null, file.name));
                button.addEventListener("click", () => close({ url: file.url, name: file.name }));
                list.append(button);
            }
            dialog.append(list);

            const cancel = element("button", "demo-files-cancel", "Cancel");
            cancel.type = "button";
            cancel.addEventListener("click", () => close(null));
            dialog.append(cancel);

            function close(result) {
                document.removeEventListener("keydown", onKey);
                backdrop.remove();
                resolve(result);
            }

            function onKey(event) {
                if (event.key === "Escape") close(null);
            }

            backdrop.addEventListener("click", event => { if (event.target === backdrop) close(null); });
            document.addEventListener("keydown", onKey);
            backdrop.append(dialog);
            document.body.append(backdrop);
            cancel.focus();
        });
    }

    return { pick };
})();
