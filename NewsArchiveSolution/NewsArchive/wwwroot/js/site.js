document.addEventListener("click", function (event) {
    var button = event.target.closest("[data-copy-link]");
    if (!button || !navigator.clipboard) return;

    navigator.clipboard.writeText(button.getAttribute("data-copy-link")).then(function () {
        var icon = button.querySelector("i");
        if (!icon) return;
        icon.className = "bi bi-check2";
        setTimeout(function () { icon.className = "bi bi-link-45deg"; }, 1500);
    });
});
