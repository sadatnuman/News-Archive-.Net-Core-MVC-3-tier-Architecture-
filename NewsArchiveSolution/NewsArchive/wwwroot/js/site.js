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

    media.addEventListener("change", function () {
        if (storedChoice() === "system") apply("system");
    });

    apply(storedChoice());
})();
