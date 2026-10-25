(function () {
    try {
        var choice = localStorage.getItem("theme") || "system";
        var dark = choice === "dark" || (choice === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches);
        document.documentElement.setAttribute("data-bs-theme", dark ? "dark" : "light");
        document.documentElement.setAttribute("data-theme-choice", choice);
    } catch (e) { }
})();
