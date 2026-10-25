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

    media.addEventListener("change", function () {
        if (storedChoice() === "system") apply("system");
    });

    apply(storedChoice());
})();
