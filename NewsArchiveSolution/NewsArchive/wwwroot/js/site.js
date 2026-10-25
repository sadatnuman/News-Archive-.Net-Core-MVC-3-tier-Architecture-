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

    media.addEventListener("change", function () {
        if (storedChoice() === "system") apply("system");
    });

    apply(storedChoice());
})();
