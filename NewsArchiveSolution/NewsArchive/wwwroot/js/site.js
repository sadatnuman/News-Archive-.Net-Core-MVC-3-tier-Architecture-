(function () {
    var root = document.documentElement;
    var media = window.matchMedia("(prefers-color-scheme: dark)");

    function storedChoice() {
        try { return localStorage.getItem("theme") || "system"; } catch (e) { return "system"; }
    }

    function apply(choice) {
        var dark = choice === "dark" || (choice === "system" && media.matches);
        root.setAttribute("data-bs-theme", dark ? "dark" : "light");
        root.setAttribute("data-theme-choice", choice);
        document.querySelectorAll("[data-theme-value]").forEach(function (button) {
            var active = button.getAttribute("data-theme-value") === choice;
            button.classList.toggle("active", active);
            button.setAttribute("aria-pressed", active ? "true" : "false");
        });
    }

    document.addEventListener("click", function (event) {
        var themeButton = event.target.closest("[data-theme-value]");
        if (themeButton) {
            var choice = themeButton.getAttribute("data-theme-value");
            try { localStorage.setItem("theme", choice); } catch (e) { }
            apply(choice);
            return;
        }

        var copyButton = event.target.closest("[data-copy-link]");
        if (copyButton && navigator.clipboard) {
            navigator.clipboard.writeText(copyButton.getAttribute("data-copy-link")).then(function () {
                var icon = copyButton.querySelector("i");
                if (!icon) return;
                icon.className = "bi bi-check2";
                setTimeout(function () { icon.className = "bi bi-link-45deg"; }, 1500);
            });
        }
    });

    function limitTags(picker) {
        var max = parseInt(picker.getAttribute("data-max-tags"), 10) || 8;
        var boxes = picker.querySelectorAll("input[type=checkbox]");
        var checked = picker.querySelectorAll("input[type=checkbox]:checked").length;
        boxes.forEach(function (box) {
            box.disabled = !box.checked && checked >= max;
        });
    }

    document.querySelectorAll(".tag-picker").forEach(function (picker) {
        picker.addEventListener("change", function () { limitTags(picker); });
        limitTags(picker);
    });

    function addPasswordToggle(input) {
        if (input.closest(".password-field")) return;
        var body = document.body;
        var showLabel = body.getAttribute("data-show-password") || "Show password";
        var hideLabel = body.getAttribute("data-hide-password") || "Hide password";

        var group = document.createElement("div");
        group.className = "input-group password-field";
        input.parentNode.insertBefore(group, input);
        group.appendChild(input);

        var button = document.createElement("button");
        button.type = "button";
        button.className = "btn password-toggle";
        button.setAttribute("aria-label", showLabel);
        button.setAttribute("aria-pressed", "false");
        button.setAttribute("title", showLabel);
        if (input.id) button.setAttribute("aria-controls", input.id);
        button.innerHTML = '<i class="bi bi-eye" aria-hidden="true"></i>';
        group.appendChild(button);

        button.addEventListener("click", function () {
            var visible = input.type === "password";
            input.type = visible ? "text" : "password";
            button.setAttribute("aria-pressed", visible ? "true" : "false");
            button.setAttribute("aria-label", visible ? hideLabel : showLabel);
            button.setAttribute("title", visible ? hideLabel : showLabel);
            button.querySelector("i").className = visible ? "bi bi-eye-slash" : "bi bi-eye";
            input.focus();
        });

        var form = input.form;
        if (form) {
            form.addEventListener("submit", function () {
                input.type = "password";
            });
        }
    }

    document.querySelectorAll("input[type=password]").forEach(addPasswordToggle);

    document.querySelectorAll("input[type=file][data-max-bytes]").forEach(function (input) {
        input.addEventListener("change", function () {
            var max = parseInt(input.getAttribute("data-max-bytes"), 10);
            var file = input.files && input.files[0];
            var tooLarge = file && max && file.size > max;
            input.setCustomValidity(tooLarge ? input.getAttribute("data-too-large") : "");
            input.classList.toggle("is-invalid", !!tooLarge);
            if (tooLarge) input.reportValidity();
        });
    });

    document.addEventListener("submit", function (event) {
        var form = event.target.closest(".bookmark-form");
        if (!form || !window.fetch) return;
        event.preventDefault();
        var button = form.querySelector(".bookmark-btn");
        var token = form.querySelector("input[name=__RequestVerificationToken]");
        button.disabled = true;
        fetch(form.action, {
            method: "POST",
            body: new FormData(form),
            credentials: "same-origin",
            headers: { "Accept": "application/json", "RequestVerificationToken": token ? token.value : "" }
        }).then(function (response) {
            if (!response.ok) throw new Error("bookmark");
            return response.json();
        }).then(function (data) {
            document.querySelectorAll(".bookmark-form input[name=articleId][value='" + form.querySelector("input[name=articleId]").value + "']").forEach(function (input) {
                var other = input.closest(".bookmark-form").querySelector(".bookmark-btn");
                var label = data.saved ? other.getAttribute("data-remove-label") : other.getAttribute("data-save-label");
                other.classList.toggle("is-saved", data.saved);
                other.setAttribute("aria-pressed", data.saved ? "true" : "false");
                other.setAttribute("aria-label", label);
                other.setAttribute("title", label);
                other.querySelector("i").className = "bi " + (data.saved ? "bi-bookmark-fill" : "bi-bookmark");
            });
        }).catch(function () {
            form.submit();
        }).finally(function () {
            button.disabled = false;
        });
    });

    function wrapSelection(textarea, kind) {
        var start = textarea.selectionStart;
        var end = textarea.selectionEnd;
        var value = textarea.value;
        var selected = value.slice(start, end);
        var lineStart = value.lastIndexOf("\n", start - 1) + 1;
        var insert;
        var cursorStart;
        var cursorEnd;

        function prefixLines(prefix, numbered) {
            var block = value.slice(lineStart, end) || "";
            var lines = block.split("\n").map(function (line, i) { return (numbered ? (i + 1) + ". " : prefix) + line; });
            insert = lines.join("\n");
            textarea.setRangeText(insert, lineStart, end, "end");
        }

        switch (kind) {
            case "bold":
                insert = "**" + (selected || "bold text") + "**";
                textarea.setRangeText(insert, start, end, "select");
                cursorStart = start + 2;
                cursorEnd = start + insert.length - 2;
                textarea.setSelectionRange(cursorStart, cursorEnd);
                break;
            case "italic":
                insert = "*" + (selected || "italic text") + "*";
                textarea.setRangeText(insert, start, end, "select");
                textarea.setSelectionRange(start + 1, start + insert.length - 1);
                break;
            case "link":
                insert = "[" + (selected || "link text") + "](https://)";
                textarea.setRangeText(insert, start, end, "end");
                textarea.setSelectionRange(start + insert.length - 9, start + insert.length - 1);
                break;
            case "heading":
                prefixLines("## ", false);
                break;
            case "quote":
                prefixLines("> ", false);
                break;
            case "list":
                prefixLines("- ", false);
                break;
            case "numbered":
                prefixLines("", true);
                break;
        }
        textarea.focus();
        textarea.dispatchEvent(new Event("input", { bubbles: true }));
    }

    document.querySelectorAll(".md-editor").forEach(function (editor) {
        var textarea = editor.querySelector(".md-input");
        var preview = editor.querySelector(".md-preview");
        var toolbar = editor.querySelector(".md-toolbar");
        var tabs = editor.querySelectorAll("[data-md-tab]");
        var form = editor.closest("form");

        editor.querySelectorAll("[data-md]").forEach(function (button) {
            button.addEventListener("click", function () { wrapSelection(textarea, button.getAttribute("data-md")); });
        });

        tabs.forEach(function (tab) {
            tab.addEventListener("click", function () {
                var showPreview = tab.getAttribute("data-md-tab") === "preview";
                tabs.forEach(function (t) {
                    var active = t === tab;
                    t.classList.toggle("active", active);
                    t.setAttribute("aria-selected", active ? "true" : "false");
                });
                textarea.hidden = showPreview;
                toolbar.hidden = showPreview;
                preview.hidden = !showPreview;
                if (!showPreview) { textarea.focus(); return; }

                var token = form ? form.querySelector("input[name=__RequestVerificationToken]") : null;
                var body = new FormData();
                body.append("content", textarea.value);
                if (token) body.append("__RequestVerificationToken", token.value);
                preview.textContent = "…";
                fetch(editor.getAttribute("data-preview-url"), { method: "POST", body: body, credentials: "same-origin" })
                    .then(function (response) { return response.ok ? response.text() : ""; })
                    .then(function (html) { preview.innerHTML = html; })
                    .catch(function () { preview.textContent = ""; });
            });
        });
    });

    var reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    var finePointer = window.matchMedia("(hover: hover) and (pointer: fine)");

    function attachTilt(card) {
        var max = parseFloat(card.getAttribute("data-tilt-max")) || 7;
        var frame = 0;
        var pending = null;

        function render() {
            frame = 0;
            if (!pending) return;
            var rect = card.getBoundingClientRect();
            var x = (pending.x - rect.left) / rect.width;
            var y = (pending.y - rect.top) / rect.height;
            card.style.setProperty("--ry", ((x - 0.5) * 2 * max).toFixed(2) + "deg");
            card.style.setProperty("--rx", ((0.5 - y) * 2 * max).toFixed(2) + "deg");
            card.style.setProperty("--mx", (x * 100).toFixed(1) + "%");
            card.style.setProperty("--my", (y * 100).toFixed(1) + "%");
        }

        card.addEventListener("pointerenter", function (event) {
            if (event.pointerType !== "mouse" || reducedMotion.matches) return;
            card.classList.add("is-tilting");
        });

        card.addEventListener("pointermove", function (event) {
            if (event.pointerType !== "mouse" || reducedMotion.matches) return;
            pending = { x: event.clientX, y: event.clientY };
            if (!frame) frame = requestAnimationFrame(render);
        });

        card.addEventListener("pointerleave", function () {
            pending = null;
            card.classList.remove("is-tilting");
            card.style.setProperty("--rx", "0deg");
            card.style.setProperty("--ry", "0deg");
        });
    }

    if (finePointer.matches) {
        document.querySelectorAll("[data-tilt]").forEach(attachTilt);
    }

    if ("IntersectionObserver" in window && !reducedMotion.matches) {
        var revealTargets = document.querySelectorAll("main .story-card, main .data-card, main .side-panel, main .section-head, main .story-compact");
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                entry.target.classList.add("is-visible");
                observer.unobserve(entry.target);
            });
        }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });

        revealTargets.forEach(function (element, index) {
            var rect = element.getBoundingClientRect();
            if (rect.top < window.innerHeight && rect.bottom > 0) return;
            element.classList.add("reveal-ready");
            element.style.transitionDelay = Math.min(index % 6, 5) * 60 + "ms";
            observer.observe(element);
        });
    }

    document.addEventListener("submit", function (event) {
        var form = event.target.closest("form[data-confirm]");
        if (form && !window.confirm(form.getAttribute("data-confirm"))) event.preventDefault();
    }, true);

    var articleBody = document.querySelector("[data-article-body]");
    var progress = document.querySelector("[data-read-progress] span");
    var readNext = document.querySelector("[data-read-next]");
    var upNext = document.querySelector("[data-up-next]");
    var dismissKey = "readNextDismissed";
    var readNextDismissed = false;
    var upNextVisible = false;

    try { readNextDismissed = sessionStorage.getItem(dismissKey) === "1"; } catch (e) { }

    function readingRatio() {
        var rect = articleBody.getBoundingClientRect();
        var total = rect.height - window.innerHeight * 0.5;
        if (total <= 0) return rect.top < window.innerHeight ? 1 : 0;
        return Math.min(1, Math.max(0, (window.innerHeight * 0.5 - rect.top) / total));
    }

    function updateReading() {
        if (!articleBody) return;
        var ratio = readingRatio();
        if (progress) progress.style.transform = "scaleX(" + ratio.toFixed(3) + ")";
        if (readNext && !readNextDismissed) {
            var show = ratio > 0.45 && !upNextVisible;
            if (show && readNext.hidden) {
                readNext.hidden = false;
                requestAnimationFrame(function () { readNext.classList.add("is-open"); });
            } else if (!show && !readNext.hidden) {
                readNext.classList.remove("is-open");
                readNext.hidden = true;
            }
        }
    }

    if (articleBody) {
        var ticking = false;
        window.addEventListener("scroll", function () {
            if (ticking) return;
            ticking = true;
            requestAnimationFrame(function () { ticking = false; updateReading(); });
        }, { passive: true });
        window.addEventListener("resize", updateReading);

        if (upNext && "IntersectionObserver" in window) {
            new IntersectionObserver(function (entries) {
                upNextVisible = entries[0].isIntersecting;
                updateReading();
            }, { threshold: 0.2 }).observe(upNext);
        }
        updateReading();
    }

    if (readNext) {
        readNext.querySelector("[data-read-next-close]").addEventListener("click", function () {
            readNextDismissed = true;
            try { sessionStorage.setItem(dismissKey, "1"); } catch (e) { }
            readNext.classList.remove("is-open");
            readNext.hidden = true;
        });
    }

    media.addEventListener("change", function () {
        if (storedChoice() === "system") apply("system");
    });

    apply(storedChoice());
})();
